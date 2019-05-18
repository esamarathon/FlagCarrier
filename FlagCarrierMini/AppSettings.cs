using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace FlagCarrierMini
{
	class AppSettings
	{
		private static Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

		static AppSettings()
		{
			foreach (PropertyInfo prop in typeof(AppSettings).GetProperties(BindingFlags.Static | BindingFlags.Public))
				prop.SetValue(null, prop.GetValue(null));
		}

		private static string Get([CallerMemberName] string name = null, string defaultValue = null)
		{
			var cfg = config.AppSettings.Settings[name];
			if (cfg != null)
				return cfg.Value;
			else
				return null;
		}

		private static void Set(string value, [CallerMemberName] string name = "")
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
			get => Get() ?? "";
			set => Set(value);
		}

		public static byte[] PubKey
		{
			get
			{
				string b64 = Get();
				return b64 != null ? Convert.FromBase64String(b64) : null;
			}
			set
			{
				Set(value != null ? Convert.ToBase64String(value) : null);
			}
		}

		public static string MqHost
		{
			get => Get() ?? "";
			set => Set(value);
		}

		public static ushort MqPort
		{
			get => ushort.Parse(Get() ?? "0");
			set => Set(value.ToString());
		}

		public static string MqUsername
		{
			get => Get() ?? "guest";
			set => Set(value);
		}

		public static string MqPassword
		{
			get => Get() ?? "";
			set => Set(value);
		}
	}
}
