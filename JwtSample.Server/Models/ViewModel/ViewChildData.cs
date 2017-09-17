using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Server.Models.ViewModel
{
    //Used only to generate a Response in childdata
    public class ViewChildData
    {
        public ICollection<Tip> Tips { get; set; }
        public ICollection<OutRecording> Recordings { get; set; }
        public ICollection<TodaysWord> TodaysWords { get; set; }
    }

    public class OutRecording
    {
        public int id { get; set; }
        public int Number { get; set; }
        public DateTime Date { get; set; }
        public int WordCounter { get; set; }
        public int Duration { get; set; }
    }

    public class TodaysWord
    {
        public int id { get; set; }
        public string Word { get; set; }
    }

    public class ChildWordModel
    {

    }

    public class DailyWord
    {
        public int DailyWordId { get; set; }
        public DateTime Date { get; set; }
        public int WordCount { get; set; }
        public int ExpectedWordCount { get; set; }
        public ICollection<OutRecording> Recordings { get; set; }
        public int? ChildId { get; set; }
    }

}
