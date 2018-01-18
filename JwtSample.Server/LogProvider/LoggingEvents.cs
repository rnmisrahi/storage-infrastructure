using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Server.LogProvider
{
        //     Defines logging severity levels.
        public enum LoggingEvents
        {
            API = 1000,
            ERROR = 4000,
            GET_ITEM,
            INSERT_ITEM,
            UPDATE_ITEM,
            DELETE_ITEM
        }
}
