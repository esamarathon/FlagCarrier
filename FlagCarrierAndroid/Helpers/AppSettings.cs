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

using FlagCarrierBase.Constants;

namespace FlagCarrierAndroid.Helpers
{
    public class AppSettings : INotifyPropertyChanged
    {
        public static AppSettings Global { get; } = new AppSettings();

        private AppSettings()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Internal Helper Functions

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

        private void SetEncrypted(string key, string data, [CallerMemberName] string propertyName = null)
        {
            SetEncrypted(key, Encoding.UTF8.GetBytes(data), propertyName);
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

        private string GetEncryptedStr(string key)
        {
            var res = GetEncrypted(key);
            if (res == null)
                return null;

            return Encoding.UTF8.GetString(res);
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
                            case ushort us:
                                if (!prefs.Contains(key) || prefs.GetInt(key, 0) != us)
                                {
                                    editor.PutInt(key, us);
                                    changed = true;
                                }
                                break;
                            case int i:
                                if (!prefs.Contains(key) || prefs.GetInt(key, 0) != i)
                                {
                                    editor.PutInt(key, i);
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
                        case int i:
                            res = prefs.GetInt(key, i);
                            break;
                        case ushort us:
                            res = (ushort)prefs.GetInt(key, us);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            return (T)res;
        }

        #endregion

        #region External Helper Functions

        public void SetByKey(string key, string value)
        {
            switch (key)
            {
                case TargetUrlKey:
                    TargetUrl = value;
                    break;
                case PositionsKey:
                    Positions = value;
                    break;
                case KioskModeKey:
                    KioskMode = Convert.ToBoolean(value);
                    break;
                case DeviceIdKey:
                    DeviceId = value;
                    break;
                case GroupIdKey:
                    GroupId = value;
                    break;
                case PubKeyKey:
                    PubKey = Convert.FromBase64String(value);
                    break;
                case PrivKeyKey:
                    PrivKey = Convert.FromBase64String(value);
                    break;
                default:
                    throw new ArgumentException("Unknown settings key " + key);
            }
        }

        public string GetByKey(string key)
        {
            switch (key)
            {
                case TargetUrlKey:
                    return TargetUrl;
                case PositionsKey:
                    return Positions;
                case KioskModeKey:
                    return KioskMode.ToString();
                case DeviceIdKey:
                    return DeviceId;
                case GroupIdKey:
                    return GroupId;
                case PubKeyKey:
                    return Convert.ToBase64String(PubKey);
                case PrivKeyKey:
                    return Convert.ToBase64String(PrivKey);
                default:
                    throw new ArgumentException("Unknown settings key " + key);
            }
        }

        public static List<string> GetAllKeys()
        {
            return new List<string>()
            {
                TargetUrlKey,
                PositionsKey,
                KioskModeKey,
                DeviceIdKey,
                GroupIdKey,
                PubKeyKey,
                PrivKeyKey
            };
        }

        #endregion

        public const string TargetUrlKey = SettingsKeys.TargetUrlKey;
        public string TargetUrl
        {
            get => Get(TargetUrlKey, "https://oromit.de/hosts.php");
            set => Set(TargetUrlKey, value);
        }

        public const string PositionsKey = SettingsKeys.PositionsKey;
        public string Positions
        {
            get => Get(PositionsKey, "left,mid,right");
            set => Set(PositionsKey, value);
        }

        public const string KioskModeKey = SettingsKeys.KioskModeKey;
        public bool KioskMode
        {
            get => Get(KioskModeKey, false);
            set => Set(KioskModeKey, value);
        }

        public const string DeviceIdKey = SettingsKeys.DeviceIdKey;
        public string DeviceId
        {
            get => Get(DeviceIdKey, "SomeAndroidDevice");
            set => Set(DeviceIdKey, value);
        }

        public const string GroupIdKey = SettingsKeys.GroupIdKey;
        public string GroupId
        {
            get => Get(GroupIdKey, "");
            set => Set(GroupIdKey, value);
        }

        public const string PubKeyKey = SettingsKeys.PubKeyKey;
        public byte[] PubKey
        {
            get => Get<byte[]>(PubKeyKey, null);
            set => Set(PubKeyKey, value);
        }

        public const string PrivKeyKey = SettingsKeys.PrivKeyKey;
        public byte[] PrivKey
        {
            get => GetEncrypted(PrivKeyKey);
            set => SetEncrypted(PrivKeyKey, value);
        }

        public const string MqHostKey = SettingsKeys.MqHostKey;
        public string MqHost
        {
            get => Get(MqHostKey, "");
            set => Set(MqHostKey, value);
        }

        public const string MqPortKey = SettingsKeys.MqPortKey;
        public ushort MqPort
        {
            get => Get(MqPortKey, (ushort)0);
            set => Set(MqPortKey, value);
        }

        public const string MqTlsKey = SettingsKeys.MqTlsKey;
        public bool MqTls
        {
            get => Get(MqTlsKey, true);
            set => Set(MqTlsKey, value);
        }

        public const string MqQueueKey = SettingsKeys.MqQueueKey;
        public string MqQueue
        {
            get => Get(MqQueueKey, "flagcarrier-tag-scanned");
            set => Set(MqQueueKey, value);
        }

        public const string MqUsernameKey = SettingsKeys.MqUsernameKey;
        public string MqUsername
        {
            get => Get(MqUsernameKey, "");
            set => Set(MqUsernameKey, value);
        }

        public const string MqPasswordKey = SettingsKeys.MqPasswordKey;
        public string MqPassword
        {
            get => GetEncryptedStr(MqPasswordKey);
            set => SetEncrypted(MqPasswordKey, value);
        }
    }
}