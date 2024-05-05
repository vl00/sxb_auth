using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Sites.UserCenter.Controllers
{
    public class CollectionController : Base
    {
        private BLL.CollectionBLL collectionBLL = new BLL.CollectionBLL();
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
            return View(collectionBLL.GetSchoolCollection(userID, page:page));
        }
        public IActionResult Article()
        {
            return View();
        }
        public IActionResult AddCollection(Guid dataID, byte dataType, string callback)
        {
            Models.RootModel json = new Models.RootModel();
            if(!collectionBLL.AddCollection(userID, dataID, dataType))
            {
                json.status = 1;
                json.errorDescription = "添加失败";
            }
            return Jsonp(json, callback);
        }
        public IActionResult RemoveCollection(Guid dataID, string callback)
        {
            
            Models.RootModel json = new Models.RootModel();
            if (!collectionBLL.RemoveCollection(userID, dataID))
            {
                json.status = 1;
                json.errorDescription = "删除失败";
            }
            return Jsonp(json, callback);
        }
        
        [AllowAnonymous]
        public IActionResult IsCollected(Guid dataID, string callback, Guid userID=new Guid())
        {
            userID = userID == Guid.Empty ? base.userID : userID;
            if (string.IsNullOrEmpty(callback))
            {
                return Json(new { status = 0, iscollected = collectionBLL.IsCollected(userID, dataID), userID });
            }
            else
            {
                return Jsonp(new { status = 0, iscollected = collectionBLL.IsCollected(userID, dataID) , userID}, callback);
            }
        }
    }
}