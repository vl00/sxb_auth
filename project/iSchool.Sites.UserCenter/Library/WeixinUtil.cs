using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Library
{
    public class WeixinUtil
    {
        public static string GetLoginURL(string url, string appid = null, string scope = "snsapi_base", string state = null)
        {
            return
                "https://open.weixin.qq.com/connect/oauth2/authorize?" +
                "appid=" + (appid ?? "wxeefc53a3617746e2") +
                "&redirect_uri="
                + Uri.EscapeDataString("https://weixin.sxkid.com/LoginAuth.aspx?redirect_uri="
                + Uri.EscapeDataString(url))
                + "&response_type=code&scope=" + scope + "&state=" + state + "#wechat_redirect";
        }
        public static string GetWebLoginUrl(string url, string appid = null, string state = null)
        {
            return @"https://open.weixin.qq.com/connect/qrconnect?appid="+ (appid ?? "wxd3064b3b16a7768f") + "&scope=snsapi_login&redirect_uri="
                + Uri.EscapeDataString("https://weixin.sxkid.com/LoginAuth.aspx?redirect_uri="
                + Uri.EscapeDataString(url))
                + "&state="+ state + "&login_type=jssdk&self_redirect=true&styletype=&sizetype=&bgcolor=&rst=&style=black";
        }
        /// <summary>
        /// 通过code获取access_token
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="appSecret"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static async Task<WXAccessToken> GetWeiXinAccessToken(string appId, string appSecret, string code)
        {
            string url = "https://api.weixin.qq.com/sns/oauth2/access_token?appid=" + appId + "&secret=" + appSecret +
                "&code=" + code + "&grant_type=authorization_code";
            return await HttpHelper.HttpGetAsync<WXAccessToken>(url);
        }
        /// <summary>
        /// 拉取用户信息
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="openId"></param>
        /// <returns></returns>
        public static async Task<WXUserInfoResult> GetWeiXinUserInfo(string accessToken, string openId)
        {
            string url = "https://api.weixin.qq.com/sns/userinfo?access_token=" + accessToken + "&openid=" + openId + "&lang=zh_CN";
            return await HttpHelper.HttpGetAsync<WXUserInfoResult>(url);
        }
    }

    public class WXAccessToken
    {
        /// <summary>
        /// 接口调用凭证
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// access_token接口调用凭证超时时间，单位（秒）
        /// </summary>
        public int expires_in { get; set; }
        /// <summary>
        /// 用户刷新access_token
        /// </summary>
        public string refresh_token { get; set; }
        /// <summary>
        /// 授权用户唯一标识
        /// </summary>
        public string openid { get; set; }
        /// <summary>
        /// 用户授权的作用域，使用逗号（,）分隔
        /// </summary>
        public string scope { get; set; }
        /// <summary>
        /// 错误编号
        /// </summary>
        public int errcode { get; set; }
        /// <summary>
        /// 错误提示消息
        /// </summary>
        public string errmsg { get; set; }

    }

    public class WXUserInfoResult
    {
        public string subscribe { get; set; }
        /// <summary>
        /// 授权用户唯一标识
        /// </summary>
        public string openid { get; set; }
        /// <summary>
        /// 用户授权的作用域，使用逗号（,）分隔
        /// </summary>
        public string scope { get; set; }
        public string unionid { get; set; }
        /// 用户昵称
        /// </summary>
        public string nickname { get; set; }
        /// <summary>
        /// 用户的性别，值为1时是男性，值为2时是女性，值为0时是未知
        /// </summary>
        public string sex { get; set; }
        /// <summary>
        /// 用户个人资料填写的省份
        /// </summary>
        public string province { get; set; }
        /// <summary>
        /// 普通用户个人资料填写的城市
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// 国家，如中国为CN
        /// </summary>
        public string country { get; set; }
        /// <summary>
        /// 用户头像，最后一个数值代表正方形头像大小（有0、46、64、96、132数值可选，0代表640*640正方形头像），用户没有头像时该项为空
        /// </summary>
        public string headimgurl { get; set; }
        /// <summary>
        /// 用户特权信息，json 数组，如微信沃卡用户为（chinaunicom）
        /// </summary>
        public string[] privilege { get; set; }
        /// <summary>
        /// 错误编号
        /// </summary>
        public int errcode { get; set; }
        /// <summary>
        /// 错误提示消息
        /// </summary>
        public string errmsg { get; set; }
    }
}
