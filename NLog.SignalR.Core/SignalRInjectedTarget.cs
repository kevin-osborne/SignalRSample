using System;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

namespace NLog.SignalR
{
    [Target("SignalRInjected")]
    public class SignalRInjectedTarget : TargetWithLayout
    {
        [RequiredParameter]
        public string Uri { get; set; }

        [DefaultValue("LoggingHub")]
        public string HubName { get; set; }

        [DefaultValue("Log")]
        public string MethodName { get; set; }

        public readonly InjectedHubProxy Proxy;
        public readonly ILoggerFactory _loggerFactory;
        public readonly Microsoft.Extensions.Logging.ILogger _logger;

        public SignalRInjectedTarget(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SignalRInjectedTarget>();
            _loggerFactory = loggerFactory;
            HubName = "LoggingHub";
            MethodName = "Log";
            Proxy = new InjectedHubProxy(this, loggerFactory);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            _logger.LogTrace("SignalRInjectedTarget.Write");
            var renderedMessage = this.Layout.Render(logEvent);
            var item = new LogEvent(logEvent, renderedMessage);

            Proxy.Log(item);
        }
    }
}
