using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Sites.UserCenter.Controllers
{
    public class VerifyController : Base
    {
        private BLL.VerifyBLL verifyBLL = new BLL.VerifyBLL();
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Senior()
        {
            return View();
        }
        public IActionResult Official()
        {
            return View();
        }
    }
}