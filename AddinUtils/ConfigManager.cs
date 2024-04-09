using System.IO;

namespace DistributionPlacement.AddinUtils
{
	internal static class ConfigManager
	{
		public static string GetConfigPath(string configFileName)
		{
			string result = "";

			var executingAssemblyDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

			if (executingAssemblyDirectory != null)
			{
				var assemblyAppConfigPath = Path.Combine(executingAssemblyDirectory, configFileName);

#if DEBUG
				result = assemblyAppConfigPath;
			}

#else
                        var addInInfo = AddInManager.GetAddInInfo();

                        var appConfigPath = Path.Combine(AddInManager.GetAddInFolder(), configFileName);

                        if (!File.Exists(appConfigPath))
                            File.Copy(assemblyAppConfigPath, appConfigPath);

                        result = appConfigPath;
#endif

			return result;
		}
	}
}
