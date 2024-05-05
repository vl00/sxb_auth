using System;
using System.Collections.Generic;

namespace iSchool.Authorization.Models
{
    public partial class Query
    {
        public Guid Id { get; set; }
        public Guid FunctionId { get; set; }
        public string Name { get; set; }
        public string Selector { get; set; }
    }
}
