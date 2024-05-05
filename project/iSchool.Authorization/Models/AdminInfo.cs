using System;
using System.Collections.Generic;

namespace iSchool.Authorization.Models
{
    [Serializable]
    public partial class AdminInfo
    {
        public AdminInfo()
        {
            Character = new HashSet<Character>();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Displayname { get; set; }
        public string Password { get; set; }
        public DateTime? RegTime { get; set; }
        public DateTime? LoginTime { get; set; }
        public DateTime? ActiveTime { get; set; }
        public bool Ad { get; set; }
        public bool Rspw { get; set; }
        public virtual ICollection<Character> Character { get; set; }
    }
}
