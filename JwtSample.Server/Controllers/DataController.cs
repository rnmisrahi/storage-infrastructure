using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace JwtSample.Server.Controllers
{
    //[Route("api/[controller]")]
    public class DataController : Controller
    {
        [Authorize]
        [HttpGet("userInfo")]
        public IActionResult GetUserInfo()
        {
            return Ok($"UserId: {User.Identity.Name}");
        }

        [Authorize]
        [HttpPost("children")]
        public IActionResult AddChild(string childName, DateTimeOffset birthDate)
        {
            return Ok($"{childName} ({birthDate.ToString("d")}) added");
        }
    }
}
