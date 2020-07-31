using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using OpenCvSharp;
using VH.Properties;
using OpenCvSharp.Extensions;

namespace VH
{
    public partial class RnD : Form
    {
        public static bool running = true;
        static Stopwatch stopwatch = new Stopwatch();
        static bool screenw_flag = false;
        static bool movew_flag = false;
        static bool objectw_flag = false;
        static Mat SamplePicture = new Mat();
        static double CompareResult = 0.0;
        static double SubCompareResult = 0.0;
        static bool CaptureModel = false;
        static bool ResetModel = false;


        public RnD()
        {
            InitializeComponent();
            MEventscheckedListBox.DataSource = Enum.GetValues(typeof(LLHook.MouseEvents));
        }

        private void Quitbutton_Click(object sender, EventArgs e)
        {
            running = false;
            ObjectModel.Reset();
            Hide();
            Settings.Default.XValue = XtextBox.Text;
            Settings.Default.YValue = YtextBox.Text;
            Settings.Default.Screen_cb = ScreencheckBox.Checked;
            Settings.Default.Move_cb = MovecheckBox.Checked;
            Settings.Default.Object_cb = ObjectcheckBox.Checked;
            Settings.Default.PointObject_cb = pointObjectcheckBox.Checked;
            Settings.Default.Save();
        }

