using System;
using System.Collections.Generic;

namespace iSchool.Authorization.Models
{
    public partial class CharacterFunction
    {
        public Guid CharacterId { get; set; }
        public Guid FunctionId { get; set; }

        public virtual Character Character { get; set; }
        public virtual Function Function { get; set; }
    }
}
