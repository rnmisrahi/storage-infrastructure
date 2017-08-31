using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JwtSample.Server.Models
{
    public class Educator
    {
        [Key]
        public int EducatorId { get; set; }
        public string FacebookId { get; set; }
        public string Token { get; set; }

        public bool SignedUp { get; set; }
        public string Role { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Birthday { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Language1 { get; set; }
        public string Language2 { get; set; }
        public string Language3 { get; set; }

        public virtual List<Child> Children { get; set; }
    }
}
