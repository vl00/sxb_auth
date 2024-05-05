using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Models.dbo
{
    public class Verify
    {
        public Guid Id { get; set; }
        public Guid UserID { get; set; }
        public string RealName { get; set; }
        public byte IdType { get; set; }
        public string IdNumber { get; set; }
        public bool valid { get; set; }
        public DateTime time { get; set; }
        public byte verifyType { get; set; }
    }
}
