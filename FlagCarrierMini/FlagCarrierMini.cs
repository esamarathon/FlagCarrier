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

        private NdefHandler NdefHandler { get; } = new NdefHandler();

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
            nfcHandler.StartMonitoring();
            Console.WriteLine("Monitoring all readers");

            HandleNewSettings();

            Console.WriteLine("Ready");
        }

        private void HandleNewSettings()
        {
            ConnectMq();
        }

        private void ConnectMq()
        {
            mqHandler.Close();

            try
            {
                mqHandler.Connect(
                    AppSettings.MqHost,
                    AppSettings.MqVHost,
                    AppSettings.MqUsername,
                    AppSettings.MqPassword,
                    AppSettings.MqPort,
                    AppSettings.MqTls);
                Console.WriteLine("Connected to MQ Server");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed connecting to MQ Server: " + e.ToString());
            }
        }

        private bool? GetSigValid(Dictionary<string, string> vals)
        {
            bool? sigValid = null;

            if (vals.ContainsKey("sig_valid"))
            {
                if (vals["sig_valid"] != "True")
                {
                    sigValid = false;
                    Console.WriteLine("Invalid tag signature!");
                }
                else
                {
                    sigValid = true;
                    Console.WriteLine("Detected valid tag signature.");
                }
            }
            else
            {
                Console.WriteLine("Tag not verified.");
            }

            return sigValid;
        }

        private bool TryHandleSettings(Dictionary<string, string> vals, bool? sigValid)
        {
            if (vals.ContainsKey("display_name") && vals["display_name"] == "set" && vals.ContainsKey("set"))
            {
                if (NdefHandler.HasPubKey() && sigValid != true)
                {
                    Console.WriteLine("Rejecting settings due to invalid signature!");
                    SignalFailure();
                    return true;
                }

                string[] keys = vals["set"].Split(',');

                Dictionary<string, string> settings = new Dictionary<string, string>();

                foreach (string key in keys)
                {
                    if (!vals.ContainsKey(key))
                    {
                        Console.WriteLine("Invalid settings data, missing value for " + key);
                        return true;
                    }

                    settings.Add(key, vals[key]);
                }

                if (AppSettings.FromDict(settings))
                {
                    HandleNewSettings();

                    Console.WriteLine("Applied settings from tag.");
                    SignalSuccess();
                }
                else
                {
                    SignalFailure();
                }

                return true;
            }

            return false;
        }

        private void HandleScannedTag(Dictionary<string, string> vals, bool? sigValid)
        {
            if (!mqHandler.IsConnected)
            {
                Console.WriteLine("Not connected to MQ, not sending event.");
                return;
            }

            TagScannedEvent tse = new TagScannedEvent();

            tse.FlagCarrier.ID = AppSettings.DeviceId;
            tse.FlagCarrier.Group = AppSettings.GroupId;
            tse.FlagCarrier.UID = curUid;
            tse.FlagCarrier.ValidSignature = sigValid;
            tse.FlagCarrier.PubKey = AppSettings.PubKey;
            tse.FlagCarrier.Time = new TagScannedEvent.TimeData();

            if (vals.ContainsKey("display_name"))
                tse.User.DisplayName = vals["display_name"];
            else
                tse.User.DisplayName = "*unset*";

            if (vals.ContainsKey("user_id"))
                tse.User.ID = vals["user_id"];

            tse.RawData = vals;

            try
            {
                mqHandler.Publish(tse);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed publishing scan: " + e.Message);
            }
        }

        private void NfcHandler_ReceiveNdefMessage(NdefMessage msg)
        {
            NdefHandler.SetKeys(AppSettings.PubKey);

            Dictionary<string, string> vals = NdefHandler.ParseNdefMessage(msg);

            bool? sigValid = GetSigValid(vals);

            if (TryHandleSettings(vals, sigValid))
                return;

            if (sigValid == false)
                SignalFailure();
            else
                SignalSuccess();

            if (NdefHandler.HasPubKey() && sigValid != true && !AppSettings.ReportAllScans)
                return;

            HandleScannedTag(vals, sigValid);
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
