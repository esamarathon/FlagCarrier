using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;

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
        protected EditText userIdText = null;
        protected EditText extraDataText = null;

        protected string lookupName = null;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_write_tag, container, false);

            displayNameText = view.FindViewById<EditText>(Resource.Id.displayNameText);
            userIdText = view.FindViewById<EditText>(Resource.Id.userIdText);
            extraDataText = view.FindViewById<EditText>(Resource.Id.extraDataText);

            displayNameText.TextChanged += AnyControl_TextChanged;
            userIdText.TextChanged += AnyControl_TextChanged;
            extraDataText.TextChanged += AnyControl_TextChanged;

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

        protected Dictionary<string, string> GetData()
        {
            var data = new Dictionary<string, string>();

            string dspName = displayNameText.Text.Trim();
            if (dspName == "")
            {
                ShowToast("A display name is required.");
                return null;
            }

            string userId = userIdText.Text.Trim();
            if (userId == "")
            {
                ShowToast("A user ID is required.");
                return null;
            }

            data["display_name"] = dspName;
            data["user_id"] = userId;

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
