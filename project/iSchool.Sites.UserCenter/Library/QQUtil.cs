using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Library
{
    public class QQUtil
    {
        public static string GetWebLoginURL(string url, string appid = null, string state=null)
        {
            return
                "https://graph.qq.com/oauth2.0/authorize?client_id="
                + (appid ?? "101645642") +
                "&redirect_uri="
                + Uri.EscapeDataString("https://qq.sxkid.com/LoginAuth?redirect_uri="
                + Uri.EscapeDataString(url))
                + "&response_type=code&scope=all&state=" + state;
        }
        /// <summary>
        /// 通过code获取access_token
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="appSecret"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static async Task<string> GetQQAccessToken(string appId, string appSecret, string code, string redirect_uri)
        {
            string url = "https://graph.qq.com/oauth2.0/token?client_id=" + appId + "&client_secret=" + appSecret +
                "&code=" + code + "&grant_type=authorization_code&redirect_uri=" + redirect_uri;
            string ret = await HttpHelper.HttpGetAsync<string>(url);
            return CommonHelper.GetQueryValue(ret, "access_token");
        }
        /// <summary>
        /// 通过access_token获取openid及unionid
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public static async Task<QQOpenID> GetQQOpenID(string accessToken)
        {
            string url = "https://graph.qq.com/oauth2.0/me?access_token=" + accessToken + "&unionid=1";
            return await HttpHelper.HttpGetAsync<QQOpenID>(url);
        }
        /// <summary>
        /// 拉取用户信息
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="openId"></param>
        /// <returns></returns>
        public static async Task<QQUserInfoResult> GetQQUserInfo(string accessToken, string appSecret, string openId)
        {
            string url = "https://graph.qq.com/user/get_user_info?access_token=" + accessToken + "&oauth_consumer_key=" + appSecret + "&openid=" + openId;
            return await HttpHelper.HttpGetAsync<QQUserInfoResult>(url);
        }
    }
    public class QQRet
    {
        public int ret { get; set; }
        /// <summary>
        /// 错误编号
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 错误提示消息
        /// </summary>
        public string msg { get; set; }
    }
    public class QQOpenID : QQRet
    {
        public string client_id { get; set; }
        /// <summary>
        /// 授权用户唯一标识
        /// </summary>
        public string openid { get; set; }
        /// <summary>
        /// 授权用户唯一联合标识
        /// </summary>
        public string unionid { get; set; }
    }

    public class QQUserInfoResult:QQRet
    {
        public bool is_lost { get; set; }
        public string nickname { get; set; }
        public string gender { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string year { get; set; }
        public string constellation { get; set; }
        public string figureurl { get; set; }
        public string figureurl_1 { get; set; }
        public string figureurl_2 { get; set; }
        public string figureurl_qq_1 { get; set; }
        public string figureurl_qq_2 { get; set; }
        public string figureurl_qq { get; set; }
        public byte figureurl_type { get; set; }
        public string is_yellow_vip { get; set; }
        public string vip { get; set; }
        public int yellow_vip_level { get; set; }
        public int level { get; set; }
        public string is_yellow_year_vip { get; set; }
    }
}
