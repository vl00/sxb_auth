using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Models.ViewsModels.Info
{
    public class Index
    {
        public dbo.UserInfo UserInfo { get; set; }
        public dbo.Interest Interest { get; set; }
    }
}
