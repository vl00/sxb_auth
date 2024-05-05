using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Library
{
    public static class ImgPropertyHelper
    {
        //////////////////////
        ///  /// <summary>
        /// 表示参数的结构
        /// </summary>
        public struct Exif
        {
            /// <summary>
            /// 数据的ID
            /// </summary>
            public string Id;
            /// <summary>
            /// 数据类型
            /// </summary>
            public int Type;
            /// <summary>
            /// 数据中值的字节长度
            /// </summary>
            public int Length;

            /// <summary>
            /// 根据ID对应的中文名
            /// </summary>
            public string Name;

            /// <summary>
            /// 根据原字节解析的参数值
            /// </summary>
            public string Value;
        }

        /// <summary>将字节通过ASCII转换为字符串
        /// </summary>
        /// <param name="bt">原字节</param>
        /// <returns></returns>
        private static string ToStrOfByte(this byte[] bt)
        {
            return System.Text.Encoding.ASCII.GetString(bt);
        }

        /// <summary>将字节转换为int
        /// </summary>
        /// <param name="bt">原字节</param>
        /// <returns></returns>
        private static int ToUnInt16(this byte[] bt)
        {
            return Convert.ToUInt16(bt[1] << 8 | bt[0]);
        }

        /// <summary>将原两组字节转换为uint
        /// </summary>
        /// <param name="bt">原字节</param>
        /// <param name="isFirst">是否转第一个字节组</param>
        /// <returns></returns>
        private static uint ToUnInt32(this byte[] bt, bool isFirst = true)
        {
            return isFirst ? Convert.ToUInt32(bt[3] << 24 | bt[2] << 16 | bt[1] << 8 | bt[0]) : Convert.ToUInt32(bt[7] << 24 | bt[6] << 16 | bt[5] << 8 | bt[4]);
        }

        /// <summary>获取曝光模式
        /// </summary>
        /// <param name="value">曝光模式值</param>
        /// <returns></returns>
        private static string ExposureMode(int value)
        {
            var rt = "Undefined";
            switch (value)
            {
                case 0:
                    rt = "自动"; break;
                case 1:
                    rt = "手动控制"; break;
                case 2:
                    rt = "程序控制"; break;
                case 3:
                    rt = "光圈优先"; break;
                case 4:
                    rt = "快门优先"; break;
                case 5:
                    rt = "夜景模式"; break;
                case 6:
                    rt = "运动模式"; break;
                case 7:
                    rt = "肖像模式"; break;
                case 8:
                    rt = "风景模式"; break;
                case 9:
                    rt = "其他模式"; break;
            }
            return rt;
        }

        /// <summary>获取测光模式
        /// </summary>
        /// <param name="value">测光模式值</param>
        /// <returns></returns>
        private static string MeteringMode(int value)
        {
            var rt = "Unknown";
            switch (value)
            {
                case 0:
                    rt = "Unknown"; break;
                case 1:
                    rt = "平均测光"; break;
                case 2:
                    rt = "中央重点平均测光"; break;
                case 3:
                    rt = "点测光"; break;
                case 4:
                    rt = "多点测光"; break;
                case 5:
                    rt = "评价测光"; break;
                case 6:
                    rt = "局部测光"; break;
                case 255:
                    rt = "其他测光"; break;
            }
            return rt;
        }

        /// <summary>获取闪光灯模式
        /// </summary>
        /// <param name="value">闪光灯值</param>
        /// <returns></returns>
        private static string FlashMode(int value)
        {
            var rt = "Unkown";
            switch (value)
            {
                case 0:
                    rt = "未使用"; break;
                case 1:
                    rt = "使用闪光灯"; break;
            }
            return rt;
        }

        /// <summary>获取白平衡模式
        /// </summary>
        /// <param name="value">白平衡值</param>
        /// <returns></returns>
        private static string WhiteBalance(int value)
        {
            var rt = "Unkown";
            switch (value)
            {
                case 0:
                    rt = "自动";//Unkown
                    break;
                case 1:
                    rt = "日光";
                    break;
                case 2:
                    rt = "荧光灯";
                    break;
                case 3:
                    rt = "白炽灯";
                    break;
                case 17:
                    rt = "标准光源A";
                    break;
                case 18:
                    rt = "标准光源B";
                    break;
                case 19:
                    rt = "标准光源C";
                    break;
                case 255:
                    rt = "其他";
                    break;
            }
            return rt;
        }

        /// <summary>通过Id获取Exif中关键名称和值
        /// </summary>
        /// <param name="pId">ID</param>
        /// <param name="pType">类型</param>
        /// <param name="pBytes">字节值</param>
        /// <returns></returns>
        public static Exif InfoOfExif(int pId, int pType, byte[] pBytes)
        {

            var rt = new Exif
            {
                Id = "0X" + pId.ToString("X"),
                Length = pBytes.Length,
                Type = pType
            };
            uint fm;
            uint fz;
            switch (pId)
            {
                case 0x010F:
                    rt.Name = "相机制造商";
                    rt.Value = pBytes.ToStrOfByte(); ;
                    break;
                case 0x0110:
                    rt.Name = "相机型号";
                    rt.Value = pBytes.ToStrOfByte();
                    break;
                case 0xA433:
                    rt.Name = "镜头制造商";
                    rt.Value = pBytes.ToStrOfByte();
                    break;
                case 0xA434:
                    rt.Name = "镜头型号";
                    rt.Value = pBytes.ToStrOfByte();
                    break;
                case 0x9003:
                    rt.Name = "拍摄时间";
                    var temp = pBytes.ToStrOfByte().Split(' ');
                    rt.Value = temp[0].Replace(":", "/") + " " + temp[1];
                    break;
                case 0x0132:
                    rt.Name = "修改时间";
                    temp = pBytes.ToStrOfByte().Split(' ');
                    rt.Value = temp[0].Replace(":", "/") + " " + temp[1];
                    break;
                case 0x0131:
                    rt.Name = "软件";
                    rt.Value = pBytes.ToStrOfByte();
                    break;
                case 0xA002:
                    rt.Name = "图像高度";
                    //rt.Value = pBytes.ToUnInt16() + " px";
                    rt.Value = pBytes.ToUnInt16().ToString();
                    break;
                case 0xA003:
                    rt.Name = "图像宽度";
                    rt.Value = pBytes.ToUnInt16() + " px";
                    rt.Value = pBytes.ToUnInt16().ToString();
                    break;
                case 0x011A:
                    fm = pBytes.ToUnInt32(false);
                    fz = pBytes.ToUnInt32();
                    rt.Value = fm == 1 ? fz.ToString() : fz + "/" + fm;
                    //rt.Value += " dpi";
                    rt.Name = "水平方向分辨率";
                    break;
                case 0x011B:
                    fm = pBytes.ToUnInt32(false);
                    fz = pBytes.ToUnInt32();
                    rt.Value = fm == 1 ? fz.ToString() : fz + "/" + fm;
                    //rt.Value += " dpi";
                    rt.Name = "垂直方向分辨率";
                    break;
                case 0x8822:
                    rt.Value = ExposureMode(pBytes.ToUnInt16());
                    rt.Name = "曝光程序";
                    break;
                case 0x9207:
                    rt.Value = MeteringMode(pBytes.ToUnInt16());
                    rt.Name = "测光模式";
                    break;
                case 0x829A:
                    fm = pBytes.ToUnInt32(false);
                    fz = pBytes.ToUnInt32();
                    //分母大于分子写为1/XXX,分母小于分子,写为保留一位小数
                    rt.Value = fm > fz ? "1/" + fm / fz : ((double)fz / fm).ToString("0.0");
                    rt.Value += " 秒";
                    rt.Name = "曝光时间";
                    break;
                case 0x8827:
                    rt.Value = pBytes.ToUnInt16().ToString();
                    rt.Name = "ISO";
                    break;
                case 0x920A:
                    fm = pBytes.ToUnInt32(false);
                    fz = pBytes.ToUnInt32();
                    rt.Value = fm == 1 ? fz.ToString() : ((double)fz / fm).ToString("0.00");
                    rt.Value += " mm";
                    rt.Name = "焦距";
                    break;
                case 0x829D:
                    rt.Value = "f/" + ((double)pBytes.ToUnInt32() / pBytes.ToUnInt32(false));
                    rt.Name = "光圈";
                    break;

                case 0x9204:
                    fm = pBytes.ToUnInt32(false);
                    var fz1 = Convert.ToInt32(pBytes[3] << 24 | pBytes[2] << 16 | pBytes[1] << 8 | pBytes[0]);
                    //曝光补偿要加+ -
                    rt.Value = fz1 > 0 ? "+" : "";
                    rt.Value += fz1 == 0 ? "0" : fz1 + "/" + fm;
                    rt.Name = "曝光补偿";
                    break;
                case 0x9208:
                    rt.Value = WhiteBalance(pBytes.ToUnInt16());
                    rt.Name = "白平衡";
                    break;
                case 0x9209:
                    rt.Value = FlashMode(pBytes.ToUnInt16());
                    rt.Name = "闪光灯";
                    break;
                default:
                    rt.Name = "其他";
                    rt.Value = "Unkown";
                    break;
            }
            return rt;
        }

        public static List<Exif> Demo(string url)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            Guid tempFileID = Guid.NewGuid();
            string fileName = tempFileID.ToString() + System.IO.Path.GetExtension(url);
            wc.DownloadFile(url, fileName);
            var img = System.DrawingCore.Image.FromFile(fileName);
            var pItems = img.PropertyItems;//将"其他"信息过滤掉
            var a = pItems.Select(pi => ImgPropertyHelper.InfoOfExif(pi.Id, pi.Type, pi.Value)).Where(j => j.Name != "其他").ToList();
            img.Dispose();
            System.IO.File.Delete(fileName);
            return a;
        }
    }
}
