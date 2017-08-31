using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace JwtSample.Server.Models
{
    public class ChildrenRecordingInOuts
    {
        public int ChildrenRecordingInOutsId { get; set; }
        public int RecordingId { get; set; }
        public ICollection<ChildRecording> ChildrenRecordings { get; set; }
    }
}
