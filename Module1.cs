using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace DistributionPlacement
{
	internal class Module1 : Module
	{
		private static Module1 _this;

		/// <summary>
		/// Retrieve the singleton instance to this module here
		/// </summary>
		public static Module1 _current => _this ??= (Module1)FrameworkApplication.FindModule("DistributionPlacement_Module");

		#region Overrides
		/// <summary>
		/// Called by Framework when ArcGIS Pro is closing
		/// </summary>
		/// <returns>False to prevent Pro from closing, otherwise True</returns>
		protected override bool CanUnload()
		{
			//TODO - add your business logic
			//return false to ~cancel~ Application close
			return true;
		}

		protected override bool Initialize()
		{
			AppLogger.Init();
			Configs.Init();
			return base.Initialize();
		}

		#endregion Overrides

	}
}
