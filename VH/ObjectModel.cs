using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace VH
{
    [Serializable()]
    public class @Object : IDisposable
    {   
        public List<String> StateNames = new List<String>();
        public List<Object> objects = new List<Object>();

        public int id=-1;
        public bool dynamic = false;

        [NonSerialized]
        public List<Mat> States = new List<Mat>();

        protected int CurrentState = -1;
        [NonSerialized] 
        public Rect rect;

        public Rectangle rectangle
        {
            get
            {
                return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
            }

            set
            {
                rect = new Rect(value.X, value.Y, value.Width, value.Height);
            }
        }

        [JsonConstructor]
        public @Object(int id, in Rect rect)
        {
            this.id = id;
            this.rect = rect;
            CurrentState = 0;
        }

        public @Object(int id, in Rect rect, Mat mat) : this(id, rect)
        {
            this.States.Add(mat.Clone());
        }

        public bool AddState(in Mat mat, bool force = false)
        {
            if (force || !ContainState(mat))
            {
                States.Add(mat.Clone());
                return true;
            }

            return false;
        }


        public int AddState(in List<Mat> mats, bool force = false)
        {
            int result = 0;
            foreach (Mat mat in mats)
                if (force || !ContainState(mat))
                {
                    result++;
                    States.Add(mat.Clone());
                }

            return result;
        }

        public bool ContainState(in Mat mat)
        {
            foreach (Mat m in States)
                if (Graphic.ComparePercentage(m, mat) == 100.0 && m.Size().Equals(mat.Size()))
                    return true;
            return false;
        }


        public bool GrabState(in Mat image)
        {
            Mat img_rect = image.SubMat(this.rect);
            bool result= AddState(img_rect);
            if (!result) img_rect.Dispose();
            return result;
        }

        [JsonIgnore]
        public bool IsStateAvaliable
        {
            get
            {
                return States.Count > 0;
            }
        }


        [JsonIgnore]
        public Mat NextState
        {
            get
            {
                Mat result = new Mat();
                if (CurrentState < States.Count)
                    result = States[CurrentState];

                CurrentState++;

                if (CurrentState >= States.Count)
                    CurrentState = 0;

                return result;
            }
        }


        [JsonIgnore]
        public int GetMaxId
        {
            get
            {
                if (objects.Count == 0)
                    return this.id;
                else
                    return Math.Max(objects.Max(item => item.id), this.id);
            }
        }


        [OnSerializing()]
        internal void OnSerializingMethod(StreamingContext context)
        {// create the image files based on hash image structure
            bool already_hashed = false;
            foreach (Mat state in States)
                StateNames.Add(ImageHashTable.ImageObject(state, Global.RecordPath, ref already_hashed));
        }

        [OnDeserialized()]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            foreach (String StateName in StateNames)
            {
                Mat State;
                if (ImageHashTable.LoadObject(Global.RecordPath + StateName + ".png", out State))
                    States.Add(State);
            }
        }



        public void Dispose()
        {
            foreach (Object obj in objects)
                obj.Dispose();
            objects.Clear();

            CurrentState = 0;
            foreach (Mat state in States)
                state.Dispose();
        }
    }

    public static class ObjectModel
    {
        public static List<@Object> objects = new List<@Object>();

        public static void Reset()
        {
            ImageHashTable.Reset();
            foreach (Object obj in objects)
                obj.Dispose();
            objects.Clear();
        }

        public static int GetNewId
        {
            get
            {
                if (objects.Count == 0)
                    return 0;
                else
                    return objects.Max(item => item.GetMaxId) +1;
            }
        }

        public static List<@Object> FindOverlappedObject(Rect rect)
        {
            return objects.FindAll(item => (item.rect & rect).Width * (item.rect & rect).Height != 0);
        }

        public static List<@Object> FindObjectByState(Mat state)
        {
            return objects.FindAll(item => item.ContainState(state));
        }

        public static int UniqueAdd(Object obj)
        {
            if (objects.Exists(item => (item.rect.Equals(obj.rect)))) {
                Object @object = objects.Find(item => (item.rect.Equals(obj.rect)));
                List<Mat> unknownStates = new List<Mat>();
                bool shared_state = false;
                foreach (Mat state in obj.States)
                    if (@object.ContainState(state))
                        shared_state = true;
                    else
                        unknownStates.Add(state);

                if (shared_state)
                {
                    @object.AddState(unknownStates, true);
                    return @object.id;
                }
            }

            objects.Add(obj);
            return obj.id;
        }


        public static void Save()
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonSerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            jsonSerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
            using (var writer = new StreamWriter(File.Create(Global.RecordPath + "Model.obj")))
                writer.Write(JsonConvert.SerializeObject(objects, jsonSerializerSettings));

            ImageHashTable.Save();
        }


        static public bool Load()
        {
            Reset();

            ImageHashTable.Load();

            if (File.Exists(Global.RecordPath + "Model.obj"))
                using (var reader = new StreamReader(File.OpenRead(Global.RecordPath + "Model.obj")))
                {
                    try
                    {
                        lock (objects)
                            objects = JsonConvert.DeserializeObject<List<@Object>>(reader.ReadToEnd(), new Newtonsoft.Json.JsonSerializerSettings
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

        public static Rect SubtractRect(Rect main_rect, Rect sub_rectrect)
        {
            Rect result = new Rect(main_rect.Location, main_rect.Size);
            //TODO
            return result;
        }

        public static List<int> ExtractObjects(in Mat Current_frame, in Mat Previous_frame, in Mat Mask, POINT actual_point, int ViewBoxSize = 0)
        {
            List<Rect> hot_rects = new List<Rect>();
            List<int> result = new List<int>();

            Rect bounds = Graphic.RectfromPoint(actual_point, Current_frame.Size(), ViewBoxSize, ViewBoxSize);

            if (Cv2.CountNonZero(Mask) == 0) //No changes detected between two frames
                return result;

            hot_rects = Graphic.DetectRegions(Mask, bounds, 128.0, 1, 0);
            
            if (hot_rects.Count == 0) // No reagion of interest detected
                return result;

            List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>();
           

            foreach (Rect r in hot_rects)
            {
                if (ViewBoxSize>0) 
                { 
                    Rect union = bounds & r;
                    if (union.X * union.Y != 0)
                    {
                        points.Add(r.TopLeft);
                        points.Add(r.BottomRight);
                    }
                }
                else
                {
                    points.Add(r.TopLeft);
                    points.Add(r.BottomRight);
                }
            }

            if (points.Count == 0) //No object in bound area detected
                return result;
            Rect rect = Cv2.BoundingRect(points);

//            foreach (Rect rect in hot_rects)
            {
                Mat rect_Current_frame = Current_frame.SubMat(rect);
                Mat rect_Previous_frame = Previous_frame.SubMat(rect);

                List<@Object> overlapping_objs = FindOverlappedObject(rect);

                List<@Object> container_objs = new List<Object>();

                foreach (@Object obj in overlapping_objs)
                {
                    if (obj.ContainState(rect_Current_frame)) break;
                    if (obj.ContainState(rect_Previous_frame)) break;

                    foreach (Mat state in obj.States)
                    {
                        List <Rect> found_rects = Graphic.MatchTemplates(rect_Current_frame, state, new Rect(0, 0, 0, 0));
                        if(found_rects.Count == 0)
                            found_rects = Graphic.MatchTemplates(rect_Previous_frame, state, new Rect(0, 0, 0, 0));

                        if (found_rects.Count > 0)
                        {
                            container_objs.Add(obj);
                            foreach (Rect frect in found_rects)
                            {
                                Rect newObject = SubtractRect(new Rect(0, 0, rect_Current_frame.Width, rect_Current_frame.Height), frect);
                                @Object fobj = new Object(GetNewId, newObject, rect_Current_frame.SubMat(newObject));
                                fobj.AddState(rect_Previous_frame.SubMat(newObject));
                                fobj.rect.X += rect.X;
                                fobj.rect.Y += rect.Y;
                                result.Add(UniqueAdd(fobj));
                            }
                        }
                    }

                }

                if (container_objs.Count == 0)
                {
                    Object newobj = new Object(GetNewId, rect, rect_Current_frame);
                    newobj.AddState(rect_Previous_frame);
                    result.Add(UniqueAdd(newobj));
                }

                rect_Current_frame.Dispose();
                rect_Previous_frame.Dispose();

            }

            return result;
        }
    }
}
