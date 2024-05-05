using System;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Enyim.Caching.Memcached;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Http;

namespace iSchool.Authorization
{
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
                return fty == typeof(Microsoft.AspNetCore.Mvc.Authorization.AllowAnonymousFilter) ||
                    typeof(IGoThroughMvcFilter).IsAssignableFrom(fty);
            }))
            {
                //业务逻辑
                base.OnActionExecuting(context);
                return;
            }

            // 已登录用户
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                Permission permission = new Permission();
                if (permission.Check(context))
                {
                    //业务逻辑
                    base.OnActionExecuting(context);
                    return;
                }
                else
                {
					if (throwIfNoQx)
                    {
                        throw new NoPermissionException("没有权限", 403);
                    }
                    else if (context.HttpContext.Request.Headers["x-requested-with"] == "XMLHttpRequest")
                    {
                        context.Result = new JsonResult(new { status = 403, errorDescription = "没有权限" });
                    }
                    else
                    {
                        ContentResult result = new ContentResult();
                        result.Content = "没有权限";
                        context.Result = result;
                    }
                    return;
                }
            }
            
            ///
            /// 未登录
            /// 

            if (IsAjaxRequest(context.HttpContext.Request))
            {
                var url = $"//auth.sxkid.com/Account/Login?redirect_uri={Uri.EscapeDataString(context.HttpContext.Request.Headers["Referer"])}";

                if (throwIfNoQx)
                {
                    throw new NoLoginException(url);
                }

                //context.Result = new JsonResult(new { status = 401, errorDescription = "未登录", url });
                context.Result = new UnauthorizedObjectResult(url);
            }
            else
            {
                string url = "//" + context.HttpContext.Request.Host + context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;

                //if (throwIfNoQx)
                //{
                //    throw new NoLoginException("//auth.sxkid.com/Account/Login?redirect_uri=" + Uri.EscapeDataString(url));
                //}

                context.Result = new RedirectResult("//auth.sxkid.com/Account/Login?redirect_uri=" + Uri.EscapeDataString(url));
            }
        }

        /// <summary>
        /// from https://stackoverflow.com/questions/29282190/where-is-request-isajaxrequest-in-asp-net-core-mvc
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        static bool IsAjaxRequest(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.Headers != null)
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";

            return false;
        }
    }
}
