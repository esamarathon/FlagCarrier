using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Com.Hbb20;

using FlagCarrierBase.Helpers;
using FlagCarrierAndroid.Activities;
using FlagCarrierAndroid.Helpers;
using FlagCarrierAndroid.Services;

namespace FlagCarrierAndroid.Fragments
{
    public class UserDataBaseFragment : BaseFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        protected EditText displayNameText = null;
        protected EditText pronounsText = null;
        protected CountryCodePicker ccp = null;
        protected EditText speedrunNameText = null;
        protected EditText twitchNameText = null;
        protected EditText twitterHandleText = null;
        protected EditText extraDataText = null;

        protected string lookupName = null;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_write_tag, container, false);

            displayNameText = view.FindViewById<EditText>(Resource.Id.displayNameText);
            pronounsText = view.FindViewById<EditText>(Resource.Id.pronounsText);
            ccp = view.FindViewById<CountryCodePicker>(Resource.Id.countryCodePicker);
            speedrunNameText = view.FindViewById<EditText>(Resource.Id.speedrunNameText);
            twitchNameText = view.FindViewById<EditText>(Resource.Id.twitchNameText);
            twitterHandleText = view.FindViewById<EditText>(Resource.Id.twitterHandleText);
            extraDataText = view.FindViewById<EditText>(Resource.Id.extraDataText);

            displayNameText.TextChanged += AnyControl_TextChanged;
            pronounsText.TextChanged += AnyControl_TextChanged;
            speedrunNameText.TextChanged += AnyControl_TextChanged;
            twitchNameText.TextChanged += AnyControl_TextChanged;
            twitterHandleText.TextChanged += AnyControl_TextChanged;

            Button submitButton = view.FindViewById<Button>(Resource.Id.writeTagButton);
            submitButton.Click += SubmitButton_Click;

            return view;
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            OnSubmit();
        }

        private void AnyControl_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditText txt = (EditText)sender;
            lookupName = txt.Text;
        }

        protected virtual void OnSubmit()
        {
            throw new NotImplementedException();
        }

        protected async Task FillFromSpeedrunCom()
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
            pronounsText.Text = data.Pronouns;
            speedrunNameText.Text = data.SrComName;
            twitchNameText.Text = data.TwitchName;
            twitterHandleText.Text = data.TwitterHandle;
            extraDataText.Text = "";
        }

        protected Dictionary<string, string> GetData()
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

            string tmp = pronounsText.Text.Trim();
            if (tmp != "")
                data["pronouns"] = tmp;

            tmp = speedrunNameText.Text.Trim();
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
    }
}
