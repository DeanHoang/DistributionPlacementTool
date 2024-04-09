using log4net;
using log4net.Config;
using System.IO;

namespace DistributionPlacement
{
	public static class AppLogger
	{
		public static ILog _log { get; private set; }

		public static void Init()
		{
			XmlConfigurator.ConfigureAndWatch(new FileInfo(AddinUtils.ConfigManager.GetConfigPath("log4net.config")));
			_log = LogManager.GetLogger("DistributionPlacement");
		}
	}
}
