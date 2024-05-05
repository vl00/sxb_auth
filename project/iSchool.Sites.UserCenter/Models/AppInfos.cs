using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Models
{
    public class AppInfos
    {
        public AppInfo Weixin { get; set; }
        public AppInfo Weixin_Web { get; set; }
        public AppInfo QQ_Web { get; set; }
    }

    public class AppInfo
    {
        public string AppName { get; set; }
        public string AppID { get; set; }
        public string AppSecret { get; set; }
    }
}
