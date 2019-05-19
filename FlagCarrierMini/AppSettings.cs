using System;
using System.Collections.Generic;
using System.ComponentModel;
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

		public static bool FromDict(Dictionary<string, string> values)
		{
			foreach (var kv in values)
			{
				PropertyInfo prop = typeof(AppSettings).GetProperty(kv.Key, BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
				if (prop == null)
				{
					Console.WriteLine("Unknown Option " + kv.Key);
					return false;
				}

				// Backup original value in case of format errors
				string oval = Get(null, prop.Name);

				try
				{
					Set(kv.Value, prop.Name);

					// Load and set via propety to normalize and verify format
					object nval = prop.GetValue(null);
					prop.SetValue(null, nval);
				}
				catch (Exception e)
				{
					// Restore original value on error
					Set(oval, prop.Name);

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
				Console.WriteLine(so("--" + prop.Name.ToLower() + "=" + Get(null, prop.Name) + " [" + prop.PropertyType.Name + "]"));
			Console.WriteLine();
			Console.WriteLine("All settings will be persisted to the application configuration automatically.");
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

		private static bool? verbose = null;
		public static bool Verbose
		{
			get
			{
				if (verbose != null)
					return verbose.Value;
				verbose = bool.Parse(Get("False"));
				return verbose.Value;
			}

			set
			{
				verbose = value;
				Set(value.ToString());
			}
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
