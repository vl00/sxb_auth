using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Models.dbo
{
    public class Interest
    {
        public Guid UserID { get; set; }
        public bool Type_1 { get; set; }
        public bool Type_2 { get; set; }
        public bool Type_3 { get; set; }
        public bool Type_4 { get; set; }
        public bool Nature_1 { get; set; }
        public bool Nature_2 { get; set; }
        public bool Nature_3 { get; set; }
        public bool Lodging_0 { get; set; }
        public bool Lodging_1 { get; set; }
    }
}
