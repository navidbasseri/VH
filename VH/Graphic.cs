using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.XFeatures2D;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Net;
using OpenCvSharp.Extensions;

namespace VH
{

    [StructLayout(LayoutKind.Sequential)]
    public struct CURSORINFO
    {
        public Int32 cbSize;
        public Int32 flags;
        public IntPtr hCursor;
        public POINT ptScreenPos;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;

        public POINT(uint X, uint Y)
        {
            x = (int)X;
            y = (int)Y;
        }

        public static implicit operator System.Drawing.Point(POINT point)
        {
            return new System.Drawing.Point(point.x, point.y);
        }
    }
    public static class Graphic
    {

        public static Rectangle desktop_rect = new Rectangle(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
        public static Rectangle WorkingArea_rect = new Rectangle(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
        static Bitmap shot;
        static Graphics gfx;
        public static Mat fmask = new Mat();
        public static ImageEncodingParam[] png_prms = new ImageEncodingParam[] { new ImageEncodingParam(ImwriteFlags.PngCompression, 0) };
        static BackgroundSubtractorMOG2 mOG2 = BackgroundSubtractorMOG2.Create(1, 16, false);
        static MSER mser = MSER.Create(5, 50, 14400, 0.25, 0.20, 200, 1.010, 0.0030, 5);
        static double hessianThreshold = 100;
        static int nOctaves = 4;
        static int nOctaveLayers = 2;
        static bool extended = true;
        static bool upright = false;
        static SURF surf = SURF.Create(hessianThreshold, nOctaves, nOctaveLayers, extended, upright);
        static DescriptorMatcher matcher = DescriptorMatcher.Create(DescriptorMatcherMethod.BRUTEFORCE);
        public static SortedDictionary<String, String> ImageHashTable = new SortedDictionary<String, String>();
        public static SortedDictionary<String, SortedDictionary<String, String>> ImageSubHashTable = new SortedDictionary<String, SortedDictionary<String, String>>();


        [DllImport("user32.dll")]
        static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);
        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("User32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("gdi32", EntryPoint = "CreateCompatibleDC")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);
        [DllImport("gdi32", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("gdi32", EntryPoint = "BitBlt")]
        public static extern bool BitBlt(IntPtr hDestDC, int X, int Y, int nWidth, int nHeight, IntPtr hSrcDC, int SrcX, int SrcY, int Rop);
        [DllImport("gdi32", EntryPoint = "DeleteDC")]
        public static extern IntPtr DeleteDC(IntPtr hDC);
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        public enum TernaryRasterOperations
        {
            SRCCOPY = 0x00CC0020, // dest = source
            SRCPAINT = 0x00EE0086, // dest = source OR dest
            SRCAND = 0x008800C6, // dest = source AND dest
            SRCINVERT = 0x00660046, // dest = source XOR dest
            SRCERASE = 0x00440328, // dest = source AND (NOT dest)
            NOTSRCCOPY = 0x00330008, // dest = (NOT source)
            NOTSRCERASE = 0x001100A6, // dest = (NOT src) AND (NOT dest)
            MERGECOPY = 0x00C000CA, // dest = (source AND pattern)
            MERGEPAINT = 0x00BB0226, // dest = (NOT source) OR dest
            PATCOPY = 0x00F00021, // dest = pattern
            PATPAINT = 0x00FB0A09, // dest = DPSnoo
            PATINVERT = 0x005A0049, // dest = pattern XOR dest
            DSTINVERT = 0x00550009, // dest = (NOT dest)
            BLACKNESS = 0x00000042, // dest = BLACK
            WHITENESS = 0x00FF0062, // dest = WHITE
        };

        static Graphic()
        {
            foreach (Screen screen in Screen.AllScreens)
                desktop_rect = Rectangle.Union(desktop_rect, screen.Bounds);

            WorkingArea_rect = Screen.PrimaryScreen.WorkingArea;

            shot = new Bitmap(desktop_rect.Width, desktop_rect.Height, PixelFormat.Format24bppRgb);
            gfx = Graphics.FromImage(shot);
        }

        public static bool Screenshot(out Mat image, Rectangle rect, bool CaptureMouse = false)
        {
            try
            {
                gfx.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
                if (CaptureMouse)
                {
                    CURSORINFO pci;
                    if (LLHook.GetCursorPos(out pci))
                    {
                        DrawIcon(gfx.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                        gfx.ReleaseHdc();
                    }
                }

                image = OpenCvSharp.Extensions.BitmapConverter.ToMat(shot);
                if (!image.Size().Equals(rect.Size))
                    CropOrg(ref image, new Rect(rect.X, rect.Y, rect.Width, rect.Height));

                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + ":" + exception.Message + Environment.NewLine);
                image = new Mat();
                return false;
            }
        }


        public static bool BlurOrg(ref Mat image, Rect rect, int alpha)
        {
            try
            {
                Mat newimage;

                if (rect.Width != 0 && rect.Height != 0)
                    newimage = image.SubMat(rect);
                else
                    newimage = image;

                Cv2.GaussianBlur(newimage, newimage, new OpenCvSharp.Size(alpha * 2 + 1, alpha * 2 + 1), 0, 0, 0);
                if (newimage != image)
                    newimage.Dispose();
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + ":" + exception.Message + Environment.NewLine);
                return false;
            }
        }


        public enum RectSerach
        {
            MaxSize,
            MinSize,
            First,
            Last,
            Center,
            Nearest,
        }

        public static double SizeComparer(in OpenCvSharp.Size major, in OpenCvSharp.Size minor)
        {
            double majorarea = major.Height * major.Width;
            double minorarea = minor.Height * minor.Width;
            if (minorarea > 0)
                return majorarea / minorarea;
            else
                return majorarea;
        }

        public static double DistanceToPoint(in POINT point, in Rect rect)
        {
            POINT RectCenter = new POINT((uint)(rect.X + rect.Width / 2), (uint)(rect.Y + rect.Height / 2));
            return Math.Sqrt(Math.Pow((RectCenter.x - point.x), 2) + Math.Pow((RectCenter.y - point.y), 2));
        }

        public static Rect ObjectfromPoint(in Mat image, in POINT point, int distance = 150, RectSerach rectSerach = RectSerach.Center)
        {
            Rect result = new Rect(point.x, point.y, 0, 0);
            if (rectSerach == RectSerach.MinSize)
                result = RectfromPoint(point, image.Size(), distance, distance); ;

            Rect search_area = RectfromPoint(point, image.Size(), distance, distance);
            List<Rect> rects = Graphic.DetectRegions(image, search_area, 255.0, 20, 3);
            //rects.AddRange(rects);
            //Cv2.GroupRectangles(rects, 5, 0.2);
            if (rects.Count == 0)
                return new Rect(point.x, point.y, 0, 0);

            foreach (Rect rect in rects)
                if (rect.Contains(point.x, point.y) || rectSerach == RectSerach.Nearest)
                {
                    switch (rectSerach)
                    {
                        case RectSerach.First:
                            return rect;
                        case RectSerach.Last:
                            if (rect == rects.Last())
                                return rect;
                            break;
                        case RectSerach.MinSize:
                            if (SizeComparer(rect.Size, result.Size) < 1.0)
                                result = rect;
                            break;
                        case RectSerach.MaxSize:
                            if (SizeComparer(rect.Size, result.Size) > 1.0)
                                result = rect;
                            break;
                        case RectSerach.Center:
                        case RectSerach.Nearest:
                            if (result.Size.Equals(new OpenCvSharp.Size(0, 0)))
                                result = rect;
                            else if (DistanceToPoint(point, rect) < DistanceToPoint(point, result))
                                result = rect;
                            break;
                    }
                }
            return result;
        }


        public static Rect RectfromPoint(in POINT point, in OpenCvSharp.Size boundry, int width = 16, int height = 16)
        {
            int x = (point.x - width / 2);
            if (x < 0) x = 0;
            if (boundry.Width != int.MaxValue && x + width > boundry.Width) x = boundry.Width - width;
            int y = point.y - height / 2;
            if (y < 0) y = 0;
            if (boundry.Height != int.MaxValue && y + height > boundry.Height) x = boundry.Height - height;
            return new Rect(x, y, width, height);
        }


        public static bool CropOrg(ref Mat image, Rect rect)
        {
            try
            {
                if (rect.Width != 0 && rect.Height != 0)
                {
                    Mat newimage = image.SubMat(rect);
                    newimage.CopyTo(image);
                    newimage.Dispose();
                }
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + ":" + exception.Message + Environment.NewLine);
                return false;
            }
        }

        public static Mat Crop(in Mat image, Rect rect)
        {
            Mat result = new Mat();
            try
            {
                if (rect.Width != 0 && rect.Height != 0)
                    result = image.SubMat(rect);
            }
            catch (Exception exception)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + ":" + exception.Message + Environment.NewLine);
            }
            return result;
        }


