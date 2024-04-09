using DistributionPlacement.AddinUtils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;


namespace DistributionPlacement
{
	internal static class Configs
	{
		private static Configuration _config;

		public static void Init()
		{
			var map = new ExeConfigurationFileMap
			{
				ExeConfigFilename = ConfigManager.GetConfigPath("App.config")
			};

			_config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
		}

		private static string GetValue(string key, string defaultValue = null)
		{
			if (_config == null || !_config.AppSettings.Settings.AllKeys.Contains(key)) return defaultValue;

			return _config.AppSettings.Settings[key].Value;
		}

		public static Dictionary<string, string> GetDictionary(string key)
		{
			var result = new Dictionary<string, string>();

			try
			{
				foreach (var pair in GetValue(key).Split(";"))
				{
					var parts = pair.Split(">");

					result.Add(parts[0], parts[1]);
				}
			}
			catch (Exception ex)
			{
				AppLogger._log.Error("GetDictionary() exception:\n" + ex.Message);
			}

			return result;
		}

		public static bool GetBoolean(string key)
		{
			return Convert.ToBoolean(GetValue(key));
		}

		public static string GetString(string key)
		{
			return GetValue(key);
		}

		public static int GetInt(string key)
		{
			return Convert.ToInt32(GetValue(key));
		}

		public static double GetDouble(string key)
		{
			return Convert.ToDouble(GetValue(key));
		}

		public static IEnumerable<int> GetInts(string key)
		{
			return GetValue(key).Split(",").Select(n => Convert.ToInt32(n));
		}

		public static IEnumerable<string> GetStrings(string key)
		{
			return GetValue(key).Split(",").Where(val => !string.IsNullOrEmpty(val));
		}
	}
}
