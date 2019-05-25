using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using FlagCarrierBase;
using FlagCarrierAndroid.Activities;
using FlagCarrierAndroid.Helpers;

namespace FlagCarrierAndroid.Fragments
{
    public class ScanTagFragment : Fragment
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

            return view;
        }

        private void ClearButton_Click(object sender, EventArgs _)
        {
            AlertDialog.Builder b = new AlertDialog.Builder(MainActivity.Instance);
            b.SetMessage(Resource.String.clear_confirmation);
            b.SetTitle(Resource.String.clear_conf_title);
            b.SetPositiveButton(Android.Resource.String.Yes, async (s, e) => await DoClear());
            b.SetNegativeButton(Android.Resource.String.No, (s, e) => { });
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

                PopUpHelper.Snackbar(res);
            }
            catch (HttpHandlerException e)
            {
                PopUpHelper.Snackbar(e.Message);
            }
        }
    }
}