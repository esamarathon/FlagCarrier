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

namespace FlagCarrierAndroid.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        public static MainActivity Instance { get; private set; } = null;

        public const string FLAGCARRIER_MIMETYPE = "application/vnd.de.oromit.flagcarrier";

        private DrawerLayout drawer = null;
        private NavigationView navigationView = null;

        private BaseFragment currentFragment = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Instance = this;

            base.OnCreate(savedInstanceState);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            SwitchToPage(Resource.Id.nav_scan_tag, false);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            return SwitchToPage(item.ItemId);
        }

        public bool SwitchToPage(int navId, bool animate = true)
        {
            switch (navId)
            {
                case Resource.Id.nav_scan_tag:
                    currentFragment = new ScanTagFragment();
                    break;
                case Resource.Id.nav_manual_login:
                    currentFragment = new ManualLoginFragment();
                    break;
                case Resource.Id.nav_write_tag:
                    currentFragment = new WriteTagFragment();
                    break;
                case Resource.Id.nav_beam_mini:
                    currentFragment = new BeamToMiniFragment();
                    break;
                case Resource.Id.nav_settings:
                    currentFragment = new SettingsFragment();
                    break;
                default:
                    return false;
            }

            navigationView.SetCheckedItem(navId);

            var tx = SupportFragmentManager.BeginTransaction();

            if (animate)
            {
                if (navId != Resource.Id.nav_scan_tag)
                    tx.SetCustomAnimations(Resource.Animation.slide_in_right, Resource.Animation.slide_out_left);
                else
                    tx.SetCustomAnimations(Resource.Animation.slide_in_left, Resource.Animation.slide_out_right);
            }

            tx.Replace(Resource.Id.content, currentFragment);
            tx.Commit();

            if (drawer.IsDrawerOpen(GravityCompat.Start))
                drawer.CloseDrawer(GravityCompat.Start);

            return true;
        }

        public override void OnBackPressed()
        {
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else if (navigationView.CheckedItem != null && navigationView.CheckedItem.ItemId != Resource.Id.nav_scan_tag)
            {
                SwitchToPage(Resource.Id.nav_scan_tag);
            }
            else
            {
                base.OnBackPressed();
            }
        }
    }
}
