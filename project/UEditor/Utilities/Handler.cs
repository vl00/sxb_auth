using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


namespace UEditor
{
    /// <summary>
    /// Handler 的摘要说明
    /// </summary>
    public abstract class Handler : Controller
    {
        public Handler(HttpContext context)
        {
            this.Request = context.Request;
            this.Response = context.Response;
            this.Context = context;
            //this.Server = context.Server;
        }

        public abstract IActionResult Process { get; }

        protected IActionResult WriteJson(object response)
        {
            string jsonpCallback = Request.Query["callback"];
            if (string.IsNullOrWhiteSpace(jsonpCallback))
            {
                return Json(response);
            }
            else
            {
                string json = JsonConvert.SerializeObject(response);
                Response.Headers.Add("Content-Type", "application/javascript");
                return Content(string.Format("{0}({1});", jsonpCallback, json));
            }
        }

        public HttpRequest Request { get; private set; }
        public HttpResponse Response { get; private set; }
        public HttpContext Context { get; private set; }
        //public HttpServerUtility Server { get; private set; }
    }
}