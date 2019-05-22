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
    public class ScanTagFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_scan_tag, container, false);

            Button clearButton = view.FindViewById<Button>(Resource.Id.clearButton);
            clearButton.Click += ClearButton_Click;

            return view;
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            PopUpHelper.Snackbar("I should clear stuff now...");
        }
    }
}