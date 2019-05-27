using System;
using System.Threading;

namespace FlagCarrierMini
{
    class Program
    {
        private static readonly ManualResetEvent exitEvent = new ManualResetEvent(false);
        private static readonly FlagCarrierMini flagCarrier = new FlagCarrierMini();

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            if (!AppSettings.FromArgs(args))
                return;

            flagCarrier.Start();

            exitEvent.WaitOne();

            flagCarrier.Dispose();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            exitEvent.Set();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs args)
        {
            flagCarrier.Dispose();
            exitEvent.Set();
        }
    }
}
