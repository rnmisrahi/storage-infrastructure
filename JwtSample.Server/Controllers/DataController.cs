using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JwtSample.Server.Data;
using JwtSample.Server.Models;
using System.Linq;
using System.Collections.Generic;
using JwtSample.Server.Models.ViewModel;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using JwtSample.Server.Models.KB;
using Microsoft.Extensions.Logging;
using JwtSample.Server.LogProvider;

namespace JwtSample.Server.Controllers
{
    //[Route("api/[controller]")]
    public class DataController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        private void logApi(Microsoft.AspNetCore.Http.HttpRequest request)
        {
            _logger.LogInformation((int)LoggingEvents.API, Request.Path);
            _logger.LogInformation("Request Headers: {0}", request.Headers);
        }


        public DataController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
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
            logApi(Request);
            _logger.LogInformation("UserId: {0}", User.Identity.Name);
            return Ok($"UserId: {User.Identity.Name}");
        }

        [Authorize]
        [HttpGet("api/v1/signedup/status")]
        public ActionResult GetSignedupStatus()
        {
            logApi(Request);
            Response.ContentType = "application/json";
            try
            {
                Educator educator = getSecureUser();
                bool res = (educator != null);

                //Now look for children of that educator
                if (res)
                {
                    Child aChild = _context.Children.FirstOrDefault(c => c.EducatorId == educator.EducatorId);
                    res = aChild != null;
                }
                //    return sendError("User NOT found");


                //string search = token;

                //var q = _context.Educators.Where(x => x.Token == search).FirstOrDefault();

                //bool exists = (q != null) && (q.FacebookId != null);

                var response = new
                {
                    signedup = res
                };

                return Ok(new SignedUp { signedup = res });
            }
            catch (Exception ex)
            {
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                return sendError(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("api/v1/senduserdetails")]
        public ActionResult SendUserDetails([FromBody]Educator educator)
        {
            logApi(Request);
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

                return Ok(null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                return sendError(ex.Message);
            }

        }

        [Authorize]
        [HttpPost("api/v1/recordings")]
        public ActionResult PostRecording([FromBody] Recording recording)
        {
            logApi(Request);
            try
            {
                // Serialize response
                Response.ContentType = "application/json";

                if (recording == null)
                    return sendError("Malformed recording object");

                Educator educator = getSecureUser();
                if (educator == null)//Should not happen, because Authorize probably would not accept that token
                    return sendError("User NOT found");

                recording.EducatorId = educator.EducatorId;
                Recording rec = new Recording
                {
                    Date = recording.Date,
                    EducatorId = educator.EducatorId,
                    RecordingName = recording.RecordingName,
                    Transcription = recording.Transcription
                };
                _context.Recordings.Add(recording);

                _context.SaveChanges();

                return Ok(null);
            }
            catch(Exception ex)
            {
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                return sendError(ex.Message);
            }
        }

        private bool validateChild(Educator educator, int? id, out string message)
        {
            message = null;
            Child aChild = _context.Children.FirstOrDefault(c => c.ChildId == id);
            if (aChild == null)
            {
                message = $"Child {id} NOT found";
                return false;
            }
            if (aChild.EducatorId != educator.EducatorId)
            {
                message = $"Child {id} Does NOT belong to this Educator";
                return false;
            }
            return true;
        }

        [Authorize]
        [HttpGet("api/v1/wordcountdata/{id}")]
        public ActionResult GetWordCountData(int? id)
        {
            try
            {
                logApi(Request);
                // Serialize response
                Response.ContentType = "application/json";

                Educator educator = getSecureUser();
                if (educator == null)
                    return sendError("User NOT found");

                string error;
                if (!validateChild(educator, id, out error))
                    return sendError(error);

                var x = (from r in _context.Recordings
                         join cr in _context.ChildRecording on r.RecordingId equals cr.RecordingId
                         where (cr.ChildId == id)
                         select new ViewChildRecording { id = r.RecordingId, Number = r.Number, Date = r.Date, WordCounter = r.WordCounter, Duration=r.Duration });

                x = x.OrderBy(c => c.Date);

                int lastDay = -1;
                int day;
                List<DailyWord> dailyWords = new List<DailyWord>();
                DailyWord dailyWord = null;
                DateTime wordDay;
                if (x.Count() > 0)
                {
                    foreach (ViewChildRecording r in x)
                    {
                        day = r.Date.Day;
                        wordDay = new DateTime(r.Date.Year, r.Date.Month, r.Date.Day);
                        if (day != lastDay)
                        {
                            if (dailyWord != null)
                            { //This is Not the first time
                                dailyWords.Add(dailyWord);
                            }
                            dailyWord = new DailyWord{ChildId = id, Date=wordDay };
                                dailyWord.WordCount += r.WordCounter;
                                dailyWord.Recordings = new List<ViewChildRecording>();
                                dailyWord.Recordings.Add(r);
                        }
                        else
                        {
                            dailyWord.WordCount += r.WordCounter;
                            dailyWord.Recordings.Add(r);
                        }
                        lastDay = day;
                    }
                    dailyWords.Add(dailyWord);
                } //if (x.count() > 0)

                DailyWordList d = new DailyWordList();
                d.days = dailyWords;

                return Ok(d);
            }
            catch (Exception ex)
            {
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                return sendError(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("api/v1/allcountdata")]
        public ActionResult GetAllCountData()
        {
            try
            {
                logApi(Request);
                // Serialize response
                Response.ContentType = "application/json";

                Educator educator = getSecureUser();
                if (educator == null)
                    return sendError("User NOT found");

                var x = (from r in _context.Recordings
                         join cr in _context.ChildRecording on r.RecordingId equals cr.RecordingId
                         where r.EducatorId == educator.EducatorId
                         select new ViewChildRecording { id = r.RecordingId, childId=cr.ChildId, Number = r.Number, Date = r.Date, WordCounter = r.WordCounter, Duration = r.Duration });

                x = x.OrderBy(c => c.Date).OrderBy(cr => cr.childId);
                int count = 0;
                int lastDay = -1;
                int dayOfTheMonth;
                int dayId = 0;
                List<DailyWord> dailyWords = new List<DailyWord>();
                DailyWord dailyWord = null;
                DateTime wordDay;
                if (x.Count() > 0)
                {
                    foreach (ViewChildRecording r in x)
                    {
                        dayOfTheMonth = r.Date.Day;
                        wordDay = new DateTime(r.Date.Year, r.Date.Month, r.Date.Day);
                        r.Number = ++count;
                        if (dayOfTheMonth != lastDay)
                        {
                            if (dailyWord != null)
                            { //This is Not the first time
                                dailyWords.Add(dailyWord);
                            }
                            dailyWord = new DailyWord { ChildId = r.childId, Date = wordDay, id = ++dayId };
                            dailyWord.WordCount += r.WordCounter;
                            dailyWord.Recordings = new List<ViewChildRecording>();
                            dailyWord.Recordings.Add(r);
                        }
                        else
                        {
                            dailyWord.WordCount += r.WordCounter;
                            dailyWord.Recordings.Add(r);
                        }
                        lastDay = dayOfTheMonth;
                    }
                    dailyWords.Add(dailyWord);
                } //if (x.count() > 0)

                DailyWordList d = new DailyWordList();
                d.days = dailyWords;

                WCL wcl = new WCL();
                wcl.WordCountList = new List<DailyWordList>();
                wcl.WordCountList.Add(d);

                return Ok(wcl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                return sendError(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("api/v1/childdata/{id}")]
        public ActionResult GetChildData(int? id)
        {
            try
            {
                logApi(Request);
                // Serialize response
                Response.ContentType = "application/json";

                Educator educator = getSecureUser();
                if (educator == null)
                    return sendError("User NOT found");

                string error;
                if (!validateChild(educator, id, out error))
                    return sendError(error);

                //var recordings = _context.Recordings.Where(r => r.EducatorId == educator.EducatorId);
                ViewChildData vcd = new ViewChildData();
                List<Recording> lr = new List<Recording>();
                //var x = lr.AsQueryable();
                var x = (from r in _context.Recordings
                     join cr in _context.ChildRecording on r.RecordingId equals cr.RecordingId
                     where (cr.ChildId == id)
                         select new ViewRecording { id = r.RecordingId, Number = r.Number, Date = r.Date, WordCounter = r.WordCounter, Duration = r.Duration });

                x = x.OrderBy(c => c.Date);
                int count = 0;
                foreach (ViewRecording viewRec in x)
                {
                    viewRec.Number = ++count;
                }

                vcd.Recordings = x.ToList();

                //Add Tips
                List<Tip> tips = _context.Tips.ToList<Tip>();
                List<ViewTip> viewTips = new List<ViewTip>();
                int n = tips.Count();
                int seed = (int)DateTime.Now.Ticks;
                Random rnd = new Random(seed);

                    int k = rnd.Next(0, n - 1);
                    viewTips.Add(getATip(tips, k, id));

                vcd.Tips = viewTips;

                return Ok(vcd);
            }
            catch (Exception ex)
            {
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                return sendError(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("api/v1/childrenrecording")]
        public ActionResult PostChildRecordings([FromBody] ChildrenRecordingInOuts childrenRecordingInOuts)
        {
            logApi(Request);
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
                    //Child aChild = _context.Children.First(c => c.ChildId == childRecording.ChildId);
                    //if (aChild == null)
                    //    return sendError($"Child {childRecording.ChildId} NOT found");
                    //if (aChild.EducatorId != educator.EducatorId)
                    //    return sendError($"Child {} Does NOT belong to this Educator");                    
                    string error;
                    if (!validateChild(educator, childRecording.ChildId, out error))
                        return sendError(error);
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
                        _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
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
            try
            {
                logApi(Request);
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
            catch (Exception ex)
            {
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                return sendError(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("api/v1/userdetails")]
        public ActionResult UserDetails()
        {
            logApi(Request);
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
                logApi(Request);
                // Serialize response
                Response.ContentType = "application/json";

                Educator educator = getSecureUser();
                if (educator == null)
                    return sendError("User NOT found");

                var all = _context.Children;
                var allChildren = _context.Children.Where(c => c.EducatorId == educator.EducatorId);

                ViewChildren VC = new ViewChildren();
                VC.children = allChildren.ToList();
                return Ok(new ViewChildren { children = allChildren.ToList() });
            }
            catch(Exception ex)
            {
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                return sendError(ex.Message);
            }
        }//GetChildren

        [Authorize]
        [HttpGet("api/v1/children/{id}")]
        public IActionResult GetChildren(int? id)
        {
            try
            {
                logApi(Request);
                // Serialize response
                Response.ContentType = "application/json";

                Educator educator = getSecureUser();
                if (educator == null)
                    return sendError("User NOT found");

                string error;
                if (!validateChild(educator, id, out error))
                    return sendError(error);
                var child = _context.Children.FirstOrDefault(c => (c.ChildId == id) && (c.EducatorId == educator.EducatorId));

                return Ok(child);
            }
            catch (Exception ex)
            {
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                return sendError(ex.Message);
            }
        }//GetChildren

        private List<WordOfTheDay> wordsOfTheDay(Child child)
        {
            var entries = (from d in _context.WordDatas
                           join w in _context.Words on d.WordId equals w.WordId
                           join c in _context.Categories on w.CategoryId equals c.CategoryId
                           where d.Age == child.age
                           where d.Percentile >= 0.80
                           orderby d.Percentile descending
                           select new WordOfTheDay
                           {
                               Word = w.Definition, Topic = c.Name, ChildId = child.ChildId
                               //Definition = w.Definition, Category = c.Name, Percentile = d.Percentile
                           }
                           );//.Take(5);
            var e = entries.ToList();
            
           int n = (e.Count > 5) ? 5 : e.Count;
            int seed = (int)DateTime.Now.Ticks;
            Random rnd = new Random(seed);
            while (e.Count > n)
            {
                int k = rnd.Next(e.Count);
                e.RemoveAt(k);
            }


            return e;// entries.ToList();

        }

        [Authorize]
        [HttpGet("api/v1/worddata/{id}")]
        public IActionResult GetWordData(int? id)
        {
            try
            {
                logApi(Request);
                // Serialize response
                Response.ContentType = "application/json";

                Educator educator = getSecureUser();
                if (educator == null)
                    return sendError("User NOT found");

                string error;
                if (!validateChild(educator, id, out error))
                    return sendError(error);

                Child child = _context.Children.FirstOrDefault(c => c.ChildId == id);
                List<WordOfTheDay> allWordsOfTheDay = new List<WordOfTheDay>();
                    allWordsOfTheDay.AddRange(wordsOfTheDay(child));

                foreach (WordOfTheDay word in allWordsOfTheDay)
                {
                    word.InfoList = FakeInfo.InfoList;
                    word.QuestionList = FakeInfo.QuestionList;
                    word.ActivitiesList = FakeInfo.ActivitiesList;
                    word.OurFaveList = FakeInfo.OurFaveList;
                }


                int count = 0;
                foreach (WordOfTheDay w in allWordsOfTheDay)
                {
                    w.Id = ++count;
                }


                ViewWordsOfTheDay VWD = new ViewWordsOfTheDay();
                VWD.wordsOfTheDay = allWordsOfTheDay;
                return Ok(VWD);
            }
            catch (Exception ex)
            {
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                return sendError(ex.Message);
            }
        }

        private ViewTip getATip(List<Tip> tips, int k, int? childId)
        {
            List<Tip> tipsList = tips.ToList();
            string s = tipsList[k].Text;
            string p = tipsList[k].Type;
            int tipId = tipsList[k].id;
            ViewTip vTip = new ViewTip { ChildId = childId, Text = s, Type = p, id = tipId };
            return vTip;
        }

        [Authorize]
        [HttpGet("api/v1/generaldata")]
        public IActionResult GetGeneralData()
        {
            try
            {
                logApi(Request);
                // Serialize response
                Response.ContentType = "application/json";

                Educator educator = getSecureUser();
                if (educator == null)
                    return sendError("User NOT found");

                //var all = _context.Children;
                var children = _context.Children.Where(c => c.EducatorId == educator.EducatorId);
                List<Tip> tips = _context.Tips.ToList<Tip>();

                List<ViewTip> viewTips = new List<ViewTip>();
                int n = tips.Count();
                int seed = (int)DateTime.Now.Ticks;
                Random rnd = new Random(seed);

                foreach (Child x in children)
                {
                    int k = rnd.Next(0, n - 1);
                    viewTips.Add(getATip(tips, k, x.ChildId));
                }

                ViewGeneralData VGD = new ViewGeneralData { tips = viewTips.ToList() };
                return Ok(VGD);
            }
            catch (Exception ex)
            {
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                return sendError(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("api/v1/children")]
        public IActionResult PostChildren([FromBody] Child child)
        {
            logApi(Request);
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

            var newChild = new Child { Name = child.Name, Birthday = child.Birthday, EducatorId = educator.EducatorId, Image=child.Image, Nickname=child.Nickname };
            _context.Children.Add(newChild);
            _context.SaveChanges();

            //await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));

            return Ok(newChild);
        }//PostChildren

        [Authorize]
        [HttpDelete("api/v1/children/{id}")]
        public IActionResult Children(int? id)
        {
            logApi(Request);
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
            logApi(Request);
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
            logApi(Request);
            List<Educator> educators = _context.Educators.ToList();
            var educator = _context.Educators.FirstOrDefault();
            var response = new
            {
                educatorId = educator.EducatorId,
                token = educator.Token
            };

            // Serialize response
            Response.ContentType = "application/json";

            return Ok(_context.Educators);
        }//Educators

        [Authorize]
        [HttpGet("api/v1/educators")]
        public IActionResult GetEducators()
        {
            try
            {
                logApi(Request);
                // Serialize response
                Response.ContentType = "application/json";

                Educator educator = getSecureUser();
                if (educator == null)
                    return sendError("User NOT found");

                var all = _context.Educators;

                ViewEducators ve = new ViewEducators();
                ve.educators = all.ToList();
                return Ok(new ViewEducators { educators = all.ToList() });
            }
            catch (Exception ex)
            {
                _logger.LogWarning((int)LoggingEvents.ERROR, ex, "Error");
                return sendError(ex.Message);
            }
        }//GetEducators


    }

}
