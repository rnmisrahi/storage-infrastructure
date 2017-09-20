using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Server.Models.KB
{
    public class Word
    {
        public int WordId { get; set; }

        public string Definition { get; set; }
        public float Average { get; set; }

        public int WordTypeId { get; set; }
        public int CategoryId { get; set; }

        public virtual WordType WordType { get; set; }
        public virtual Category Category { get; set; }
    }
}
