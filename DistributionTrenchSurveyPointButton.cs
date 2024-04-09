using ArcGIS.Desktop.Framework.Contracts;
using System.Windows;

namespace DistributionPlacement
{
	internal class DistributionTrenchSurveyPointButton : Button
	{
		private DistributionTrenchSurveyPoint _distributionTrenchSurveyPoint;
		protected override void OnClick()
		{
			if (_distributionTrenchSurveyPoint != null)
				return;
			_distributionTrenchSurveyPoint = new DistributionTrenchSurveyPoint
			{
				Owner = Application.Current.MainWindow
			};
			_distributionTrenchSurveyPoint.Closed += (o, e) => { _distributionTrenchSurveyPoint = null; };
			_distributionTrenchSurveyPoint.Show();
		}
	}
}
