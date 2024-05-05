using System;
using System.Collections.Generic;

namespace iSchool.Authorization.Models
{
    public partial class CharacterQuery
    {
        public Guid CharacterId { get; set; }
        public Guid QueryId { get; set; }

        public virtual Character Character { get; set; }
        public virtual Query Query { get; set; }
    }
}
