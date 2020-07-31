using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using OpenCvSharp;
using Newtonsoft.Json;

namespace VH
{
    class ImageHashTable
    {
        public static SortedDictionary<String, String> ImageMainHashTable = new SortedDictionary<String, String>();
        public static SortedDictionary<String, SortedDictionary<String, String>> ImageSubHashTable = new SortedDictionary<String, SortedDictionary<String, String>>();

        public static String HashImage(in Mat image, in int size = 8)
        {
            Mat img = new Mat();
            Cv2.Resize(image, img, new OpenCvSharp.Size(size, size));
            Cv2.CvtColor(img, img, ColorConversionCodes.BGRA2GRAY);
            img.ConvertTo(img, MatType.CV_8U);
            int length = (int)(img.Total() * img.Channels());
            byte[] bytes = new byte[length];
            img.GetArray(out bytes);
            //Debug: Cv2.ImEncode(".png", img, out bytes, Global.png_prms);
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
            }
            else
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
                image.ImWrite(RecordPath + object_name + ".png", Global.png_prms);
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
            if (!ImageMainHashTable.ContainsKey(Hash))
            {
                object_name = "Object" + ImageMainHashTable.Count();
                image.ImWrite(RecordPath + object_name + ".png", Global.png_prms);
                ImageMainHashTable.Add(Hash, object_name);
            }
            else
            {
                string value;
                ImageMainHashTable.TryGetValue(Hash, out value);
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
                        ImageMainHashTable[Hash] = "SubHash";
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
            if (ImageMainHashTable.TryGetValue(hashstring, out Value))
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

        static public void Reset()
        {
            ImageMainHashTable.Clear();
            ImageSubHashTable.Clear();
        }


        public static void Save()
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonSerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            jsonSerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;

            lock (ImageHashTable.ImageMainHashTable)
            {
                using (var writer = new StreamWriter(File.Create(Global.RecordPath + "Objects.djs")))
                    writer.Write(JsonConvert.SerializeObject(ImageHashTable.ImageMainHashTable, jsonSerializerSettings));
                lock (ImageHashTable.ImageSubHashTable)
                {
                    using (var writer = new StreamWriter(File.Create(Global.RecordPath + "Objects.sdjs")))
                        writer.Write(JsonConvert.SerializeObject(ImageHashTable.ImageSubHashTable, jsonSerializerSettings));
                }
            }
        }


        static public bool Load()
        {
            Reset();

            if (File.Exists(Global.RecordPath + "Objects.djs"))
                using (var reader = new StreamReader(File.OpenRead(Global.RecordPath + "Objects.djs")))
                {
                    try
                    {
                        lock (ImageMainHashTable)
                            ImageMainHashTable = JsonConvert.DeserializeObject<SortedDictionary<String, String>>(reader.ReadToEnd(), new Newtonsoft.Json.JsonSerializerSettings
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

            if (File.Exists(Global.RecordPath + "Objects.sdjs"))
                using (var reader = new StreamReader(File.OpenRead(Global.RecordPath + "Objects.sdjs")))
                {
                    try
                    {
                        lock (ImageSubHashTable)
                            ImageSubHashTable = JsonConvert.DeserializeObject<SortedDictionary<String, SortedDictionary<String, String>>>(reader.ReadToEnd(), new Newtonsoft.Json.JsonSerializerSettings
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


    }

}
