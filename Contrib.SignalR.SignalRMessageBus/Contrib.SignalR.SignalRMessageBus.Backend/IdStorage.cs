using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security;
using System.Threading;

namespace Contrib.SignalR.SignalRMessageBus.Backend
{
	public class IdStorage
	{
		private const string FileName = "LastKnownId.key";

		public void OnStart()
		{
			if (attemptIsolatedStorageOperation(tryLoadLastId)) return;

			_nextId = long.MinValue;
		}

		private static bool tryLoadLastId()
		{
			var store = IsolatedStorageFile.GetUserStoreForAssembly();
			if (store.FileExists(FileName))
			{
				using (var stream = store.OpenFile(FileName, FileMode.OpenOrCreate, FileAccess.Read))
				using (var streamReader = new StreamReader(stream))
				{
					var result = streamReader.ReadToEnd();

					long value;
					if (long.TryParse(result, out value))
					{
						_nextId = value;
						return true;
					}
				}
			}

			return false;
		}

		public void OnStop()
		{
			attemptIsolatedStorageOperation(trySaveLastId);
		}

		private static bool attemptIsolatedStorageOperation(Func<bool> isolatedStorageOperation)
		{
			try
			{
				return isolatedStorageOperation();
			}
			catch (SecurityException)
			{
				
			}
			catch (IsolatedStorageException)
			{
				
			}
			return false;
		}

		private static bool trySaveLastId()
		{
			var store = IsolatedStorageFile.GetUserStoreForAssembly();

			using (var stream = store.OpenFile(FileName, FileMode.OpenOrCreate, FileAccess.Write))
			using (var streamWriter = new StreamWriter(stream))
			{
				streamWriter.Write(_nextId);
			}

			return true;
		}

		private static long _nextId;

		public static long NextId()
		{
			return Interlocked.Increment(ref _nextId);
		}
	}
}
