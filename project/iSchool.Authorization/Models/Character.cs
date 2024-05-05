using System;
using System.Collections.Generic;

namespace iSchool.Authorization.Models
{
    public partial class Character
    {
        public Character()
        {
            AdminInfo = new HashSet<AdminInfo>();
            Function = new HashSet<Function>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Time { get; set; }

        public virtual ICollection<AdminInfo> AdminInfo { get; set; }
        public virtual ICollection<Function> Function { get; set; }
    }
}
