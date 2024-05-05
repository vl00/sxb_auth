using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Sites.UserCenter.Controllers
{
    public class InfoController : Base
    {
        protected BLL.AccountBLL accountBLL = new BLL.AccountBLL();
        public IActionResult Index()
        {
            Models.ViewsModels.Info.Index model = new Models.ViewsModels.Info.Index();
            model.UserInfo = accountBLL.GetUserInfo(userID);
            model.Interest = accountBLL.GetUserInterest(userID);
            return View(model);
        }
        public IActionResult UpdateNickname(string nickname)
        {
            Models.RootModel json = new Models.RootModel();
            var info = accountBLL.GetUserInfo(userID);
            info.Nickname = nickname;
            if (!accountBLL.UpdateUserInfo(info))
            {
                json.status = 1;
                json.errorDescription = "更新失败";
            }
            return Json(json);
        }
        public IActionResult UploadHeadImg(List<IFormFile> files)
        {
            Stream fileStream;
            string ext;
            
            var info = accountBLL.GetUserInfo(userID);
            Models.FileUploadResponseModel result = new Models.FileUploadResponseModel();
            if (files.Count > 0)
            {
                fileStream = files[0].OpenReadStream();
                ext = Path.GetExtension(files[0].FileName);
            }
            else if (Request.Form.Files.Count > 0)
            {
                fileStream = Request.Form.Files[0].OpenReadStream();
                ext = Path.GetExtension(Request.Form.Files[0].FileName);
            }
            else
            {
                result.status = 1;
                result.errorDescription = "没有文件";
                return Json(result);
            }
            result = Library.HttpHelper.HttpPost<Models.FileUploadResponseModel>("https://file.sxkid.com/Upload/UserHeadImg?filename=" + User.Identity.Name + "/" + Guid.NewGuid() + ext
                , fileStream);
            info.HeadImgUrl = result.cdnUrl;
            if (!accountBLL.UpdateUserInfo(info))
            {
                return Json(new Models.RootModel() { status = 1, errorDescription = "提交失败" });
            }
            return Json(result);
        }
        public IActionResult SetUserInterest(Models.dbo.Interest interest)
        {
            Models.RootModel json = new Models.RootModel();
            interest.UserID = userID;
            if (!accountBLL.SetUserInterest(interest))
            {
                json.status = 1;
                json.errorDescription = "修改失败";
            }
            return Json(json);
        }
    }
}