using Android.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace FlagCarrierAndroid.Helpers
{
    public class AppSettings : INotifyPropertyChanged
    {
        const string TAG = "AppSettings";

        private static readonly AppSettings inst = new AppSettings();
        public static AppSettings Global { get => inst;  }

        private AppSettings()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        const string TargetUrlKey = "target_url";
        public string TargetUrl
        {
            get => Preferences.Get(TargetUrlKey, "");
            set
            {
                if (value == TargetUrl)
                    return;

                Preferences.Set(TargetUrlKey, value);
                Notify();
            }
        }

        const string PositionsKey = "pos_avail";
        public string[] Positions
        {
            get => Preferences.Get(PositionsKey, "").Split(',');
            set
            {
                string v = "";
                if (value != null)
                    v = string.Join(',', value);

                if (v == Preferences.Get(PositionsKey, ""))
                    return;

                Preferences.Set(PositionsKey, v);
                Notify();
            }
        }

        const string KioskModeKey = "kiosk_mode";
        public bool KioskMode
        {
            get => Preferences.Get(KioskModeKey, false);
            set
            {
                if (value == KioskMode)
                    return;

                Preferences.Set(KioskModeKey, value);
                Notify();
            }
        }

        const string DeviceIdKey = "device_id";
        public string DeviceId
        {
            get => Preferences.Get(DeviceIdKey, "");
            set
            {
                if (value == DeviceId)
                    return;

                Preferences.Set(DeviceIdKey, value);
                Notify();
            }
        }

        const string GroupIdKey = "group_id";
        public string GroupId
        {
            get => Preferences.Get(GroupIdKey, "");
            set
            {
                if (value == GroupId)
                    return;

                Preferences.Set(GroupIdKey, value);
                Notify();
            }
        }

        const string PubKeyKey = "pub_key";
        public byte[] PubKey
        {
            get
            {
                try
                {
                    return Convert.FromBase64String(Preferences.Get(PubKeyKey, ""));
                } catch (Exception)
                {
                    return null;
                }
            }
            set
            {
                string v = "";
                if (value != null && value.Length > 0)
                    v = Convert.ToBase64String(value);

                Preferences.Set(PubKeyKey, v);
                Notify();
            }
        }

        const string PrivKeyKey = "priv_key";

        public async Task<byte[]> GetPrivKey()
        {
            try
            {
                string v = await SecureStorage.GetAsync(PrivKeyKey);
                return Convert.FromBase64String(v);
            }
            catch (Exception)
            {
                Log.Error(TAG, "Failed reading from secure storage!");
                return null;
            }
        }

        public async void SetPrivKey(byte[] privKey)
        {
            string v = "";
            if (privKey != null && privKey.Length > 0)
                v = Convert.ToBase64String(privKey);

            try
            {
                await SecureStorage.SetAsync(PrivKeyKey, v);
            }
            catch (Exception)
            {
                Log.Error(TAG, "Failed writing to secure storage!");
                return;
            }

            Notify();
        }
    }
}