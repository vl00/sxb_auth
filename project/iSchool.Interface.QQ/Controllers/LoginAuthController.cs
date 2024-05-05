using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace iSchool.Interface.QQ.Controllers
{
    public class LoginAuthController : Controller
    {
        List<string> domainList { get; set; }
        public LoginAuthController(IOptions<List<string>> _domainList)
        {
            domainList = _domainList.Value;
        }
        public IActionResult Index(string redirect_uri, string code, string state)
        {
            Uri uri = new Uri(redirect_uri);
            if (domainList.Contains(uri.Host.ToLower()))
            {
                return Redirect(uri.ToString() + (string.IsNullOrEmpty(uri.Query) ? "?" : "&") + "code=" + code + "&state=" + state);
            }
            return new ForbidResult();
        }
    }
}