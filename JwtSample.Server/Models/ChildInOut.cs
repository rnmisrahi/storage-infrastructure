using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Server.Models
{
    public class ChildInOut
    {
        public int ChildInOutId { get; set; }
        public int ChildId { get; set; }
        public string Number { get; set; }//The time at which the children entered the conversation
        public bool In { get; set; }//true => In, false => Out
    }
}
