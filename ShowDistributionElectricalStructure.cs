using ArcGIS.Desktop.Framework.Contracts;
using System.Windows;

namespace DistributionPlacement
{
	internal class ShowDistributionElectricalStructure : Button
	{

		private DistributionElectricalStructure _distributionElectricalStructure;

		protected override void OnClick()
		{
			//already open?
			if (_distributionElectricalStructure != null)
				return;
			_distributionElectricalStructure = new DistributionElectricalStructure
			{
				Owner = Application.Current.MainWindow
			};
			_distributionElectricalStructure.Closed += (o, e) => { _distributionElectricalStructure = null; };
			_distributionElectricalStructure.Show();
			//uncomment for modal
			//_distributionElectricalStructure.ShowDialog();
		}

	}
}
