using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Sites.UserCenter.Controllers
{
    public class MessageController : Base
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Private()
        {
            return View();
        }
        public IActionResult Follow()
        {
            return View();
        }
        public IActionResult System()
        {
            return View();
        }
    }
}