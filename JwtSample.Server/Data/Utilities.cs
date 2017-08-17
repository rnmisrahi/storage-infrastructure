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
            new Educator { EducatorId="RubenMisrahi", Role = "admin" }
        };


        public static string generateToken(string educatorId)
        {
            var identity = GetIdentity(educatorId);
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
        /// <param name="educatorId"></param>
        /// <returns></returns>
        public static ClaimsIdentity GetIdentity(string educatorId)
        {
            Educator educator = educators.FirstOrDefault(x => x.EducatorId == educatorId);
            if (educator == null)
            {
                educator = new Educator { EducatorId = educatorId, Role = "User" };
            }
            var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, educator.EducatorId),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, educator.Role)
                };
            ClaimsIdentity claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;

            // if the user cannot be found and could not be found or added
            throw new Exception(educatorId + " Could NOT be found or added");
        }
    }


}

