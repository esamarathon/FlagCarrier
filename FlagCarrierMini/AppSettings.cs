using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Text;
using System.Reflection;

namespace FlagCarrierMini
{
	class AppSettings
	{
		private static Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

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

			config.AppSettings.Settings.Add(name, value);
			config.Save(ConfigurationSaveMode.Modified);
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
	}
}
