﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Sites.UserCenter.Controllers
{
    public class PublishController : Base
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Comment()
        {
            return View();
        }
        public IActionResult QA()
        {
            return View();
        }
    }
}