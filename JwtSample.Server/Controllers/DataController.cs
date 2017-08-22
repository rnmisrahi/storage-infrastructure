using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JwtSample.Server.Data;
using JwtSample.Server.Models;
using System.Linq;
using System.Collections.Generic;
using System.IO;

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

        private Educator getSecureUser()
        {
            try
            {
                var id = HttpContext.User.Claims.First().Value;
                return _context.Educators.SingleOrDefault(e => e.EducatorId == id);
            }
            catch
            {
                return null;
            }
        }

        [Authorize]
        [HttpGet("api/v1/userInfo")]
        public IActionResult GetUserInfo()
        {
            return Ok($"UserId: {User.Identity.Name}");
        }

        [Authorize]
        [HttpPost("api/v1/senduserdetails")]
        public ActionResult SendUserDetails([FromBody]Educator educator)
        {
            Response.ContentType = "application/json";
            try
            {
                Educator userInfo = getSecureUser();
                if (userInfo == null)
                    sendError("User NOT found");
                userInfo.Birthday = educator.Birthday ?? userInfo.Birthday;
                userInfo.email = educator.email ?? userInfo.email;
                userInfo.FirstName = educator.FirstName ?? userInfo.FirstName;
                userInfo.LastName = educator.LastName ?? userInfo.LastName;
                userInfo.Gender = educator.Gender ?? userInfo.Gender;
                userInfo.Language1 = educator.Language1 ?? userInfo.Language1;
                userInfo.Language2 = educator.Language2 ?? userInfo.Language2;
                userInfo.Language3 = educator.Language3 ?? userInfo.Language3;

                _context.SaveChanges();

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                return sendError(ex.Message);
            }

        }

        [Authorize]
        [HttpGet("api/v1/userdetails")]
        public ActionResult UserDetails()
        {
            Response.ContentType = "application/json";
            Educator userInfo = getSecureUser();
            if (userInfo == null)
                sendError("User NOT found");
            return Ok(userInfo);
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
        public IActionResult GetChildren()
        {
            try
            {
                // Serialize response
                Response.ContentType = "application/json";

                Educator educator = getSecureUser();
                if (educator == null)
                    return sendError("User NOT found");

                var children = _context.Children.Where(c => c.EducatorId == educator.EducatorId);

                return Ok(children);
            }
            catch(Exception ex)
            {
                return sendError(ex.Message);
            }
        }//GetChildren

        [Authorize]
        [HttpGet("api/v1/children/{id}")]
        public IActionResult GetChildren(int? id)
        {
            try
            {
                // Serialize response
                Response.ContentType = "application/json";

                Educator educator = getSecureUser();
                if (educator == null)
                    return sendError("User NOT found");

                var child = _context.Children.FirstOrDefault(c => (c.ChildId == id) && (c.EducatorId == educator.EducatorId));
                if (child == null)
                    return sendError("Child NOT found");

                return Ok(child);
            }
            catch (Exception ex)
            {
                return sendError(ex.Message);
            }
        }//GetChildren

        [Authorize]
        [HttpPost("api/v1/children")]
        public IActionResult PostChildren([FromBody] Child child)
        {
            // Serialize response
            Response.ContentType = "application/json";
            Request.ContentType = "application/json";
            Educator educator = getSecureUser();
            if (educator == null)//Should not happen, because Authorize probably would not accept that token
                return sendError("User NOT found");

            //Find the educator and check child unicity
            var q = _context.Children.Where(c => (c.Name == child.Name) && c.EducatorId == educator.EducatorId);
            if (q.Count() > 0)
                return sendError("Child already exists");

            var newChild = new Child { Name = child.Name, Birthday = child.Birthday, EducatorId = educator.EducatorId };
            _context.Children.Add(newChild);
            _context.SaveChanges();

            //await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));

            return Ok(newChild);
        }//PostChildren

        [Authorize]
        [HttpDelete("api/v1/children/{id}")]
        public IActionResult Children(int? id)
        {
            Response.ContentType = "application/json";

            Educator educator = getSecureUser();
            if (educator == null)//Should not happen, because Authorize probably would not accept that token
                sendError("User NOT found");

            //Find the educator and check child unicity
            Child childInFile = _context.Children.FirstOrDefault(c => (c.ChildId == id) && c.EducatorId == educator.EducatorId);
            if (childInFile == null)
                return sendError("Child NOT found");

            _context.Children.Remove(childInFile);
            _context.SaveChanges();

            return Ok(childInFile);
        }

        [Authorize]
        [HttpPut("api/v1/children/{id}")]
        public IActionResult Children(int? id, [FromBody][Bind("Name, Nickname, Birthday,Image")]Child child)
        {
            Response.ContentType = "application/json";

            Educator educator = getSecureUser();
            if (educator == null)//Should not happen, because Authorize probably would not accept that token
                sendError("User NOT found");

            //Find the educator and check child unicity
            Child childInFile = _context.Children.FirstOrDefault(c => (c.ChildId == id) && c.EducatorId == educator.EducatorId);
            if (childInFile == null)
                return sendError("Child NOT found");

            if (child.Birthday.Year > 1) //Not Unassigned
                childInFile.Birthday = child.Birthday;
            childInFile.Image = child.Image ?? childInFile.Image;
            childInFile.Name = child.Name ?? childInFile.Name;
            childInFile.Nickname = child.Nickname ?? childInFile.Nickname;
            
            //var child = new Child { Name = name, Birthday = birthday, EducatorId = educator.EducatorId };
            //_context.Children.Add(child);
            _context.SaveChanges();

            return Ok(childInFile);
        }//PostChildren

        [Authorize]
        [HttpPost("api/v1/educators")]
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
        }//Educators

    }
}
