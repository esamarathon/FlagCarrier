using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

using FlagCarrierBase;
using NdefLibrary.Ndef;

namespace FlagCarrierMini
{
	class FlagCarrierMini : IDisposable
	{
		private readonly NfcHandler nfcHandler;
		private readonly string acrReader = null;
		private MqHandler mqHandler;
		private byte[] curUid = null;

		public FlagCarrierMini()
		{
			nfcHandler = new NfcHandler();

			nfcHandler.CardAdded += NfcHandler_CardAdded;
			nfcHandler.StatusMessage += NfcHandler_StatusMessage;
			nfcHandler.ErrorMessage += NfcHandler_ErrorMessage;
			nfcHandler.ReceiveNdefMessage += NfcHandler_ReceiveNdefMessage;
			nfcHandler.CardHandlingDone += NfcHandler_CardHandlingDone;
			nfcHandler.NewTagUid += NfcHandler_NewTagUid;

			acrReader = nfcHandler.GetACRReader();

			mqHandler = new MqHandler();
		}

		public void Dispose()
		{
			if (mqHandler != null)
			{
				mqHandler.Dispose();
				mqHandler = null;
			}
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
			if (acrReader != null)
			{
				try
				{
					// This cannot be done in the middle of handling a card, hence this construct.
					if (signalFailure)
						nfcHandler.SignalFailure(acrReader);
					else if (signalSuccess)
						nfcHandler.SignalSuccess(acrReader);
				}
				catch (PCSC.Exceptions.PCSCException e)
				{
					Console.WriteLine("Failed signaling: " + e.Message);
				}
			}

			signalSuccess = signalFailure = false;
		}

		public void Start()
		{
			HandleOptions();

			nfcHandler.StartMonitoring();
			Console.WriteLine("Monitoring all readers");

			ConnectMq();

			Console.WriteLine("Ready");
		}

		private void ConnectMq()
		{
			mqHandler.Close();

			try
			{
				mqHandler.Connect(
					AppSettings.MqHost,
					AppSettings.MqUsername,
					AppSettings.MqPassword,
					AppSettings.MqPort);
				mqHandler.Subscribe(AppSettings.MqQueue);
				Console.WriteLine("Connected to MQ Server");
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed connecting to MQ Server: " + e.ToString());
			}
		}

		private void HandleOptions()
		{
			byte[] pubKey = AppSettings.PubKey;
			if (pubKey != null && pubKey.Length > 0)
			{
				Console.WriteLine("Applied public key!");
				NdefHandler.SetKeys(pubKey);
			}
		}

		private void NfcHandler_ReceiveNdefMessage(NdefMessage msg)
		{
			Dictionary<string, string> vals = NdefHandler.ParseNdefMessage(msg);

			string disp_name = null;
			if (vals.ContainsKey("display_name"))
				disp_name = vals["display_name"];

			string user_id = null;
			if (vals.ContainsKey("user_id"))
				user_id = vals["user_id"];

			bool? sigValid = null;

			if (vals.ContainsKey("sig_valid"))
			{
				if (vals["sig_valid"] != "True")
				{
					SignalFailure();
					sigValid = false;
					Console.WriteLine("Invalid signature trying to parse tag for " + disp_name);
				}
				else
				{
					SignalSuccess();
					sigValid = true;
					Console.WriteLine("Successfully detected valid tag owned by " + disp_name);
				}
			}
			else
			{
				SignalSuccess();
				Console.WriteLine("Successfully detected unverified tag owned by " + disp_name);
			}

			if (NdefHandler.HasPubKey() && sigValid != true && !AppSettings.ReportAllScans)
				return;

			if (!mqHandler.IsConnected)
			{
				Console.WriteLine("Not connected to MQ, not sending event.");
				return;
			}

			TagScannedEvent tse = new TagScannedEvent();

			tse.FlagCarrier.ID = AppSettings.ID;
			tse.FlagCarrier.Time = DateTime.UtcNow;
			tse.FlagCarrier.UID = curUid;
			tse.FlagCarrier.ValidSignature = sigValid;
			tse.FlagCarrier.PubKey = AppSettings.PubKey;

			tse.User.DisplayName = disp_name;
			tse.User.ID = user_id;

			tse.RawData = vals;

			mqHandler.Publish(tse);
		}

		private void NfcHandler_ErrorMessage(string msg)
		{
			Console.WriteLine(msg);
		}

		private void NfcHandler_StatusMessage(string msg)
		{
			if (AppSettings.Verbose)
				Console.WriteLine(msg);
		}

		private void NfcHandler_CardAdded(string readerName)
		{
			if (AppSettings.Verbose)
				Console.WriteLine("Card added!");

			NdefHandler.ClearExtraSignData();
			curUid = null;
		}

		private void NfcHandler_NewTagUid(byte[] uid)
		{
			NdefHandler.SetExtraSignData(uid);
			curUid = uid;
		}
	}
}
