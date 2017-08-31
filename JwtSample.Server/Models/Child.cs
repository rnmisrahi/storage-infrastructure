using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace JwtSample.Server.Models
{
    public class Child
    {
        [Key]
        public int ChildId { get; set; }
        public int EducatorId { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public DateTime Birthday { get; set; }
        public string Image { get; set; }

        ///public virtual ICollection<Recording> Recordings { get; set; }

        public virtual int months { get
            {
                int m = (DateTime.Now.Year - Birthday.Year) * 12 + (DateTime.Now.Month - Birthday.Month);
                int d = DateTime.Now.Day - Birthday.Day;
                double days = 30.42 * m + d;
                return (int)(Math.Round(days / 30.42));
            }
        }
    }
}
