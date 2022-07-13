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
using AndroidX.AppCompat.App;
using Google.Android.Material;

using FlagCarrierBase;
using FlagCarrierAndroid.Helpers;

using SupportToolbar = AndroidX.AppCompat.Widget.Toolbar;
using System.Threading.Tasks;
using Android.Runtime;

namespace FlagCarrierAndroid.Activities
{
    [Activity(Label = "@string/login_title", Theme = "@style/AppTheme.NoActionBar", Exported = true)]
    [IntentFilter(new[] { NfcAdapter.ActionNdefDiscovered }, Categories = new[] { Intent.CategoryDefault }, DataMimeType = NdefHandler.FLAGCARRIER_MIME_TYPE)]
    public class LoginActivity : BaseActivity
    {
        public const string ManualLoginIntentAction = "de.oromit.flagcarrier.manual_login_intent";
        public const string ManualLoginIntentData = "MANUAL_TAG_LOGIN_DATA";

        private readonly HttpHandler httpHandler = new HttpHandler();
        private readonly NdefHandler ndefHandler = new NdefHandler();

        private Dictionary<string, string> tagData = null;

        Switch clearSwitch = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTitle(Resource.String.login_title);
            SetContentView(Resource.Layout.activity_login);

            SupportToolbar toolbar = FindViewById<SupportToolbar>(Resource.Id.loginToolbar);
            SetSupportActionBar(toolbar);

            clearSwitch = FindViewById<Switch>(Resource.Id.clearOthersSwitch);

            PopulateButtons();
            ParseIntent();
        }

        private void PopulateButtons()
        {
            LinearLayout buttonsLayout = FindViewById<LinearLayout>(Resource.Id.loginButtonsLayout);

            buttonsLayout.RemoveAllViews();

            var posAvail = AppSettings.Global.Positions.Split(',').Select(s => s.Trim());

            ViewGroup.LayoutParams layoutParams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent
            );

