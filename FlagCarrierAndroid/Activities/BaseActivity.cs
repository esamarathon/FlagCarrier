using Android.Widget;
using AndroidX.AppCompat.App;

using ASnackbar = Google.Android.Material.Snackbar.Snackbar;
using AToast = Android.Widget.Toast;

namespace FlagCarrierAndroid.Activities
{
    public class BaseActivity : AppCompatActivity
    {
        protected void ShowSnackbar(string message, int duration = ASnackbar.LengthLong)
        {
            var view = FindViewById(Android.Resource.Id.Content);
            if (view == null)
                return;

            ASnackbar.Make(view, message, duration).Show();
        }

        protected void ShowToast(string message, ToastLength length = ToastLength.Long)
        {
            AToast.MakeText(this, message, length).Show();
        }
    }
}
