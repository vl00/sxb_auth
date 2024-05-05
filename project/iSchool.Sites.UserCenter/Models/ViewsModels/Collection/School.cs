using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Models.ViewsModels.Collection
{
    /// <summary>
    /// 学校类型
    /// </summary>
    public enum SchoolType
    {
        [Description("公办")]
        Public = 1,
        [Description("民办")]
        Private = 2,
        [Description("国际")]
        International = 3,
        [Description("港澳台")]
        SAR = 80,
        [Description("其它")]
        Other = 99

    }
    public class School
    {
        public Guid Sid { get; set; }
        public Guid ExtId { get; set; }
        public byte Grade { get; set; }
        public SchoolType Type { get; set; }
        public string SchoolName { get; set; }
        public string ExtName { get; set; }
        public double? Tuition { get; set; }
        public bool? Lodging { get; set; }
        public int? Score { get; set; }
        public List<string> Tags { get; set; }
        public int? City { get; set; }
        public int? Area { get; set; }
        public int? Province { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }

        ////扩展字段
        public string CityName { get; set; }
        public string AreaName { get; set; }
        public string Distance { get; set; }
    }
}
