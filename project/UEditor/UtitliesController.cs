using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UEditor.Utitlies;

namespace UEditor
{
    public class UtitliesController : Controller
    {
        private Handler _action = null;
        //private HttpContext context = System.Web.HttpContext.Current;
        public IActionResult config()
        {
            HttpContext context = Request.HttpContext;
            _action = new ConfigHandler(context);
            return _action.Process;
        }
        public IActionResult uploadimage()
        {
            HttpContext context = Request.HttpContext;
            for (int i = 0; i < Request.Form.Files.Count; i++)
            {
                if (string.IsNullOrEmpty(Library.GetFileExtension(Request.Form.Files[i].OpenReadStream())))
                {
                    return Json(new { state = "未知文件类型" });
                }
            }
            _action = new UploadHandler(context, new UploadConfig()
            {
                AllowExtensions = Config.GetStringList("imageAllowFiles"),
                PathFormat = Config.GetString("imagePathFormat"),
                SizeLimit = Config.GetInt("imageMaxSize"),
                UploadFieldName = Config.GetString("imageFieldName")
            });
            return _action.Process;
        }
        public IActionResult uploadscrawl()
        {
            HttpContext context = Request.HttpContext;
            _action = new UploadHandler(context, new UploadConfig()
            {
                AllowExtensions = new string[] { ".png" },
                PathFormat = Config.GetString("scrawlPathFormat"),
                SizeLimit = Config.GetInt("scrawlMaxSize"),
                UploadFieldName = Config.GetString("scrawlFieldName"),
                Base64 = true,
                Base64Filename = "scrawl.png"
            });
            return _action.Process;
        }
        public IActionResult uploadvideo()
        {
            HttpContext context = Request.HttpContext;
            _action = new UploadHandler(context, new UploadConfig()
            {
                AllowExtensions = Config.GetStringList("videoAllowFiles"),
                PathFormat = Config.GetString("videoPathFormat"),
                SizeLimit = Config.GetInt("videoMaxSize"),
                UploadFieldName = Config.GetString("videoFieldName")
            });
            return _action.Process;
        }
        public IActionResult uploadfile()
        {
            HttpContext context = Request.HttpContext;
            _action = new UploadHandler(context, new UploadConfig()
            {
                AllowExtensions = Config.GetStringList("fileAllowFiles"),
                PathFormat = Config.GetString("filePathFormat"),
                SizeLimit = Config.GetInt("fileMaxSize"),
                UploadFieldName = Config.GetString("fileFieldName")
            });
            return _action.Process;
        }
        public IActionResult listimage()
        {
            HttpContext context = Request.HttpContext;
            _action = new ListFileManager(context, Config.GetString("imageManagerListPath"), Config.GetStringList("imageManagerAllowFiles"));
            return _action.Process;
        }
        public IActionResult listfile()
        {
            HttpContext context = Request.HttpContext;
            _action = new ListFileManager(context, Config.GetString("fileManagerListPath"), Config.GetStringList("fileManagerAllowFiles"));
            return _action.Process;
        }
        public IActionResult catchimage()
        {
            HttpContext context = Request.HttpContext;
            _action = new CrawlerHandler(context);
            return _action.Process;
        }
        public IActionResult Index(string action)
        {
            HttpContext context = Request.HttpContext;
            switch (Request.Query["action"])
            {
                default:
                    _action = new NotSupportedHandler(context);
                    break;
            }
            return _action.Process;
        }
    }
}
