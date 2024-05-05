using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using iSchool.Console.Auth.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.DirectoryServices;
using iSchool.Authorization.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using iSchool.Authorization.Lib;
using Enyim.Caching;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace iSchool.Console.Auth.Controllers
{
    public class HomeController : BaseController
    {

        [AllowAnonymous]
        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                string url = "//" + Request.Host + Request.Path + Request.QueryString;
                return Redirect("//auth.sxkid.com/Account/Login?redirect_uri=" + Uri.EscapeDataString(url));
            }
            List<Platform> list = connection.Query<Platform>(@"select [platform].id, [platform].name, [platform].domain from platform
left join [function] on [function].platformID = platform.id
left join character_function on character_function.functionID = [function].id
left join admin_character on admin_character.characterID=character_function.characterID
where admin_character.adminID=@adminID
group by [platform].id, [platform].name, [platform].domain order by id", new { adminID = User.Identity.Name }).ToList();
            connection.Close();
            return View(list);
        }
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
