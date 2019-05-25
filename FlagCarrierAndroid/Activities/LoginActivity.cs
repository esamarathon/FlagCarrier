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
using Android.Support.V7.App;
using Android.Support.Design.Widget;

using FlagCarrierBase;
using FlagCarrierAndroid.Helpers;

using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using System.Threading.Tasks;

namespace FlagCarrierAndroid.Activities
{
    [Activity(Label = "@string/login_title", Theme = "@style/AppTheme.NoActionBar")]
    [IntentFilter(new[] { NfcAdapter.ActionNdefDiscovered }, Categories = new[] { Intent.CategoryDefault }, DataMimeType = NdefHandler.FLAGCARRIER_MIME_TYPE)]
    public class LoginActivity : AppCompatActivity
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

                PopUpHelper.Toast(res);
                Finish();
            }
            catch (HttpHandlerException e)
            {
                PopUpHelper.Snackbar(e.Message);
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
                PopUpHelper.Toast("Give me a tag!");
                BackToMain();
            }
        }

        private void ParseManualLoginIntent(Intent intent)
        {
            // TODO: Test this
            tagData = (Dictionary<string, string>)intent.GetSerializableExtra(ManualLoginIntentData);
            UpdateTextView();
        }

        private void ParseNdefIntent(Intent intent)
        {
            var rawMessages = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
            if (rawMessages == null || rawMessages.Length != 1)
            {
                PopUpHelper.Toast("Can't handle this tag.");
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

                if (ndefHandler.HasPubKey() && tagData.ContainsKey(NdefHandler.SIG_VALID_KEY))
                {
                    bool sigValid = Convert.ToBoolean(tagData[NdefHandler.SIG_VALID_KEY]);
                    if (!sigValid)
                    {
                        PopUpHelper.Toast("Invalid Tag Signature!");
                        BackToMain();
                        return;
                    }
                }

                CheckForSettings();
                UpdateTextView();
            }
            catch (NdefHandlerException e)
            {
                PopUpHelper.Toast(e.Message);
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
                    PopUpHelper.Toast("Malformed settings: " + setting + " missing on tag.");
                    BackToMain();
                    return;
                }

                string val = tagData[setting];

                try
                {
                    switch (setting)
                    {
                        case AppSettings.TargetUrlKey:
                            AppSettings.Global.TargetUrl = val;
                            res.Append(setting).Append('=').AppendLine(val);
                            break;
                        case AppSettings.PositionsKey:
                            AppSettings.Global.Positions = val;
                            res.Append(setting).Append('=').AppendLine(val);
                            break;
                        case AppSettings.KioskModeKey:
                            AppSettings.Global.KioskMode = bool.Parse(val);
                            res.Append(setting).Append('=').AppendLine(bool.Parse(val).ToString());
                            break;
                        case AppSettings.DeviceIdKey:
                            AppSettings.Global.DeviceId = val;
                            res.Append(setting).Append('=').AppendLine(val);
                            break;
                        case AppSettings.GroupIdKey:
                            AppSettings.Global.GroupId = val;
                            res.Append(setting).Append('=').AppendLine(val);
                            break;
                        case AppSettings.PubKeyKey:
                            AppSettings.Global.PubKey = Convert.FromBase64String(val);
                            res.Append(setting).Append('=').AppendLine(val);
                            break;
                        case AppSettings.PrivKeyKey:
                            AppSettings.Global.PrivKey = Convert.FromBase64String(val);
                            res.Append(setting).Append('=').AppendLine(val);
                            break;
                        default:
                            throw new Exception("Unknown setting");
                    }
                }
                catch (Exception e)
                {
                    PopUpHelper.Toast("Failed applying setting " + setting + " from tag:" + e.Message);
                    BackToMain();
                    return;
                }
            }

            PopUpHelper.Toast(res.ToString());
            BackToMain();
        }

        private void UpdateTextView()
        {
            StringBuilder bldr = new StringBuilder();
            StringBuilder other = new StringBuilder();

            Dictionary<string, string> knownIdx = new Dictionary<string, string>
            {
                ["display_name"] = "Display Name: ",
                ["country_code"] = "Country Code: ",
                ["speedruncom_name"] = "speedrun.com Name: ",
                ["twitch_name"] = "Twitch Name: ",
                ["twitter_handle"] = "Twitter Handle: "
            };

            foreach (var entry in tagData)
            {
                if (knownIdx.ContainsKey(entry.Key))
                {
                    bldr.Append(knownIdx[entry.Key]);
                    bldr.AppendLine(entry.Value);
                }
                else
                {
                    other.Append(entry.Key);
                    other.Append('=');
                    other.AppendLine(entry.Value);
                }
            }

            if (other.Length > 0)
            {
                bldr.AppendLine();
                bldr.AppendLine("Extra Data:");
                bldr.Append(other);
            }

            FindViewById<TextView>(Resource.Id.loginTextView).Text = bldr.ToString();
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