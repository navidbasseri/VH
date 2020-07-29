using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using OpenCvSharp;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace VH
{
public partial class SideBar : Form
    {
        const int max_width = 80;
        const int min_width = 5;
        const int ViewBoxSize = 200;
        const int mouse_tol = 8;
        const double min_opacity = 0.5;
        const int change_speed_bg = 3;
        static bool Capturing = false;
        static Stopwatch stopwatch = new Stopwatch();
        static Mat Previous_frame = new Mat();
        static Mat Last_Change = new Mat();
        static bool CaptureEngineRunning = false;
        static POINT Current_mouse_pos = new POINT();
        public static List<Rect> IgnoreAre = new List<Rect>();
        const double Framechange_tolerance = 5.00;
        const double Similarity_tolerance = 80.00;
        public static double CurrentFrameChanges = 0.0;


        public delegate bool FlushEventsHandler(in Mat image, DateTime TimeStamp, bool end = false);
        public delegate bool ChangesEventHandler(in Mat Current_frame, in Mat Previous_frame, in Mat mask, POINT cursor_position, DateTime TimeStamp);
        public static event FlushEventsHandler FlushEventsTrigger;
        public static event ChangesEventHandler ChangesEventTrigger;

        static bool OnSelf = true;
        bool AutoHide = false;
        Color normal_bg = ColorCalib(SystemColors.ControlDarkDark);
        Color current_bg = SystemColors.ControlDarkDark;
        Color record_bg = ColorCalib( Color.FromArgb(200, 30, 30));
        Color record_Dark_bg = ColorCalib (Color.FromArgb(100, 0, 0));

        static string RecordPath = Application.StartupPath +  "\\Record\\";
        static PreviewBar previewBar = new PreviewBar();

        static List<LLHook.Event> events = new List<LLHook.Event>();
        static List<EventArea> eventAreas = new List<EventArea>();

        static Color ColorCalib(Color col)
        {
            return Color.FromArgb((col.R / change_speed_bg) * change_speed_bg,
                                  (col.G / change_speed_bg) * change_speed_bg,
                                  (col.B / change_speed_bg) * change_speed_bg);
        }


        [Serializable()]
        public class EventArea
        {
            public DateTime time = DateTime.Now;
            public POINT actual_point = LLHook.GetCursorPos();
            public bool capslock = (((ushort)LLHook.GetKeyState(0x14)) & 0xffff) != 0;
            public bool shift = (((ushort)LLHook.GetKeyState(0x10)) & 0x1000) != 0;
            public List<Rectangle> Rects = new List<Rectangle>();
            public List<Tuple<String, String>> ObjectNames = new List<Tuple<String, String>>();

            protected List<Rect> cv_rects = new List<Rect>();
            protected List<Tuple<Mat,Mat>> Objects = new List<Tuple<Mat, Mat>>();

            public EventArea()
            {
            }

            public EventArea(DateTime value, POINT actual_point)
            {
                time = value;
                this.actual_point = actual_point;
            }


            public void AddRects(IEnumerable<Rect> collection)
            {
                cv_rects.AddRange(collection);
                foreach (Rect rect in cv_rects)
                    Rects.Add(new Rectangle(rect.X,rect.Y, rect.Width,rect.Height));
            }

            public void AddObjects(in Mat Current_frame, in Mat Previous_frame)
            {
                foreach (Rect rect in cv_rects)
                    Objects.Add(new Tuple<Mat, Mat>(Graphic.Crop(Previous_frame, rect), Graphic.Crop(Current_frame, rect)));
            }

            public List<Rect> FindRects(POINT pt)
            {
                return cv_rects.FindAll(item => item.Contains(pt.x,pt.y));
            }

            public List<Tuple<Mat, Mat, Rectangle>> FindRectObjects(POINT pt)
            {
                List<Tuple<Mat, Mat, Rectangle>> result = new List<Tuple<Mat, Mat, Rectangle>>();
                foreach (Rectangle rect in Rects)
                    if (rect.Contains(pt))
                        result.Add(new Tuple<Mat, Mat, Rectangle>(Objects[Rects.IndexOf(rect)].Item1,
                                                                  Objects[Rects.IndexOf(rect)].Item2,
                                                                  rect));
                return result;
            }
            public List<Tuple<Mat, Mat, Rectangle>> AllRectObjects()
            {
                List<Tuple<Mat, Mat, Rectangle>> result = new List<Tuple<Mat, Mat, Rectangle>>();
                foreach (Rectangle rect in Rects)
                    result.Add(new Tuple<Mat, Mat, Rectangle>(Objects[Rects.IndexOf(rect)].Item1,
                                                              Objects[Rects.IndexOf(rect)].Item2,
                                                              rect));
                return result;
            }


            public List<Rect> GetRects()
            {
                return cv_rects;
            }

            public List<Tuple<Mat, Mat>> GetObjects()
            {
                return Objects;
            }

            public bool OverlapRect(Rect bound_rect)
            {
                foreach (Rect rect in cv_rects)
                {
                    Rect union = bound_rect & rect;
                    if (union.Width * union.Height != 0)
                        return true;
                }

                return false;
            }

            public bool ContainPoint(OpenCvSharp.Point pt)
            {
                foreach (Rect rect in cv_rects)
                {
                        if(rect.Contains(pt))
                            return true;
                }

                return false;
            }


            //
            // Summary:
            //     Check if the current change contains the changes from argument.
            //
            // Parameters:
            //   item:
            //     The ChangeAreas object to be checked against current ChangeAreas object
            //     the Hashs in each object will be considered for comparing.
            //
            // Returns:
            //     true if the both object have overlapping Hashs.
            //     otherwise false.
            public bool ContainObjects(EventArea item)
            {
                foreach (Tuple<String, String> h in item.ObjectNames)
                    if (ObjectNames.Contains(h))
                        return true;

                return false;
            }

            [OnDeserialized()]
            internal void OnDeserializedMethod(StreamingContext context)
            {// recreate the rects from rectangles on deserialize
                foreach (Rectangle rect in Rects)
                    cv_rects.Add(new Rect(rect.X, rect.Y, rect.Width, rect.Height));

                foreach(Tuple<String, String> ObjectName in ObjectNames)
                {
                    Mat Object1, Object2;
                    if (Graphic.LoadObject(RecordPath + ObjectName.Item1 + ".png", out Object1) &&
                        Graphic.LoadObject(RecordPath + ObjectName.Item2 + ".png", out Object2))
                        Objects.Add(new Tuple<Mat, Mat>(Object1, Object2));
                    else
                        Objects.Add(new Tuple<Mat, Mat>(new Mat(), new Mat()));
                }
            }

            [OnSerializing()]
            internal void OnSerializingMethod(StreamingContext context)
            {// create the image files based on hash image structure
                bool already_hashed = false;
                foreach (Tuple < Mat,Mat > @object in Objects)
                {
                    ObjectNames.Add(new Tuple<String, String>(
                        Graphic.ImageObject(@object.Item1, RecordPath, ref already_hashed),
                        Graphic.ImageObject(@object.Item2, RecordPath, ref already_hashed))
                    );
                }
            }

            ~EventArea()
            {
                foreach (Tuple<Mat, Mat> @object in Objects)
                {
                    @object.Item1.Dispose();
                    @object.Item2.Dispose();
                }
            }

        }

        public bool ObjectTracker(in Mat Current_frame, in Mat Previous_frame, in Mat Mask, POINT actual_point, DateTime TimeStamp)
        {
            EventArea eventarea = new EventArea(TimeStamp, actual_point);

            List<Rect> change_rects = new List<Rect>();
            if (AllowedArea(actual_point))
            {
                //POC: Create object model of picture based on positions and movements in a screen box (or bigger, etc.)
                change_rects = Graphic.DetectRegions(Mask, Graphic.RectfromPoint(actual_point, Current_frame.Size(), ViewBoxSize, ViewBoxSize), 128.0, 1, 0);
                //TODO: no rect changes => take rect from point using DetectRegions from source Current_frame
                //TODO: Optimization => combine changed rects to form a bigger rect and check for point inside it (or relevalant position)

                if (change_rects.Exists(item => item.Contains(new OpenCvSharp.Point(actual_point.x, actual_point.y))))
                {
                    change_rects.RemoveAll(item => !item.Contains(new OpenCvSharp.Point(actual_point.x, actual_point.y)));
                    if (change_rects.Count > 1)
                        Cv2.GroupRectangles(change_rects, 0, 0.2);
                }
                else
                {
                    Rect bound_rect = Graphic.RectfromPoint(actual_point, Current_frame.Size(), ViewBoxSize, ViewBoxSize);
                    List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>();
                    foreach (Rect rect in change_rects)
                    {
                        Rect union = bound_rect & rect;
                        if (union.Width * union.Height != 0)
                        {
                            points.Add(rect.TopLeft);
                            points.Add(rect.BottomRight);
                        }
                    }
                    change_rects.Clear();
                    Rect final = Cv2.BoundingRect(points);
                    if (final.Width * final.Height > 0)
                        change_rects.Add(final);
                }
            }

            if (change_rects.Count() > 0)
            {
                eventarea.AddRects(change_rects);
                eventarea.AddObjects(Current_frame, Previous_frame);
                //drop this event area if similar event with the same image areas and changes already logged
                if (!eventAreas.Exists(item => item.ContainObjects(eventarea)))
                    eventAreas.Add(eventarea);
            }

            //POC: R&D -> the whole image needs to be saved in case there are changes not under cursor but related to current event (example : keyboard events)

            return true;
        }

        public class EventStruct
        {
            public string time = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            public POINT actual_point = LLHook.GetCursorPos();
            public bool capslock = (((ushort)LLHook.GetKeyState(0x14)) & 0xffff) != 0;
            public bool shift = (((ushort)LLHook.GetKeyState(0x10)) & 0x1000) != 0;
            public List<LLHook.Event> events = new List<LLHook.Event>();
        }
        public bool FlushEvents(in Mat image, DateTime TimeStamp, bool end=false)
        {
            //TODO : compare the changed part with old changed part. if the same the movement happend then look for next change
            //if no movement happened cash the current state to be saved before next event hapenning


            if (events.Count() == 0)
            {
                if (!end) 
                    return false;
                else  // in ending senarion no need to keep any change area if there is no event happened
                    eventAreas.Clear();
            }

            EventStruct event_struct = new EventStruct();
            event_struct.time = TimeStamp.ToString("yyyyMMddHHmmssfff");
            lock (events) {
                event_struct.events.AddRange(events);
                events.Clear();
            }

            List<EventArea> event_Areas = new List<EventArea>();
            if (eventAreas.Count() != 0)
                lock (eventAreas)
                {
                    event_Areas.AddRange(eventAreas);
                    eventAreas.Clear();
                }


            Mat freezd_image = new Mat();
            image.CopyTo(freezd_image);


            Task FlushEventsTask = Task.Run(() =>
            {
                string filename = RecordPath + event_struct.time ;
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonSerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                jsonSerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                using (var writer = new StreamWriter(File.Create(filename + ".ejs")))
                    writer.Write(JsonConvert.SerializeObject(event_struct, jsonSerializerSettings));

                if (event_Areas.Count() != 0)
                {
                    //TODO: remove the un-needed event areas which does not have effect on behaviour (files and entries in dictionaries)

                    //TODO: track the changes back to separate connected objects from eachother (case on menu)
                    //      case 1: hover cause change under cursor (probible object)
                    //      case 2: hover casue another area change (probible objects out of cursor area in combining with case 1)
                    //      case 3: hover cause both area changes (track and depart objects from previous object state in case 1)
                    HashSet<EventArea> Hot_Areas = new HashSet<EventArea>();

                    /* REFACTOR : all changed area should be considered to create the last state therefore cannot be removed easily
                    // Rect bound_rect = Graphic.RectfromPoint(event_struct.actual_point, freezd_image.Size(), ViewBoxSize, ViewBoxSize);
                    List<POINT> points = new List<POINT>();
                    foreach (LLHook.Event event_ in event_struct.events)
                        if (event_.HookType == LLHook.HookType.WH_MOUSE_LL)
                            points.Add((event_ as LLHook.MouseEvent).msllhookstruct.pt);

                    // event_Areas.RemoveAll(item => !item.OverlapRect(bound_rect));
                    foreach (POINT p in points)
                        foreach(EventArea eventArea in event_Areas.FindAll(item => item.ContainPoint(new OpenCvSharp.Point(p.x, p.y))))
                            Hot_Areas.Add(eventArea);

                    Hot_Areas.Clear();
                    */

                    foreach (EventArea eventArea in event_Areas)
                        Hot_Areas.Add(eventArea);


                    using (var writer = new StreamWriter(File.Create(filename + ".cjs")))
                        writer.Write(JsonConvert.SerializeObject(Hot_Areas.ToList(), jsonSerializerSettings));
                }

                //flushing the hash table
                lock (Graphic.ImageHashTable)
                {
                    using (var writer = new StreamWriter(File.Create(RecordPath + "Objects.djs")))
                        writer.Write(JsonConvert.SerializeObject(Graphic.ImageHashTable, jsonSerializerSettings));
                    lock (Graphic.ImageSubHashTable)
                    {
                        using (var writer = new StreamWriter(File.Create(RecordPath + "Objects.sdjs")))
                            writer.Write(JsonConvert.SerializeObject(Graphic.ImageSubHashTable, jsonSerializerSettings));
                    }
                }

                freezd_image.ImWrite(filename + ".png", Graphic.png_prms);
                freezd_image.Dispose();
            });
            
            return true;

            /*
            double prev_opaciry = Opacity;
            if (OnBar) Opacity = 0;
            Opacity = prev_opaciry;
            */
        }

        public static bool AllowedArea(POINT point)
        {
            if (OnSelf)
                return false;
            if (IgnoreAre.Exists(item => item.Contains(point.x, point.y)))
                return false;
            
            return true;
        }

        public static bool Track_Changes()
        {
            lock (stopwatch)
            {
                stopwatch.Restart();
                Mat Current_frame ;
                DateTime TimeStamp = DateTime.Now;
                if (Graphic.Screenshot(out Current_frame, Graphic.desktop_rect, false))
                {
                    Mat Mask;
                    CurrentFrameChanges = 100.0 - Graphic.ComparePercentage(Current_frame, Previous_frame, out Mask);
                    if (Capturing)
                    {
                        Mat previous_image = new Mat();
                        Previous_frame.CopyTo(previous_image);
                        POINT mouse_pos = Current_mouse_pos;

                        //History the changes by objects found under the cursor position
                        Task.Run(() =>
                        {
                            ChangesEventTrigger(Current_frame, previous_image, Mask, mouse_pos, TimeStamp);
                            Model.ExtractObjects(Current_frame, previous_image, Mask, mouse_pos, TimeStamp);
                            Model.Highlight();
                        });
                        
                        
                        if (CurrentFrameChanges > Framechange_tolerance)
                        {//if picture similarities are smaller that tolerance then flush the events
                            if (FlushEventsTrigger(previous_image, TimeStamp)) ;
                            //TODO check if the flush was not succeed rise an error
                        }
                       

                    }
                    Current_frame.CopyTo(Previous_frame);
                }
                stopwatch.Stop();
            }
            return true;
        }

        void EventFunction(LLHook.Event event_)
        {
            if (event_.HookType == LLHook.HookType.WH_MOUSE_LL)
            {
                Current_mouse_pos = (event_ as LLHook.MouseEvent).msllhookstruct.pt;
                if (!AllowedArea(Current_mouse_pos)) return;
            }

            if (!Capturing) return;
            //on mousemove we won't keep the event 
            if (event_.HookType == LLHook.HookType.WH_MOUSE_LL &&
               (LLHook.MouseState.WM_MOUSEMOVE == (LLHook.MouseState)(event_ as LLHook.MouseEvent).state ||
                LLHook.MouseState.WM_NCMOUSEMOVE == (LLHook.MouseState)(event_ as LLHook.MouseEvent).state)
              )
            {
                //mouse movement
            }
            else
            {
                lock (events)
                    events.Add(event_);
                Track_Changes();
            }
        }


        public bool load_Dictionaries()
        {
            Graphic.ImageHashTable.Clear();
            Graphic.ImageSubHashTable.Clear();

            if (File.Exists(RecordPath + "Objects.djs"))
            using (var reader = new StreamReader(File.OpenRead(RecordPath + "Objects.djs")))
            {
                try
                {
                    lock(Graphic.ImageHashTable)
                    Graphic.ImageHashTable = JsonConvert.DeserializeObject<SortedDictionary<String, String>>(reader.ReadToEnd(), new Newtonsoft.Json.JsonSerializerSettings
                    {
                        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                    });
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    return false;

                }
            }

            if (File.Exists(RecordPath + "Objects.sdjs"))
                using (var reader = new StreamReader(File.OpenRead(RecordPath + "Objects.sdjs")))
                {
                    try
                    {
                        lock (Graphic.ImageSubHashTable)
                            Graphic.ImageSubHashTable = JsonConvert.DeserializeObject<SortedDictionary<String, SortedDictionary<String, String>>>(reader.ReadToEnd(), new Newtonsoft.Json.JsonSerializerSettings
                            {
                                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                            });
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                        return false;

                    }
                }

            return true;
        }

        public SideBar()
        {
            InitializeComponent();
            if (!Directory.Exists(RecordPath))
                Directory.CreateDirectory(RecordPath);
            else
                load_Dictionaries();

#if (!DEBUG)
            Runbutton.Enabled =
            EditpictureBox.Enabled =
            SavepictureBox.Enabled =
            LoadpictureBox.Enabled =
            SettingpictureBox.Enabled =
            false;
#endif 

            FlushEventsTrigger += new FlushEventsHandler(FlushEvents);
            ChangesEventTrigger += new ChangesEventHandler(ObjectTracker);
            CaptureEngine();
            LLHook.EventTrigger += new LLHook.EventHandler(EventFunction);
            LLHook.InstallHook();
            Left = Screen.PrimaryScreen.Bounds.Width - Width;
            Top = (Screen.PrimaryScreen.Bounds.Height - Height) / 2;
        }

        private void SideBar_Load(object sender, EventArgs e)
        {
            AutoHide = Properties.Settings.Default.Autohide;
            Width = max_width;
            Left = Screen.PrimaryScreen.Bounds.Width - max_width;
            OnSelf = true;
        }

        private void SideBar_MouseEnter(object sender, EventArgs e)
        {
            OnSelf = true;
        }

        private void SideBar_MouseLeave(object sender, EventArgs e)
        {
            if (this.ClientRectangle.Contains(this.PointToClient(Control.MousePosition)))
                return;

            OnSelf = false;
        }


        private static void CaptureEngine(bool StopEngine=false)
        {
            CaptureEngineRunning = !StopEngine;
            if (StopEngine)
                return;

           Task render = Task.Run(() =>
            {
                while (CaptureEngineRunning)
                {
                    if (Capturing)
                        Track_Changes();
                    else
                        Application.DoEvents();
                }

                if (Capturing)
                {
                    FlushEventsTrigger(Previous_frame,DateTime.Now , true);
                    Capturing = false;
                }
            });

        }

        private void Recordbutton_Click(object sender, EventArgs e)
        {
            if (!Capturing)
            {
                Graphic.ImageHashTable.Clear();
                Graphic.ImageSubHashTable.Clear();
                foreach (String file in Directory.GetFiles(RecordPath, "*.*", SearchOption.TopDirectoryOnly))
                    File.Delete(file);
            }
            Capturing = !Capturing;
            Recordbutton.Text = Capturing ? "Stop" : "Record";
            if (!Capturing)
            {
                FlushEventsTrigger(Previous_frame,DateTime.Now, true);
            }
        }

        private void BarHandlertimer_Tick(object sender, EventArgs e)
        {
            if (Capturing)
            {
                if (BackColor.ToArgb() != current_bg.ToArgb())
                {
                    BackColor = Color.FromArgb(BackColor.R == current_bg.R ? BackColor.R : (BackColor.R > current_bg.R ? BackColor.R - change_speed_bg : BackColor.R + change_speed_bg),
                                               BackColor.G == current_bg.G ? BackColor.G : (BackColor.G > current_bg.G ? BackColor.G - change_speed_bg : BackColor.G + change_speed_bg),
                                               BackColor.B == current_bg.B ? BackColor.B : (BackColor.B > current_bg.B ? BackColor.B - change_speed_bg : BackColor.B + change_speed_bg));
                }
                else if (current_bg.ToArgb() == record_Dark_bg.ToArgb())
                    current_bg = record_bg;
                else if (current_bg.ToArgb() == record_bg.ToArgb())
                    current_bg = record_Dark_bg;
                else
                    current_bg = record_bg;

            }
            else if(BackColor.ToArgb()!= normal_bg.ToArgb())
            {
                BackColor = Color.FromArgb(BackColor.R == normal_bg.R ? BackColor.R : (BackColor.R > normal_bg.R ? BackColor.R - change_speed_bg : BackColor.R + change_speed_bg),
                                           BackColor.G == normal_bg.G ? BackColor.G : (BackColor.G > normal_bg.G ? BackColor.G - change_speed_bg : BackColor.G + change_speed_bg),
                                           BackColor.B == normal_bg.B ? BackColor.B : (BackColor.B > normal_bg.B ? BackColor.B - change_speed_bg : BackColor.B + change_speed_bg));
            }

            if (!OnSelf)
            {
                if (min_width < Width)
                    if (AutoHide)
                    {
                        Width -= 5;
                        Left = Screen.PrimaryScreen.Bounds.Width - Width;
                    }

                if (Opacity > min_opacity)
                    Opacity -= 0.05;
                else if (Opacity != min_opacity)
                    Opacity = min_opacity;

            }
            else
            {
                if (max_width > Width)
                {
                    Width += 5;
                    Left = Screen.PrimaryScreen.Bounds.Width - Width;
                }

                if (Opacity < 1)
                {
                    Opacity += 0.05;
                    Refresh();
                }
            }

        }

        static RnD gt = new RnD();

        private void Runbutton_Click(object sender, EventArgs e)
        {
            gt.Show();
            gt.start();
        }


        private void EditpictureBox_Click(object sender, EventArgs e)
        {
            previewBar.Show();
        }

        private void ExitpictureBox_Click(object sender, EventArgs e)
        {
            LLHook.UninstallHook();
            CaptureEngine(true);
            Properties.Settings.Default.Autohide = AutoHide;
            Properties.Settings.Default.Save();
            while(Capturing);
            Application.Exit();
        }

        private void PinPintureBox_Click(object sender, EventArgs e)
        {
            AutoHide = !AutoHide;
            if (AutoHide)
                PinPintureBox.BackgroundImage = Properties.Resources.Pin;
            else
                PinPintureBox.BackgroundImage = Properties.Resources.Hide;
        }

        public bool ExecuteEvent(in LLHook.Event event_, in List<EventArea> eventareas, in Mat image, in double wait=0, in int retry = 0)
        {
            switch (event_.HookType)
            {
                case LLHook.HookType.WH_MOUSE_LL:
                    LLHook.MouseEvent me = (event_ as LLHook.MouseEvent);
                    POINT point = me.msllhookstruct.pt;
                    if (Graphic.RectfromPoint(Previous_Position,new OpenCvSharp.Size(int.MaxValue, int.MaxValue), mouse_tol, mouse_tol).Contains(new OpenCvSharp.Point(point.x, point.y)))
                    {
                        Previous_Position = me.msllhookstruct.pt;
                        me.msllhookstruct.pt = Previous_Calc_Position;
                        LLHook.run_event(me);
                    }
                    else
                    {
                        //Trackback with changesareas to find the current object point on screen based on it's previous status
                        //Try to get the rect in current screen from changing area which happened previously 
                        List<Tuple<Rect, OpenCvSharp.Point>> found_objects = new List<Tuple<Rect, OpenCvSharp.Point>>();
                        List<EventArea> SearchArea = new List<EventArea>();

                        
                        if (eventareas.Count == 0) // try to extract the object from previous point in the previously stored image
                        {
                            List<Rect> change_rects = new List<Rect>();
                            Rect bound_rect = Graphic.RectfromPoint(me.msllhookstruct.pt, image.Size(), ViewBoxSize, ViewBoxSize);
                            change_rects = Graphic.DetectRegions(image, bound_rect, 255.0, 15, 0);
                            //find rects under the last mouse position
                            if (change_rects.Exists(item => item.Contains(new OpenCvSharp.Point(me.msllhookstruct.pt.x, me.msllhookstruct.pt.y))))
                            {
                                change_rects.RemoveAll(item => !item.Contains(new OpenCvSharp.Point(me.msllhookstruct.pt.x, me.msllhookstruct.pt.y)));
                                if (change_rects.Count > 1)
                                    Cv2.GroupRectangles(change_rects, 0, 0.2);
                            }
                            else
                            {
                                //find nearest object
                                List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>();
                                foreach (Rect rect in change_rects)
                                {
                                    Rect union = bound_rect & rect;
                                    if (union.Width * union.Height != 0)
                                    {
                                        points.Add(rect.TopLeft);
                                        points.Add(rect.BottomRight);
                                    }
                                }
                                change_rects.Clear();
                                Rect final = Cv2.BoundingRect(points);
                                if (final.Width * final.Height > 0)
                                    change_rects.Add(final);
                            }

                            EventArea searchArea = new EventArea(me.time, me.msllhookstruct.pt);
                            searchArea.shift = me.shift;
                            searchArea.capslock = me.capslock;
                            searchArea.AddRects(change_rects);
                            searchArea.AddObjects(image, image);
                            SearchArea.Add(searchArea);
                            
                        }
                        else //if there is area of changes happened before this event
                        {
                            SearchArea.AddRange(eventareas);

                            //move mouse on the places hovered as before
                            foreach (EventArea eventarea in SearchArea)
                            {
                                Track_Changes();
                                foreach (Tuple<Mat, Mat, Rectangle> RectObject in eventarea.AllRectObjects())
                                {
                                    List<Rect> found_rects = new List<Rect>();
                                    Action<Mat> CheckRects = delegate(Mat CurrentRect)
                                    {
                                        found_rects = Graphic.MatchTemplates(Previous_frame, CurrentRect, new Rect(0, 0, 0, 0));
                                        if (found_rects.Count() > 0)
                                        {
                                            foreach (Rect rect in found_rects)
                                            {
                                                //make sure if the objects are the same by comparing the percentage
                                                Mat FoundRect = Previous_frame.SubMat(rect);
                                                if (Graphic.ComparePercentage(CurrentRect, FoundRect) > Similarity_tolerance)
                                                {
                                                    //calculate relative click position from start of rect
                                                    found_objects.Add(new Tuple<Rect, OpenCvSharp.Point>(
                                                                                      rect, new OpenCvSharp.Point(eventarea.actual_point.x - RectObject.Item3.X, eventarea.actual_point.y - RectObject.Item3.Y)
                                                                      )
                                                  );
                                                }
                                            }
                                        }
                                    };

                                    CheckRects(RectObject.Item1.Clone());
                                    //if item1 failed try item2
                                    //(1st:before capture if failes => after capture)
                                    if (found_objects.Count == 0)
                                    {
                                        CheckRects(RectObject.Item2.Clone());
                                    }

                                    if (found_objects.Count() != 0)
                                    {
                                        Graphic.HighlightRects(found_objects.ConvertAll(pair => pair.Item1), Color.Red);
                                        Graphic.HighlightRects(found_objects.ConvertAll(pair => pair.Item1), Color.Red);
                                        Graphic.HighlightRects(found_objects.ConvertAll(pair => pair.Item1), Color.Red);
                                        POINT pt = new POINT((uint)(found_objects[0].Item1.X + found_objects[0].Item2.X),
                                                            (uint)(found_objects[0].Item1.Y + found_objects[0].Item2.Y));
                                        LLHook.MoveMouse(pt);
                                        found_objects.Clear();

                                    }

                                }

                            }

                            //from last change area to first change area
                            SearchArea.Reverse();
                        }

                        Stack<EventArea> event_history = new Stack<EventArea>();
                        bool found = false;
                        int timegap = (int)wait; 

                        Action FindObject = delegate
                        {
                            int HistoryBackcount = 0;
                            foreach (EventArea eventarea in SearchArea)
                            {
                                Action FindEventArea = delegate
                                {
                                    foreach (Tuple<Mat, Mat, Rectangle> RectObject in eventarea.AllRectObjects())
                                    {
                                        List<Rect> found_rects = new List<Rect>();

                                        Action<Mat> CheckRects = delegate(Mat CurrentRect)
                                        {
                                            found_rects = Graphic.MatchTemplates(Previous_frame, CurrentRect, new Rect(0, 0, 0, 0));
                                            if (found_rects.Count() > 0)
                                            {
                                                foreach (Rect rect in found_rects)
                                                {
                                                    //make sure if the objects are the same by comparing the percentage
                                                    Mat FoundRect = Previous_frame.SubMat(rect);
                                                    if (Graphic.ComparePercentage(CurrentRect, FoundRect) > Similarity_tolerance)
                                                    {
                                                        //calculate relative click position from start of rect
                                                        found_objects.Add(new Tuple<Rect, OpenCvSharp.Point>(
                                                                                          rect, new OpenCvSharp.Point(eventarea.actual_point.x - RectObject.Item3.X, eventarea.actual_point.y - RectObject.Item3.Y)
                                                                          )
                                                      );
                                                    }
                                                }

                                                if (found_objects.Count != 0)
                                                    found = true;
                                            }
                                        };

                                        CheckRects(RectObject.Item1.Clone());
                                        //if item1 failed try item2
                                        //(1st:before capture if failes => after capture)
                                        if (!found)
                                        {
                                            CheckRects(RectObject.Item2.Clone());
                                            if (found_rects.Count() == 0)
                                            {
                                                //DEBUG:
                                                //Cv2.ImShow("image1", RectObject.Item1);
                                                //Cv2.ImShow("image2", RectObject.Item2);
                                                //Application.DoEvents();
                                                //Thread.Sleep(2000);
                                            }
                                        }

                                        if (found)
                                            return;
                                    }
                                };

                                FindEventArea();

                                if (!found)
                                {
                                    if (HistoryBackcount == 0)
                                    {
                                        //Try as long as previous time gap to see if the object appears on screen
                                        //and only for one step backward from event time 
                                        Stopwatch timekeeper = new Stopwatch();
                                        timekeeper.Start();
                                        while (!found && timekeeper.ElapsedMilliseconds < timegap)
                                        {
                                            Track_Changes();
                                            if (CurrentFrameChanges != 0.0)
                                                FindEventArea();
                                        }
                                        timekeeper.Stop();
                                    }

                                    //this event could not be found. keep it for more research
                                    if (!found)
                                        event_history.Push(eventarea);
                                    else
                                        return;
                                }
                                else
                                {
                                    return;
                                }

                                HistoryBackcount++;
                            }
                        };

                        //look in current shot to find similar object
                        Track_Changes();
                        FindObject();

                        //if not any object detected Wait for screen changes within a timeout and try again to find object
                        if (!found)
                        {
                            Stopwatch timekeeper = new Stopwatch();
                            timekeeper.Start();
                            while (!found && timekeeper.ElapsedMilliseconds < wait)
                            {
                                Track_Changes();
                                if (CurrentFrameChanges != 0.0)
                                    FindObject();
                            }
                            timekeeper.Stop();
                        }

                        //TODO : track back the chain of changes from stacked changes
                        //TODO : remove the un-needed event areas which does not have effect on behaviour (files and entries in dictionaries)

                        /* Archive : find the object in current frame and try to detelct it in old stored picture
                            * this approch was wrong as the object can change the face by hovering so the tracking object history is created
                            * by looking at the changes in screen and storing the objects. in case if the object won't change face by hover then 
                            * the current object should be find again in old stored picture.
                            * 
                            * R&D: check if the object is changing the face by hovering or other actions
                            * 
                        Rect obj_rect = Graphic.ObjectfromPoint(image, point);
                        if (obj_rect.Width * obj_rect.Height == 0)
                            obj_rect = Graphic.RectfromPoint(me.msllhookstruct.pt, image.Size(), 25, 25);

                        Mat obj_pic = Graphic.Crop(image, obj_rect);
                        List<Rect> rects = Graphic.MatchTemplates(Current_frame, obj_pic, new Rect(0, 0, 0, 0));
                        */


                        //TEST:
                        //List<Rect> rects = found_objects.ConvertAll(pair => pair.Item1);
                        //if(rects.Count>0) Graphic.ZoomRect(rects[0]);

                        //Graphic.HighlightRects(found_objects.ConvertAll(pair => pair.Item1), Color.Yellow);
                        //Graphic.HighlightRects(found_objects.ConvertAll(pair => pair.Item1), Color.Yellow);
                        //Graphic.HighlightRects(found_objects.ConvertAll(pair => pair.Item1), Color.Yellow);
                        if (found_objects.Count == 0)
                        {
                            if (retry > 0)
                            {//TODO : retry and fail after timeout
                             //DEBUG: obj_pic.ImWrite("debug.png", Graphic.png_prms);
                             //DEBUG: Current_frame.ImWrite("screen.png", Graphic.png_prms);
                                return ExecuteEvent(event_, eventareas, image, wait * 2, retry - 1);
                            }
                            else if (retry == 0)
                            {
                                //TODO: retry other methods, Knnmatch 
                                return false;
                            }
                        }

                        if (found_objects.Count > 1) ;
                        //TODO :Multiple area found
                        // full -> look at previously near objects (if object model is created use that data)
                        // simple -> make search area bigger
                        // mediaum -> ask user which object to continue with

                        if (found_objects.Count >= 1)
                        {
                            Previous_Position = me.msllhookstruct.pt;

                            //move to relative previous object position 
                            me.msllhookstruct.pt.x = found_objects[0].Item1.X + found_objects[0].Item2.X;
                            me.msllhookstruct.pt.y = found_objects[0].Item1.Y + found_objects[0].Item2.Y;

                            if (!Previous_Calc_Position.Equals(me.msllhookstruct.pt))
                            {
                                Previous_Calc_Position = me.msllhookstruct.pt;
                                LLHook.MoveMouse(me.msllhookstruct.pt);
                            }
                            LLHook.run_event(me);
                        }
                    }
                    break;

                //TODO check for object avaliablity! and evecute keyboard event on it!
                case LLHook.HookType.WH_KEYBOARD_LL:
                    LLHook.run_event((event_ as LLHook.KeyboardEvent));
                    break;
            }
            return true;
        }

        public bool ExecuteEvents(in EventStruct jsonstruct, in List<EventArea> changesareas, in Mat image)
        {
            if (jsonstruct.events.Count == 0) return true;
            double timespan = (jsonstruct.events[jsonstruct.events.Count-1].time - jsonstruct.events[0].time).TotalMilliseconds;
            //Lock the mouse and keyboard till the event is finish executing
            //Debug: LLHook.LockHook(timespan);
            DateTime last_event_time = jsonstruct.events[0].time.Add(new TimeSpan(0, 0, -60));
            foreach (LLHook.Event event_ in jsonstruct.events)
            {
                List<EventArea> changes_before_event = changesareas.FindAll(item => item.time <= event_.time && item.time > last_event_time);
                if (!ExecuteEvent(event_, changes_before_event, image, (event_.time - last_event_time).TotalMilliseconds))
                {
                    LLHook.UnlockHook();
                    return false;
                }
                last_event_time = event_.time;
            }
            return true;
        }

        static private POINT Previous_Position = new POINT(0, 0);
        static private POINT Previous_Calc_Position = new POINT(0, 0);
        private void Playbutton_Click(object sender, EventArgs e)
        {
            bool failure = false;
            Previous_Position = new POINT(0, 0);
            Previous_Calc_Position = new POINT(0, 0);
            List<String> jsonfiles = new List<string>();
            jsonfiles.AddRange(Directory.GetFiles(RecordPath, "*.ejs", SearchOption.TopDirectoryOnly));
            foreach(String jsonfile in jsonfiles)
            {
                EventStruct events = new EventStruct();
                Mat image = new Mat();
                using (var reader = new StreamReader(File.OpenRead(jsonfile)))
                {
                    try
                    {
                        events = JsonConvert.DeserializeObject<EventStruct>(reader.ReadToEnd(), new Newtonsoft.Json.JsonSerializerSettings
                        {
                            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                        });
                        if (File.Exists(Path.ChangeExtension(jsonfile, ".png")))
                        {
                            image = Cv2.ImRead(Path.ChangeExtension(jsonfile, ".png"));

                            if(File.Exists(Path.ChangeExtension(jsonfile, ".cjs")))
                            {
                                List<EventArea> eventareas = new List<EventArea>();
                                using (var changesreader = new StreamReader(File.OpenRead(Path.ChangeExtension(jsonfile, ".cjs"))))
                                    eventareas = JsonConvert.DeserializeObject<List<EventArea>>(changesreader.ReadToEnd(), new Newtonsoft.Json.JsonSerializerSettings
                                    {
                                        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                                    });

                                if (!ExecuteEvents(events, eventareas, image))
                                {
                                    //TODO failed to continue
                                    failure = true;
                                    break;
                                }
                            }

                        }
                    }catch(Exception exception)
                    {
                        Console.WriteLine(exception.Message);

                    }
                }

            }
            MessageBox.Show("Run Finished " + (failure? "with failure" : "successfully"));
        }

        private void SavepictureBox_Click(object sender, EventArgs e)
        {

        }

        private void LoadpictureBox_Click(object sender, EventArgs e)
        {

        }

        private void SettingpictureBox_Click(object sender, EventArgs e)
        {

        }
    }
}
