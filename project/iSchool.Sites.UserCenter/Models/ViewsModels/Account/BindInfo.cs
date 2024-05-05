using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Models.ViewsModels.Account
{
    public class BindInfo
    {
        public string Mobile { get; set; }
        public string UnionID_Weixin { get; set; }
        public Guid? UnionID_QQ { get; set; }

    }
}
