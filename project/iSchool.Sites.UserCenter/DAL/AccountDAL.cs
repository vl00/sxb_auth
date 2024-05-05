using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace iSchool.Sites.UserCenter.DAL
{
    public class AccountDAL
    {
        protected IDbConnection connection = new Library.DataAccess().iSchoolUserConnection();
        /// <summary>
        /// 判断旧手机号是否正确
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public bool OldMobileMatch(Guid userID, short nationCode, string mobile)
        {
            if (string.IsNullOrEmpty(mobile))
            {
                return connection.ExecuteScalar<int>("select count(*) from userInfo where id=@userID and mobile is null", new { userID }) == 1;
            }
            else
            {
                return connection.ExecuteScalar<int>("select count(*) from userInfo where id=@userID and nationCode=@nationCode and mobile=@mobile", new { userID, nationCode, mobile }) == 1;
            }
        }
        /// <summary>
        /// 检查手机号是否冲突
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public bool CheckMobileConflict(short nationCode, string mobile)
        {
            return connection.ExecuteScalar<int>("select count(*) from userInfo where nationCode=@nationCode and mobile=@mobile", new { nationCode, mobile }) == 0;
        }
        /// <summary>
        /// 检查微信UnionID号是否冲突
        /// </summary>
        /// <param name="unionID"></param>
        /// <returns></returns>
        public bool CheckWXConflict(string unionID)
        {
            return connection.ExecuteScalar<int>("select count(*) from unionid_weixin where unionID=@unionID", new { unionID }) == 0;
        }
        /// <summary>
        /// 检查QQUnionID号是否冲突
        /// </summary>
        /// <param name="unionID"></param>
        /// <returns></returns>
        public bool CheckQQConflict(Guid unionID)
        {
            return connection.ExecuteScalar<int>("select count(*) from unionid_qq where unionID=@unionID", new { unionID }) == 0;
        }
        /// <summary>
        /// 列出手机号码冲突账号
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public List<Models.dbo.UserInfo> ListMobileConflict(short nationCode, string mobile)
        {
            return connection.Query<Models.dbo.UserInfo>("select id, nickname, headImgUrl from userInfo where nationCode=@nationCode and mobile=@mobile", new { nationCode, mobile }).ToList();
        }
        /// <summary>
        /// 列出微信冲突账号
        /// </summary>
        /// <param name="unionid"></param>
        /// <returns></returns>
        public List<Models.dbo.UserInfo> ListWXConflict(string unionid)
        {
            return connection.Query<Models.dbo.UserInfo>(@"select id, nickname, headImgUrl from userInfo where 
exists (select 1 from unionid_weixin where unionid=@unionid and userID=userInfo.id)", new { unionid }).ToList();
        }
        /// <summary>
        /// 列出QQ冲突账号
        /// </summary>
        /// <param name="unionid"></param>
        /// <returns></returns>
        public List<Models.dbo.UserInfo> ListQQConflict(Guid unionid, Guid openid)
        {
            return connection.Query<Models.dbo.UserInfo>(@"select id, nickname, headImgUrl from userInfo where 
exists (select 1 from unionid_qq where unionid=@unionid and userID=userInfo.id)
or exists (select 1 from openid_qq where openid=@openid and userID=userInfo.id)
", new { unionid, openid }).ToList();
        }
        /// <summary>
        /// 绑定手机号
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public bool BindMobile(Guid userID, short nationCode, string mobile)
        {
            return connection.Execute("update userInfo set nationCode=@nationCode, mobile=@mobile where id=@userID", new { nationCode, mobile, userID }) > 0;
        }
        /// <summary>
        /// 绑定QQUnionID
        /// </summary>
        /// <param name="unionID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public bool BindQQUnionID(Guid unionID, Guid userID)
        {
            using (var connection = new Library.DataAccess().iSchoolUserConnection())
            {
                return connection.Execute(@"merge into unionid_qq
using (select 1 as o) t
on unionid_qq.unionID=@unionid
when not matched then insert 
(unionID, userID) values (@unionID, @userID)
when matched then update
set userID=@userID;", new { unionID, userID }) > 0;
            }
        }
        /// <summary>
        /// 绑定QQOpenID
        /// </summary>
        /// <param name="openID"></param>
        /// <param name="userID"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        public bool BindQQOpenID(Guid openID, Guid userID, string appName)
        {
            using (var connection = new Library.DataAccess().iSchoolUserConnection())
            {
                return connection.Execute(@"merge into openid_qq
using (select 1 as o) t
on openid_qq.openID=@openID
when not matched then insert 
(openID, userID, appName) values (@openID, @userID, @appName)
when matched then update
set userID=@userID;", new { openID, userID, appName }) > 0;
            }
        }
        /// <summary>
        /// 绑定微信UnionID
        /// </summary>
        /// <param name="unionID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public bool BindWXUnionID(string unionID, Guid userID)
        {
            using (var connection = new Library.DataAccess().iSchoolUserConnection())
            {
                return connection.Execute(@"merge into unionid_weixin 
using (select 1 as o) t
on unionid_weixin.unionID = @unionID
when not matched then insert 
(unionID, userID) values (@unionID, @userID)
when matched then update
set userID=@userID;", new { unionID, userID }) > 0;
            }
        }
        /// <summary>
        /// 绑定微信OpenID
        /// </summary>
        /// <param name="openID"></param>
        /// <param name="appName"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public bool BindWXOpenID(string openID, Guid userID, string appName)
        {
            using (var connection = new Library.DataAccess().iSchoolUserConnection())
            {
                //return connection.Execute(@"insert into openid_weixin (openID, userID, appName, valid) values (@openID, @userID, @appName, @valid)", new { openID, userID, appName, valid = false }) > 0;
                return connection.Execute(@"merge into openid_weixin
using (select 1 as o) t
on openid_weixin.openid = @openID
when not matched then insert 
(openID, userID, appName, valid) values (@openID, @userID, @appName, @valid)
when matched then update
set userID=@userID;", new { openID, userID, appName, valid = false }) > 0;
            }
        }
        /// <summary>
        /// 解绑手机号
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public bool UnBindMobile(Guid userID)
        {
            return connection.Execute("update userInfo set mobile=null where id=@userID", new { userID }) > 0;
        }
        /// <summary>
        /// 获取绑定的手机号
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public string GetBindMobile(Guid userID)
        {
            return connection.QueryFirstOrDefault<string>("select mobile from userInfo where id=@userID", new { userID });
        }
        /// <summary>
        /// 获取绑定的微信UnionID
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public string GetBindWeixin(Guid userID)
        {
            return connection.QueryFirstOrDefault<string>("select unionID from unionid_weixin where userID=@userID", new { userID });
        }
        /// <summary>
        /// 获取绑定的QQUnionID
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public Guid? GetBindQQ(Guid userID)
        {
            return connection.QueryFirstOrDefault<Guid?>("select unionID from unionid_qq where userID=@userID", new { userID });
        }
        public Models.dbo.UserInfo GetUserInfo(Guid userID)
        {
            return connection.QueryFirst<Models.dbo.UserInfo>("select * from userInfo where id=@userID", new { userID });
        }
        public bool ChangePSW(short nationCode, string mobile, Guid password)
        {
            return connection.Execute("update userInfo set password=@password where nationCode=@nationCode and mobile=@mobile", new { nationCode, mobile, password }) > 0;
        }
        public bool UpdateUserInfo(Models.dbo.UserInfo userInfo)
        {
            return connection.Execute("update userInfo set nickname=@nickname, headImgUrl=@headImgUrl where id=@id", userInfo) > 0;
        }
        public List<Models.dbo.Verify> GetVerifies(Guid userID)
        {
            return connection.Query<Models.dbo.Verify>("select * from verify where userID=@userID", new { userID }).ToList();
        }
        public bool SetUserInterest(Models.dbo.Interest interest)
        {
            return connection.Execute(@"update interest set 
type_1=@type_1, type_2=@type_2, type_3=@type_3, type_4=@type_4, 
nature_1=@nature_1, nature_2=@nature_2, nature_3=@nature_3,
lodging_0=@lodging_0, lodging_1=@lodging_1
where userID=@userID", interest) > 0;
        }
        public Models.dbo.Interest GetUserInterest(Guid userID)
        {
            return connection.QueryFirstOrDefault<Models.dbo.Interest>(@"select * from interest where userID=@userID", new { userID });
        }
    }
}
