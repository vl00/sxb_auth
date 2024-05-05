using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Library
{
    public class MD5Helper
    {
        private static string MD5Tail;
        public MD5Helper(string _MD5Tail)
        {
            MD5Tail = _MD5Tail;
        }
        public static string GetMD5(string str)
        {
            return GetSimpleMD5(str + MD5Tail);
        }
        public static string GetSimpleMD5(string str)
        {
            byte[] data = Encoding.GetEncoding("UTF-8").GetBytes(str);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] OutBytes = md5.ComputeHash(data);

            string OutString = "";
            for (int i = 0; i < OutBytes.Length; i++)
            {
                OutString += OutBytes[i].ToString("x2");
            }
            return OutString.ToUpper();
        }
    }
}
