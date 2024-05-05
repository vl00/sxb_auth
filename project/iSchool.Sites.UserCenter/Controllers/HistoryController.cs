using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Sites.UserCenter.Controllers
{
    public class HistoryController : Base
    {

        private BLL.HistoryBLL historyBLL = new BLL.HistoryBLL();
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
        public IActionResult School(int page=1)
        {
            return View(historyBLL.GetSchoolHistory(userID, page: page));
        }
        public IActionResult Article()
        {
            return View();
        }
        public IActionResult AddHistory(Guid dataID, byte dataType)
        {
            Models.RootModel json = new Models.RootModel();
            if (!historyBLL.AddHistory(userID, dataID, dataType))
            {
                json.status = 1;
                json.errorDescription = "添加失败";
            }
            return Json(json);
        }
    }
}