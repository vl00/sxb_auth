using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace iSchool.Sites.Auth.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
        {

        }
        public BaseController(IConfiguration config)
        {
            configuration = config;
        }
        protected readonly IConfiguration configuration;
        protected string connectionString;
        protected System.Data.IDbConnection connection;
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            connectionString = configuration.GetConnectionString("iSchool");
            connection = new System.Data.SqlClient.SqlConnection(connectionString);
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                //ViewData["adminInfo"] = new Authorization.Account().Info(context.HttpContext);
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