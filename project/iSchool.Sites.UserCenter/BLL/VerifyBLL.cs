using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.BLL
{
    public class VerifyBLL : AccountBLL
    {
        public bool IsClearHeadImg(Guid userID, string url)
        {
            var img = Image.FromFile(url);
            var pItems = img.PropertyItems;//将"其他"信息过滤掉
            return true;
        }
        public bool IsBindMobile(Guid userID)
        {
            return !string.IsNullOrEmpty(accountDAL.GetBindMobile(userID));
        }
        public int ReplyCount()
        {
            return 0;
        }
        public List<Models.dbo.Verify> GetVerifies(Guid userID)
        {
            return accountDAL.GetVerifies(userID);
        }
    }
}
