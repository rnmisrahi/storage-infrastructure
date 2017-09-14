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
        public ICollection<Recording> Recordings { get; set; }
        public ICollection<TodaysWord> TodaysWords { get; set; }
    }

    public class TodaysWord
    {
        public int id { get; set; }
        public string Word { get; set; }

    }

}
