using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Controllers
{
    public class Base : Controller
    {
        protected Guid userID { get; set; }
        protected double? latitude { get; set; }
        protected double? longitude { get; set; }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.FileServer = "https://file.sxkid.com";
            ViewBag.V4FileServer = "https://file.sxkid.com/v4source";
            if (User.Identity.IsAuthenticated)
            {
                var id = User.Identity as ClaimsIdentity;
                var claim = id.FindFirst(JwtClaimTypes.Id);
                userID = Guid.Parse(claim.Value);
            }
            context.HttpContext.Request.Cookies.TryGetValue("latitude", out string _lat);
            context.HttpContext.Request.Cookies.TryGetValue("longitude", out string _lng);
            if(double.TryParse(_lat, out double _latitude))
            {
                latitude = _latitude;
            }
            if (double.TryParse(_lng, out double _longitude))
            {
                longitude = _longitude;
            }
        }
        protected new ContentResult Json(object obj)
        {
            Newtonsoft.Json.JsonSerializerSettings js = new Newtonsoft.Json.JsonSerializerSettings();
            js.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            return new ContentResult() { Content = Newtonsoft.Json.JsonConvert.SerializeObject(obj, js), ContentType = "text/json", StatusCode = 200 };
        }
        protected ContentResult Jsonp(object obj, string callback)
        {
            Newtonsoft.Json.JsonSerializerSettings js = new Newtonsoft.Json.JsonSerializerSettings();
            js.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            return new ContentResult() { Content = (callback ?? "callback") + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(obj, js) + ");", ContentType = "text/html", StatusCode = 200 };
        }
        public static bool IsAjaxRequest(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.Headers != null)
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";

            return false;
        }
    }

    public class AuthorizeAttribute : ActionFilterAttribute
    {
        readonly bool throwIfNoQx;

        public AuthorizeAttribute() : this(false) { }

        public AuthorizeAttribute(bool throwIfNoQx)
        {
            this.throwIfNoQx = throwIfNoQx;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // [AllowAnonymous] or IGoThroughMvcFilter
            if (context.ActionDescriptor.FilterDescriptors.Any(a =>
            {
                var fty = a.Filter.GetType();
                return fty == typeof(Microsoft.AspNetCore.Mvc.Authorization.AllowAnonymousFilter);
            }))
            {
                //业务逻辑
                base.OnActionExecuting(context);
                return;
            }

            // 已登录用户
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                //业务逻辑
                base.OnActionExecuting(context);
                return;
            }

            ///
            /// 未登录
            /// 
            if (IsJsonpRequest(context.HttpContext.Request))
            {
                var url = $"/Login?ReturnUrl={Uri.EscapeDataString(context.HttpContext.Request.Headers["Referer"])}";
                var callback = context.HttpContext.Request.Query["callback"][0];
                Newtonsoft.Json.JsonSerializerSettings js = new Newtonsoft.Json.JsonSerializerSettings();
                js.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                context.Result = new ContentResult() { Content = (callback ?? "callback") + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(
                    new { status = 2, errorDescription = "未登录", url }, js) + ");", ContentType = "text/html", StatusCode = 200 };
            }
            else if (IsAjaxRequest(context.HttpContext.Request))
            {
                var url = $"/Login?ReturnUrl={Uri.EscapeDataString(context.HttpContext.Request.Headers["Referer"])}";
                context.Result = new JsonResult(new { status = 2, errorDescription = "未登录", url });
                //context.Result = new UnauthorizedObjectResult(url);
            }
            else
            {
                string url = "//" + context.HttpContext.Request.Host + context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectResult("/Login?ReturnUrl=" + Uri.EscapeDataString(url));
            }
        }

        /// <summary>
        /// from https://stackoverflow.com/questions/29282190/where-is-request-isajaxrequest-in-asp-net-core-mvc
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsAjaxRequest(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.Headers != null)
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";

            return false;
        }
        public static bool IsJsonpRequest(HttpRequest request)
        {
            return request.Query["callback"].Count > 0;
        }
    }
}
