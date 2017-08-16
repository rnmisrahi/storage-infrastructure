using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JwtSample.Server.Data;

namespace JwtSample.Server.Controllers
{
    //[Route("api/[controller]")]
    public class DataController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DataController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("userInfo")]
        public IActionResult GetUserInfo()
        {
            return Ok($"UserId: {User.Identity.Name}");
        }

        [Authorize]
        [HttpPost("children")]
        public IActionResult AddChild(string name, DateTimeOffset birthday)
        //public IActionResult AddChild(string name, DateTimeOffset birthday)
        {
            var response = new
            {
                name = name,
                birthday = birthday
            };

            // Serialize response
            Response.ContentType = "application/json";
            
            //await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));

            return Ok($"{name} ({birthday.ToString("d")}) added");
        }
    }
}
