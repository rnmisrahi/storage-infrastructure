using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Server.Models.ViewModel
{
    public class WordOfTheDay
    {
        public string Word { get; set; }
        public string Subtext { get; set; }
        public string Topic { get; set; }
        public int ChildId { get; set; }
        public int Id { get; set; }
        public List<string> InfoList{ get; set; }
        public List<string> QuestionList { get; set; }
        public List<string> ActivitiesList { get; set; }
        public List<string> OurFaveList { get; set; }
    }

    public class ViewWordsOfTheDay
    {
        public List<WordOfTheDay> wordsOfTheDay { get; set; }
    }
}
