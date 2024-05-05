using iSchool.Sites.UserCenter.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.BLL
{
    public class AccountBLL
    {
        protected DAL.AccountDAL accountDAL = new DAL.AccountDAL();
        public Models.ViewsModels.Account.BindInfo GetBindInfo(Guid userID)
        {
            var bindInfo = new Models.ViewsModels.Account.BindInfo()
            {
                Mobile = accountDAL.GetBindMobile(userID),
                UnionID_QQ = accountDAL.GetBindQQ(userID),
                UnionID_Weixin = accountDAL.GetBindWeixin(userID)
            };
            if (!string.IsNullOrEmpty(bindInfo.Mobile))
            {
                bindInfo.Mobile = bindInfo.Mobile.Substring(0, 3) + "****" + bindInfo.Mobile.Substring(7);
            }
            return bindInfo;
        }
        public bool BindMobile(Guid userID, Guid kid, short nationCode_o, string mobile_o, short nationCode_n, string mobile_n, string rndCode, out string failResult)
        {
            failResult = null;
            if (string.IsNullOrEmpty(mobile_n) || string.IsNullOrEmpty(rndCode))
            {
                failResult = "请填写完整信息";
                return false;
            }
            string privateKey = RedisHelper.Get<string>(kid.ToString())?.ToString();
            if (string.IsNullOrEmpty(privateKey))
            {
                failResult = "呆太久了，再试一下吧";
                return false;
            }
            try
            {
                mobile_n = RSAHelper.RSADecrypt(privateKey, mobile_n);
                mobile_o = string.IsNullOrEmpty(mobile_o) ? null : RSAHelper.RSADecrypt(privateKey, mobile_o);
            }
            catch (Exception ex)
            {
                failResult = "数据解密失败，请再试一下吧";
                return false;
            }
            if ((nationCode_o + mobile_o) == (nationCode_n + mobile_n))
            {
                failResult = "新旧号码一样无需修改";
                return false;
            }
            if (!new SMSHelper().CheckRndCode(nationCode_n, mobile_n, rndCode, "RegistOrLogin", false))
            {
                failResult = "验证码错误或已失效";
                return false;
            }
            if (!accountDAL.OldMobileMatch(userID, nationCode_o, mobile_o))
            {
                failResult = "原手机号码不正确";
                return false;
            }
            if (!accountDAL.CheckMobileConflict(nationCode_n, mobile_n))
            {
                failResult = "账号冲突";
                return false;
            }
            if (accountDAL.BindMobile(userID, nationCode_n, mobile_n))
            {
                RedisHelper.Remove("RNDCode-" + mobile_n + "-" + "RegistOrLogin");
                return true;
            }
            else
            {
                failResult = "手机号绑定失败";
                return false;
            }
        }
        public bool BindQQ(Guid userID, Guid unionID, Guid openID, bool confirm)
        {
            if (!confirm && !accountDAL.CheckQQConflict(unionID))
            {
                return false;
            }
            return accountDAL.BindQQUnionID(unionID, userID) && accountDAL.BindQQOpenID(openID, userID, "web");
        }
        public bool BindWX(Guid userID, string unionID, string openID, bool confirm)
        {
            if (!confirm && !accountDAL.CheckWXConflict(unionID))
            {
                return false;
            }
            return accountDAL.BindWXUnionID(unionID, userID) && accountDAL.BindWXOpenID(openID, userID, "web");
        }
        public (bool result, List<Models.dbo.UserInfo> list, string errorDescription) ListConflict(Guid userID, Guid kid, short nationCode, ref string data, string rndCode, byte dataType)
        {
            if (dataType == 0)
            {
                string privateKey = RedisHelper.Get<string>(kid.ToString())?.ToString();
                if (string.IsNullOrEmpty(privateKey))
                {
                    return (false, new List<Models.dbo.UserInfo>(), "呆太久了，再试一下吧");
                }
                try
                {
                    data = RSAHelper.RSADecrypt(privateKey, data);
                }
                catch (Exception ex)
                {
                    return (false, new List<Models.dbo.UserInfo>(), "数据解密失败，请再试一下吧");
                }
                if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(rndCode))
                {
                    return (false, new List<Models.dbo.UserInfo>(), "无效请求");
                }
                if (!new SMSHelper().CheckRndCode(nationCode, data, rndCode, "RegistOrLogin"))
                {
                    return (false, new List<Models.dbo.UserInfo>(), "验证失败，请再试一下吧");
                }
                return (true, ListMobileConflict(nationCode, data), null);
            }
            else if (dataType == 1)
            {
                var WXuserinfo = RedisHelper.Get<WXUserInfoResult>("WXBindInfo-" + userID);
                if (WXuserinfo == null)
                {
                    return (false, new List<Models.dbo.UserInfo>(), "呆太久了，再试一下吧");
                }
                return (true, ListWXConflict(WXuserinfo.unionid, WXuserinfo.openid), null);
            }
            else if (dataType == 2)
            {
                var OpenIDInfo = RedisHelper.Get<QQOpenID>("QQBindInfo-" + userID);
                if (OpenIDInfo == null)
                {
                    return (false, new List<Models.dbo.UserInfo>(), "呆太久了，再试一下吧");
                }
                return (true, ListQQConflict(OpenIDInfo.unionid, OpenIDInfo.openid), null);
            }
            else
            {
                return (false, new List<Models.dbo.UserInfo>(), "无效请求");
            }
        }
        private List<Models.dbo.UserInfo> ListMobileConflict(short nationCode, string mobile)
        {
            return accountDAL.ListMobileConflict(nationCode, mobile);
        }
        private List<Models.dbo.UserInfo> ListWXConflict(string unionID, string openID)
        {
            return accountDAL.ListWXConflict(unionID);
        }
        private List<Models.dbo.UserInfo> ListQQConflict(string unionID, string openID)
        {
            return accountDAL.ListQQConflict(Guid.Parse(unionID), Guid.Parse(openID));
        }
        public Models.dbo.UserInfo GetUserInfo(Guid userID)
        {
            return accountDAL.GetUserInfo(userID);
        }
        public bool ChangePSW(Guid kid, short nationCode, string mobile, string password, string rndCode, out string failResult, bool mobileIsEncode=false)
        {
            failResult = null;
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(rndCode))
            {
                failResult = "请填写完整信息";
                return false;
            }
            string privateKey = RedisHelper.Get<string>(kid.ToString())?.ToString();
            if (string.IsNullOrEmpty(privateKey))
            {
                failResult = "呆太久了，再试一下吧";
                return false;
            }
            try
            {
                if (mobileIsEncode)
                {
                    mobile= RSAHelper.RSADecrypt(privateKey, mobile);
                }
                password = RSAHelper.RSADecrypt(privateKey, password);
                password = MD5Helper.GetMD5(password);
            }
            catch (Exception ex)
            {
                failResult = "数据解密失败，请再试一下吧";
                return false;
            }
            if (!new SMSHelper().CheckRndCode(nationCode, mobile, rndCode, "ChangePSW", false))
            {
                failResult = "验证码错误或已失效";
                return false;
            }
            if (accountDAL.ChangePSW(nationCode, mobile, Guid.Parse(password)))
            {
                RedisHelper.Remove("RNDCode-" + mobile + "-" + "ChangePSW");
                return true;
            }
            else
            {
                failResult = "手机号绑定失败";
                return false;
            }
        }
        public bool UpdateUserInfo(Models.dbo.UserInfo userInfo)
        {
            return accountDAL.UpdateUserInfo(userInfo);
        }
        public bool SetUserInterest(Models.dbo.Interest interest)
        {
            return accountDAL.SetUserInterest(interest);
        }
        public Models.dbo.Interest GetUserInterest(Guid userID)
        {
            return accountDAL.GetUserInterest(userID);
        }
    }
}
