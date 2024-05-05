using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Sites.UserCenter.Models.dbo;
using Dapper;

namespace iSchool.Sites.UserCenter.DAL
{
    public class LoginDAL : AccountDAL
    {
        private static readonly object wxlock = new object();
        private static readonly object qqlock = new object();
        public bool RegisUserInfo(ref UserInfo userInfo)
        {
            if (userInfo.Id == Guid.Empty)
            {
                userInfo.Id = Guid.NewGuid();
            }
            userInfo.RegTime = DateTime.Now;
            userInfo.LoginTime = userInfo.RegTime;
            return connection.Execute(@"insert into userInfo 
(id,nationCode,mobile,password,nickname,regTime,loginTime,blockage,headImgUrl,sex,city,channel) values 
(@id,@nationCode,@mobile,@password,@nickname,@regTime,@loginTime,@blockage,@headImgUrl,@sex,@city,@channel)", userInfo) > 0;
        }
        public UserInfo PasswordLogin(short nationCode, string mobile, Guid password)
        {
            return connection.QueryFirstOrDefault<UserInfo>(@"select * from userInfo where nationCode=@nationCode and mobile=@mobile and password=@password", new { nationCode, mobile, password });
        }
        public UserInfo MobileLogin(short nationCode, string mobile)
        {
            return connection.QueryFirstOrDefault<UserInfo>(@"select * from userInfo where mobile=@mobile and nationCode=@nationCode", new { nationCode, mobile });
        }
        public bool WXUnionIDLogin(string unionID, string openID, string appName, ref UserInfo userInfo)
        {
            lock (wxlock)
            {
                var info = connection.QueryFirstOrDefault<UserInfo>(@"select * from userInfo where exists 
(select 1 from unionid_weixin where userID=id and unionID=@unionID)", new { unionID });
                if (info == null)
                {
                    WXOpenIDLogin(openID, appName, ref userInfo);
                    BindWXUnionID(unionID, userInfo.Id);
                }
                else
                {
                    BindWXOpenID(openID, userInfo.Id, appName);
                    userInfo = info;
                }
                return true;
            }
        }
        public bool WXOpenIDLogin(string openID, string appName, ref UserInfo userInfo)
        {
            lock (wxlock)
            {
                var info = connection.QueryFirstOrDefault<UserInfo>(@"select * from userInfo where exists 
(select 1 from openid_weixin where userID=id and openid=@openid)", new { openID });
                if (info == null)
                {
                    userInfo.Id = Guid.NewGuid();
                    if (string.IsNullOrEmpty(userInfo.Nickname))
                    {
                        userInfo.Nickname = "微信用户-" + userInfo.Id.ToString("N").Substring(0, 8);
                    }
                    RegisUserInfo(ref userInfo);
                    BindWXOpenID(openID, userInfo.Id, appName);
                }
                else
                {
                    userInfo = info;
                }
                return true;
            }
        }
        public bool QQUnionIDLogin(Guid unionID, Guid openID, string appName, ref UserInfo userInfo)
        {
            lock (qqlock)
            {
                var info = connection.QueryFirstOrDefault<UserInfo>(@"select * from userInfo where exists 
(select 1 from unionid_qq where userID=id and unionID=@unionID)", new { unionID });
                if (info == null)
                {
                    QQOpenIDLogin(openID, appName, ref userInfo);
                    BindQQUnionID(unionID, userInfo.Id);
                }
                else
                {
                    BindQQOpenID(openID, userInfo.Id, appName);
                    userInfo = info;
                }
                return true;
            }
        }
        public bool QQOpenIDLogin(Guid openID, string appName, ref UserInfo userInfo)
        {
            lock (qqlock)
            {
                var info = connection.QueryFirstOrDefault<UserInfo>(@"select * from userInfo where exists 
(select 1 from openid_qq where userID=id and openid=@openid)", new { openID });
                if (info == null)
                {
                    userInfo.Id = Guid.NewGuid();
                    RegisUserInfo(ref userInfo);
                    BindQQOpenID(openID, userInfo.Id, appName);
                }
                else
                {
                    userInfo = info;
                }
                return true;
            }
        }
    }
}
