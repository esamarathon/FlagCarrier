using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Reflection;

using FlagCarrierBase.Constants;

namespace FlagCarrierMini
{
    class AppSettings
    {
        private static readonly Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

        static AppSettings()
        {
            foreach (PropertyInfo prop in typeof(AppSettings).GetProperties(BindingFlags.Static | BindingFlags.Public))
                prop.SetValue(null, prop.GetValue(null));
        }

        internal static bool FromArgs(IEnumerable<string> args)
        {
            IEnumerator<string> en = args.GetEnumerator();
            Dictionary<string, string> values = new Dictionary<string, string>();

            while (en.MoveNext())
            {
                switch (en.Current)
                {
                    case "-v":
                        verbose = true;
                        continue;
                    case "--help":
                        PrintHelp();
                        return false;
                    default:
                        break;
                }

                if (!en.Current.StartsWith("--"))
                {
                    Console.WriteLine("Invalid option " + en.Current);
                    return false;
                }
                string currentSetting = en.Current.Substring(2);

                if (currentSetting.Contains("="))
                {
                    string[] kv = currentSetting.Split("=", 2);
                    values.Add(kv[0], kv[1]);
                }
                else
                {
                    if (!en.MoveNext())
                    {
                        Console.WriteLine("Missing value for argument " + currentSetting);
                        return false;
                    }

                    values.Add(currentSetting, en.Current);
                }
            }

            return FromDict(values);
        }

        private static PropertyInfo FindPropByKey(string key)
        {
            foreach (PropertyInfo prop in typeof(AppSettings).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                FieldInfo keyField = typeof(AppSettings).GetField(prop.Name + "Key", BindingFlags.Public | BindingFlags.Static);
                if (keyField == null || keyField.IsInitOnly || !keyField.IsLiteral)
                    continue;

                if (keyField.GetRawConstantValue().ToString() == key)
                    return prop;
            }

            return null;
        }

        public static bool FromDict(Dictionary<string, string> values)
        {
            foreach (var kv in values)
            {
                PropertyInfo prop = FindPropByKey(kv.Key);
                if (prop == null)
                {
                    Console.WriteLine("Unknown Option " + kv.Key);
                    return false;
                }

                // Backup original value in case of format errors
                string oval = Get(kv.Key, null);

                try
                {
                    Set(kv.Key, kv.Value);

                    // Load and set via propety to normalize and verify format
                    object nval = prop.GetValue(null);
                    prop.SetValue(null, nval);
                }
                catch (Exception e)
                {
                    // Restore original value on error
                    Set(kv.Key, oval);

                    Console.WriteLine("Failed setting option " + kv.Key + ": " + e.Message);
                    return false;
                }
            }

            return true;
        }

        public static void PrintHelp()
        {
            string lPref = "   ";
            int rOffset = 35;
            string so(string s) => (lPref + s + " ").PadRight(rOffset, ' ');

            Console.WriteLine("Available options:");
            Console.WriteLine(so("--help") + "Print this help text.");
            Console.WriteLine(so("-v") + "Enable Verbose Output");
            Console.WriteLine();
            Console.WriteLine("Available settings:");
            foreach (PropertyInfo prop in typeof(AppSettings).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                FieldInfo keyField = typeof(AppSettings).GetField(prop.Name + "Key", BindingFlags.Public | BindingFlags.Static);
                if (keyField == null || keyField.IsInitOnly || !keyField.IsLiteral)
                    continue;

                Console.WriteLine(so("--" + keyField.GetRawConstantValue().ToString() + "=" + Get(keyField.GetRawConstantValue().ToString(), null) + " [" + prop.PropertyType.Name + "]"));
            }
            Console.WriteLine();
            Console.WriteLine("All settings will be persisted to the application configuration automatically.");
        }

        private static string Get(string name, string defaultValue = null)
        {
            var cfg = config.AppSettings.Settings[name];
            if (cfg != null)
                return cfg.Value;
            else
                return defaultValue;
        }

        private static void Set(string name, string value)
        {
            if (config.AppSettings.Settings[name] != null)
                config.AppSettings.Settings.Remove(name);

            if (value == null)
                return;

            config.AppSettings.Settings.Add(name, value);
            config.Save(ConfigurationSaveMode.Modified);
        }

        public const string DeviceIdKey = SettingsKeys.DeviceIdKey;
        public static string DeviceId
        {
            get => Get(DeviceIdKey, "unset");
            set => Set(DeviceIdKey, value);
        }

        public const string GroupIdKey = SettingsKeys.GroupIdKey;
        public static string GroupId
        {
            get => Get(GroupIdKey, "unset");
            set => Set(GroupIdKey, value);
        }

        public const string PubKeyKey = SettingsKeys.PubKeyKey;
        public static byte[] PubKey
        {
            get
            {
                string v = Get(PubKeyKey);
                if (v != null)
                    return Convert.FromBase64String(v);
                return null;
            }
            set
            {
                if (value != null)
                    Set(PubKeyKey, Convert.ToBase64String(value));
                else
                    Set(PubKeyKey, null);
            }
        }

        public const string ReportAllScansKey = SettingsKeys.ReportAllScansKey;
        public static bool ReportAllScans
        {
            get => bool.Parse(Get(ReportAllScansKey, "False"));
            set => Set(ReportAllScansKey, value.ToString());
        }

        public const string VerboseKey = SettingsKeys.VerboseKey;
        private static bool? verbose = null;
        public static bool Verbose
        {
            get
            {
                if (verbose != null)
                    return verbose.Value;
                verbose = bool.Parse(Get(VerboseKey, "False"));
                return verbose.Value;
            }

            set
            {
                verbose = value;
                Set(VerboseKey, value.ToString());
            }
        }

        public const string MqHostKey = SettingsKeys.MqHostKey;
        public static string MqHost
        {
            get => Get(MqHostKey, "");
            set => Set(MqHostKey, value);
        }

        public const string MqPortKey = SettingsKeys.MqPortKey;
        public static ushort MqPort
        {
            get => ushort.Parse(Get(MqPortKey, "0"));
            set => Set(MqPortKey, value.ToString());
        }

        public const string MqTlsKey = SettingsKeys.MqTlsKey;
        public static bool MqTls
        {
            get => bool.Parse(Get(MqTlsKey, "True"));
            set => Set(MqTlsKey, value.ToString());
        }

        public const string MqUsernameKey = SettingsKeys.MqUsernameKey;
        public static string MqUsername
        {
            get => Get(MqUsernameKey, "guest");
            set => Set(MqUsernameKey, value);
        }

        public const string MqPasswordKey = SettingsKeys.MqPasswordKey;
        public static string MqPassword
        {
            get => Get(MqPasswordKey, "");
            set => Set(MqPasswordKey, value);
        }
    }
}
