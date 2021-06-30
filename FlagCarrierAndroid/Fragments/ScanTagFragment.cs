using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Nfc;
using AndroidX.AppCompat.App;

using FlagCarrierBase;
using FlagCarrierAndroid.Activities;
using FlagCarrierAndroid.Helpers;

namespace FlagCarrierAndroid.Fragments
{
    public class ScanTagFragment : BaseFragment
    {
        private readonly HttpHandler httpHandler = new HttpHandler();

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_scan_tag, container, false);

            Button clearButton = view.FindViewById<Button>(Resource.Id.clearButton);
            clearButton.Click += ClearButton_Click;

            var nfcAdapter = NfcAdapter.GetDefaultAdapter(MainActivity.Instance);
            var mainTextView = view.FindViewById<TextView>(Resource.Id.mainTextView);

            if (nfcAdapter == null)
            {
                mainTextView.Text = "No NFC Adapter found!";
            }
            else if (!nfcAdapter.IsEnabled)
            {
                mainTextView.Text = "NFC Adapter is disabled!";
            }

            return view;
        }

        private void ClearButton_Click(object sender, EventArgs _)
        {
            AlertDialog.Builder b = new AlertDialog.Builder(MainActivity.Instance);
            b.SetMessage(Resource.String.clear_confirmation);
            b.SetTitle(Resource.String.clear_conf_title);
            b.SetPositiveButton(Resource.String.ok, async (s, e) => await DoClear());
            b.SetNegativeButton(Resource.String.cancel, (s, e) => { });
            b.Show();
        }

        private async Task DoClear()
        {
            try
            {
                string action = "clear";

                string res = await httpHandler.DoRequestAsync(AppSettings.Global.TargetUrl,
                                                              AppSettings.Global.DeviceId,
                                                              AppSettings.Global.GroupId,
                                                              action);

                ShowSnackbar(res);
            }
            catch (HttpHandlerException e)
            {
                ShowSnackbar(e.Message);
            }
        }
    }
}
