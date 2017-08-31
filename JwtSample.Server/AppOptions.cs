using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Server
{
    public class AppOptions
    {
        public string JwtApiKey { get; set; }
        public string SuperUser { get; set; }
        public string Issuer { get; set; }
    }
}
