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

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Birthday { get; set; }
        public string email { get; set; }
        public string Gender { get; set; }
        public string Language1 { get; set; }
        public string Language2 { get; set; }
        public string Language3 { get; set; }
    }
}
