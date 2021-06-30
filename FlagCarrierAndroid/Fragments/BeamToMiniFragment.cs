using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Nfc;

using FlagCarrierBase.Constants;
using FlagCarrierAndroid.Services;
using FlagCarrierAndroid.Helpers;

namespace FlagCarrierAndroid.Fragments
{
    public class BeamToMiniFragment : BaseFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        private EditText deviceIdText;
        private EditText groupIdText;
        private Switch reportAllScansSwitch;
        private Switch verboseSwitch;
        private EditText pubKeyText;
        private EditText mqHostText;
        private EditText mqVHostText;
        private EditText mqPortText;
        private Switch mqTlsSwitch;
        private EditText mqUserText;
        private EditText mqPassText;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_beam_to_mini, container, false);

            deviceIdText = view.FindViewById<EditText>(Resource.Id.deviceIdText);
            groupIdText = view.FindViewById<EditText>(Resource.Id.groupIdText);
            reportAllScansSwitch = view.FindViewById<Switch>(Resource.Id.reportAllScansSwitch);
            verboseSwitch = view.FindViewById<Switch>(Resource.Id.verboseSwitch);
            pubKeyText = view.FindViewById<EditText>(Resource.Id.pubKeyText);
            mqHostText = view.FindViewById<EditText>(Resource.Id.mqHostText);
            mqVHostText = view.FindViewById<EditText>(Resource.Id.mqVHostText);
            mqPortText = view.FindViewById<EditText>(Resource.Id.mqPortText);
            mqTlsSwitch = view.FindViewById<Switch>(Resource.Id.mqTlsSwitch);
            mqUserText = view.FindViewById<EditText>(Resource.Id.mqUserText);
            mqPassText = view.FindViewById<EditText>(Resource.Id.mqPassText);

            pubKeyText.Text = Convert.ToBase64String(AppSettings.Global.PubKey ?? new byte[0]);

            mqHostText.Text = AppSettings.Global.MqHost;
            mqVHostText.Text = AppSettings.Global.MqVHost;
            mqPortText.Text = AppSettings.Global.MqPort.ToString();
            mqTlsSwitch.Checked = AppSettings.Global.MqTls;
            mqUserText.Text = AppSettings.Global.MqUsername;
            mqPassText.Text = AppSettings.Global.MqPassword;

            view.FindViewById<Button>(Resource.Id.beamButton).Click += BeamButton_Click;

            return view;
        }

        public override void OnDestroy()
        {
            HCEService.Publish(null);

            base.OnDestroy();
        }

        private void BeamButton_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> settingsDict = new Dictionary<string, string>();

            string deviceId = deviceIdText.Text.Trim();
            if (deviceId == "null")
                settingsDict[SettingsKeys.DeviceIdKey] = "";
            else if (deviceId != "")
                settingsDict[SettingsKeys.DeviceIdKey] = deviceId;

            string groupId = groupIdText.Text.Trim();
            if (groupId == "null")
                settingsDict[SettingsKeys.GroupIdKey] = "";
            else if (groupId != "")
                settingsDict[SettingsKeys.GroupIdKey] = groupId;

            settingsDict[SettingsKeys.ReportAllScansKey] = reportAllScansSwitch.Checked.ToString();
            settingsDict[SettingsKeys.VerboseKey] = verboseSwitch.Checked.ToString();

            string pubKey = pubKeyText.Text.Trim();
            if (pubKey == "null")
                settingsDict[SettingsKeys.PubKeyKey] = "";
            else if(pubKey != "")
                settingsDict[SettingsKeys.PubKeyKey] = pubKey;

            string mqHost = mqHostText.Text.Trim();
            if (mqHost == "null")
            {
                settingsDict[SettingsKeys.MqHostKey] = "";
            }
            else if (mqHost != "")
            {
                settingsDict[SettingsKeys.MqHostKey] = mqHost;

                settingsDict[SettingsKeys.MqTlsKey] = mqTlsSwitch.Checked.ToString();
            }

            string mqVhost = mqVHostText.Text.Trim();
            if (mqVhost == "null")
                settingsDict[SettingsKeys.MqVHostKey] = "";
            else if (mqVhost != "")
                settingsDict[SettingsKeys.MqVHostKey] = mqVhost;

            string mqPort = mqPortText.Text.Trim();
            if (mqPort == "null")
                settingsDict[SettingsKeys.MqPortKey] = "";
            else if (mqPort != "")
                settingsDict[SettingsKeys.MqPortKey] = mqPort;

            string mqUser = mqUserText.Text.Trim();
            if (mqUser == "null")
            {
                settingsDict[SettingsKeys.MqUsernameKey] = "";

                settingsDict[SettingsKeys.MqPasswordKey] = mqPassText.Text;
            }
            else if (mqUser != "")
            {
                settingsDict[SettingsKeys.MqUsernameKey] = mqUser;

                settingsDict[SettingsKeys.MqPasswordKey] = mqPassText.Text;
            }

            AppSettings.Global.MqHost = mqHost;
            AppSettings.Global.MqVHost = mqVhost;
            AppSettings.Global.MqPort = (ushort)Convert.ToInt64(mqPort);
            AppSettings.Global.MqTls = mqTlsSwitch.Checked;
            AppSettings.Global.MqUsername = mqUser;
            AppSettings.Global.MqPassword = mqPassText.Text;

            string activeSettings = string.Join(',', settingsDict.Keys);

            settingsDict["display_name"] = "set";
            settingsDict["set"] = activeSettings;

            HCEService.Publish(settingsDict);

            ShowToast("Scan phone with Flag Carrier Mini device now to beam settings!");
        }
    }
}
