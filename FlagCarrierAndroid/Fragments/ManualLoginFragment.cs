using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Android.Runtime;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Com.Hbb20;

using FlagCarrierBase.Helpers;
using FlagCarrierAndroid.Helpers;
using FlagCarrierAndroid.Activities;

namespace FlagCarrierAndroid.Fragments
{
    public class ManualLoginFragment : UserDataBaseFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_from_srcom, menu);

            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = base.OnCreateView(inflater, container, savedInstanceState);

            view.FindViewById(Resource.Id.extraDataLabel).Visibility = ViewStates.Invisible;
            view.FindViewById(Resource.Id.extraDataText).Visibility = ViewStates.Invisible;

            view.FindViewById<Button>(Resource.Id.writeTagButton).SetText(Resource.String.manual_login_submit);

            return view;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.fromSrComOptionSingle:
                    _ = FillFromSpeedrunCom();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        protected override void OnSubmit()
        {
            var data = GetData();

            Intent intent = new Intent(MainActivity.Instance, Java.Lang.Class.FromType(typeof(LoginActivity)));
            intent.SetAction(LoginActivity.ManualLoginIntentAction);
            intent.PutExtra(LoginActivity.ManualLoginIntentData, new Java.Util.HashMap(data));
            StartActivity(intent);

            MainActivity.Instance.SwitchToPage(Resource.Id.nav_scan_tag);
        }
    }
}
