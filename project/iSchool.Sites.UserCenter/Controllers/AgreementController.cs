﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Sites.UserCenter.Controllers
{
    [AllowAnonymous]
    public class AgreementController : Base
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}