using System;
using System.Collections.Generic;
using System.Text;

using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

using FlagCarrierAndroid.Helpers;

namespace FlagCarrierAndroid.Fragments
{
    public class ManualLoginFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_write_tag, container, false);

            view.FindViewById(Resource.Id.extraDataLabel).Visibility = ViewStates.Invisible;
            view.FindViewById(Resource.Id.extraDataText).Visibility = ViewStates.Invisible;

            Button submitButton = view.FindViewById<Button>(Resource.Id.writeTagButton);
            submitButton.SetText(Resource.String.manual_login_submit);
            submitButton.Click += SubmitButton_Click;

            return view;
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            
        }
    }
}