using System;
using System.Collections.Generic;

namespace iSchool.Authorization.Models
{
    public partial class Function
    {
        public Function()
        {
            Query = new HashSet<Query>();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public byte PlatformId { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }

        public virtual Platform Platform { get; set; }
        public virtual ICollection<Query> Query { get; set; }
    }
}
