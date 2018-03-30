using System;
using Microsoft.AspNetCore.SignalR;
using NLog.SignalR;

namespace SignalRSample
{
    public class LoggingHub : Hub<ILoggingHub>
    {
        public void Log(LogEvent logEvent)
        {
            Clients.All.Log(logEvent);
        }
    }
}