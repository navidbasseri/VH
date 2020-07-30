using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VH
{

    public class @Object : IDisposable
    {
        public Rect rect;
        public List<Mat> States = new List<Mat>();
        public int CurrentState = 0;

        public @Object(in Rect rect)
        {
            this.rect = rect;
        }

        public @Object(in Rect rect, Mat mat)
        {
            this.rect = rect;
            this.States.Add(mat.Clone());
        }

        public bool AddState(in Mat mat, bool force = false)
        {
            if (force || !IsSame(mat))
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
                if (force || !IsSame(mat))
                {
                    result++;
                    States.Add(mat.Clone());
                }

            return result;
        }

        public bool IsSame(in Mat mat)
        {
            foreach (Mat m in States)
                if (Graphic.ComparePercentage(m, mat) == 100.0 && m.Size().Equals(mat.Size()))
                    return true;
            return false;
        }


        public bool Grab(in Mat image)
        {
            Mat img_rect = image.SubMat(this.rect);
            bool result= AddState(img_rect);
            if (!result) img_rect.Dispose();
            return result;
        }


        public bool IsAvaliableState
        {
            get
            {
                return States.Count > 0;
            }
        }

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

        public void Dispose()
        {
            foreach (Mat mat in States)
                mat.Dispose();
        }

    }

    public static class Model
    {
        public static List<@Object> objects = new List<@Object>();

        public static void Reset()
        {
            foreach (Object obj in objects)
                obj.Dispose();
            objects.Clear();

        }

        public static List<@Object> FindOverlappedObject(Rect rect)
        {
            return objects.FindAll(item => (item.rect & rect).Width * (item.rect & rect).Height != 0);
        }

        public static void Highlight()
        {
            Color color = Color.Red;
            foreach (Object obj in objects)
            {
                Graphic.HighlightRect(obj.rect, color);
                if (color == Color.Red)
                    color = Color.Blue;
                else
                    color = Color.Red;
            }
        }

        public static bool UniqueAdd(Object obj)
        {
            if (objects.Exists(item => (item.rect.Equals(obj.rect)))) {
                Object @object = objects.Find(item => (item.rect.Equals(obj.rect)));
                List<Mat> unknownStates = new List<Mat>();
                bool shared_state = false;
                foreach (Mat state in obj.States)
                    if (@object.IsSame(state))
                        shared_state = true;
                    else
                        unknownStates.Add(state);

                if (shared_state)
                {
                    @object.AddState(unknownStates, true);
                    return true;
                }
            }

            objects.Add(obj);
            return true;
        }

        public static Rect SubtractRect(Rect main_rect, Rect sub_rectrect)
        {
            Rect result = new Rect(main_rect.Location, main_rect.Size);
            //TODO
            return result;
        }

        public static void ExtractObjects(in Mat Current_frame, in Mat Previous_frame, in Mat Mask, POINT actual_point, DateTime TimeStamp)
        {
            List<Rect> hot_rects = new List<Rect>();
            hot_rects = Graphic.DetectRegions(Mask, new Rect(0, 0, 0, 0), 128.0, 1, 0);

            List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>();
//            Rect bounds = Graphic.RectfromPoint(actual_point, Current_frame.Size(), 200, 200);
            foreach (Rect r in hot_rects)
            {
//                Rect union = bounds & r;
//                if (union.X * union.Y != 0)
                {
                    points.Add(r.TopLeft);
                    points.Add(r.BottomRight);
                }
            }
            if (points.Count == 0)
                return;
            Rect rect = Cv2.BoundingRect(points);

//            foreach (Rect rect in hot_rects)
            {
                Mat rect_Current_frame = Current_frame.SubMat(rect);
                Mat rect_Previous_frame = Previous_frame.SubMat(rect);

                List<@Object> overlapping_objs = FindOverlappedObject(rect);

                List<@Object> container_objs = new List<Object>();

                foreach (@Object obj in overlapping_objs)
                {
                    if (obj.IsSame(rect_Current_frame)) break;
                    if (obj.IsSame(rect_Previous_frame)) break;

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
                                @Object fobj = new Object(newObject, rect_Current_frame.SubMat(newObject));
                                fobj.AddState(rect_Previous_frame.SubMat(newObject));
                                fobj.rect.X += rect.X;
                                fobj.rect.Y += rect.Y;
                                UniqueAdd(fobj);
                            }
                        }
                    }

                }

                if (container_objs.Count == 0)
                {
                    Object newobj = new Object(rect, rect_Current_frame);
                    newobj.AddState(rect_Previous_frame);
                    UniqueAdd(newobj);
                }

                rect_Current_frame.Dispose();
                rect_Previous_frame.Dispose();

            }
        }
    }
}
