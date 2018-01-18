using JwtSample.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using JwtSample.Server.Data;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.Extensions.Logging;
using JwtSample.Server.LogProvider;

namespace JwtSample.Server.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        private void logApi(HttpRequest request)
        {
            _logger.LogInformation((int)LoggingEvents.API, Request.Path);
            _logger.LogInformation("Request Headers: {0}", request.Headers);
        }

        //public AccountController(ILoggerFactory loggerFactory)
        //{

        //}

        public AccountController(ApplicationDbContext context,
        ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
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
                //var facebookId2 = user.facebookId;
                _logger.LogTrace((int)LoggingEvents.API, Request.Path);
                string s = Request.ContentType;
                _logger.LogTrace("Request.ContentType: {0}", s);

                //This is as it is now: urlencode
                //var facebookId = Request.Form["facebookId"];  //This used to work For some reason it stopped working! It works when called from my WFACallAPI05.csproj desktop app              
                var facebookId = Request.Headers["facebookId"];
                if (facebookId.Count == 0)
                    throw new Exception("No facebookId found");

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
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                Response.StatusCode = 400;
                await Response.WriteAsync("Internal Error: " + ex.Message);
                return;
            }

        }

        [HttpPost("api/v1/loginFacebook")]
        public async Task JwtToken()
        {
            try
            {
                logApi(Request);

                string s = Request.ContentType;

                //This is as it is now: urlencode
                //var facebookId = Request.Form["facebookId"];  //This used to work For some reason it stopped working! It works when called from my WFACallAPI05.csproj desktop app              
                var facebookId = Request.Headers["facebookId"];
                _logger.LogInformation("facebookId: {0}", facebookId);
                if (facebookId.Count == 0)
                    throw new Exception("No facebookId found");

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
                            token = q.Token
                        };
                        await Response.WriteAsync(JsonConvert.SerializeObject(responseExists, new JsonSerializerSettings { Formatting = Formatting.Indented }));
                        return;
                    }
                }
                if (!exists)
                { //Doesn't exist. Generate token and add
                    var responseNew = new
                    {
                        token = Utilities.generateToken(facebookId)
                    };
                    Educator educator = new Educator { FacebookId = facebookId, Token = responseNew.token };
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
            catch (Exception ex)
            {
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                Response.StatusCode = 400;
                await Response.WriteAsync("Internal Error: " + ex.Message);
                return;
            }

        }


        [HttpPost("api/v1/loginFacebookOLD")]
        public async Task FacebookToken()
        {
            try
            {
                _logger.LogTrace((int)LoggingEvents.API, Request.Path);
                string res = "";

                //var facebookId = Request.Form["facebookId"];
                var facebookId = Request.Headers["facebookId"];
                if (facebookId.Count == 0)
                    throw new Exception("No facebookId found");

                string search = facebookId;

                var q = _context.Educators.Where(x => x.FacebookId == search).FirstOrDefault();

                bool exists = (q != null) && (q.FacebookId != null);
                if (exists)
                    res = q.Token;

                var response = new
                {
                    token = res
                };
                await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
            }
            catch (Exception ex)
            {
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                Response.StatusCode = 400;
                await Response.WriteAsync("Internal Error: " + ex.Message);
                return;
            }

        }

    } //AccountController
}
