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

        private ObjectResult sendError(string msg)
        {
            var error = new
            {
                message = msg,
                status = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError
            };
            Response.StatusCode = error.status;
            return new ObjectResult(error);
        }

        [Authorize]
        [HttpGet("api/v1/children")]
        public IActionResult Children()
        {
            try
            {
                // Serialize response
                Response.ContentType = "application/json";

                var claims = HttpContext.User.Claims;
                var id = HttpContext.User.Claims.First().Value;
                var educator = _context.Educators.SingleOrDefault(e => e.EducatorId == id);
                if (educator == null)
                    return sendError("Educator NOT found");

                var children = _context.Children.Where(c => c.EducatorId == id);

                return Ok(children);
            }
            catch(Exception ex)
            {
                return sendError(ex.Message);
            }

        }

        [Authorize]
        [HttpPost("children")]
        public IActionResult AddChild(string name, DateTime birthday)
        //public IActionResult AddChild(string name, DateTimeOffset birthday)
        {
            // Serialize response
            Response.ContentType = "application/json";

            //List<Educator> educators = _context.Educators.ToList();
            //Educator educatorX= _context.Educators.FirstOrDefault();

            var claims = HttpContext.User.Claims;
            var id = HttpContext.User.Claims.First().Value;
            var educator = _context.Educators.SingleOrDefault(e => e.EducatorId == id);

            if (educator == null)//Should not happen, because Authorize probably would not accept that token
            {
                var error = new
                {
                    message = "Educator NOT found",
                    status = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError
                };
                Response.StatusCode = error.status;
                return new ObjectResult(error);                
            }

            //Find the educator and check child unicity
            var q = _context.Children.Where(c => (c.Name == name) && c.EducatorId == educator.EducatorId);
            if (q.Count() > 0)
            {
                return sendError("Child already exists");

                //var error = new
                //{
                //    message = "Child already added",
                //    status = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError
                //};
                //Response.StatusCode = error.status;
                //return new ObjectResult(error);
            }

            var child = new Child { Name = name, Birthday = birthday, EducatorId = educator.EducatorId };
            _context.Children.Add(child);
            _context.SaveChanges();

            //await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));

            return Ok(child);
        }
    }
}
