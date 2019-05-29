using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Android.Content;
using Android.OS;
using Android.Text;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Com.Hbb20;

using FlagCarrierBase.Helpers;
using FlagCarrierAndroid.Activities;
using FlagCarrierAndroid.Helpers;
using FlagCarrierAndroid.Services;

namespace FlagCarrierAndroid.Fragments
{
    public class WriteTagFragment : BaseFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_write, menu);

            base.OnCreateOptionsMenu(menu, inflater);
        }

        private EditText displayNameText = null;
        private CountryCodePicker ccp = null;
        private EditText speedrunNameText = null;
        private EditText twitchNameText = null;
        private EditText twitterHandleText = null;
        private EditText extraDataText = null;

        private string lookupName = null;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_write_tag, container, false);

            displayNameText = view.FindViewById<EditText>(Resource.Id.displayNameText);
            ccp = view.FindViewById<CountryCodePicker>(Resource.Id.countryCodePicker);
            speedrunNameText = view.FindViewById<EditText>(Resource.Id.speedrunNameText);
            twitchNameText = view.FindViewById<EditText>(Resource.Id.twitchNameText);
            twitterHandleText = view.FindViewById<EditText>(Resource.Id.twitterHandleText);
            extraDataText = view.FindViewById<EditText>(Resource.Id.extraDataText);

            displayNameText.TextChanged += AnyControl_TextChanged;
            speedrunNameText.TextChanged += AnyControl_TextChanged;
            twitchNameText.TextChanged += AnyControl_TextChanged;
            twitterHandleText.TextChanged += AnyControl_TextChanged;

            Button submitButton = view.FindViewById<Button>(Resource.Id.writeTagButton);
            submitButton.Click += SubmitButton_Click;

            return view;
        }

        public override void OnDestroy()
        {
            HCEService.Publish(null);

            base.OnDestroy();
        }

        private void AnyControl_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditText txt = (EditText)sender;
            lookupName = txt.Text;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.fillSetOption:
                    FillWithSettings();
                    return true;
                case Resource.Id.fromSrComOption:
                    _ = FillFromSpeedrunCom();
                    return true;
                case Resource.Id.beamWriteData:
                    Beam();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private async Task FillFromSpeedrunCom()
        {
            SpeedrunComHelperData data = null;

            try
            {
                data = await SpeedrunComHelper.GetUserInfo(lookupName);
            }
            catch (SpeedrunComHelperException e)
            {
                ShowToast(e.Message);
            }
            catch (Exception e)
            {
                ShowToast("Failed getting sr.com data: " + e.Message);
            }

            if (data == null)
                return;

            ccp.SetCountryForNameCode(data.CountryCode);
            if (!ccp.SelectedCountryNameCode.Equals(data.CountryCode, StringComparison.OrdinalIgnoreCase))
                ccp.SetCountryForNameCode(data.CountryCode.Split('/', 2)[0]);

            displayNameText.Text = data.DisplayName;
            speedrunNameText.Text = data.SrComName;
            twitchNameText.Text = data.TwitchName;
            twitterHandleText.Text = data.TwitterHandle;
            extraDataText.Text = "";
        }

        private Dictionary<string, string> GetData()
        {
            var data = new Dictionary<string, string>();

            string dspName = displayNameText.Text.Trim();
            if (dspName == "")
            {
                ShowToast("A display name is required.");
                return null;
            }

            data["display_name"] = dspName;
            data["country_code"] = ccp.SelectedCountryNameCode;

            string tmp = speedrunNameText.Text.Trim();
            if (tmp != "")
                data["speedruncom_name"] = tmp;

            tmp = twitchNameText.Text.Trim();
            if (tmp != "")
                data["twitch_name"] = tmp;

            tmp = twitterHandleText.Text.Trim();
            if (tmp != "")
                data["twitter_handle"] = tmp;

            string extra = extraDataText.Text.Trim();
            foreach (string lineRaw in extra.Split('\n'))
            {
                string line = lineRaw.Trim();
                if (line == "")
                    continue;

                string[] parts = line.Split('=', 2);
                if (parts.Length != 2)
                {
                    ShowToast("Invalid extra data");
                    return null;
                }

                if (parts[0].Length > 32)
                {
                    ShowToast("Extra data key length > 32");
                    return null;
                }

                if (parts[1].Length > 255)
                {
                    ShowToast("Extra data value length > 255");
                    return null;
                }

                data[parts[0]] = parts[1];
            }

            return data;
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            var data = GetData();
            if (data == null)
                return;

            Intent intent = new Intent(Activity, Java.Lang.Class.FromType(typeof(WriteTagActivity)));
            intent.SetAction(WriteTagActivity.WriteTagIntentAction);
            intent.PutExtra(WriteTagActivity.WriteTagIntentData, new Java.Util.HashMap(data));
            StartActivity(intent);
        }

        private void Beam()
        {
            var data = GetData();
            if (data == null)
                return;

            HCEService.Publish(data);

            ShowToast("Ready to beam data to device.");
        }

        private void FillWithSettings()
        {
            displayNameText.Text = "set";
            speedrunNameText.Text = "";
            twitchNameText.Text = "";
            twitterHandleText.Text = "";

            StringBuilder extra = new StringBuilder();

            List<string> keys = AppSettings.GetAllKeys();

            keys.Remove(AppSettings.DeviceIdKey);
            keys.Remove(AppSettings.PrivKeyKey);

            extra.Append("set=");
            extra.Append(string.Join(',', keys));

            foreach (string key in keys)
            {
                extra.Append("\n");
                extra.Append(key).Append('=').Append(AppSettings.Global.GetByKey(key));
            }

            extraDataText.Text = extra.ToString();
        }
    }
}
