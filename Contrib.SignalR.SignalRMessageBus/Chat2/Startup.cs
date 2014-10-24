using System;
using Chat2;
using Contrib.SignalR.SignalRMessageBus;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace Chat2
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