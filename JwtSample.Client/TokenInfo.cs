using System;
using System.Collections.Generic;
using System.Text;

namespace JwtSample.Client
{
    public class TokenInfo
    {
        public string token { get; set; }
        public string username { get; set; }
        public bool signedUp { get; set; }
    }
}
