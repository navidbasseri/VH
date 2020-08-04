using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Drawing;
using OpenCvSharp;

namespace VH
{
    [Serializable()]
    public class EventArea : IDisposable
    {
        public DateTime time = DateTime.Now;
        public POINT actual_point = LLHook.GetCursorPos();
        public bool capslock = (((ushort)LLHook.GetKeyState(0x14)) & 0xffff) != 0;
        public bool shift = (((ushort)LLHook.GetKeyState(0x10)) & 0x1000) != 0;
        public List<int> ObjectIds = new List<int>();

        public List<Rectangle> Rects = new List<Rectangle>();
        public List<Tuple<String, String>> ObjectNames = new List<Tuple<String, String>>();

        protected List<Rect> cv_rects = new List<Rect>();
        protected List<Tuple<Mat, Mat>> Objects = new List<Tuple<Mat, Mat>>();

        public EventArea()
        {
        }

        public void Dispose()
        {

        }

        public EventArea(DateTime value, POINT actual_point)
        {
            time = value;
            this.actual_point = actual_point;
        }

        public bool AddObjects(List<int> ObjectIds)
        {
            if (ObjectIds.Count == 0)
                return false;
            else
                this.ObjectIds.AddRange(ObjectIds);

            return true;
        }


        public void AddRects(IEnumerable<Rect> collection)
        {
            cv_rects.AddRange(collection);
            foreach (Rect rect in cv_rects)
                Rects.Add(new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
        }

        public void AddObjects(in Mat Current_frame, in Mat Previous_frame)
        {
            foreach (Rect rect in cv_rects)
                Objects.Add(new Tuple<Mat, Mat>(Graphic.Crop(Previous_frame, rect), Graphic.Crop(Current_frame, rect)));
        }

        public List<Rect> FindRects(POINT pt)
        {
            return cv_rects.FindAll(item => item.Contains(pt.x, pt.y));
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
                if (rect.Contains(pt))
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

            foreach (Tuple<String, String> ObjectName in ObjectNames)
            {
                Mat Object1, Object2;
                if (ImageHashTable.LoadObject(Global.RecordPath + ObjectName.Item1 + ".png", out Object1) &&
                    ImageHashTable.LoadObject(Global.RecordPath + ObjectName.Item2 + ".png", out Object2))
                    Objects.Add(new Tuple<Mat, Mat>(Object1, Object2));
                else
                    Objects.Add(new Tuple<Mat, Mat>(new Mat(), new Mat()));
            }
        }

        [OnSerializing()]
        internal void OnSerializingMethod(StreamingContext context)
        {// create the image files based on hash image structure
            bool already_hashed = false;
            foreach (Tuple<Mat, Mat> @object in Objects)
            {
                ObjectNames.Add(new Tuple<String, String>(
                    ImageHashTable.ImageObject(@object.Item1, Global.RecordPath, ref already_hashed),
                    ImageHashTable.ImageObject(@object.Item2, Global.RecordPath, ref already_hashed))
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

}
