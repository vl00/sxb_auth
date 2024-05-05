using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UEditor.Utitlies
{
    /// <summary>
    /// Config 的摘要说明
    /// </summary>
    public class ConfigHandler : Handler
    {
        public ConfigHandler(HttpContext context) : base(context) { }

        public override IActionResult Process
        {
            get
            {
                Config.configPath = Request.Query["configPath"];
                return Content(Config.Items.ToString());
            }
        }
    }
}