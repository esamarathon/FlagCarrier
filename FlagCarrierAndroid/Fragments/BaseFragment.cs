using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Nfc;

using AndroidX.Fragment.App;

using ASnackbar = Google.Android.Material.Snackbar.Snackbar;
using AToast = Android.Widget.Toast;

namespace FlagCarrierAndroid.Fragments
{
    public class BaseFragment : Fragment
    {
        protected void ShowSnackbar(string message, int duration = ASnackbar.LengthLong)
        {
            var activity = Activity;
            if (activity == null)
                return;

            var view = activity.FindViewById(Android.Resource.Id.Content);
            if (view == null)
                return;

            ASnackbar.Make(view, message, duration).Show();
        }

        protected void ShowToast(string message, ToastLength length = ToastLength.Long)
        {
            var activity = Activity;
            if (activity == null)
                return;

            AToast.MakeText(activity, message, length).Show();
        }
    }

}
