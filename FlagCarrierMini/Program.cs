using System;
using System.Threading;

using FlagCarrierBase;
using NdefLibrary.Ndef;

namespace FlagCarrierMini
{
    class Program
    {
		public static ManualResetEvent exitEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
			Console.CancelKeyPress += (sender, eArgs) =>
			{
				eArgs.Cancel = true;
				exitEvent.Set();
			};

			FlagCarrierMini fcm = new FlagCarrierMini();
			fcm.Start();

			exitEvent.WaitOne();
        }
	}
}
