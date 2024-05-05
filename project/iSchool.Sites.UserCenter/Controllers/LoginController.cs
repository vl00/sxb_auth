using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using IdentityModel;
using iSchool.Sites.UserCenter.Library;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace iSchool.Sites.UserCenter.Controllers
{
    [AllowAnonymous]
    public class LoginController : Base
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        public Models.AppInfos AppInfos { get; }
        public LoginController(IOptions<Models.AppInfos> _appInfos, IHttpContextAccessor _httpContextAccessor)
        {
            AppInfos = _appInfos.Value;
            httpContextAccessor = _httpContextAccessor;
        }


        private async Task SetSignInCookie(Models.dbo.UserInfo userInfo, List<Models.dbo.Verify> verifies)
        {
            var claims = new List<Claim>() {
                new Claim(JwtClaimTypes.Name, userInfo.Nickname),
                new Claim(JwtClaimTypes.Id, userInfo.Id.ToString())
            };
            List<byte> role = new List<byte>();
            foreach(var verify in verifies)
            {
                if (verify.valid)
                {
                    role.Add(verify.verifyType);
                }
            }
            claims.Add(new Claim(JwtClaimTypes.Role, string.Join(',', role)));
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, JwtClaimTypes.Name, JwtClaimTypes.Id);
            identity.AddClaims(claims);
            var princial = new ClaimsPrincipal(identity);
            var properties = new AuthenticationProperties();
            var ticket = new AuthenticationTicket(princial, properties, "CookieScheme");
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, princial);
        }
        public IActionResult Index(string ReturnUrl)
        {
            string redirect_uri = ReturnUrl;
            if (User.Identity.IsAuthenticated)
            {
                return Redirect(redirect_uri ?? "/");
            }
            string redirect_uri_wx = Request.Scheme + "://" + Request.Host + "/Login/WXAuth?redirect_uri=" +
                Uri.EscapeDataString(redirect_uri ?? "/");
            string redirect_uri_qq = Request.Scheme + "://" + Request.Host + "/Login/QQAuth?redirect_uri=" +
                Uri.EscapeDataString(redirect_uri ?? "/");

            string state = Guid.NewGuid().ToString("N");
            RedisHelper.Set("wx_redirect-" + state, redirect_uri_wx, 600);
            RedisHelper.Set("qq_redirect-" + state, redirect_uri_qq, 600);

            if (Request.Headers["User-Agent"].ToString().ToLower().Contains("micromessenger"))
            {
                string appid = AppInfos.Weixin.AppID;
                ViewBag.Login_Uri_WX = WeixinUtil.GetLoginURL(redirect_uri_wx, appid, "snsapi_userinfo", state);
            }
            else
            {
                string appid = AppInfos.Weixin_Web.AppID;
                ViewBag.Login_Uri_WX = WeixinUtil.GetWebLoginUrl(redirect_uri_wx, appid, state);
            }
            ViewBag.Login_Uri_QQ = QQUtil.GetWebLoginURL(redirect_uri_qq, state: state);
            RSAKeyPair keyPair = RSAHelper.GenerateRSAKeyPair();

            ViewBag.PublicKey = keyPair.PublicKey;
            string privateKey = keyPair.PrivateKey;
            Guid _kid = Guid.NewGuid();
            if (RedisHelper.Set(_kid.ToString(), privateKey, 300))
            {
                ViewBag.Kid = _kid.ToString();
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(Guid kid, short nationCode, string mobile, string password, string rnd, string ReturnUrl)
        {
            string redirect_uri = ReturnUrl;
            BLL.LoginBLL loginBLL = new BLL.LoginBLL();
            var userModel = loginBLL.Login(kid, nationCode, mobile, password, rnd, out string failResult);
            if (userModel == null)
            {
                return Json(new Models.RootModel() { status = 1, errorDescription = failResult });
            }
            var verifyInfo = new BLL.VerifyBLL().GetVerifies(userModel.Id);
            await SetSignInCookie(userModel, verifyInfo);
            RedisHelper.Remove(kid.ToString());

            return Json(new { status = 0, redirect_uri });
        }
        public async Task<IActionResult> QQAuth(string code, string state, string redirect_uri)
        {
            if (string.IsNullOrEmpty(state) || string.IsNullOrEmpty(code) || code.Length != 32)
            {
                return Content("第三方登录授权失败");
            }
            string stateData = RedisHelper.Get<string>("qq_redirect-" + state);
            //RedisHelper.Remove("qq_redirect-" + state);
            if (string.IsNullOrEmpty(stateData))
            {
                return Content("第三方登录授权失败");
            }
            string qq_AppID = AppInfos.QQ_Web.AppID;
            string qq_AppName = AppInfos.QQ_Web.AppName;
            string appsecret = AppInfos.QQ_Web.AppSecret;
            string AuthAccessToken = await QQUtil.GetQQAccessToken(qq_AppID, appsecret, code, stateData);
            if (!string.IsNullOrEmpty(AuthAccessToken))
            {
                QQOpenID OpenID = await QQUtil.GetQQOpenID(AuthAccessToken);
                if (!string.IsNullOrEmpty(OpenID.openid))
                {
                    OpenID.unionid = OpenID.unionid.Replace("UID_", "");
                    QQUserInfoResult QQuserinfo = await QQUtil.GetQQUserInfo(AuthAccessToken, qq_AppID, OpenID.openid);
                    if (!User.Identity.IsAuthenticated)
                    {
                        BLL.LoginBLL loginBLL = new BLL.LoginBLL();
                        Models.dbo.UserInfo userInfo = new Models.dbo.UserInfo();
                        if (!string.IsNullOrEmpty(OpenID.unionid))
                        {
                            userInfo.Nickname = QQuserinfo.nickname;
                            userInfo.HeadImgUrl = QQuserinfo.figureurl_qq ?? QQuserinfo.figureurl_qq_2 ?? QQuserinfo.figureurl_qq_1;
                            userInfo.Sex = QQuserinfo.gender == "男" ? 1 : QQuserinfo.gender == "女" ? 0 : (byte?)null;
                        }
                        loginBLL.QQLogin(OpenID.unionid, OpenID.openid, qq_AppName, ref userInfo);
                        var verifyInfo = new BLL.VerifyBLL().GetVerifies(userInfo.Id);
                        await SetSignInCookie(userInfo, verifyInfo);
                    }
                    else
                    {
                        RedisHelper.Set("QQBindInfo-" + User.Identity.Name, OpenID, 300);
                    }
                }
            }
            ViewBag.RedirectUri = redirect_uri ?? "/";
            return View("~/Views/Login/LoginAuth.cshtml");
        }
        public async Task<IActionResult> WXAuth(string code, string state, string redirect_uri)
        {
            if (string.IsNullOrEmpty(state) || string.IsNullOrEmpty(code) || code.Length != 32)
            {
                return Content("第三方登录授权失败");
            }
            string stateData = RedisHelper.Get<string>("wx_redirect-" + state);
            if (string.IsNullOrEmpty(stateData))
            {
                return Content("第三方登录授权失败");
            }
            string weixin_AppID = AppInfos.Weixin_Web.AppID;
            string weixin_AppName = AppInfos.Weixin_Web.AppName;
            string appsecret = AppInfos.Weixin_Web.AppSecret;
            if (Request.Headers["User-Agent"].ToString().ToLower().Contains("micromessenger"))
            {
                weixin_AppID = AppInfos.Weixin.AppID;
                weixin_AppName = AppInfos.Weixin.AppName;
                appsecret = AppInfos.Weixin.AppSecret;
            }
            WXAccessToken AuthAccessToken = await WeixinUtil.GetWeiXinAccessToken(weixin_AppID, appsecret, code);
            if (!string.IsNullOrEmpty(AuthAccessToken.openid))
            {
                WXUserInfoResult WXuserinfo = await WeixinUtil.GetWeiXinUserInfo(AuthAccessToken.access_token, AuthAccessToken.openid);
                if (!User.Identity.IsAuthenticated)
                {
                    BLL.LoginBLL loginBLL = new BLL.LoginBLL();
                    Models.dbo.UserInfo userInfo = new Models.dbo.UserInfo();
                    if (!string.IsNullOrEmpty(WXuserinfo.unionid))
                    {
                        userInfo.Nickname = WXuserinfo.nickname;
                        userInfo.HeadImgUrl = WXuserinfo.headimgurl;
                        userInfo.Sex = WXuserinfo.sex == "1" ? 1 : WXuserinfo.sex == "2" ? 0 : (byte?)null;
                    }
                    loginBLL.WXLogin(WXuserinfo.unionid, AuthAccessToken.openid, weixin_AppName, ref userInfo);
                    var verifyInfo = new BLL.VerifyBLL().GetVerifies(userInfo.Id);
                    await SetSignInCookie(userInfo, verifyInfo);
                }
                else
                {
                    RedisHelper.Set("WXBindInfo-" + User.Identity.Name, WXuserinfo, 300);
                }
            }
            ViewBag.RedirectUri = redirect_uri ?? "/";
            return View("~/Views/Login/LoginAuth.cshtml");
        }
        [HttpPost]
        public IActionResult GetRndCode(string mobile, short nationCode=86, string codeType= "RegistOrLogin")
        {
            Models.RootModel json = new Models.RootModel();
            if (string.IsNullOrEmpty(mobile))
            {
                json.status = 1;
                json.errorDescription = "参数错误";
                return Json(json);
            }
            if (User.Identity.IsAuthenticated && mobile == "self")
            {
                var info = new BLL.AccountBLL().GetUserInfo(userID);
                if (!string.IsNullOrEmpty(info.Mobile))
                {
                    nationCode = info.NationCode.Value;
                    mobile = info.Mobile;
                }
            }
            if (!CommonHelper.isMobile(mobile))
            {
                json.status = 1;
                json.errorDescription = "请输入有效的手机号码";
                return Json(json);
            }
            SMSHelper sMSHelper = new SMSHelper();
            bool sendStatus = sMSHelper.SendRndCode(mobile, codeType, nationCode, out string failReason);
            if (!sendStatus)
            {
                json.errorDescription = failReason;
            }
            return Json(json);
        }
        public IActionResult ForgetPSW()
        {
            RSAKeyPair keyPair = RSAHelper.GenerateRSAKeyPair();
            ViewBag.PublicKey = keyPair.PublicKey;
            string privateKey = keyPair.PrivateKey;
            Guid _kid = Guid.NewGuid();
            if (RedisHelper.Set(_kid.ToString(), privateKey, 300))
            {
                ViewBag.Kid = _kid.ToString();
            }
            return View();
        }
        [HttpPost]
        public IActionResult ForgetPSW(Guid kid, short nationCode, string mobile, string password, string rnd)
        {
            Models.RootModel json = new Models.RootModel();
            BLL.AccountBLL accountBLL = new BLL.AccountBLL();
            if(!accountBLL.ChangePSW(kid, nationCode, mobile, password, rnd, out string failResult, true))
            {
                json.status = 1;
                json.errorDescription = failResult;
            }
            return Json(json);
        }
        [HttpPost]
        public ActionResult GetFGRndCode(string imgrnd, string mobile, short nationCode = 86, string codeType = "ChangePSW")
        {
            Models.RootModel json = new Models.RootModel();
            if (string.IsNullOrEmpty(mobile) || string.IsNullOrEmpty(imgrnd))
            {
                json.status = 1;
                json.errorDescription = "参数错误";
                return Json(json);
            }
            Guid.TryParse(Request.Cookies["verifycode"], out Guid ImgRndCodeID);
            if (!VerifyCodeHelper.Check(ImgRndCodeID, imgrnd, false))
            {
                json.status = 1;
                json.errorDescription = "图形验证码错误";
                return Json(json);
            }
            if (!CommonHelper.isMobile(mobile))
            {
                json.status = 1;
                json.errorDescription = "请输入有效的手机号码";
                return Json(json);
            }
            SMSHelper sMSHelper = new SMSHelper();
            bool sendStatus = sMSHelper.SendRndCode(mobile, codeType, nationCode, out string failReason);
            if (!sendStatus)
            {
                json.errorDescription = failReason;
            }
            return Json(json);
        }
    }
}