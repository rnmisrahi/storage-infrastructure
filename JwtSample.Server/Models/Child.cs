using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Server.Models
{
    public class Child
    {
        public int ChildId { get; set; }
        public string EducatorId { get; set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public string Image { get; set; }
    }
}
