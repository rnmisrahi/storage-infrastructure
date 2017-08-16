using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtSample.Server.Models;

namespace JwtSample.Server.Data
{
    public class DBInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}
