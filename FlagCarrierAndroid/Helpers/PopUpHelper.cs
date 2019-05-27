using Android.App;
using Android.Widget;
using Plugin.CurrentActivity;

using ASnackbar = Android.Support.Design.Widget.Snackbar;

namespace FlagCarrierAndroid.Helpers
{
    public static class PopUpHelper
    {
        public static void Snackbar(string message, int duration = ASnackbar.LengthLong)
        {
            Activity activity = CrossCurrentActivity.Current.Activity;
            Android.Views.View view = activity.FindViewById(Android.Resource.Id.Content);
            ASnackbar.Make(view, message, duration).Show();
        }

        public static void Toast(string message, ToastLength length = ToastLength.Long)
        {
            Android.Widget.Toast.MakeText(Android.App.Application.Context, message, length).Show();
        }
    }
}