        private void report_timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (stopwatch.ElapsedMilliseconds != 0)
                    fps_label.Text = "fps:" + 1000 / stopwatch.ElapsedMilliseconds + Environment.NewLine +
                                     "ms:" + stopwatch.ElapsedMilliseconds + Environment.NewLine +
                                     "CP:" + CompareResult + Environment.NewLine +
                                     "SCP:" + SubCompareResult + Environment.NewLine +
                                     "OC:" + ObjectModel.objects.Count();
            }
            catch (Exception exception)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + ":" + exception.Message + Environment.NewLine);
            }
        }

        public void start()
        {
            Settings.Default.Reload();
            ScreencheckBox.Checked = Settings.Default.Screen_cb;
            MovecheckBox.Checked = Settings.Default.Move_cb;
            ObjectcheckBox.Checked = Settings.Default.Object_cb;
            pointObjectcheckBox.Checked = Settings.Default.PointObject_cb;
            XtextBox.Text = Settings.Default.XValue;
            YtextBox.Text = Settings.Default.YValue;

            running = true;
            Rect client = Graphic.Rectangle2Rect(Graphic.desktop_rect);
           // client.X = client.Width / 2;
            client.Width /= 2;
            client.Height /= 2;

            ScreencheckBox_CheckedChanged(this,null);
            MovecheckBox_CheckedChanged(this, null);
            ObjectcheckBox_CheckedChanged(this, null);


            Task render = Task.Run(() =>
            {
                //Cv2.NamedWindow("image2", WindowMode.Normal);
                //Cv2.SetWindowProperty("image2", WindowProperty.Fullscreen, 1);
                //Cv2.ResizeWindow("image2", Screen.AllScreens[0].Bounds.Right, Screen.AllScreens[0].Bounds.Bottom);
                //Cv2.MoveWindow("image2", Screen.AllScreens[0].Bounds.Right, 0);

                Mat Perv_image = new Mat();
                while (running)
                {
                    stopwatch.Restart();
                    Mat image = new Mat();
                    Mat Fullscreenshot ;
                    if (Graphic.Screenshot(out Fullscreenshot, Screen.PrimaryScreen.Bounds, false))
                    {
                        image = Graphic.Crop(Fullscreenshot, client);
                        Mat movehistory;
                        CompareResult = Graphic.ComparePercentage(image, Perv_image,out movehistory);
                        if (ResetModel)
                        {
                            ObjectModel.Reset();
                            ImageHashTable.Reset();
                            ResetModel = false;
                        }

                        if (CaptureModel)
                            ObjectModel.ExtractObjects(image, Perv_image, movehistory, LLHook.GetCursorPos(), DateTime.Now);
                        

                        if (!SamplePicture.Empty())
                        {
                            List<Rect> mrects = Graphic.MatchTemplates(Fullscreenshot, SamplePicture, new Rect(0, 0, 0, 0));

                            if (mrects.Count > 0) 
                            {
                                Mat FoundRect = Fullscreenshot.SubMat(mrects[0]);
                                SubCompareResult= Graphic.ComparePercentage(SamplePicture, FoundRect);
                            }
                            Graphic.HighlightRects(mrects, Color.Yellow);
                        }

                        image.CopyTo(Perv_image);

                        if (pointObjectcheckBox.Checked)
                        {
                            Rect obj_rect = Graphic.ObjectfromPoint(Fullscreenshot, LLHook.GetCursorPos());
                            Graphic.HighlightRect(obj_rect, Color.Yellow);
                            if (SamplePicture.Empty() && obj_rect.Width * obj_rect.Height != 0 )
                            {
                                SamplePicture = Graphic.Crop(Fullscreenshot, obj_rect);
                                Bitmap img = SamplePicture.ToBitmap();
                                Picturepanel.BackgroundImage = img;
                            }
                        }

                        if (ScreencheckBox.Checked)
                        {
                            if (screenw_flag)
                                Cv2.ImShow("image", image);
                        }
                        else
                        {
                            Cv2.DestroyWindow("image");
                            screenw_flag = false;
                        }

                        //Graphic.Crop(ref image, new Rect(0, 0, 0, 0));
                        if (MovecheckBox.Checked)
                        {
                            if (movew_flag && !movehistory.Empty())
                                Cv2.ImShow("pimage", movehistory);
                        }
                        else if (movew_flag)
                        {

                            Cv2.DestroyWindow("pimage");
                            movew_flag = false;
                        }

                        //Graphic.Blur(ref image, new Rect(0, 0, 0, 0), 5);
                        if (ObjectcheckBox.Checked)
                        {
                            //Debug:
                            //if (!movehistory.Empty())
                            //{
                            //    movehistory.ImWrite("Record\\" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png", Graphic.png_prms);
                            //}
                            //List<Rect> Moverects = Graphic.DetectRegions(movehistory, new Rect(0, 0, 0, 0), 128.0, 1, 0);

                            /*  Changing Areas preview
                            POINT position = LLHook.GetCursorPos();
                            Rect bounds = Graphic.RectfromPoint(position, image.Size(), 200, 200);
                            //Cv2.Rectangle(image, bounds, Scalar.FromRgb(255, 0, 0), 1);

                            List<Rect> hot_rects = Graphic.DetectRegions(movehistory, client, 128.0, 1, 0);
                            List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>();
                            foreach (Rect rect in hot_rects)
                            {
                                Cv2.Rectangle(image, rect, Scalar.FromRgb(0, 255, 0), 1);
                                
                                Rect union = bounds & rect;
                                if (union.X* union.Y!=0)
                                {
                                    points.Add(rect.TopLeft);
                                    points.Add(rect.BottomRight);
                                }
                            }
                            Rect final = Cv2.BoundingRect(points);
                            Cv2.Rectangle(image, final, Scalar.FromRgb(0, 0, 255), 2);
                            */

                            /*All Objects Preview
                            List<Rect> rects = Graphic.DetectRegions(image, new Rect(0, 0, 0, 0), 128.0, 1, 0);
                            //List<Rect> rects = Graphic.DetectRegions(image, new Rect(0, 0, 0, 0), 255.0, 50, 5);
                            Cv2.GroupRectangles(rects, 0, 0.2);
                            foreach (Rect rect in rects)
                                Cv2.Rectangle(image, rect, Scalar.FromRgb(0, 255, 0),1);
                            */

                            // Create Object model
                            Cv2.Rectangle(image, new OpenCvSharp.Point(0, 0), new OpenCvSharp.Point(image.Width, image.Height), Scalar.FromRgb(0, 0, 0), -1);
                            foreach (Object obj in ObjectModel.objects)
                                if (obj.IsStateAvaliable)
                                    obj.NextState.CopyTo(image.SubMat(obj.rect));


                            if (objectw_flag)
                                Cv2.ImShow("object", image);
                        }
                        else if (objectw_flag)
                        {
                            Cv2.DestroyWindow("object");
                            objectw_flag = false;
                        }

                    }

                    image.Dispose();
                    stopwatch.Stop();
                }
                Cv2.DestroyAllWindows();
            });


        }

        private void ScreencheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ScreencheckBox.Checked)
            {
                Cv2.NamedWindow("image", WindowMode.Normal);
                Cv2.SetWindowProperty("image", WindowProperty.Fullscreen, 1);
                Cv2.ResizeWindow("image", Screen.PrimaryScreen.Bounds.Right / 2 - 1, Screen.PrimaryScreen.Bounds.Bottom / 2 - 1);
                Cv2.MoveWindow("image", 0, Screen.PrimaryScreen.Bounds.Bottom / 2 + 1);
                screenw_flag = true;
            }

        }

        private void MovecheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (MovecheckBox.Checked)
            {
                Cv2.NamedWindow("pimage", WindowMode.Normal);
                Cv2.SetWindowProperty("pimage", WindowProperty.Fullscreen, 1);
                Cv2.ResizeWindow("pimage", Screen.PrimaryScreen.Bounds.Right / 2 - 1, Screen.PrimaryScreen.Bounds.Bottom / 2 - 1);
                Cv2.MoveWindow("pimage", Screen.PrimaryScreen.Bounds.Right / 2 + 1, Screen.PrimaryScreen.Bounds.Bottom / 2 + 1);
                movew_flag = true;
            }
        }

        private void ObjectcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ObjectcheckBox.Checked)
            {
                Cv2.NamedWindow("object", WindowMode.Normal);
                Cv2.SetWindowProperty("object", WindowProperty.Fullscreen, 1);
                Cv2.ResizeWindow("object", Screen.PrimaryScreen.Bounds.Right / 2 - 1, Screen.PrimaryScreen.Bounds.Bottom / 2 - 1);
                Cv2.MoveWindow("object", Screen.PrimaryScreen.Bounds.Right / 2 + 1, 0);
                objectw_flag = true;
            }
        }
        private void Runbutton_Click(object sender, EventArgs e)
        {
            uint x = 0;
            uint y = 0;
            if (uint.TryParse(XtextBox.Text, out x) && uint.TryParse(YtextBox.Text, out y))
            {
                LLHook.MoveMouse(new POINT(x, y));

                for (int i =0;i< MEventscheckedListBox.CheckedItems.Count;i++)
                {
                    uint message =  (uint)(LLHook.MouseEvents)MEventscheckedListBox.CheckedItems[i];
                    LLHook.mouse_event(message, x, y, 0, 0);
                }
            }
        }

        private void RnD_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.M)
            {
                POINT mpoint = new POINT();
                if (LLHook.GetCursorPos(out mpoint))
                {
                    XtextBox.Text = mpoint.x.ToString();
                    YtextBox.Text = mpoint.y.ToString();
                }
            }

        }

        private void Picturebutton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog()== DialogResult.OK)
            {
                SamplePicture = Cv2.ImRead(ofd.FileName);
                Bitmap img = SamplePicture.ToBitmap();
                Picturepanel.BackgroundImage = img;
            }
        }

        private void Resetbutton_Click(object sender, EventArgs e)
        {
            SamplePicture = new Mat();
        }

        private void Savebutton_Click(object sender, EventArgs e)
        {
            if (!SamplePicture.Empty())
            {
                SamplePicture.ImWrite("capture.png", Global.png_prms);
            }

        }

        private void Modelbutton_Click(object sender, EventArgs e)
        {
            CaptureModel = !CaptureModel;
            if (CaptureModel)
                Modelbutton.Text = "Stop";
            else
                Modelbutton.Text = "Start";
        }

        private void ResetModelbutton_Click(object sender, EventArgs e)
        {
            ResetModel = true;
        }

        private void SaveObjectModelbutton_Click(object sender, EventArgs e)
        {
            Global.Reset();

            ObjectModel.Save();
            ImageHashTable.Save();
        }

        private void LoadObjectModelbutton_Click(object sender, EventArgs e)
        {
            ImageHashTable.Load();
            ObjectModel.Load();
        }
    }
}
