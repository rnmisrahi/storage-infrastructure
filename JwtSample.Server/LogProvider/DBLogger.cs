﻿using Microsoft.Extensions.Logging;
using System;
using JwtSample.Server.Models;

namespace JwtSample.Server.LogProvider
{
    public class DBLogger : ILogger
    {

        private string _educatorId;
        private Func<string, LogLevel, bool> _filter;
        private SqlHelper _helper;
        private int MessageMaxLength = 4000;

        public DBLogger(string educatorId, Func<string, LogLevel, bool> filter, string connectionString)
        {
            _educatorId = educatorId;
            _filter = filter;
            _helper = new SqlHelper(connectionString);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (exception != null)
            {
                message += "\n" + exception.ToString();
            }
            message = message.Length > MessageMaxLength ? message.Substring(0, MessageMaxLength) : message;
            EventLog eventLog = new EventLog
            {
                Message = message,
                EventId = eventId.Id,
                LogLevel = logLevel.ToString(),
                CreatedTime = DateTime.UtcNow
            };
            _helper.InsertLog(eventLog);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (_filter == null || _filter(_educatorId, logLevel));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

    }
}
