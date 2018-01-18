using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtSample.Server.LogProvider
{
    public class DBLoggerProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool> _filter;
        private string _connectionString;

        public DBLoggerProvider(Func<string, LogLevel, bool> filter, string connectionStr)
        {
            _filter = filter;
            _connectionString = connectionStr;
        }

        public ILogger CreateLogger(string educatorId)
        {
            return new DBLogger(educatorId, _filter, _connectionString);
        }

        public void Dispose()
        {

        }
    }

}
