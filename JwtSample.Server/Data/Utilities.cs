using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using JwtSample.Server.Models;
using Microsoft.IdentityModel.Tokens;

namespace JwtSample.Server.Data
{
    public static class Utilities
    {
        public static List<Educator> educators = new List<Educator>
        {
            new Educator { FacebookId="RNMisrahi", Role = "admin" }
        };


        public static string generateToken(string facebookId)
        {
            var identity = GetIdentity(facebookId);
            var now = DateTime.UtcNow;
            // creating JWT-token
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.Issuer,
                    audience: AuthOptions.Audience,
                    notBefore: now,
                    claims: identity.Claims,
                    //expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LifeTime)),
                    expires: DateTime.UtcNow.AddYears(1),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;

        }

        /// <summary>
        /// Here we define who has the right to sign in.
        /// For now we allow anyone
        /// </summary>
        /// <param name="facebookId"></param>
        /// <returns></returns>
        public static ClaimsIdentity GetIdentity(string facebookId)
        {
            Educator educator = educators.FirstOrDefault(x => x.FacebookId == facebookId);
            if (educator == null)
            {
                educator = new Educator { FacebookId = facebookId, Role = "User" };
            }
            var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, educator.FacebookId),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, educator.Role)
                };
            ClaimsIdentity claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;

            // if the user cannot be found and could not be found or added
            throw new Exception(facebookId + " Could NOT be found or added");
        }
    }


}

