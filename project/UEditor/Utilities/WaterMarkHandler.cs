using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UEditor.Utitlies
{
    public class WaterMarkHandler// : Handler
    {
        private string Sources;
        private Crawler Crawler;
        //public WaterMarkHandler(HttpContext context) : base(context) { }

        //public override ActionResult Process
        //{
        //    get
        //    {
        //        //Sources = Request.Form["upfile"];
        //        //if (!Guid.TryParse(Request.QueryString["contentID"], out Guid contentID) || Sources == null || Sources.Length == 0)
        //        //{
        //        //    return Json(new
        //        //    {
        //        //        state = "参数错误：没有指定抓取源"
        //        //    }, JsonRequestBehavior.AllowGet);
        //        //}
        //        //bool.TryParse(Request.QueryString["watermark"], out bool watermark);
        //        //Crawler = new Crawler(Sources, Server).Fetch(watermark, contentID, Request.Form["DataUrl"]);
        //        //return Json(new
        //        //{
        //        //    state = Crawler.State,
        //        //    source = Crawler.SourceUrl,
        //        //    url = Crawler.ServerUrl
        //        //}, JsonRequestBehavior.AllowGet);
        //    }
        //}
    }
}