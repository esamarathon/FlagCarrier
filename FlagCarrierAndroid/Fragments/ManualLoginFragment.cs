using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Android.Runtime;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Com.Hbb20;

using FlagCarrierBase.Helpers;
using FlagCarrierAndroid.Helpers;
using FlagCarrierAndroid.Activities;

namespace FlagCarrierAndroid.Fragments
{
    public class ManualLoginFragment : BaseFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_from_srcom, menu);

            base.OnCreateOptionsMenu(menu, inflater);
        }

        private EditText displayNameText = null;
        private CountryCodePicker ccp = null;
        private EditText speedrunNameText = null;
        private EditText twitchNameText = null;
        private EditText twitterHandleText = null;

        private string lookupName = null;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_write_tag, container, false);

            view.FindViewById(Resource.Id.extraDataLabel).Visibility = ViewStates.Invisible;
            view.FindViewById(Resource.Id.extraDataText).Visibility = ViewStates.Invisible;

            displayNameText = view.FindViewById<EditText>(Resource.Id.displayNameText);
            ccp = view.FindViewById<CountryCodePicker>(Resource.Id.countryCodePicker);
            speedrunNameText = view.FindViewById<EditText>(Resource.Id.speedrunNameText);
            twitchNameText = view.FindViewById<EditText>(Resource.Id.twitchNameText);
            twitterHandleText = view.FindViewById<EditText>(Resource.Id.twitterHandleText);

            displayNameText.TextChanged += AnyControl_TextChanged;
            speedrunNameText.TextChanged += AnyControl_TextChanged;
            twitchNameText.TextChanged += AnyControl_TextChanged;
            twitterHandleText.TextChanged += AnyControl_TextChanged;

            Button submitButton = view.FindViewById<Button>(Resource.Id.writeTagButton);
            submitButton.SetText(Resource.String.manual_login_submit);
            submitButton.Click += SubmitButton_Click;

            return view;
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
                case Resource.Id.fromSrComOptionSingle:
                    _ = FillFromSpeedrunCom();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private async Task FillFromSpeedrunCom()
        {
            var data = await SpeedrunComHelper.GetUserInfo(lookupName);

            if (data == null)
                return;

            displayNameText.Text = data.DisplayName;
            ccp.SetCountryForNameCode(data.CountryCode.Split('/')[0]);
            speedrunNameText.Text = data.SrComName;
            twitchNameText.Text = data.TwitchName;
            twitterHandleText.Text = data.TwitterHandle;
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

            Intent intent = new Intent(MainActivity.Instance, Java.Lang.Class.FromType(typeof(LoginActivity)));
            intent.SetAction(LoginActivity.ManualLoginIntentAction);
            intent.PutExtra(LoginActivity.ManualLoginIntentData, new Java.Util.HashMap(data));
            StartActivity(intent);

            MainActivity.Instance.SwitchToPage(Resource.Id.nav_scan_tag);
        }
    }
}
