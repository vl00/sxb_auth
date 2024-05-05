using System;
using System.Collections.Generic;

namespace iSchool.Authorization.Models
{
    public partial class AdminCharacter
    {
        public Guid AdminId { get; set; }
        public Guid CharacterId { get; set; }

        public virtual AdminInfo AdminInfo { get; set; }
        public virtual Character Character { get; set; }
    }
}
