using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Server.Models.KB
{
    public class WordData
    {
        public int WordDataId { get; set; }
        public int Age { get; set; }
        public int WordId { get; set; }
        public float Percentile { get; set; }

        public virtual Word Word { get; set; }
    }
}
