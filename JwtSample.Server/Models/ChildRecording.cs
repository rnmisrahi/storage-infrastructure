using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Server.Models
{
    public class ChildRecording
    {
        public int ChildRecordingId { get; set; }
        public int RecordingId { get; set; }
        public int ChildId { get; set; }
        public bool In { get; set; }
        public string Number { get; set; }

        //public virtual Child Child { get; set; }
        //public virtual Recording Recording { get; set; }
    }
}
