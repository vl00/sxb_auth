using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Sites.UserCenter.Controllers
{
    public class SettingController : Base
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult NationCodeSelect()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult VerifyCode()
        {
            if(Guid.TryParse(Request.Cookies["verifycode"], out Guid OcodeID))
            {
                Library.RedisHelper.Remove("verifyCode-" + OcodeID);
            }
            System.IO.MemoryStream ms = Library.VerifyCodeHelper.Create(out Guid codeID, out string code, Library.VerifyCodeHelper.VerifyCodeType.Number);
            Response.Body.Dispose();
            Response.Cookies.Append("verifycode", codeID.ToString(), new Microsoft.AspNetCore.Http.CookieOptions() { Expires = DateTime.Now.AddMinutes(5), Path = "/" });
            return File(ms.ToArray(), @"image/png");
        }
        [AllowAnonymous]
        public IActionResult CheckVerifyCode(string code)
        {
            Guid.TryParse(Request.Cookies["verifycode"], out Guid codeID);
            return Json(new { status = !codeID.Equals(Guid.Empty) && Library.VerifyCodeHelper.Check(codeID, code, false) ? 0 : 1 });
        }
    }
}