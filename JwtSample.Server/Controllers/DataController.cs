using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JwtSample.Server.Data;
using JwtSample.Server.Models;
using System.Linq;
using System.Collections.Generic;
using JwtSample.Server.Models.ViewModel;
using System.Reflection;

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
                return _context.Educators.SingleOrDefault(e => e.FacebookId == id);
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
                userInfo.Email = educator.Email ?? userInfo.Email;
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
        [HttpPost("api/v1/recordings")]
        public ActionResult PostRecording([FromBody] Recording recording)
        {
            // Serialize response
            Response.ContentType = "application/json";

            if (recording == null)
                return sendError("Malformed recording object");

            Educator educator = getSecureUser();
            if (educator == null)//Should not happen, because Authorize probably would not accept that token
                return sendError("User NOT found");

            recording.EducatorId = educator.EducatorId;
            Recording rec = new Recording { Date = recording.Date, EducatorId = educator.EducatorId, RecordingName = recording.RecordingName,
                Transcription = recording.Transcription};
            _context.Recordings.Add(recording);

            _context.SaveChanges();

            return Ok(recording);
        }

        [Authorize]
        [HttpGet("api/v1/childdata/{id}")]
        public ActionResult GetChildData(int? id)
        {
            try
            {
                // Serialize response
                Response.ContentType = "application/json";

                Educator educator = getSecureUser();
                if (educator == null)
                    return sendError("User NOT found");

                Child aChild = _context.Children.First(c => c.ChildId == id);
                if (aChild == null)
                    return sendError($"Child {id} NOT found");
                if (aChild.EducatorId != educator.EducatorId)
                    return sendError($"Child {id} Does NOT belong to this Educator");

                //var recordings = _context.Recordings.Where(r => r.EducatorId == educator.EducatorId);
                ViewChildData vcd = new ViewChildData();
                List<Recording> lr = new List<Recording>();
                //var x = lr.AsQueryable();
                var x = (from r in _context.Recordings
                     join cr in _context.ChildRecording on r.RecordingId equals cr.RecordingId
                     where (cr.ChildId == id)
                     select new Recording { Date = r.Date, RecordingName = r.RecordingName, WordCounter = r.WordCounter  });
                vcd.Recordings = x.ToList();

                return Ok(vcd);
            }
            catch (Exception ex)
            {
                return sendError(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("api/v1/childrenrecording")]
        public ActionResult PostChildRecordings([FromBody] ChildrenRecordingInOuts childrenRecordingInOuts)
        {
            // Serialize response
            Response.ContentType = "application/json";

            if (childrenRecordingInOuts == null)
                return sendError("Malformed recording object");

            Educator educator = getSecureUser();
            if (educator == null)//Should not happen, because Authorize probably would not accept that token
                return sendError("User NOT found");

            //Check that recording exists
            int recordingId = childrenRecordingInOuts.RecordingId;
            var recording = _context.Recordings.FirstOrDefault(r => r.RecordingId == recordingId);
            if (recording == null)
                return sendError($"Recording {recordingId} NOT found");

            //Check that all children belong to that Educator
            if (childrenRecordingInOuts.ChildrenRecordings != null)
            {
                foreach (ChildRecording childRecording in childrenRecordingInOuts.ChildrenRecordings)
                {
                    //                    Child c = _context.Children.First(c => c.ChildId == child.ChildId);
                    Child aChild = _context.Children.First(c => c.ChildId == childRecording.ChildId);
                    if (aChild == null)
                        return sendError($"Child {childRecording.ChildId} NOT found");
                    if (aChild.EducatorId != educator.EducatorId)
                        return sendError($"Child {childRecording.ChildId} Does NOT belong to this Educator");                    
                }
                //Update the ChildRecording relationship
                foreach (ChildRecording childRecording in childrenRecordingInOuts.ChildrenRecordings)
                {
                    try
                    {
                        ChildRecording cr = new ChildRecording { RecordingId = childrenRecordingInOuts.RecordingId, ChildId = childRecording.ChildId };
                        _context.ChildRecording.Add(cr);
                        _context.SaveChanges();
                    }
                    catch(Exception ex)
                    {
                        return sendError("Error updating. Perhaps record already exists. " + ex.Message);
                    }
                }
                return Ok(null);
            }
            return sendError("No Data found");

        }

        [Authorize]
        [HttpPut("api/v1/recordings/{id}")]
        public ActionResult PutRecording(int? id, [FromBody] Recording recording)
        {
            // Serialize response
            Response.ContentType = "application/json";

            if (recording == null)
                return sendError("Malformed recording object");

            Educator educator = getSecureUser();
            if (educator == null)//Should not happen, because Authorize probably would not accept that token
                return sendError("User NOT found");

            Recording localRecording = new Recording
            {
                Date = recording.Date,
                EducatorId = educator.EducatorId,
                RecordingName = recording.RecordingName,
                Transcription = recording.Transcription
            };
            _context.Recordings.Add(localRecording);

            _context.SaveChanges();

            return Ok(recording);
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

                var all = _context.Children;
                foreach(Child x in all)
                {
                    Console.WriteLine("Name: " + x.Name);
                }

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
                token = educator.Token
            };

            // Serialize response
            Response.ContentType = "application/json";

            //await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));

            //return Ok($"First Educator: {educator.EducatorId} - ({educator.token}) There");
            return Ok(_context.Educators);
        }//Educators

    }
}
