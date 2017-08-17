using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace JwtSample.Server
{
    public class AuthOptions
    {
        public const string Issuer = "MyAuthServer"; // issuer of the token
        public const string Audience = "http://localhost:51884/"; // consumer of the token
        const string Key = "mysupersecret_secretkey!123";   // Encryption key //TODO In Production, this needs to be in configuration
        public const int LifeTime = 1; // Token's life time (1 minute)
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
        }
    }
}
