using JwtSample.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using JwtSample.Server.Data;

namespace JwtSample.Server.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("/login")]
        public async Task Token()
        {
            try
            {
                var
                    educatorId = Request.Form["educatorId"];
                var identity = Utilities.GetIdentity(educatorId);
                Response.ContentType = "application/json";
                if (identity == null)
                {
                    Response.StatusCode = 400;
                    await Response.WriteAsync("Invalid User ID.");
                    return;
                }
                bool mayExist = _context.Educators.Any();
                bool exists = false;
                if (mayExist)
                {
                    string search = educatorId;
                    var q = _context.Educators.Where(x => x.EducatorId == search).FirstOrDefault();

                    //var q = _context.Educators.Find(x => x.EducatorId == "educatorId");
                    exists = (q != null) && (q.EducatorId != null);
                    if (exists) // The following implies that the token was found
                    {
                        var responseExists = new
                        {
                            token = q.token,
                            username = identity.Name,
                            signedUp = true
                        };
                        await Response.WriteAsync(JsonConvert.SerializeObject(responseExists, new JsonSerializerSettings { Formatting = Formatting.Indented }));
                        return;
                    }
                }
                if (!exists)
                { //Doesn't exist. Generate token and add
                    var responseNew = new
                    {
                        token = Utilities.generateToken(educatorId),
                        username = identity.Name,
                        signedUp = true
                    };
                    Educator educator = new Educator { EducatorId = educatorId, token = responseNew.token, SignedUp = true };
                    _context.Educators.Add(educator);
                    _context.SaveChanges();
                    await Response.WriteAsync(JsonConvert.SerializeObject(responseNew, new JsonSerializerSettings { Formatting = Formatting.Indented }));
                }
                else //This should not happen
                {
                    Response.StatusCode = 400;
                    await Response.WriteAsync("Internal Error");
                    return;
                }
            }
            catch(Exception ex)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("Internal Error: " + ex.Message);
                return;
            }

            //var qs = Request.Query.FirstOrDefault(x => x.Key == "educatorId");

            //var response = new
            //{
            //    token = ,
            //    username = identity.Name,
            //    signedUp = true
            //};

            //// Serialize response
            //await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        ///// <summary>
        ///// Here we define who has the right to sign in.
        ///// For now we allow anyone
        ///// </summary>
        ///// <param name="educatorId"></param>
        ///// <returns></returns>
        //private ClaimsIdentity GetIdentity(string educatorId)
        //{
        //    Educator educator = educators.FirstOrDefault(x => x.EducatorId == educatorId);
        //    if (educator == null)
        //    {
        //        educator = new Educator { EducatorId = educatorId, Role = "User" };
        //    }
        //        var claims = new List<Claim>
        //        {
        //            new Claim(ClaimsIdentity.DefaultNameClaimType, educator.EducatorId),
        //            new Claim(ClaimsIdentity.DefaultRoleClaimType, educator.Role)
        //        };
        //        ClaimsIdentity claimsIdentity =
        //        new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
        //            ClaimsIdentity.DefaultRoleClaimType);
        //        return claimsIdentity;

        //    // if the user cannot be found and could not be found or added
        //    throw new Exception(educatorId + " Could NOT be found or added");
        //}

        //private string generateToken(string educatorId)
        //{
        //    var identity = Utilities.GetIdentity(educatorId);
        //    var now = DateTime.UtcNow;
        //    // creating JWT-token
        //    var jwt = new JwtSecurityToken(
        //            issuer: AuthOptions.Issuer,
        //            audience: AuthOptions.Audience,
        //            notBefore: now,
        //            claims: identity.Claims,
        //            expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LifeTime)),
        //            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        //    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        //    return encodedJwt;

        //}

    } //AccountController
}
