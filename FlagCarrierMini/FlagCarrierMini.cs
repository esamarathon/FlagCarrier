using System;
using System.Collections.Generic;
using System.Text;

using FlagCarrierBase;
using NdefLibrary.Ndef;

namespace FlagCarrierMini
{
	class FlagCarrierMini
	{
		private NfcHandler nfcHandler;

		public FlagCarrierMini()
		{
			nfcHandler = new NfcHandler();

			nfcHandler.CardAdded += NfcHandler_CardAdded;
			nfcHandler.StatusMessage += NfcHandler_StatusMessage;
			nfcHandler.ErrorMessage += NfcHandler_ErrorMessage;
			nfcHandler.ReceiveNdefMessage += NfcHandler_ReceiveNdefMessage;
		}

		public void Start()
		{
			nfcHandler.StartMonitoring();
			Console.WriteLine("Monitoring all readers");
		}

		private void NfcHandler_ReceiveNdefMessage(NdefMessage msg)
		{
			Console.WriteLine("Got ndef msg!");
		}

		private void NfcHandler_ErrorMessage(string msg)
		{
			Console.WriteLine(msg);
		}

		private void NfcHandler_StatusMessage(string msg)
		{
			Console.WriteLine(msg);
		}

		private void NfcHandler_CardAdded(string readerName)
		{
			Console.WriteLine("Card added!");
		}
	}
}
