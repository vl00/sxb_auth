using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace UEditor
{
    public class Library
    {
        public static string GetFileExtension(Stream fileStream)
        {
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            fileStream.Seek(0, SeekOrigin.Begin);
            return GetFileExtension(bytes);
        }
        public static string GetFileExtension(byte[] fileBytes)
        {
            string Extension = "";
            Dictionary<string, byte[]> ImageHeader = new Dictionary<string, byte[]>();
            ImageHeader.Add(".jpg", new byte[] { 255, 216, 255 });
            ImageHeader.Add(".png", new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
            ImageHeader.Add(".gif", new byte[] { 71, 73, 70, 56, 57, 97 });
            foreach (string ext in ImageHeader.Keys)
            {
                byte[] header = ImageHeader[ext];
                byte[] test = new byte[header.Length];
                Array.Copy(fileBytes, 0, test, 0, test.Length);
                bool same = true;
                for (int i = 0; i < test.Length; i++)
                {
                    if (test[i] != header[i])
                    {
                        same = false;
                        break;
                    }
                }
                if (same)
                {
                    Extension = ext;
                    break;
                }
            }
            return Extension;
        }
        public static bool TransportFile(byte[] bytes, string FileID, string Extension, string type, string subType)
        {
            return PostFile("https://file_local.sxkid.com/op/uploadHandler_v2.ashx?type=" + type + "&subtype=" + subType + "&filename=" + FileID + Extension, bytes);
        }
        public static bool TransportFile(byte[] bytes, string FileID, out string Extension, string type, string subType)
        {
            try
            {
                Extension = GetFileExtension(bytes);
                if (string.IsNullOrEmpty(Extension))
                {
                    return false;
                }
                return PostFile("https://file_local.sxkid.com/op/uploadHandler_v2.ashx?type=" + type + "&subtype=" + subType + "&filename=" + FileID + Extension.ToLower(), bytes);
            }
            catch (Exception ex)
            {
                Extension = ".jpg";
                return false;
            }
        }
        private static bool PostFile(string url, byte[] data)
        {
            try
            {
                return GetHTTPResponse(url, data) == "0";
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static string GetHTTPResponse(string url, byte[] postData)
        {
            try
            {
                HttpWebRequest HRQ = (HttpWebRequest)WebRequest.Create(url);
                HRQ.Method = "post";
                HRQ.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
                HRQ.Timeout = 30000;
                HRQ.ContentLength = postData.Length;
                Stream sr_s = HRQ.GetRequestStream();
                sr_s.Write(postData, 0, postData.Length);
                HttpWebResponse RES = (HttpWebResponse)HRQ.GetResponse();
                if (HRQ.HaveResponse)
                {
                    Stream Rs = RES.GetResponseStream();
                    StreamReader RsRead = new StreamReader(Rs);
                    string RetJson = RsRead.ReadToEnd();
                    RsRead.Close();
                    return RetJson;
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
    }
}
