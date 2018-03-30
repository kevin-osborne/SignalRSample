using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace NLog.SignalR
{
    public class InjectedHubProxy
    {
        private readonly SignalRInjectedTarget _target;
        public readonly Microsoft.Extensions.Logging.ILogger _logger;
        public HubConnection Connection;

        private bool ResetConnection { get; set; }

        public InjectedHubProxy(SignalRInjectedTarget target, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<InjectedHubProxy>();
            _target = target;
            ResetConnection = false;
        }

        public async void Log(LogEvent logEvent)
        {
            _logger.LogTrace("InjectedHubProxy.Log");
            EnsureProxyExists();

            if (Connection != null)
            {
                try
                {
                    //await Connection.InvokeAsync<LogEvent>(_target.MethodName, logEvent);
                    await Connection.SendAsync(_target.MethodName, logEvent); 
                }
                catch (Exception ex)
                {
                    //
                    _logger.LogTrace("InjectedHubProxy.Log.Exception: " + ex.Message);
                    _logger.LogTrace("InjectedHubProxy.Log.Exception: " + ex.StackTrace);
                    _logger.LogTrace("InjectedHubProxy.Log.Exception - Connection: " + Connection);
                    _logger.LogTrace("InjectedHubProxy.Log.Exception - _target: " + _target);
                    _logger.LogTrace("InjectedHubProxy.Log.Exception - _target.MethodName: " + _target.MethodName);
                    _logger.LogTrace("InjectedHubProxy.Log.Exception - _target.HubName: " + _target.HubName);
                    _logger.LogTrace("InjectedHubProxy.Log.Exception - _target.Uri: " + _target.Uri);
                    //Connection = null;
                    ResetConnection = true;
                }
            }
            else
            {
                _logger.LogDebug("InjectedHubProxy.Log Connection==null");
            }
                
        }

        public void EnsureProxyExists()
        {
            if (Connection == null) //  || ResetConnection
            {
                _logger.LogDebug("InjectedHubProxy.Log Connection==null");
                BeginNewConnection();
            }
            else
            {
                _logger.LogTrace("InjectedHubProxy.Log Connection!=null");
            }
            // todo public event Action<Exception> Closed;
            //else if (Connection.State == ConnectionState.Disconnected)
            //{
            //    StartExistingConnection();
            //}
        }

        private void BeginNewConnection()
        {
            try
            {
                string name = _target.Name.Replace("_wrapped", "");
                string url = string.Format("{0}/{1}", _target.Uri, name);
                Connection = new HubConnectionBuilder()
                    .WithUrl(url)
                    .Build();
                Connection.StartAsync(); // todo await
            }
            catch (Exception ex)
            {
                //
                _logger.LogDebug("InjectedHubProxy.BeginNewConnection.Exception: " + ex.Message);
            }
        }

    }
}
