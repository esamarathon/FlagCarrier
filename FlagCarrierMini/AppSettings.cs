using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Reflection;

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

		private static string Get(string defaultValue = null, [CallerMemberName] string name = null)
		{
			var cfg = config.AppSettings.Settings[name];
			if (cfg != null)
				return cfg.Value;
			else
				return defaultValue;
		}

		private static void Set(string value, [CallerMemberName] string name = null)
		{
			if (config.AppSettings.Settings[name] != null)
				config.AppSettings.Settings.Remove(name);

			if (value == null)
				return;

			config.AppSettings.Settings.Add(name, value);
			config.Save(ConfigurationSaveMode.Modified);
		}

		public static string ID
		{
			get => Get("unset");
			set => Set(value);
		}

		public static byte[] PubKey
		{
			get
			{
				string v = Get();
				if (v != null)
					return Convert.FromBase64String(v);
				return null;
			}
			set
			{
				if (value != null)
					Set(Convert.ToBase64String(value));
				else
					Set(null);
			}
		}

		public static bool ReportAllScans
		{
			get => bool.Parse(Get("False"));
			set => Set(value.ToString());
		}

		public static string MqHost
		{
			get => Get("");
			set => Set(value);
		}

		public static ushort MqPort
		{
			get => ushort.Parse(Get("0"));
			set => Set(value.ToString());
		}

		public static bool MqTls
		{
			get => bool.Parse(Get("True"));
			set => Set(value.ToString());
		}

		public static string MqQueue
		{
			get => Get("flagcarrier-tag-scanned");
			set => Set(value);
		}

		public static string MqUsername
		{
			get => Get("guest");
			set => Set(value);
		}

		public static string MqPassword
		{
			get => Get("");
			set => Set(value);
		}
	}
}
