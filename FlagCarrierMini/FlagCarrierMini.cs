using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

using FlagCarrierBase;
using NdefLibrary.Ndef;

namespace FlagCarrierMini
{
	class FlagCarrierMini
	{
		private NfcHandler nfcHandler;
		private string acrReader = null;

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
			nfcHandler.CardHandlingDone += NfcHandler_CardHandlingDone;
			nfcHandler.NewTagUid += NdefHandler.SetExtraSignData;

			acrReader = nfcHandler.GetACRReader();
		}

		private bool signalSuccess, signalFailure;

		private void SignalSuccess()
		{
			signalSuccess = true;
			signalFailure = false;
		}

		private void SignalFailure()
		{
			signalSuccess = false;
			signalFailure = true;
		}

		private void NfcHandler_CardHandlingDone(string obj)
		{
			try
			{
				// This cannot be done in the middle of handling a card, hence this construct.
				if (signalFailure && acrReader != null)
					nfcHandler.SignalFailure(acrReader);
				else if (signalSuccess && acrReader != null)
					nfcHandler.SignalSuccess(acrReader);
			}
			catch (PCSC.Exceptions.PCSCException e)
			{
				Console.WriteLine("Failed signaling: " + e.Message);
			}

			signalSuccess = signalFailure = false;
		}

		public void Start()
		{
			HandleSettings();

			nfcHandler.StartMonitoring();
			Console.WriteLine("Monitoring all readers");
		}

		private void HandleSettings()
		{
			if (Options.PubKey != null && Options.PubKey.Trim() != "")
			{
				Console.WriteLine("Updated public key from commandline.");
				AppSettings.PubKey = Options.PubKeyBin;
			}

			byte[] pubKey = AppSettings.PubKey;
			if (pubKey != null)
			{
				Console.WriteLine("Applied public key!");
				NdefHandler.SetKeys(pubKey);
			}
		}

		private void NfcHandler_ReceiveNdefMessage(NdefMessage msg)
		{
			Dictionary<string, string> vals = NdefHandler.ParseNdefMessage(msg);

			string disp_name = "an unknown display name";
			if (vals.ContainsKey("display_name"))
				disp_name = vals["display_name"];

			if (vals.ContainsKey("sig_valid"))
			{
				if (vals["sig_valid"] != "True")
				{
					SignalFailure();
					Console.WriteLine("Invalid signature trying to parse tag for " + disp_name);

					return;
				}

				SignalSuccess();
				Console.WriteLine("Successfully detected valid tag owned by " + disp_name);
			}
			else
			{
				SignalSuccess();
				Console.WriteLine("Successfully detected unverified tag owned by " + disp_name);
			}

			//TODO: Actually do stuff.
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
			NdefHandler.ClearExtraSignData();
		}
	}
}
