using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;

using FlagCarrierBase.Helpers;
using FlagCarrierAndroid.Activities;
using FlagCarrierAndroid.Helpers;
using FlagCarrierAndroid.Services;

namespace FlagCarrierAndroid.Fragments
{
    public class WriteTagFragment : UserDataBaseFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_write, menu);

            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        public override void OnDestroy()
        {
            HCEService.Publish(null);

            base.OnDestroy();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.fillSetOption:
                    FillWithSettings();
                    return true;
                case Resource.Id.beamWriteData:
                    Beam();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }


        protected override void OnSubmit()
        {
            var data = GetData();
            if (data == null)
                return;

            Intent intent = new Intent(Activity, Java.Lang.Class.FromType(typeof(WriteTagActivity)));
            intent.SetAction(WriteTagActivity.WriteTagIntentAction);
            intent.PutExtra(WriteTagActivity.WriteTagIntentData, new Java.Util.HashMap(data));
            StartActivity(intent);
        }

        private void Beam()
        {
            var data = GetData();
            if (data == null)
                return;

            HCEService.Publish(data);

            ShowToast("Ready to beam data to device.");
        }

        private void FillWithSettings()
        {
            displayNameText.Text = "set";
            userIdText.Text = "0";

            StringBuilder extra = new StringBuilder();

            List<string> keys = AppSettings.GetAllKeys();

            keys.Remove(AppSettings.DeviceIdKey);
            keys.Remove(AppSettings.PrivKeyKey);

            extra.Append("set=");
            extra.Append(string.Join(',', keys));

            foreach (string key in keys)
            {
                extra.Append("\n");
                extra.Append(key).Append('=').Append(AppSettings.Global.GetByKey(key));
            }

            extraDataText.Text = extra.ToString();
        }
    }
}
