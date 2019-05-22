using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.OS;
using Plugin.CurrentActivity;
using Android.Support.V7.App;
using Android.Support.V7.Widget;

namespace FlagCarrierAndroid.Activities
{
    [Activity(Label = "FlagCarrierAndroid", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { "android.nfc.action.NDEF_DISCOVERED" }, Categories = new[] { "android.intent.category.DEFAULT" }, DataMimeType = FLAGCARRIER_MIMETYPE)]
    public class MainActivity : AppCompatActivity
    {
        public const string FLAGCARRIER_MIMETYPE = "application/vnd.de.oromit.flagcarrier";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            SupportActionBar.Title = "@string/app_name";
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}