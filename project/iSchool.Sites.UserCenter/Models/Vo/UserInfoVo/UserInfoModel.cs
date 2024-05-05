using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Models.Vo.UserInfoVo
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public class UserInfoModel
    {
        private string QueryImage { get; set; } = "";
        public UserInfoModel(string queryImage)
        {
            QueryImage = queryImage;
        }
        public UserInfoModel()
        {
        }

        public Guid Id { get; set; }
        public string NickName { get; set; }
        public List<int> Role { get; set; }
        public string HeadImager { get { return QueryImage + HeadImager; } set { HeadImager = value; } }

    }
}
