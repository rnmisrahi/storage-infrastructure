using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Server.Models
{
    public class Person
    {
        public string FacebookId { get; set; }

        public bool SignedUp { get; set; }
        public string Role { get; set; }
    }
}
