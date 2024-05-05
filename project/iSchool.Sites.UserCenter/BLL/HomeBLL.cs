using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.BLL
{
    public class HomeBLL : AccountBLL
    {
        public Models.ViewsModels.Home.Index GetUserCenterHomeInfo(Guid userID)
        {
            var userInfo = GetUserInfo(userID);
            Models.ViewsModels.Home.Index info = new Models.ViewsModels.Home.Index() {
                nickname = userInfo.Nickname,
                headImgUrl = userInfo.HeadImgUrl
            };
            return info;
        }
    }
}