        public static List<Rect> DetectRegions(in Mat image, Rect rect, double thresh_val_ = 255, int dev_val_ = 15, int blur_size_ = 2)
        {

            Mat newimage = new Mat();
            List<Rect> results = new List<Rect>();
            if (image.Empty())
                return results;
            try
            {
                //Debug: HighlightRect(rect,Color.White);
                if (rect.X < 0) rect.X = 0;
                if (rect.Y < 0) rect.Y = 0;
                if (rect.Width > image.Width) rect.Width = image.Width;
                if (rect.Height > image.Height) rect.Height = image.Height;
                if (rect.X + rect.Width > image.Width) rect.Width = image.Width - rect.X;
                if (rect.Y + rect.Height > image.Height) rect.Height = image.Height - rect.Y;

                if (rect.Width != 0 && rect.Height != 0)
                    newimage = image.SubMat(rect);
                else
                    image.CopyTo(newimage);

                //MessageBox.Show(newimage.Type().ToString());
                OpenCvSharp.Point[][] msers;
                Rect[] bboxes;
                mser.DetectRegions(newimage, out msers, out bboxes);
                if (bboxes.Count() > 0)
                    if (rect.X > 0 || rect.Y > 0)
                        foreach (Rect r in bboxes)
                            results.Add(new Rect(r.Location + rect.Location, r.Size));
                    else
                        results.AddRange(bboxes);

                if (newimage.Type() != MatType.CV_8UC1)
                    Cv2.CvtColor(newimage, newimage, ColorConversionCodes.BGRA2GRAY);

                if (blur_size_ > 0)
                    BlurOrg(ref newimage, new Rect(0, 0, 0, 0), blur_size_);

                mser.DetectRegions(newimage, out msers, out bboxes);
                if (bboxes.Count() > 0)
                    if (rect.X > 0 || rect.Y > 0)
                        foreach (Rect r in bboxes)
                            results.Add(new Rect(r.Location + rect.Location, r.Size));
                    else
                        results.AddRange(bboxes);


                if (results.Count == 0)
                {
                    double divider_ = thresh_val_;

                    for (int resolution_counter = 0; resolution_counter < dev_val_; resolution_counter++)
                    {

                        Mat bwimage = new Mat();
                        Cv2.Threshold(newimage, bwimage, divider_, 255.0, ThresholdTypes.Binary);
                        mser.DetectRegions(bwimage, out msers, out bboxes);

                        if (bboxes.Count() > 0)
                        {
                            if (rect.X > 0 || rect.Y > 0)
                                foreach (Rect r in bboxes)
                                    results.Add(new Rect(r.Location + rect.Location, r.Size));
                            else
                                results.AddRange(bboxes);
                        }

                        divider_ -= (thresh_val_ / dev_val_);
                    }
                }

                results.AddRange(results);

                Cv2.GroupRectangles(results, 1, 0.2);

                newimage.Dispose();
            }
            catch (Exception exception)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + ":" + exception.Message + Environment.NewLine);
            }
            return results;
        }

        public static Mat History(in Mat image)
        {
            mOG2.Apply(image, fmask);
            return fmask;
        }


        //
        // Summary:
        //     Compare two images together and return the amount of similarity.
        //     the size considered mandatory not to be the same
        //
        // Parameters:
        //   ImageA:
        //   ImageB:
        //     the source images to be compared.
        //
        // Returns:
        //     double value between 0.0 to 100.0 for similarity. 100 equals totally the same
        //     in case of exceptio the 0.0 will be returns
        public static double ComparePercentage(in Mat imageA, in Mat imageB)
        {
            try
            {
                Mat mask = new Mat();
                //if (imageA.Size() != imageB.Size()) return 0.0;
                //Cv2.Compare(imageA, imageB, mask, CmpTypes.NE);
                //Cv2.CvtColor(mask, mask, ColorConversionCodes.BGR2GRAY);
                lock (mOG2)
                {
                    mOG2.Apply(imageA, mask);
                    mOG2.Apply(imageB, mask);
                }
                double result = (((double)(mask.Total() - Cv2.CountNonZero(mask)) / (double)(mask.Total())) * 100);
                mask.Dispose();
                if (double.IsNaN(result))
                    return 0.0;
                return result;
            }
            catch (Exception exception)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + ":" + exception.Message + Environment.NewLine);
            }
            return 0.0;
        }
        public static double ComparePercentage(in Mat imageA, in Mat imageB, out Mat mask)
        {
            mask = new Mat();
            {
                try
                {
                    //if (imageA.Size() != imageB.Size()) return 0.0;
                    //Cv2.Compare(imageA, imageB, mask, CmpTypes.NE);
                    //Cv2.CvtColor(mask, mask, ColorConversionCodes.BGR2GRAY);
                    lock (mOG2)
                    {
                        mOG2.Apply(imageA, mask);
                        mOG2.Apply(imageB, mask);
                    }
                    double result = (((double)(mask.Total() - Cv2.CountNonZero(mask)) / (double)(mask.Total())) * 100);
                    if (double.IsNaN(result))
                        return 0.0;
                    return result;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + ":" + exception.Message + Environment.NewLine);
                }
            }
            return 0.0;
        }

        public static void HighlightRect(in Rect rect, in Color cl)
        {
            if (rect.Width * rect.Height == 0) return;
            IntPtr desktop = Graphic.GetDC(IntPtr.Zero);
            using (Graphics g = Graphics.FromHdc(desktop))
            {
                Bitmap bmp = new Bitmap(rect.Width + 4, rect.Height + 4);
                using (Graphics bmpGrf = Graphics.FromImage(bmp))
                {
                    IntPtr bmphdc = bmpGrf.GetHdc();
                    Graphic.BitBlt(bmphdc, 0, 0, bmp.Width, bmp.Height, desktop, rect.X - 2 < 0 ? 0 : rect.X - 2, rect.Y - 2 < 0 ? 0 : rect.Y - 2, Graphic.TernaryRasterOperations.SRCCOPY);
                    g.DrawRectangle(new Pen(cl, 2), rect.Left, rect.Top, rect.Width, rect.Height);
                    Thread.Sleep(100);
                    Graphic.BitBlt(desktop, rect.X - 2 < 0 ? 0 : rect.X - 2, rect.Y - 2 < 0 ? 0 : rect.Y - 2, bmp.Width, bmp.Height, bmphdc, 0, 0, Graphic.TernaryRasterOperations.SRCCOPY);
                    Graphic.ReleaseDC(IntPtr.Zero, bmphdc);
                }
            }
            Graphic.ReleaseDC(IntPtr.Zero, desktop);
        }

        public static void HighlightRects(in List<Rect> rects, in Color cl)
        {
            if (rects.Count == 0) return;
            List<Tuple<Bitmap, Rect, IntPtr>> infos = new List<Tuple<Bitmap, Rect, IntPtr>>();
            IntPtr desktop = Graphic.GetDC(IntPtr.Zero);
            using (Graphics g = Graphics.FromHdc(desktop))
            {
                foreach (Rect rect in rects)
                {
                    if (rect.Width * rect.Height == 0) continue;
                    Bitmap bmp = new Bitmap(rect.Width + 4, rect.Height + 4);
                    using (Graphics bmpGrf = Graphics.FromImage(bmp))
                    {
                        IntPtr bmphdc = bmpGrf.GetHdc();
                        Graphic.BitBlt(bmphdc, 0, 0, bmp.Width, bmp.Height, desktop, rect.X - 2 < 0 ? 0 : rect.X - 2, rect.Y - 2 < 0 ? 0 : rect.Y - 2, Graphic.TernaryRasterOperations.SRCCOPY);
                        infos.Add(new Tuple<Bitmap, Rect, IntPtr>(bmp, rect, bmphdc));
                        g.DrawRectangle(new Pen(cl, 2), rect.Left, rect.Top, rect.Width, rect.Height);
                    }
                }

                Thread.Sleep(100);
                infos.Reverse();
                foreach (Tuple<Bitmap, Rect, IntPtr> info in infos)
                {
                    Bitmap bmp = info.Item1;
                    Rect rect = info.Item2;
                    IntPtr bmphdc = info.Item3;
                    Graphic.BitBlt(desktop, rect.X - 2 < 0 ? 0 : rect.X - 2, rect.Y - 2 < 0 ? 0 : rect.Y - 2, bmp.Width, bmp.Height, bmphdc, 0, 0, Graphic.TernaryRasterOperations.SRCCOPY);
                    Graphic.ReleaseDC(IntPtr.Zero, bmphdc);
                }

            }
            Graphic.ReleaseDC(IntPtr.Zero, desktop);
        }

        public static List<Rect> MatchTemplates(in Mat image, in Mat sub_image, Rect rect)
        {
            List<Rect> results = new List<Rect>();
            // results.AddRange( MatchTemplate(image, sub_image, rect, TemplateMatchModes.CCoeff));
            results.AddRange(MatchTemplate(image, sub_image, rect, TemplateMatchModes.CCoeffNormed));
            //HighlightRects(results, Color.Green);
            //results.Clear();

            //  results.AddRange(MatchTemplate(image, sub_image, rect, TemplateMatchModes.CCorr));
            results.AddRange(MatchTemplate(image, sub_image, rect, TemplateMatchModes.CCorrNormed));
            //HighlightRects(results, Color.Blue);
            //results.Clear();
            Cv2.GroupRectangles(results, 1, 0.2);
            //   results.AddRange(MatchTemplate(image, sub_image, rect, TemplateMatchModes.SqDiff));
            results.AddRange(MatchTemplate(image, sub_image, rect, TemplateMatchModes.SqDiffNormed));
            //HighlightRects(results, Color.Red);
            //Graphic.HighlightRects(results, Color.Yellow);
            Cv2.GroupRectangles(results, 1, 0.2);
            //Application.DoEvents();
            return results;
        }


        public static List<Rect> MatchTemplate(in Mat image, in Mat sub_image, Rect rect, in TemplateMatchModes match_method = TemplateMatchModes.SqDiffNormed)
        {
            List<Rect> results = new List<Rect>();

            Mat main = new Mat();
            Mat template = new Mat();

            if (rect.X < 0) rect.X = 0;
            if (rect.Y < 0) rect.Y = 0;
            if (rect.Width > image.Width) rect.Width = image.Width;
            if (rect.Height > image.Height) rect.Height = image.Height;
            if (rect.X + rect.Width > image.Width) rect.Width = image.Width - rect.X;
            if (rect.Y + rect.Height > image.Height) rect.Height = image.Height - rect.Y;

            if (rect.Width != 0 && rect.Height != 0)
                main = image.SubMat(rect);
            else
                image.CopyTo(main);

            if (main.Cols - sub_image.Cols + 1 <= 0 ||
                main.Rows - sub_image.Rows + 1 <= 0)
                return results;

            sub_image.CopyTo(template);

            try
            {
                //               Cv2.CvtColor(main, main, ColorConversionCodes.BGR2GRAY);
                Cv2.CvtColor(template, template, ColorConversionCodes.BGRA2BGR);
                //               Cv2.CvtColor(template, template, ColorConversionCodes.BGR2GRAY);

                Mat result = new Mat();

                Cv2.MatchTemplate(main, template, result, match_method);
                Cv2.Normalize(result, result, 0, 1, NormTypes.MinMax, -1);

                int counter = 0;
                while (true)
                {
                    double minVal; double maxVal;
                    OpenCvSharp.Point minPos;
                    OpenCvSharp.Point maxPos;
                    Cv2.MinMaxLoc(result, out minVal, out maxVal, out minPos, out maxPos);
                    OpenCvSharp.Point matchLoc;

                    if ((match_method == TemplateMatchModes.CCoeff || match_method == TemplateMatchModes.CCoeffNormed) ||
                        (match_method == TemplateMatchModes.CCorr || match_method == TemplateMatchModes.CCorrNormed))
                    {
                        matchLoc = maxPos;
                        if (maxVal < 1)
                            break;
                        Cv2.Rectangle(result, new OpenCvSharp.Point(matchLoc.X - template.Cols, matchLoc.Y - template.Rows), new OpenCvSharp.Point(matchLoc.X + template.Cols, matchLoc.Y + template.Rows), new Scalar(0), -1);

                        //DEBUG:
                        //HighlightRect(new Rect(matchLoc, template.Size()), Color.White);
                        //Cv2.Rectangle(result, new OpenCvSharp.Point(matchLoc.X - template.Cols, matchLoc.Y - template.Rows), new OpenCvSharp.Point(matchLoc.X + template.Cols, matchLoc.Y + template.Rows), Scalar.FromRgb(0, 0, 0));
                        //Cv2.ImShow("image2", result);
                        //Application.DoEvents();

                    }
                    else if (match_method == TemplateMatchModes.SqDiff || match_method == TemplateMatchModes.SqDiffNormed)
                    {
                        matchLoc = minPos;
                        if (minVal > 0)
                            break;

                        //DEBUG:
                        //HighlightRect(new Rect(matchLoc, template.Size()), Color.White);
                        //Cv2.Rectangle(result, new OpenCvSharp.Point(matchLoc.X - template.Cols, matchLoc.Y - template.Rows), new OpenCvSharp.Point(matchLoc.X + template.Cols, matchLoc.Y + template.Rows), Scalar.FromRgb(0, 0, 0));
                        //Cv2.ImShow("image2", result);
                        //Application.DoEvents();

                        Cv2.Rectangle(result, new OpenCvSharp.Point(matchLoc.X - template.Cols, matchLoc.Y - template.Rows), new OpenCvSharp.Point(matchLoc.X + template.Cols + 5, matchLoc.Y + template.Rows + 5), new Scalar(2.0), -1);
                    }
                    else
                        break;

                    if (counter++ > 20)
                        break;

                    results.Add(new Rect(new OpenCvSharp.Point(matchLoc.X + rect.X, matchLoc.Y + rect.Y), template.Size()));

                }

                template.Dispose();
                main.Dispose();
                result.Dispose();
                results.AddRange(results);
                Cv2.GroupRectangles(results, 1, 0.2);
            }
            catch (Exception exception)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + ":" + exception.Message + Environment.NewLine);
            }

            return results;
        }


        public struct DescriptorMatcherMethod
        {
            public static string FLANNBASED { get { return "FlannBased"; } }
            public static string BRUTEFORCE { get { return "BruteForce"; } }
            public static string BRUTEFORCE_L1 { get { return "BruteForce-L1"; } }
            public static string BRUTEFORCE_HAMMING { get { return "BruteForce-Hamming"; } }
            public static string BRUTEFORCE_HAMMINGLUT { get { return "BruteForce-Hamming(2)"; } }
            public static string BRUTEFORCE_SL2 { get { return "BruteForce-SL2"; } }
        }
        public static List<Rect> KnnBestMatch(in Mat image, in Mat subimage, Rect rect)
        {
            int countdown = 10;
            List<Rect> results = new List<Rect>();
            List<Tuple<Rect, float>> FetchResult = KnnMatch(image, subimage, rect);
            float last_value = 0f;
            foreach (Tuple<Rect, float> item in FetchResult.OrderBy(item => item.Item2).ToList())
            {
                if (last_value == 0f)
                    last_value = item.Item2;
                else if (last_value != item.Item2)
                {
                    countdown--;
                    last_value = item.Item2;
                    if (countdown == 0)
                        break;
                }

                results.Add(item.Item1);
            }

            Cv2.GroupRectangles(results, 1, 0.5);

            return results;
        }

        public static List<Tuple<Rect, float>> KnnMatch(in Mat image, in Mat subimage, Rect rect)
        {
            List<Tuple<Rect, float>> results = new List<Tuple<Rect, float>>();

            if (rect.X < 0) rect.X = 0;
            if (rect.Y < 0) rect.Y = 0;
            if (rect.Width > image.Width) rect.Width = image.Width;
            if (rect.Height > image.Height) rect.Height = image.Height;
            if (rect.X + rect.Width > image.Width) rect.Width = image.Width - rect.X;
            if (rect.Y + rect.Height > image.Height) rect.Height = image.Height - rect.Y;

            Mat uModelImage = new Mat();

            if (rect.Width != 0 && rect.Height != 0)
                uModelImage = image.SubMat(rect);
            else
                image.CopyTo(uModelImage);

            Mat uObservedImage = new Mat();
            subimage.CopyTo(uObservedImage);
            int scalefactor = 4;
            int mainscalefactor = 2;
            //Cv2.CvtColor(uObservedImage, uObservedImage, ColorConversionCodes.BGR2GRAY);
            Cv2.Resize(uObservedImage, uObservedImage, new OpenCvSharp.Size(uObservedImage.Cols * scalefactor, uObservedImage.Rows * scalefactor));

            try
            {
                //Cv2.CvtColor(uModelImage, uModelImage, ColorConversionCodes.BGR2GRAY);
                Cv2.Resize(uModelImage, uModelImage, new OpenCvSharp.Size(uModelImage.Cols * mainscalefactor, uModelImage.Rows * mainscalefactor));


                KeyPoint[] modelKeyPoints;
                KeyPoint[] observedKeyPoints;
                Mat modelDescriptors = new Mat();
                Mat observedDescriptors = new Mat();
                surf.DetectAndCompute(uObservedImage, null, out observedKeyPoints, observedDescriptors);

                if (observedKeyPoints.Count() != 0)
                {
                    surf.DetectAndCompute(uModelImage, null, out modelKeyPoints, modelDescriptors);

                    DMatch[][] knn_matches = matcher.KnnMatch(modelDescriptors, observedDescriptors, 4);
                    const float tolerance = 0.9f;
                    foreach (DMatch[] matchitem in knn_matches)
                    {
                        if (matchitem[0].Distance / matchitem[1].Distance <= tolerance) {
                            OpenCvSharp.Point2f Source = modelKeyPoints[matchitem[0].QueryIdx].Pt;
                            OpenCvSharp.Point2f Dest = observedKeyPoints[matchitem[0].TrainIdx].Pt;
                            Rect rectres = new Rect(new OpenCvSharp.Point((int)(Source.X * (scalefactor / mainscalefactor) - Dest.X) / scalefactor + rect.X, (int)(Source.Y * (scalefactor / mainscalefactor) - Dest.Y) / scalefactor + rect.Y), subimage.Size());
                            results.Add(new Tuple<Rect, float>(rectres, matchitem[0].Distance / matchitem[1].Distance));
                        }
                    }
                }
                observedDescriptors.Dispose();
                modelDescriptors.Dispose();
                uModelImage.Dispose();
            }
            catch (Exception exception)
            {
                Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name + ":" + exception.Message + Environment.NewLine);
            }

            return results;
        }

        public static Rect Rectangle2Rect(in Rectangle rect)
        {
            return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static String HashImage(in Mat image, in int size = 8)
        {
            Mat img = new Mat();
            Cv2.Resize(image, img, new OpenCvSharp.Size(size, size));
            Cv2.CvtColor(img, img, ColorConversionCodes.BGRA2GRAY);
            img.ConvertTo(img, MatType.CV_8U);
            int length = (int)(img.Total() * img.Channels());
            byte[] bytes = new byte[length];
            img.GetArray(out bytes);
            //Debug: Cv2.ImEncode(".png", img, out bytes, png_prms);
            return System.Convert.ToBase64String(bytes);
        }


        public static String ImageSubObject(in Mat image, string RecordPath, ref SortedDictionary<String, String> SubDic, int hashsize = 16)
        {
            string Hash = HashImage(image, hashsize);
            string object_name = "";

            string value = "";
            if (SubDic.Count == 0) // sub dictionary cannot be empty!
            {
                return "";
            } else
            {
                //get the first value as key for creating object names
                value = SubDic.Values.First();
            }

            if (value == "SubHash")
            {// hash has more sub hashs. create sub hash or look if the sub hash exists
                SortedDictionary<String, String> Sub_Dic;
                if (ImageSubHashTable.TryGetValue(Hash, out Sub_Dic))
                    object_name = ImageSubObject(image, RecordPath, ref Sub_Dic, hashsize + 8);
            }
            else if (SubDic.ContainsKey(Hash))
            {//Sub hash exists already => create deeper level of hash
                SortedDictionary<String, String> Sub_Dic = new SortedDictionary<String, String>();
                Mat similar = Cv2.ImRead(RecordPath + value + ".png");
                string Similar_SubHash = HashImage(similar, hashsize + 8);
                //put similar object name and hash in sub dictionary
                Sub_Dic.Add(Similar_SubHash, value);
                object_name = ImageSubObject(image, RecordPath, ref Sub_Dic);

                //add new entry to sub dictionary
                ImageSubHashTable.Add(Hash, Sub_Dic);

                //mark the value in top dictionary for having sub hash
                SubDic[Hash] = "SubHash";
            }
            else
            {
                // create the image object file
                object_name = value + "-" + SubDic.Count();
                image.ImWrite(RecordPath + object_name + ".png", png_prms);
                // put the image object name and hash in dictionary
                SubDic.Add(Hash, value + "-" + SubDic.Count());
                //put back the sub dictionary
                //ImageSubHashTable[Hash] = SubDic;
            }

            return object_name;
        }

        //recursive hash builder
        public static String ImageObject(in Mat image, string RecordPath, ref bool already_hashed)
        {
            string Hash = HashImage(image);
            string object_name = "";
            if (!ImageHashTable.ContainsKey(Hash))
            {
                object_name = "Object" + ImageHashTable.Count();
                image.ImWrite(RecordPath + object_name + ".png", png_prms);
                ImageHashTable.Add(Hash, object_name);
            } else
            {
                string value;
                ImageHashTable.TryGetValue(Hash, out value);
                if (value == "SubHash")
                {// hash has more sub hashs. create sub hash or look if the sub hash exists
                    SortedDictionary<String, String> SubDic;
                    if (ImageSubHashTable.TryGetValue(Hash, out SubDic))
                        object_name = ImageSubObject(image, RecordPath, ref SubDic);
                }
                else
                {
                    //check for original image to compare. if not equal create sub hash. 
                    //move the similar one to sub hash and rename the similar file
                    Mat similar = Cv2.ImRead(RecordPath + value + ".png");
                    if (Graphic.ComparePercentage(similar, image) != 100.0)
                    {// images are different but the hash was the same. create sub hash with bigger hash size

                        SortedDictionary<String, String> SubDic = new SortedDictionary<String, String>();
                        string Similar_SubHash = HashImage(similar, 16);
                        //put similar object name and hash in sub dictionary
                        SubDic.Add(Similar_SubHash, value);
                        object_name = ImageSubObject(image, RecordPath, ref SubDic);

                        //add new entry to sub dictionary
                        ImageSubHashTable.Add(Hash, SubDic);

                        //mark the value in top dictionary for having sub hash
                        ImageHashTable[Hash] = "SubHash";
                    }
                    else
                    { // Hash already exist
                        already_hashed = true;
                        object_name = value;
                    }

                    similar.Dispose();
                }


            }
            return object_name;
        }


        static public List<String> HashObjectName(string hashstring)
        {
            String Value;
            List<String> result = new List<String>();
            if (ImageHashTable.TryGetValue(hashstring, out Value))
            {
                if (Value.Equals("SubHash"))
                {
                    SortedDictionary<String, String> SubDic;
                    if (ImageSubHashTable.TryGetValue(hashstring, out SubDic))
                        result.AddRange(SubDic.Values);
                }
                else
                    result.Add(Value);
            }

            return result;
        }

        static public bool LoadObject(String name, out Mat @object)
        {
            if (File.Exists(name))
            {
                @object = Cv2.ImRead(name);
                return true;
            }
            else
            {
                @object = new Mat();
                return false;
            }

        }


        public class MaskScreen : IDisposable
        {
            int steps, counter;
            Rect dest;
            private System.Windows.Forms.Timer Runtimer = new System.Windows.Forms.Timer();
            private MaskForm TopForm = new MaskForm(), LeftForm = new MaskForm(), RightForm = new MaskForm(), BottomForm = new MaskForm();

            public void Dispose()
            {
                while(Runtimer.Enabled)
                    Application.DoEvents();

                if (TopForm != null) TopForm.Dispose();
                if (BottomForm != null) BottomForm.Dispose();
                if (LeftForm != null) LeftForm.Dispose();
                if (RightForm != null) RightForm.Dispose();
            }

            public MaskScreen(Rect dest, int speed = 50, int steps = 20, bool Autostart = true)
            {
                counter = this.steps = steps;
                this.dest = dest;
                Runtimer.Interval = speed;
                //force the forms to load
                IntPtr dummy = TopForm.Handle;
                dummy = LeftForm.Handle;
                dummy = RightForm.Handle;
                dummy = BottomForm.Handle;
                InitSizes(1);
                Runtimer.Tick += new System.EventHandler(this.Runtimer_Tick);
                if (Autostart) Start();
            }

            private void Start()
            {
                if (TopForm != null) TopForm.Show();
                if (BottomForm != null) BottomForm.Show();
                if (LeftForm != null) LeftForm.Show();
                if (RightForm != null) RightForm.Show();
                Runtimer.Enabled = true;
            }


            private void InitSizes(int initSize=1)
            {
                if (TopForm != null)
                    TopForm.SetBounds(desktop_rect.X, desktop_rect.Y, desktop_rect.Width, initSize);

                if (LeftForm != null)
                    LeftForm.SetBounds(desktop_rect.X, desktop_rect.Y, initSize, desktop_rect.Height);

                if (BottomForm != null)
                    BottomForm.SetBounds(desktop_rect.X , desktop_rect.Y + desktop_rect.Height - initSize, desktop_rect.Width, initSize);

                if (RightForm != null)
                    RightForm.SetBounds(desktop_rect.X + desktop_rect.Width - initSize, desktop_rect.Y, initSize, desktop_rect.Height);
            }

            protected void SetSizes(int counter)
            {
                if (TopForm != null) 
                {
                    if (counter == 0)
                        TopForm.Height = dest.Top - TopForm.Top;
                    else
                        TopForm.Height = dest.Top - (int)(dest.Top / Math.Pow(steps - counter, 2.0)) - TopForm.Top;
                    if (LeftForm != null)
                        LeftForm.Top = TopForm.Bottom;
                    if (RightForm != null)
                        RightForm.Top = TopForm.Bottom;
                }


                if (LeftForm != null)
                {
                    if (counter == 0)
                        LeftForm.Width = dest.Left - LeftForm.Left;
                    else
                        LeftForm.Width = dest.Left - (int)(dest.Left / Math.Pow(steps - counter, 2.0)) - LeftForm.Left;
                }

                if (BottomForm != null)
                {
                    if (counter == 0)
                    {
                        int newheight = BottomForm.Bottom - dest.Bottom;
                        BottomForm.Top = dest.Bottom;
                        BottomForm.Height = newheight;
                    }
                    else
                    {
                        BottomForm.Top = dest.Bottom + (int)(dest.Bottom / Math.Pow(steps - counter, 2.0));
                        BottomForm.Height = desktop_rect.Y + desktop_rect.Height - BottomForm.Top;
                    }

                    if (LeftForm != null)
                        LeftForm.Height = BottomForm.Top - LeftForm.Top;
                    if (RightForm != null)
                        RightForm.Height = BottomForm.Top - RightForm.Top;
                }

                if (RightForm != null)
                {
                    if (counter == 0)
                    {
                        RightForm.Width = desktop_rect.X + desktop_rect.Width - dest.Right;
                        RightForm.Left = dest.Right;
                    }
                    else
                    {
                        RightForm.Left = dest.Right + (int)(dest.Right / Math.Pow(steps - counter, 2.0));
                        RightForm.Width = desktop_rect.X + desktop_rect.Width - RightForm.Left;
                    }
                }
                

            }

            private void Runtimer_Tick(object sender, EventArgs e)
            {
                counter--;

                SetSizes(counter);

                if (counter == 0)
                    Runtimer.Stop();
            }

        }

        static public void ZoomRect(in Rect rect)
        {
            if (rect.Width * rect.Height == 0) return;

            MaskScreen maskscreen = new MaskScreen(rect);
            maskscreen.Dispose();
            /*
            IntPtr desktop = Graphic.GetDC(IntPtr.Zero);
            Bitmap bmp = new Bitmap(desktop_rect.Width, desktop_rect.Height, PixelFormat.Format24bppRgb);
            gfx = Graphics.FromImage(bmp);
            gfx.CopyFromScreen(0, 0, 0, 0, desktop_rect.Size, CopyPixelOperation.SourceCopy);
            gfx.Dispose();
            Mat img = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp);
            bmp.Dispose();
            Mat Originalimage = img.Clone();
            BlurOrg(ref img, new Rect(0,0,0,0), 5);

            IntPtr sourceDC = CreateCompatibleDC(desktop);
            bmp = img.ToBitmap();
            img.Dispose();

            IntPtr sourceBitmap = bmp.GetHbitmap();
            IntPtr originalBitmap = SelectObject(sourceDC, sourceBitmap);
            BitBlt(desktop, 0, 0, bmp.Width, bmp.Height, sourceDC, 0, 0, TernaryRasterOperations.SRCCOPY);
            SelectObject(sourceDC, originalBitmap);
            DeleteObject(sourceBitmap);
            DeleteObject(originalBitmap);
            bmp.Dispose();

            //g.DrawRectangle(new Pen(Color.DarkGray, 2), rect.Left, rect.Top, rect.Width, rect.Height);
            Application.DoEvents();
            Thread.Sleep(500);

            bmp = Originalimage.ToBitmap();
            sourceBitmap = bmp.GetHbitmap();
            originalBitmap = SelectObject(sourceDC, sourceBitmap);
            BitBlt(desktop, 0, 0, bmp.Width, bmp.Height, sourceDC, 0, 0, TernaryRasterOperations.SRCCOPY);
            SelectObject(sourceDC, originalBitmap);
            DeleteObject(sourceBitmap);
            DeleteObject(originalBitmap);
            bmp.Dispose();
            Originalimage.Dispose();
            DeleteDC(sourceDC);
            Graphic.ReleaseDC(IntPtr.Zero, desktop);*/
        }

    }
}
