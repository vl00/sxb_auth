using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Models.dbo
{
    public class UserInfo
    {
        public Guid Id { get; set; }
        public short? NationCode { get; set; }
        public string Mobile { get; set; }
        public Guid? Password { get; set; }
        public string Nickname { get; set; }
        public DateTime RegTime { get; set; }
        public DateTime LoginTime { get; set; }
        public bool Blockage { get; set; }
        public string HeadImgUrl { get; set; } = "https://cdn.sxkid.com/images/head.png";
        public byte? Sex { get; set; }
        public int? City { get; set; }
        public string Channel { get; set; }
    }
}
