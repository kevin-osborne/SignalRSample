using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using NLog.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.Sockets;
using NLog.SignalR;
using NLog.Config;

namespace SignalRSample
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddFilter("Microsoft", LogLevel.Warning); // switch to Information to enable request tracing. 
                builder.AddFilter("System", LogLevel.Error);
                builder.AddFilter("Engine", LogLevel.Debug);
            });

            services.AddScoped<SignalRInjectedTarget>();

            services.AddCors(o =>
            {
                o.AddPolicy("Everything", p =>
                {
                    p.AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowAnyOrigin();
                });
            });

            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddMvc();
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("Everything"));
            });

            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var path = PlatformServices.Default.Application.ApplicationBasePath;

            ConfigurationItemFactory.Default.CreateInstance = (Type type) =>
            {
                if (type == typeof(SignalRInjectedTarget))
                    return new SignalRInjectedTarget(loggerFactory);
                else
                    return Activator.CreateInstance(type); //default
            };

            //configure NLog
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            loggerFactory.ConfigureNLog("nlog.config");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseCors("Everything");

            app.UseMvc();

            var webSocketOptions = new Microsoft.AspNetCore.Builder.WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(5),
                ReceiveBufferSize = 999999
            };

            app.UseWebSockets(webSocketOptions);

            app.UseSignalR(routes =>
            {
                routes.MapHub<LoggingHub>("/signalr", _ =>
                {
                    _.Transports = TransportType.All;
                });
            });

        }
    }
}
