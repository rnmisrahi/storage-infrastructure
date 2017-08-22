using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Server.Models
{
    public class UserLogin
    {
        public string token { get; set; }
        public string facebookId { get; set; }
        public bool sugnedUp { get; set; }
    }
}
