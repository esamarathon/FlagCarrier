using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Views;
using Android.OS;
using Android.App;
using Android.Widget;
using Android.Nfc;
using Android.Content;
using Android.Runtime;
using Android.Nfc.Tech;

using FlagCarrierBase;
using FlagCarrierAndroid.Helpers;

using SupportToolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace FlagCarrierAndroid.Activities
{
    [Activity(Label = "@string/write_tag_title", Theme = "@style/AppTheme.NoActionBar", Exported = true)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataScheme = "esa-flagcarrier", DataHost = "write")]
    public class WriteTagActivity : BaseActivity
    {
        public const string WriteTagIntentAction = "de.oromit.flagcarrier.write_tag_intent";
        public const string WriteTagIntentData = "WRITE_TAG_WRITE_DATA";

        private NfcAdapter nfcAdapter;
        private PendingIntent pendingIntent;
        private IntentFilter[] writeTagFilters;

        private TextView writeDataView;

        private readonly NdefHandler ndefHandler = new NdefHandler();

        private Dictionary<string, string> writeData;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_write_tag);
            SetTitle(Resource.String.write_tag_title);

            SupportToolbar toolbar = FindViewById<SupportToolbar>(Resource.Id.writeTagToolbar);
            SetSupportActionBar(toolbar);

            writeDataView = FindViewById<TextView>(Resource.Id.writeDataView);

            nfcAdapter = NfcAdapter.GetDefaultAdapter(this);

            pendingIntent = PendingIntent.GetActivity(
                this,
                0,
                new Intent(this, Class)
                    .AddFlags(ActivityFlags.SingleTop),
                PendingIntentFlags.Mutable);

            writeTagFilters = new IntentFilter[]
            {
                new IntentFilter(NfcAdapter.ActionTagDiscovered),
                new IntentFilter(NfcAdapter.ActionNdefDiscovered),
                new IntentFilter(NfcAdapter.ActionTechDiscovered)
            };

            ParseIntent();
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (nfcAdapter == null)
            {
                ShowToast("No NFC Adapter found.");
                Finish();
                return;
            }

            if (!nfcAdapter.IsEnabled)
            {
                ShowToast("NFC Adapter is disabled.");
                Finish();
                return;
            }

            nfcAdapter.EnableForegroundDispatch(this, pendingIntent, writeTagFilters, null);
        }

        protected override void OnPause()
        {
            base.OnPause();

            if (nfcAdapter != null)
                nfcAdapter.DisableForegroundDispatch(this);
        }

        private void ParseIntent()
        {
            Intent intent = Intent;

            if (intent.Action == WriteTagIntentAction)
            {
                ParseWriteTagIntent(intent);
            }
            else if (intent.Action == Intent.ActionView)
            {
                ParseUrlActivation(intent);
            }
            else
            {
                ShowToast("Unknown WriteTag Intent: " + intent.Action);
                Finish();
            }
        }

        private void ParseWriteTagIntent(Intent intent)
        {
            JavaDictionary<string, string> data = new JavaDictionary<string, string>(
                intent.GetSerializableExtra(WriteTagIntentData).Handle,
                JniHandleOwnership.DoNotRegister);

            writeData = new Dictionary<string, string>(data);

            if (writeData.Count == 0)
            {
                ShowToast("Got empty data to write to tag!");
                Finish();
                return;
            }

            RefreshWriteDataView();
        }

        private void ParseUrlActivation(Intent intent)
        {
            var uri = intent.Data;

            if (uri is null || uri.Scheme != "esa-flagcarrier" || uri.Host != "write")
            {
                ShowToast("Invalid url scheme activation!");
                Finish();
                return;
            }

            if (uri.QueryParameterNames?.Any() != true)
            {
                ShowToast("No data to be written!");
                Finish();
                return;
            }

            writeData = new Dictionary<string, string>();
            foreach (string key in uri.QueryParameterNames)
                writeData[key] = uri.GetQueryParameter(key);

            RefreshWriteDataView();
        }

        private void RefreshWriteDataView()
        {
            writeDataView.Text = writeData
                .Select(kv => kv.Key + "=" + kv.Value)
                .Aggregate((cur, next) => cur + "\n" + next);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            string action = intent.Action;
            if (action == NfcAdapter.ActionTagDiscovered ||
                action == NfcAdapter.ActionNdefDiscovered ||
                action == NfcAdapter.ActionTechDiscovered)
            {
                Tag tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
                WriteDataToTag(tag);
            }
        }

        private void WriteDataToTag(Tag tag)
        {
            try
            {
                ndefHandler.SetKeys(AppSettings.Global.PubKey, AppSettings.Global.PrivKey);
                ndefHandler.SetExtraSignDataFromTag(tag);

                byte[] rawNdefMsg = ndefHandler.GenerateRawNdefMessage(writeData);
                NdefMessage ndefMsg = new NdefMessage(rawNdefMsg);

                bool? written = WriteNdef(tag, ndefMsg);

                if (written == false)
                    written = FormatNdef(tag, ndefMsg);

                if (written == true)
                {
                    ShowToast("Written " + ndefMsg.ByteArrayLength + " bytes of NDEF data.");
                    Finish();
                }
            }
            finally
            {
                ndefHandler.ClearKeys();
                ndefHandler.ClearExtraSignData();
            }
        }

        private bool? WriteNdef(Tag tag, NdefMessage msg)
        {
            Ndef ndef = Ndef.Get(tag);
            if (ndef == null)
                return false;

            try
            {
                ndef.Connect();

                if (!ndef.IsWritable)
                {
                    ShowToast("Tag is not writable.");
                    return null;
                }

                if (msg.ByteArrayLength > ndef.MaxSize)
                {
                    ShowToast("Tag is too small: " + msg.ByteArrayLength + "/" + ndef.MaxSize);
                    return null;
                }

                ndef.WriteNdefMessage(msg);
            }
            catch (Exception e)
            {
                ShowToast("Failed writing to tag: " + e.Message);
                return null;
            }
            finally
            {
                try
                {
                    ndef.Close();
                }
                catch (Exception)
                {
                    ShowToast("Tag connection failed to close.");
                }
            }

            return true;
        }

        private bool? FormatNdef(Tag tag, NdefMessage ndefMsg)
        {
            NdefFormatable ndef = NdefFormatable.Get(tag);
            if (ndef == null)
            {
                ShowToast("Tag is not NDEF formatable.");
                return false;
            }

            try
            {
                ndef.Connect();
                ndef.Format(ndefMsg);
            }
            catch (Exception e)
            {
                ShowToast("Failed formating tag: " + e.Message);
                return null;
            }
            finally
            {
                try
                {
                    ndef.Close();
                }
                catch (Exception)
                {
                    ShowToast("Tag connection failed to close.");
                }
            }

            return true;
        }
    }
}
