using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using iSchool.Sites.UserCenter.Models;
using iSchool.Sites.UserCenter.Library;
using Dapper;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace iSchool.Sites.UserCenter.Controllers
{
    [AllowAnonymous]
    public class HomeController : Base
    {
        private BLL.HomeBLL homeBLL = new BLL.HomeBLL();
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                Models.ViewsModels.Home.Index model = homeBLL.GetUserCenterHomeInfo(userID);
                return View(model);
            }
            else
            {
                Models.ViewsModels.Home.Index model = new Models.ViewsModels.Home.Index();
                return View(model);
            }
        }

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
