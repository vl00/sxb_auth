using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSchool.Sites.UserCenter.Library;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace iSchool.Sites.UserCenter.Controllers
{
    public class AccountController : Base
    {
        protected BLL.AccountBLL accountBLL = new BLL.AccountBLL();
        public Models.AppInfos AppInfos { get; }
        public AccountController(IOptions<Models.AppInfos> _appInfos)
        {
            AppInfos = _appInfos.Value;
        }
        public IActionResult Index()
        {
            return View(accountBLL.GetBindInfo(userID));
        }
        public IActionResult ChangePSW()
        {
            var info = accountBLL.GetUserInfo(userID);
            if (string.IsNullOrEmpty(info.Mobile))
            {
                return View("~/Views/Shared/Prompt.cshtml", ("请先绑定手机号", Url.Action("BindMobile")));
            }
            RSAKeyPair keyPair = RSAHelper.GenerateRSAKeyPair();
            ViewBag.PublicKey = keyPair.PublicKey;
            string privateKey = keyPair.PrivateKey;
            Guid _kid = Guid.NewGuid();
            if (RedisHelper.Set(_kid.ToString(), privateKey, 300))
            {
                ViewBag.Kid = _kid.ToString();
            }
            return View(info);
        }
        [HttpPost]
        public IActionResult ChangePSW(Guid kid, string password, string rnd)
        {
            var info = accountBLL.GetUserInfo(userID);
            Models.RootModel json = new Models.RootModel();
            if (string.IsNullOrEmpty(info.Mobile))
            {
                json.status = 1;
                json.errorDescription = "请先绑定手机号";
            }
            else if(!accountBLL.ChangePSW(kid, info.NationCode.Value, info.Mobile, password, rnd, out string failResult))
            {
                json.status = 1;
                json.errorDescription = failResult;
            }
            return Json(json);
        }
        /// <summary>
        /// 修改手机号
        /// </summary>
        /// <returns></returns>
        public IActionResult BindMobile()
        {
            var info = accountBLL.GetUserInfo(userID);
            RSAKeyPair keyPair = RSAHelper.GenerateRSAKeyPair();
            ViewBag.PublicKey = keyPair.PublicKey;
            string privateKey = keyPair.PrivateKey;
            Guid _kid = Guid.NewGuid();
            if (RedisHelper.Set(_kid.ToString(), privateKey, 300))
            {
                ViewBag.Kid = _kid.ToString();
            }
            return View(info);
        }
        [HttpPost]
        public IActionResult BindMobile(Guid kid, string mobile_o, string mobile_n, string rnd, short nationCode_o=86, short nationCode_n=86)
        {
            Models.RootModel json = new Models.RootModel();
            
            if (!accountBLL.BindMobile(userID, kid, nationCode_o, mobile_o, nationCode_n, mobile_n, rnd, out string failResult))
            {
                json.status = 1;
                json.errorDescription = failResult;
            }
            return Json(json);
        }
        /// <summary>
        /// 账号冲突详情
        /// </summary>
        /// <param name="kid"></param>
        /// <param name="data"></param>
        /// <param name="rnd"></param>
        /// <param name="dataType">0:手机号/1:微信/2:QQ</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Conflict(string data, string rnd, byte dataType, Guid kid = new Guid(), short nationCode=86)
        {
            var myInfo = accountBLL.GetUserInfo(userID);
            var a = accountBLL.ListConflict(userID, kid, nationCode, ref data, rnd, dataType);
            if (a.result)
            {
                return View((myInfo, a.list, data, dataType));
            }
            else
            {
                return View("~/Views/Shared/Prompt.cshtml", (a.errorDescription, Url.Action("Index")));
            }
        }
        public IActionResult BindWX(bool confirm = false)
        {
            var WXuserinfo = RedisHelper.Get<WXUserInfoResult>("WXBindInfo-" + User.Identity.Name);
            if (WXuserinfo == null)
            {
                string redirect_uri = Request.Scheme + "://" + Request.Host + "/Login/WXAuth?redirect_uri=" +
                    Uri.EscapeDataString(Url.Action() ?? "/");
                string state = Guid.NewGuid().ToString("N");
                RedisHelper.Set("wx_redirect-" + state, redirect_uri, 600);
                if (Request.Headers["User-Agent"].ToString().ToLower().Contains("micromessenger"))
                {
                    return Redirect(WeixinUtil.GetLoginURL(redirect_uri, AppInfos.Weixin.AppID, "snsapi_userinfo", state));
                }
                else
                {
                    return Redirect(WeixinUtil.GetWebLoginUrl(redirect_uri, AppInfos.Weixin_Web.AppID, state));
                }
            }
            else if (accountBLL.BindWX(userID, WXuserinfo.unionid, WXuserinfo.openid, confirm))
            {
                return RedirectToAction("Index");
            }
            return Bind(1);
        }
        public IActionResult BindQQ(bool confirm = false)
        {
            var OpenIDInfo = RedisHelper.Get<QQOpenID>("QQBindInfo-" + User.Identity.Name);
            if (OpenIDInfo == null)
            {
                string redirect_uri = Request.Scheme + "://" + Request.Host + "/Login/QQAuth?redirect_uri=" +
                    Uri.EscapeDataString(Url.Action() ?? "/");

                string state = Guid.NewGuid().ToString("N");
                RedisHelper.Set("qq_redirect-" + state, redirect_uri, 600);
                return Redirect(QQUtil.GetWebLoginURL(redirect_uri, state: state));
            }
            else if (accountBLL.BindQQ(userID, Guid.Parse(OpenIDInfo.unionid), Guid.Parse(OpenIDInfo.openid), confirm))
            {
                return RedirectToAction("Index");
            }
            return Bind(2);
        }
        public IActionResult Bind(byte dataType, bool confirm = false)
        {
            if (!confirm)
            {
                return View("~/Views/Account/Bind.cshtml", dataType);
            }
            else
            {
                if (dataType == 2)
                {
                    return BindQQ(true);
                }
                else if (dataType == 1)
                {
                    return BindWX(true);
                }
                return RedirectToAction("Index");
            }
        }
        public async Task<IActionResult> Logout(string redirect_uri)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect(redirect_uri ?? "/");
        }
        /// <summary>
        /// 注销账号
        /// </summary>
        /// <returns></returns>
        public IActionResult Destroy()
        {
            return new EmptyResult();
        }
    }
}