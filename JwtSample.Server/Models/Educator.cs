using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Server.Models
{
    public class Educator
    {
        public string EducatorId { get; set; }
        public string token { get; set; }

        public bool SignedUp { get; set; }
        public string Role { get; set; }
    }
}
