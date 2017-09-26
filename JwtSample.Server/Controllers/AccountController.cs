using JwtSample.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using JwtSample.Server.Data;
using Microsoft.Extensions.Options;

namespace JwtSample.Server.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("api/v1/login")]
        public async Task Token()
        {
            try
            {
                //This could be used to get the Login as Json
                //StreamReader am = new StreamReader(Request.Body);
                //string result = await am.ReadToEndAsync();
                //JsonSerializer ser = new JsonSerializer();
                //UserLogin user = JsonConvert.DeserializeObject<UserLogin>(result);
                //var facebookId = user.facebookId;

                //This is as it is now: urlencode
                var facebookId = Request.Form["facebookId"];

                var identity = Utilities.GetIdentity(facebookId);

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
                    string search = facebookId;
                    var q = _context.Educators.Where(x => x.FacebookId == search).FirstOrDefault();

                    //var q = _context.Educators.Find(x => x.EducatorId == "educatorId");
                    exists = (q != null) && (q.FacebookId != null);
                    if (exists) // The following implies that the token was found
                    {
                        var children = _context.Children.Where(c => c.EducatorId == q.EducatorId);
                        int childrenCount = children.Count();
                        var responseExists = new
                        {
                            token = q.Token,
                            username = identity.Name,
                            signedUp = (childrenCount > 0)
                        };
                        await Response.WriteAsync(JsonConvert.SerializeObject(responseExists, new JsonSerializerSettings { Formatting = Formatting.Indented }));
                        return;
                    }
                }
                if (!exists)
                { //Doesn't exist. Generate token and add
                    var responseNew = new
                    {
                        token = Utilities.generateToken(facebookId),
                        username = identity.Name,
                        signedUp = false
                    };
                    Educator educator = new Educator { FacebookId = facebookId, Token = responseNew.token, SignedUp = false };
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

        }

    } //AccountController
}
