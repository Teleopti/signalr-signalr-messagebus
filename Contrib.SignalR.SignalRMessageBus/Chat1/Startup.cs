using System;
using Chat1;
using Contrib.SignalR.SignalRMessageBus;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace Chat1
{

	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.MapSignalR();

			GlobalHost.DependencyResolver.UseSignalRServer(new Uri("http://localhost:56715/backplane"));
		}
	}
}