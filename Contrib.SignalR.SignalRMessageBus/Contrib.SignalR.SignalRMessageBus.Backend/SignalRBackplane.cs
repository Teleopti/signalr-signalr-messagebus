using Microsoft.AspNet.SignalR;

namespace Contrib.SignalR.SignalRMessageBus.Backend
{
	public class SignalRBackplane : PersistentConnection
	{
		protected override System.Threading.Tasks.Task OnReceived(IRequest request, string connectionId, string data)
		{
			var id = IdStorage.NextId();
			return Connection.Broadcast(id + "#" + data);
		}
	}
}