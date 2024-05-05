using iSchool.Sites.UserCenter.Library;
using iSchool.Sites.UserCenter.Models.dbo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.BLL
{
    public class LoginBLL
    {
        private DAL.LoginDAL loginDAL = new DAL.LoginDAL();
        public UserInfo Login(Guid kid, short nationCode, string mobile, string password, string rndCode, out string failResult)
        {
            failResult = null;
            string privateKey = RedisHelper.Get<string>(kid.ToString())?.ToString();
            if (string.IsNullOrEmpty(privateKey))
            {
                failResult = "呆太久了，再试一下吧";
                return null;
            }
            try
            {
                mobile = RSAHelper.RSADecrypt(privateKey, mobile);
                password = string.IsNullOrEmpty(password) ? null : RSAHelper.RSADecrypt(privateKey, password);
            }
            catch(Exception ex)
            {
                failResult = "数据解密失败，请再试一下吧";
                return null;
            }
            if (!string.IsNullOrEmpty(password))
            {
                return PasswordLogin(nationCode, mobile, password, out failResult);
            }
            else if (!string.IsNullOrEmpty(rndCode))
            {
                return RndCodeLogin(nationCode, mobile, rndCode, out failResult);
            }
            else
            {
                failResult = "请输入完整登录信息";
                return null;
            }
        }
        public void WXLogin(string unionid, string openid, string appName, ref UserInfo userInfo)
        {
            if (!string.IsNullOrEmpty(unionid))
            {
                loginDAL.WXUnionIDLogin(unionid, openid, appName, ref userInfo);
            }
            else
            {
                loginDAL.WXOpenIDLogin(openid, appName, ref userInfo);
            }
        }
        public void QQLogin(string unionid, string openid, string appName, ref UserInfo userInfo)
        {
            loginDAL.QQUnionIDLogin(Guid.Parse(unionid), Guid.Parse(openid), appName, ref userInfo);
        }
        private UserInfo PasswordLogin(short nationCode, string mobile, string password, out string failResult)
        {
            password = MD5Helper.GetMD5(password);
            var info = loginDAL.PasswordLogin(nationCode, mobile, Guid.Parse(password));
            failResult = null;
            if (info==null)
            {
                failResult = "手机号或密码错误";
            }
            else if (info.Blockage)
            {
                failResult = "账号已被封禁，请与客服联系";
                info = null;
            }
            return info;
        }
        private UserInfo RndCodeLogin(short nationCode, string mobile, string rndCode, out string failResult)
        {
            if(!new SMSHelper().CheckRndCode(nationCode, mobile, rndCode, "RegistOrLogin"))
            {
                failResult = "验证码错误";
                return null;
            }
            var info = loginDAL.MobileLogin(nationCode, mobile);
            failResult = null;
            if (info == null)
            {
                info = new UserInfo()
                {
                    Id = Guid.NewGuid(),
                    Mobile = mobile,
                    NationCode = nationCode
                };
                info.Nickname = "手机用户-" + info.Id.ToString("N").Substring(0, 8);
                loginDAL.RegisUserInfo(ref info);
            }
            return info;
        }
    }
}
