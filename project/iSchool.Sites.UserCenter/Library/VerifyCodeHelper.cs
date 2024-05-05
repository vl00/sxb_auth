using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.DrawingCore.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Library
{
    public class VerifyCodeHelper
    {
        public class VerifyCodeInfo
        {
            public Guid CodeID { get; set; }
            public string Code { get; set; }
            public DateTime CreateTime { get; set; }
            public DateTime ExpireTime { get; set; }
        }
        public enum VerifyCodeType
        {
            Number = 0,
            Word = 1,
            WordAndNumber = 2
        }
        /// <summary>  
        /// 生成指定长度的随机字符串 
        /// </summary>  
        /// <param name="codeLength">字符串的长度</param>  
        /// <returns>返回随机数字符串</returns>  
        private static string RndomStr(VerifyCodeType codeType, int codeLength)
        {
            //组成字符串的字符集合  0-9数字、大小写字母
            string chars = "";
            switch (codeType)
            {
                case VerifyCodeType.Word:
                    chars = "a,b,c,d,e,f,g,h,i,j,k,m,n,o,p,q,r,s,t,u,v,w,x,y,z,A,B,C,D,E,F,G,H,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
                    break;
                case VerifyCodeType.Number:
                    chars = "0,1,2,3,4,5,6,7,8,9";
                    break;
                default:
                    chars = "2,3,4,5,6,7,8,9,a,b,c,d,e,f,g,h,i,j,k,m,n,p,q,r,s,t,u,v,w,x,y,z,A,B,C,D,E,F,G,H,J,K,L,M,N,P,Q,R,S,T,U,V,W,X,Y,Z";
                    break;
            }


            string[] charArray = chars.Split(new Char[] { ',' });
            string code = "";
            int temp = -1;//记录上次随机数值，尽量避避免生产几个一样的随机数  
            Random rand = new Random();
            //采用一个简单的算法以保证生成随机数的不同  
            for (int i = 1; i < codeLength + 1; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(i * temp * unchecked((int)DateTime.Now.Ticks));//初始化随机类  
                }
                int t = rand.Next(charArray.Length);
                if (temp == t)
                {
                    return RndomStr(codeType, codeLength);//如果获取的随机数重复，则递归调用  
                }
                temp = t;//把本次产生的随机数记录起来  
                code += charArray[t];//随机数的位数加一  
            }
            return code;
        }

        /// <summary>  
        /// 将生成的字符串写入图像文件  
        /// </summary>  
        /// <param name="code">验证码字符串</param>
        /// <param name="length">生成位数（默认4位）</param>  
        public static MemoryStream Create(out Guid codeID, out string code, VerifyCodeType codeType = VerifyCodeType.WordAndNumber, int length = 4)
        {
            code = RndomStr(codeType, length);
            Bitmap Img = null;
            Graphics graphics = null;
            MemoryStream ms = null;
            Random random = new Random();
            //颜色集合  
            Color[] color = { Color.Black, Color.Red, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown, Color.DarkCyan, Color.Purple };
            //字体集合
            string[] fonts = { "Verdana", "Microsoft Sans Serif", "Comic Sans MS", "Arial", "宋体" };
            //定义图像的大小，生成图像的实例  
            Img = new Bitmap((int)code.Length * 18, 32);
            graphics = Graphics.FromImage(Img);//从Img对象生成新的Graphics对象    
            graphics.Clear(Color.White);//背景设为白色  

            //在随机位置画背景点  

            for (int i = 0; i < 100; i++)
            {
                int x = random.Next(Img.Width);
                int y = random.Next(Img.Height);
                graphics.DrawRectangle(new Pen(Color.Gray, 0), x, y, 1, 1);
            }

            //验证码绘制在graphics中  

            for (int i = 0; i < code.Length; i++)
            {
                int colorIndex = random.Next(7);//随机颜色索引值  
                int fontIndex = random.Next(4);//随机字体索引值  
                Font font = new Font(fonts[fontIndex], 15, FontStyle.Bold);//字体  
                Brush brush = new SolidBrush(color[colorIndex]);//颜色  
                int y = 4;
                if ((i + 1) % 2 == 0)//控制验证码不在同一高度  
                {
                    y = 2;
                }
                graphics.DrawString(code.Substring(i, 1), font, brush, 3 + (i * 12), y);//绘制一个验证字符  
            }
            ms = new MemoryStream();//生成内存流对象  
            Img.Save(ms, ImageFormat.Png);//将此图像以Png图像文件的格式保存到流中  
            graphics.Dispose();
            Img.Dispose();

            codeID = Guid.NewGuid();
            code = code.ToLower();
            DateTime now = DateTime.Now;
            DateTime expire = now.AddSeconds(300);
            RedisHelper.Set("verifyCode-" + codeID, new VerifyCodeInfo() { CodeID = codeID, Code = code, CreateTime = now, ExpireTime = expire }, 300);
            return ms;
        }
        public static bool Check(Guid codeID, string code, bool removeCache=true, DateTime createTimeLimit = new DateTime())
        {
            VerifyCodeInfo cacheCodeInfo = RedisHelper.Get<VerifyCodeInfo>("verifyCode-" + codeID);
            if (cacheCodeInfo.Code == code.ToLower() && cacheCodeInfo.CreateTime > createTimeLimit)
            {
                if (removeCache)
                {
                    RedisHelper.Remove("verifyCode-" + codeID);
                }
                return true;
            }
            return false;
        }
    }
}
