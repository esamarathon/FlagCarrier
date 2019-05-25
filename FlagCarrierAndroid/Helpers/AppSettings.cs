using Android.App;
using Android.Preferences;
using Android.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Javax.Crypto;

namespace FlagCarrierAndroid.Helpers
{
    public class AppSettings : INotifyPropertyChanged
    {
        public static AppSettings Global { get; } = new AppSettings();

        private AppSettings()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Helper Functions

        private readonly AndroidKeyStore keyStore = new AndroidKeyStore();

        private void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string HashKey(string key)
        {
            using (var sha = SHA1.Create())
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(key)));
        }

        private void SetEncrypted(string key, byte[] data, [CallerMemberName] string propertyName = null)
        {
            bool changed = false;

            key = HashKey(key);

            lock (this)
            {
                string encryptedValue = data != null ? Convert.ToBase64String(keyStore.Encrypt(data)) : null;

                using (var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context))
                using (var editor = prefs.Edit())
                {
                    if (encryptedValue == null)
                    {
                        if (prefs.Contains(key))
                        {
                            editor.Remove(key);
                            changed = true;
                        }
                    }
                    else
                    {
                        if (prefs.GetString(key, null) != encryptedValue)
                        {
                            editor.PutString(key, encryptedValue);
                            changed = true;
                        }
                    }

                    editor.Apply();
                }
            }

            if (changed)
                Notify(propertyName);
        }

        private byte[] GetEncrypted(string key)
        {
            key = HashKey(key);

            lock (this)
            {
                string encVal;

                using (var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context))
                {
                    encVal = prefs.GetString(key, null);

                    if (encVal == null)
                        return null;
                }

                try
                {
                    byte[] encData = Convert.FromBase64String(encVal);
                    return keyStore.Decrypt(encData);
                }
                catch (FormatException)
                {
                    return null;
                }
                catch (AEADBadTagException)
                {
                    return null;
                }
            }
        }

        private void Set<T>(string key, T value, [CallerMemberName] string propertyName = null)
        {
            bool changed = false;

            lock (this)
            {
                using (var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context))
                using (var editor = prefs.Edit())
                {
                    if (value == null)
                    {
                        if (prefs.Contains(key))
                        {
                            editor.Remove(key);
                            changed = true;
                        }
                    }
                    else
                    {
                        switch (value)
                        {
                            case string s:
                                if (!prefs.Contains(key) || prefs.GetString(key, null) != s)
                                {
                                    editor.PutString(key, s);
                                    changed = true;
                                }
                                break;
                            case string[] sr:
                                if (!prefs.Contains(key) || !sr.SequenceEqual(prefs.GetStringSet(key, null)))
                                {
                                    editor.PutStringSet(key, sr);
                                    changed = true;
                                }
                                break;
                            case bool b:
                                if (!prefs.Contains(key) || prefs.GetBoolean(key, false) != b)
                                {
                                    editor.PutBoolean(key, b);
                                    changed = true;
                                }
                                break;
                            case byte[] br:
                                string brs = Convert.ToBase64String(br);
                                if (!prefs.Contains(key) || prefs.GetString(key, null) != brs)
                                {
                                    editor.PutString(key, brs);
                                    changed = true;
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }

                    editor.Apply();
                }
            }

            if (changed)
                Notify(propertyName);
        }

        private T Get<T>(string key, T defaultValue)
        {
            object res = null;

            lock (this)
            {
                using (var prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context))
                {
                    if (defaultValue == null)
                    {
                        if (typeof(T) == typeof(string))
                        {
                            res = prefs.GetString(key, null);
                        }
                        else if (typeof(T) == typeof(byte[]))
                        {
                            string brs = prefs.GetString(key, null);
                            if (brs != null)
                                res = Convert.FromBase64String(brs);
                        }
                        else if (typeof(T) == typeof(string[]))
                        {
                            var sr = prefs.GetStringSet(key, null);
                            if (sr != null)
                                res = sr.ToArray();
                        }

                        return (T)res;
                    }

                    switch (defaultValue)
                    {
                        case string s:
                            res = prefs.GetString(key, s);
                            break;
                        case string[] sr:
                            res = prefs.GetStringSet(key, sr).ToArray();
                            break;
                        case bool b:
                            res = prefs.GetBoolean(key, b);
                            break;
                        case byte[] br:
                            string brs = prefs.GetString(key, null);
                            if (brs != null)
                                res = Convert.FromBase64String(brs);
                            else
                                res = br;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            return (T)res;
        }

        #endregion

        public const string TargetUrlKey = "target_url";
        public string TargetUrl
        {
            get => Get(TargetUrlKey, "https://oromit.de/hosts.php");
            set => Set(TargetUrlKey, value);
        }

        public const string PositionsKey = "pos_avail";
        public string Positions
        {
            get => Get(PositionsKey, "left,mid,right");
            set => Set(PositionsKey, value);
        }

        public const string KioskModeKey = "kiosk_mode";
        public bool KioskMode
        {
            get => Get(KioskModeKey, false);
            set => Set(KioskModeKey, value);
        }

        public const string DeviceIdKey = "device_id";
        public string DeviceId
        {
            get => Get(DeviceIdKey, "SomeAndroidDevice");
            set => Set(DeviceIdKey, value);
        }

        public const string GroupIdKey = "group_id";
        public string GroupId
        {
            get => Get(GroupIdKey, "");
            set => Set(GroupIdKey, value);
        }

        public const string PubKeyKey = "pub_key";
        public byte[] PubKey
        {
            get => Get<byte[]>(PubKeyKey, null);
            set => Set(PubKeyKey, value);
        }

        public const string PrivKeyKey = "priv_key";
        public byte[] PrivKey
        {
            get => GetEncrypted(PrivKeyKey);
            set => SetEncrypted(PrivKeyKey, value);
        }
    }
}