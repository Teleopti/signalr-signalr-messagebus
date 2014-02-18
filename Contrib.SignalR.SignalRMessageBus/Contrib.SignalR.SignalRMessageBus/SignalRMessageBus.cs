using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Messaging;
using Connection = Microsoft.AspNet.SignalR.Client.Connection;

namespace Contrib.SignalR.SignalRMessageBus
{
    public class SignalRMessageBus : ScaleoutMessageBus
    {
    	private readonly Connection _connection;
	    private const int streamIndex = 0;

	    public SignalRMessageBus(SignalRScaleoutConfiguration scaleoutConfiguration, IDependencyResolver dependencyResolver)
			: base(dependencyResolver, scaleoutConfiguration)
        {
			_connection = new Connection(scaleoutConfiguration.ServerUri.ToString());
			_connection.Closed += connectionOnClosed;
    		_connection.Received += notificationRecieved;
			_connection.Error += onConnectionOnError;
    		var startTask = _connection.Start();
		    startTask.ContinueWith(t =>
			    {
				    if (t.IsFaulted && t.Exception != null)
					    throw t.Exception.GetBaseException();
			    }, TaskContinuationOptions.OnlyOnFaulted);
		    startTask.ContinueWith(_ => Open(streamIndex), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

	    private void onConnectionOnError(Exception e)
	    {
		    Debug.WriteLine(e.ToString());
		    OnError(0, e);
	    }

	    private void connectionOnClosed()
	    {
		    delay(TimeSpan.FromSeconds(5)).Wait();
			var startTask = _connection.Start();
			startTask.ContinueWith(t =>
			{
				if (t.IsFaulted && t.Exception != null)
					throw t.Exception.GetBaseException();
			}, TaskContinuationOptions.OnlyOnFaulted);
	    }

		private static Task delay(TimeSpan timeOut)
		{
			var tcs = new TaskCompletionSource<object>();

			var timer = new Timer(tcs.SetResult,
								  null,
								  timeOut,
								  TimeSpan.FromMilliseconds(-1));
			return tcs.Task.ContinueWith(_ => timer.Dispose(), TaskContinuationOptions.ExecuteSynchronously);
		}

	    private void notificationRecieved(string obj)
    	{
    		var indexOfFirstHash = obj.IndexOf('#');
    		var message = obj.Substring(indexOfFirstHash + 3).ToScaleoutMessage();

			if (message.Messages == null || message.Messages.Count == 0)
			{
				Open(streamIndex);
			}

			OnReceived(streamIndex, (ulong)Convert.ToInt64(obj.Substring(0, indexOfFirstHash)), message);
    	}

	    protected override Task Send(IList<Message> messages)
	    {
		    if (messages == null || 
				messages.Count == 0 || 
				_connection.State != ConnectionState.Connected)
		    {
				return makeEmptyTask();
		    }

		    return _connection.Send("s:" + messages.ToScaleoutString());

	    }

	    private static Task makeEmptyTask()
	    {
		    var emptyTask = new TaskCompletionSource<object>();
		    emptyTask.SetResult(null);
		    return emptyTask.Task;
	    }

	    protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_connection.Closed -= connectionOnClosed;
				_connection.Received -= notificationRecieved;
				_connection.Error -= onConnectionOnError;
				if (_connection.State == ConnectionState.Connected)
					_connection.Stop();
			}
		}
    }
}
