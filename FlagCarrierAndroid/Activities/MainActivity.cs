using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.OS;
using Android.App;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;

using Plugin.CurrentActivity;

using FlagCarrierAndroid.Fragments;

using SupportFragment = Android.Support.V4.App.Fragment;

namespace FlagCarrierAndroid.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    [IntentFilter(new[] { "android.nfc.action.NDEF_DISCOVERED" }, Categories = new[] { "android.intent.category.DEFAULT" }, DataMimeType = FLAGCARRIER_MIMETYPE)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        public const string FLAGCARRIER_MIMETYPE = "application/vnd.de.oromit.flagcarrier";

        private DrawerLayout drawer = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);
            navigationView.SetCheckedItem(Resource.Id.nav_scan_tag);

            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.content, new ScanTagFragment())
                .Commit();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            SupportFragment fragment;

            switch (item.ItemId)
            {
                case Resource.Id.nav_scan_tag:
                    fragment = new ScanTagFragment();
                    break;
                case Resource.Id.nav_write_tag:
                    fragment = new WriteTagFragment();
                    break;
                case Resource.Id.nav_manual_login:
                case Resource.Id.nav_beam_mini:
                case Resource.Id.nav_settings:
                default:
                    return false;
            }

            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.content, fragment)
                .Commit();

            drawer.CloseDrawer(GravityCompat.Start);

            return true;
        }

        public override void OnBackPressed()
        {
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }
    }
}