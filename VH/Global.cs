using System;
using System.Windows.Forms;
using OpenCvSharp;
using System.IO;

namespace VH
{
    public static class Global
    {
        public static string RecordPath = Application.StartupPath + "\\Record\\";
        public static ImageEncodingParam[] png_prms = new ImageEncodingParam[] { new ImageEncodingParam(ImwriteFlags.PngCompression, 0) };

        public static void Reset()
        {
            foreach (String file in Directory.GetFiles(Global.RecordPath, "*.*", SearchOption.TopDirectoryOnly))
                File.Delete(file);
        }
    }
}
