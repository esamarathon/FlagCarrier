using System;
using System.Collections.Generic;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Com.Hbb20;

using FlagCarrierAndroid.Activities;
using FlagCarrierAndroid.Helpers;

namespace FlagCarrierAndroid.Fragments
{
    public class WriteTagFragment : BaseFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
        }

        EditText displayNameText = null;
        CountryCodePicker ccp = null;
        EditText speedrunNameText = null;
        EditText twitchNameText = null;
        EditText twitterHandleText = null;
        EditText extraDataText = null;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_write_tag, container, false);

            displayNameText = view.FindViewById<EditText>(Resource.Id.displayNameText);
            ccp = view.FindViewById<CountryCodePicker>(Resource.Id.countryCodePicker);
            speedrunNameText = view.FindViewById<EditText>(Resource.Id.speedrunNameText);
            twitchNameText = view.FindViewById<EditText>(Resource.Id.twitchNameText);
            twitterHandleText = view.FindViewById<EditText>(Resource.Id.twitterHandleText);
            extraDataText = view.FindViewById<EditText>(Resource.Id.extraDataText);

            Button submitButton = view.FindViewById<Button>(Resource.Id.writeTagButton);
            submitButton.Click += SubmitButton_Click;

            return view;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_write, menu);

            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.fillSetOption:
                    FillWithSettings();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            var data = new Dictionary<string, string>();

            string dspName = displayNameText.Text.Trim();
            if (dspName == "")
            {
                PopUpHelper.Toast("A display name is required.");
                return;
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
            foreach(string lineRaw in extra.Split('\n'))
            {
                string line = lineRaw.Trim();
                if (line == "")
                    continue;

                string[] parts = line.Split('=', 2);
                if (parts.Length != 2)
                {
                    PopUpHelper.Toast("Invalid extra data");
                    return;
                }

                if (parts[0].Length > 32)
                {
                    PopUpHelper.Toast("Extra data key length > 32");
                    return;
                }

                if (parts[1].Length > 255)
                {
                    PopUpHelper.Toast("Extra data value length > 255");
                    return;
                }

                data[parts[0]] = parts[1];
            }

            Intent intent = new Intent(MainActivity.Instance, Java.Lang.Class.FromType(typeof(WriteTagActivity)));
            intent.SetAction(WriteTagActivity.WriteTagIntentAction);
            intent.PutExtra(WriteTagActivity.WriteTagIntentData, new Java.Util.HashMap(data));
            StartActivity(intent);
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