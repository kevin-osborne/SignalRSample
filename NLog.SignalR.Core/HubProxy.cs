using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace NLog.SignalR
{
    public class HubProxy
    {
        private readonly SignalRTarget _target;
        public HubConnection Connection;

        public HubProxy(SignalRTarget target)
        {
            _target = target;
        }

        public async void Log(LogEvent logEvent)
        {
            EnsureProxyExists();

            if (Connection != null)
            {
                try
                {
                    await Connection.InvokeAsync<LogEvent>(_target.MethodName, logEvent);
                }
                catch (Exception)
                {
                    //
                }
            }
                
        }

        public void EnsureProxyExists()
        {
            if (Connection == null)
            {
                BeginNewConnection();
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
                Connection.StartAsync();
            }
            catch (Exception)
            {
                //
            }
        }

    }
}
