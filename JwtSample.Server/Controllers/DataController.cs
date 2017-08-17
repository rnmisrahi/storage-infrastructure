using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JwtSample.Server.Data;
using JwtSample.Server.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

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

        private Educator findEducator(string token)
        {
            try
            {
                var claims = HttpContext.User.Claims;
                var id = HttpContext.User.Claims.First().Value;
                var q = _context.Educators.Where(x => x.token == token).FirstOrDefault();
                Educator educator = q;
                return educator;
            }
            catch
            {
                return null;
            }
        }

        [Authorize]
        [HttpGet("userInfo")]
        public IActionResult GetUserInfo()
        {
            return Ok($"UserId: {User.Identity.Name}");
        }

        [Authorize]
        [HttpPost("educators")]
        public ActionResult Educators()
        {
            List<Educator> educators = _context.Educators.ToList();
            var educator = _context.Educators.FirstOrDefault();
            var response = new
            {
                educatorId = educator.EducatorId,
                token = educator.token
            };

            // Serialize response
            Response.ContentType = "application/json";

            //await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));

            //return Ok($"First Educator: {educator.EducatorId} - ({educator.token}) There");
            return Ok(_context.Educators);
        }

        [Authorize]
        [HttpPost("children")]
        public IActionResult AddChild(string name, DateTime birthday)
        //public IActionResult AddChild(string name, DateTimeOffset birthday)
        {
            List<Educator> educators = _context.Educators.ToList();
            Educator educatorX = _context.Educators.FirstOrDefault();

            var claims = HttpContext.User.Claims;
            var id = HttpContext.User.Claims.First().Value;
            var educator = _context.Educators.SingleOrDefault(e => e.EducatorId == id);

            var child = new Child { Name = name, Birthday = birthday, EducatorId = educator.EducatorId };
            _context.Children.Add(child);
            _context.SaveChanges();

            var response = new
            {
                name = name,
                birthday = birthday
            };

            // Serialize response
            Response.ContentType = "application/json";
            
            //await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));

            return Ok(child);
        }
    }
}
