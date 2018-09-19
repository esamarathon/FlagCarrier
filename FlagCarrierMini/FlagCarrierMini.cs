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

		private Options options = new Options();
		public Options Options
		{
			get
			{
				return options;
			}
			set
			{
				options = value;
			}
		}

		public byte[] PubKey
		{
			set
			{
				NdefHandler.SetKeys(value);
			}
		}

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
			NdefHandler.SetKeys(options.PubKeyBin);

			nfcHandler.StartMonitoring();
			Console.WriteLine("Monitoring all readers");
		}

		private void NfcHandler_ReceiveNdefMessage(NdefMessage msg)
		{
			Dictionary<string, string> vals = NdefHandler.ParseNdefMessage(msg);

			string disp_name = "an unknown display name";
			if (vals.ContainsKey("display_name"))
				disp_name = vals["display_name"];

			if (!vals.ContainsKey("sig_valid") || vals["sig_valid"] != "True")
			{

				Console.WriteLine("Invalid signature trying to parse tag for " + disp_name);
				return;
			}

			Console.WriteLine("Successfully detected valid tag owned by " + disp_name);
			//TODO: Actually do stuff. Trigger the relaise!
		}

		private void NfcHandler_ErrorMessage(string msg)
		{
			Console.WriteLine(msg);
		}

		private void NfcHandler_StatusMessage(string msg)
		{
			if (options.Verbose)
				Console.WriteLine(msg);
		}

		private void NfcHandler_CardAdded(string readerName)
		{
			Console.WriteLine("Card added!");
		}
	}
}
