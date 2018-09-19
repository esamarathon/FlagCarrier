using System;
using System.Threading;

using CommandLine;

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

			Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(options =>
				{
					exitEvent.Set();
				})
				.WithParsed(options =>
				{
					fcm.Options = options;
					fcm.Start();
				});

			exitEvent.WaitOne();
        }
	}
}
