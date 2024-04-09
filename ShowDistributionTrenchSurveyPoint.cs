using ArcGIS.Desktop.Framework.Contracts;
using System.Windows;

namespace DistributionPlacement
{
	internal class ShowDistributionTrenchSurveyPoint : Button
	{

		private DistributionTrenchSurveyPoint _distributionTrenchSurveyPoint;

		protected override void OnClick()
		{
			//already open?
			if (_distributionTrenchSurveyPoint != null)
				return;
			_distributionTrenchSurveyPoint = new DistributionTrenchSurveyPoint
			{
				Owner = Application.Current.MainWindow
			};
			_distributionTrenchSurveyPoint.Closed += (o, e) => { _distributionTrenchSurveyPoint = null; };
			_distributionTrenchSurveyPoint.Show();
			//uncomment for modal
			//_distributionTrenchSurveyPoint.ShowDialog();
		}

	}
}
