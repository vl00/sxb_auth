using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace iSchool.Console.Auth.Controllers
{
    public class BaseController : Controller
    {
        protected const string connectionString = "Data Source=10.1.0.16;Initial Catalog=iSchoolConsole;User ID=iSchool;password=SxbLucas$0769;";
        protected System.Data.IDbConnection connection = new System.Data.SqlClient.SqlConnection(connectionString);
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                ViewData["adminInfo"] = new Authorization.Account().Info(context.HttpContext);
            }
            base.OnActionExecuting(context);
        }
        protected string QueryStringRemoveKey(string key)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(key + "=([^&]*)(&*)");
            return reg.Replace(Request.QueryString.ToString(), "");
        }
    }
}