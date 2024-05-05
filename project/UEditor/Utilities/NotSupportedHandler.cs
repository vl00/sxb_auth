using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UEditor.Utitlies
{
    /// <summary>
    /// NotSupportedHandler 的摘要说明
    /// </summary>
    public class NotSupportedHandler : Handler
    {
        public NotSupportedHandler(HttpContext context)
            : base(context)
        {
        }

        public override IActionResult Process => WriteJson(new
        {
            state = "action 参数为空或者 action 不被支持。"
        });
    }
}