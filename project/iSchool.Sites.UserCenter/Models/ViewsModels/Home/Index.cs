using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Models.ViewsModels.Home
{
    public class Index: RootModel
    {
        public Index()
        {
            count = new CountData();
        }
        public string nickname { get; set; }
        public string headImgUrl { get; set; }
        public string title { get; set; }
        public CountData count { get; set; }
    }
    public class CountData
    {
        public int publish { get; set; }
        public int reply { get; set; }
        public int follow { get; set; }
        public int like { get; set; }
        public int message { get; set; }
    }
}
