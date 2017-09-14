using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JwtSample.Server.Models
{
    public class Recording
    {
        public int RecordingId { get; set; }
        public int EducatorId { get; set; }
        public string RecordingName { get; set; }
        public int Number { get; set; }
        public DateTime Date { get; set; }
        public int WordCounter { get; set; }
        public int Duration { get; set; }
        [Column(TypeName = "varchar(MAX)")]
        public string Transcription { get; set; }
        public string RecordingFileName { get; set; }

        ///public virtual ICollection<Child> Children { get; set; }
    }
}
