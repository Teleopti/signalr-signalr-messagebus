using System.Threading.Tasks;
using System.Web.Routing;
using Chat.Backplane;
using Contrib.SignalR.SignalRMessageBus.Backend;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace Chat.Backplane
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.MapSignalR<SignalRBackplane>("/backplane");
		}
	}
}