            foreach(string pos in posAvail)
            {
                Button button = new Button(this)
                {
                    Text = pos,
                    LayoutParameters = layoutParams,
                };

                button.Click += async (s, e) => await DoLogin(pos);

                buttonsLayout.AddView(button);
            }
        }

        private async Task DoLogin(string pos)
        {
            Dictionary<string, string> extraData = new Dictionary<string, string>()
            {
                ["position"] = pos
            };

            try
            {
                string action = "login";
                if (clearSwitch.Checked)
                    action = "login_clear";

                string res = await httpHandler.DoRequestAsync(AppSettings.Global.TargetUrl,
                                                              AppSettings.Global.DeviceId,
                                                              AppSettings.Global.GroupId,
                                                              action, tagData, extraData);

                ShowToast(res);
                Finish();
            }
            catch (HttpHandlerException e)
            {
                ShowSnackbar(e.Message);
            }
        }

        private void ParseIntent()
        {
            Intent intent = Intent;

            if (Intent.Action == NfcAdapter.ActionNdefDiscovered)
            {
                ParseNdefIntent(intent);
            }
            else if (Intent.Action == ManualLoginIntentAction)
            {
                ParseManualLoginIntent(intent);
            }
            else
            {
                ShowToast("Give me a tag!");
                BackToMain();
            }
        }

        private void ParseManualLoginIntent(Intent intent)
        {
            JavaDictionary<string, string> data = new JavaDictionary<string, string>(
                intent.GetSerializableExtra(ManualLoginIntentData).Handle,
                JniHandleOwnership.DoNotRegister);

            tagData = new Dictionary<string, string>(data);

            UpdateTextView();
        }

        private void ParseNdefIntent(Intent intent)
        {
            var rawMessages = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
            if (rawMessages == null || rawMessages.Length != 1)
            {
                ShowToast("Can't handle this tag.");
                BackToMain();
                return;
            }

            NdefMessage msg = rawMessages[0] as NdefMessage;
            Tag tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;

            ndefHandler.SetExtraSignDataFromTag(tag);
            ndefHandler.SetKeys(AppSettings.Global.PubKey, AppSettings.Global.PrivKey);

            try
            {
                tagData = ndefHandler.ParseNdefMessage(msg.ToByteArray());

                if (ndefHandler.HasPubKey() && (tagData.ContainsKey(NdefHandler.SIG_VALID_KEY) || AppSettings.Global.KioskMode))
                {
                    bool sigValid = Convert.ToBoolean(tagData.GetValueOrDefault(NdefHandler.SIG_VALID_KEY, bool.FalseString));
                    if (!sigValid)
                    {
                        ShowToast("Invalid Tag Signature!");
                        BackToMain();
                        return;
                    }
                }

                CheckForSettings();
                UpdateTextView();
            }
            catch (NdefHandlerException e)
            {
                ShowToast(e.Message);
                BackToMain();
            }
            finally
            {
                ndefHandler.ClearExtraSignData();
                ndefHandler.ClearKeys();
            }
        }

        private void CheckForSettings()
        {
            const string display_name = "display_name";
            const string trigger_dsp_name = "set";
            const string trigger_name = "set";

            if (!tagData.ContainsKey(display_name) || tagData[display_name] != trigger_dsp_name || !tagData.ContainsKey(trigger_name))
                return;

            string[] settings = tagData[trigger_name].Split(',');

            StringBuilder res = new StringBuilder();
            res.AppendLine("Applied settings:");

            foreach (string setting in settings)
            {
                if (!tagData.ContainsKey(setting))
                {
                    ShowToast("Malformed settings: " + setting + " missing on tag.");
                    BackToMain();
                    return;
                }

                string val = tagData[setting];

                try
                {
                    AppSettings.Global.SetByKey(setting, val);
                    res.Append(setting).Append('=').AppendLine(val);
                }
                catch (Exception e)
                {
                    ShowToast("Failed applying setting " + setting + " from tag:" + e.Message);
                    BackToMain();
                    return;
                }
            }

            ShowToast(res.ToString());
            BackToMain();
        }

        private void UpdateTextView()
        {
            StringBuilder bldr = new StringBuilder();
            StringBuilder other = new StringBuilder();

            List<string> entryOrder = new List<string>
            {
                "display_name",
                "user_id"
            };

            AppendKnownKeys(bldr, entryOrder);
            AppendSignatureStatus(bldr);

            entryOrder.Add(NdefHandler.SIG_KEY);
            entryOrder.Add(NdefHandler.SIG_VALID_KEY);

            foreach (var entry in tagData)
            {
                if (entryOrder.Contains(entry.Key))
                    continue;

                other.Append(entry.Key);
                other.Append('=');
                other.AppendLine(entry.Value);
            }

            if (other.Length > 0)
            {
                bldr.AppendLine();
                bldr.AppendLine("Extra Data:");
                bldr.Append(other);
            }

            FindViewById<TextView>(Resource.Id.loginTextView).Text = bldr.ToString();
        }

        private void AppendKnownKeys(StringBuilder bldr, List<string> entryOrder)
        {
            foreach (var knownKey in entryOrder)
            {
                if (tagData.TryGetValue(knownKey, out string value))
                {
                    switch (knownKey)
                    {
                        case "display_name":
                            bldr.Append("Display Name");
                            break;
                        case "user_id":
                            bldr.Append("User ID");
                            break;
                        default:
                            bldr.Append(knownKey);
                            break;
                    }
                    bldr.Append(": ").AppendLine(value);
                }
            }

            bldr.AppendLine();
        }

        private void AppendSignatureStatus(StringBuilder bldr)
        {
            if (tagData.ContainsKey(NdefHandler.SIG_KEY))
            {
                if (tagData.TryGetValue(NdefHandler.SIG_VALID_KEY, out string sig_valid))
                {
                    if (sig_valid == bool.TrueString)
                    {
                        bldr.AppendLine("Signature Valid");
                    }
                    else
                    {
                        bldr.AppendLine("Signature INVALID");
                    }
                }
                else
                {
                    bldr.AppendLine("Signature Unverified");
                }
            }
            else
            {
                bldr.AppendLine("No Signature");
            }

            bldr.AppendLine();
        }

        private void BackToMain()
        {
            Intent intent = new Intent(this, Java.Lang.Class.FromType(typeof(MainActivity)));
            intent.SetFlags(ActivityFlags.ClearTop);
            StartActivity(intent);
            Finish();
        }
    }
}
