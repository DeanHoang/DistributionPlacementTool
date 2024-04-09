using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Catalog;
using ArcGIS.Desktop.Mapping;
using DistributionPlacement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DistributionPlacement
{
	/// <summary>
	/// Interaction logic for DistributionElectricalStructure.xaml
	/// </summary>
	public partial class DistributionElectricalStructure
	{
		private readonly string _shd = Configs.GetString("SHD") ?? "SHD";
		private readonly string _assetGroup = Configs.GetString("ASSETGROUP_STRUCTURE") ?? "42";
		private readonly string _assetType = Configs.GetString("ASSETTYPE_STRUCTURE") ?? "421";
		private readonly string _operatingVoltage = Configs.GetString("OperatingVoltage") ?? "150";
		private readonly string _assetGroupField = Configs.GetString("ASSETGROUPFIELD") ?? "ASSETGROUP";
		private readonly string _assetTypeField = Configs.GetString("ASSETTYPEFIELD") ?? "ASSETTYPE";
		private readonly string _operatingVoltageField = Configs.GetString("OperatingVoltageField") ?? "OperatingVoltage";
		private readonly string _layer1ElectricNetwork = Configs.GetString("LAYER1_ELECTRICITY_NETWORK") ?? "CONTRACTOR SUBMISSION ";
		private readonly string _layer2Distribution = Configs.GetString("LAYER2_DISTRIBUTION") ?? "DISTRIBUTION";
		private readonly string _layer3Xyz = Configs.GetString("LAYER3_XYZ") ?? "XYZ";
		private readonly string _xyzElectricStructure = Configs.GetString("XYZ_ElectricStructure") ?? "XYZ_ElectricStructure";
		private readonly string _shape = Configs.GetString("SHAPE") ?? "SHAPE";
		private readonly string _topLevel = Configs.GetString("TopLevel") ?? "TopLevel";
		private readonly string _height = Configs.GetString("Height") ?? "Height";
		private readonly string _externalLength = Configs.GetString("ExternalLength") ?? "ExternalLength";
		private readonly string _externalWidth = Configs.GetString("ExternalWidth") ?? "ExternalWidth";
		private readonly string _structureType = Configs.GetString("StructureType") ?? "StructureType";
		private readonly string _quality = Configs.GetString("Quality") ?? "Quality";
		private readonly string _dateOfLastSurvey = Configs.GetString("Date_of_Last_Survey") ?? "Date_of_Last_Survey";
		private readonly string _surveyedBy = Configs.GetString("Surveyed_by") ?? "Surveyed_by";
		private readonly string _status = Configs.GetString("Status") ?? "Status";
		private readonly string _installationDate = Configs.GetString("InstallationDate") ?? "InstallationDate";

        private readonly string _externalSourceField = Configs.GetString("ExternalSourceField") ?? "ExternalSource";
        private readonly string _externalSource = Configs.GetString("ExternalSource") ?? "1";

        private ObservableCollection<XyzModel> StructureTypes { get; } = new ObservableCollection<XyzModel>();
		private ObservableCollection<XyzModel> Qualities { get; } = new ObservableCollection<XyzModel>();
		private ObservableCollection<XyzModel> Statuses { get; } = new ObservableCollection<XyzModel>();

		public DistributionElectricalStructure()
		{
			InitializeComponent();
			InitCollections();
		}

		/// <summary>
		/// Initialize data collections from app.config
		/// </summary>
		private void InitCollections()
		{
			StructureTypes.Add(new XyzModel { Code = "JB", Description = Configs.GetString("StructureType_JB") ?? "Joint Bay" });
			StructureTypes.Add(new XyzModel { Code = "PT", Description = Configs.GetString("StructureType_PT") ?? "Pressure Tank (PT) Pit" });
			StructureTypes.Add(new XyzModel { Code = "LBP", Description = Configs.GetString("StructureType_LBP") ?? "Link Box Pit" });
			StructureTypes.Add(new XyzModel { Code = "OG", Description = Configs.GetString("StructureType_OG") ?? "Oil Gauge (OG) Box" });
			StructureTypes.Add(new XyzModel { Code = "PT", Description = Configs.GetString("StructureType_PT") ?? "Partial Discharge (PD) Terminal Box Pit" });

			Qualities.Add(new XyzModel { Code = "1", Description = Configs.GetString("Quality_1") ?? "±100mm" });
			Qualities.Add(new XyzModel { Code = "2", Description = Configs.GetString("Quality_2") ?? "±300mm" });
			Qualities.Add(new XyzModel { Code = "3", Description = Configs.GetString("Quality_3") ?? "±500mm" });
			Qualities.Add(new XyzModel { Code = "4", Description = Configs.GetString("Quality_4") ?? "Unknown Accuracy" });
			Qualities.Add(new XyzModel { Code = "5", Description = Configs.GetString("Quality_5") ?? "Trenchless Method" });

			Statuses.Add(new XyzModel { Code = "ABN", Description = Configs.GetString("Status_ABN") ?? "Abandoned" });
			Statuses.Add(new XyzModel { Code = "EXT", Description = Configs.GetString("Status_EXT") ?? "Existing" });
            RefreshCollectionViews();
        }

		private void RefreshCollectionViews()
		{
			StructureType.ItemsSource = StructureTypes;
			StructureType.Items.Refresh();
			QualityValue.ItemsSource = Qualities;
			QualityValue.Items.Refresh();
			StatusValue.ItemsSource = Statuses;
			StatusValue.Items.Refresh();
		}

		/// <summary>
		/// This method will open XY Window input X and Y values
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GeometryOK_Click(object sender, RoutedEventArgs e)
		{
			
		}

		/// <summary>
		/// This method will close the opening XY Window input
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GeometryCancel_Click(object sender, RoutedEventArgs e)
		{
			PlaceElectricStructure.Close();
		}

		/// <summary>
		/// This method get input values and create survey
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void AttributeOK_Click(object sender, RoutedEventArgs e)
		{
            
            try
			{
				AppLogger._log.Info(nameof(AttributeOK_Click) + " started");             
               
				
                string regex = "^[1-9][0-9]*$";
                Regex regexXY = new(@"^[0-9]\d*(\.\d+)?$");
                string regexDecimal = "^\\d+(\\.\\d{1,2})?$";
                Regex alphaRegex = new Regex(@"^[a-zA-Z]+$");
                string errorMessage = string.Empty;
                
                ///////////////////////////////////////////////

                foreach (Control tb in AttributeWindow.Children) {

                    switch (tb.Name)
                    {
                        case "Xtb":
                            if (Xtb.Text == "")
                            {
                                errorMessage += "X value cannot be empty";
                            }
                            else
                            {

                                var matches = regexXY.Matches(Xtb.Text);
                                if (!matches.Any())
                                {
                                    errorMessage += "\nX: input data must be numeric";
                                }
                            }

                            break;
                        case "Ytb":
                            if (Ytb.Text == "")
                            {
                                errorMessage += "\nY value cannot be empty";
                            }
                            else
                            {

                                var matches = regexXY.Matches(Ytb.Text);
                                if (!matches.Any())
                                {
                                    errorMessage += "\nY: input data must be numeric";
                                }
                            }
                            break;
                        case "ShdValue":
                            if (ShdValue.Text == "")
                            {
                                errorMessage += "SHD value cannot be empty";
                            }
                            else
                            {
                                var matches = regexXY.Matches(ShdValue.Text);
                                if (!matches.Any())
                                {
                                    errorMessage += "\n SHD: Invalid input";
                                }
                            }
                            break;
                        case "TopLevel":
                            if (TopLevel.Text == "")
                            {
                                errorMessage += "\n Top Level value cannot be empty";
                            }
                            else
                            {
                                var matches = regexXY.Matches(TopLevel.Text);
                                if (!matches.Any())
                                {

                                    errorMessage += "\n Top Level: Invalid input";

                                }

                            }
                            break;
                        case "HeightValue":
                            if (HeightValue.Text == "")
                            {
                                errorMessage += "\n Height value cannot be empty";
                            }
                            else
                            {

                                var matches = regexXY.Matches(HeightValue.Text);
                                if (!matches.Any())
                                {

                                    errorMessage += "\n Height: Invalid input";

                                }

                            }
                            break;                        
                        case "ExternalLength":
                            if (ExternalLength.Text == "")
                            {
                                errorMessage += "\n Width value cannot be empty";
                            }
                            else
                            {
                                var matches = regexXY.Matches(ExternalLength.Text);
                                if (!matches.Any())
                                {

                                    errorMessage += "\n External Length: Invalid input";
                                }
                            }
                            break;
                        case "ExternalWidth":
                            if (ExternalWidth.Text == "")
                            {
                                errorMessage += "\n Number Of Columns value cannot be empty";
                            }
                            else
                            {
                                var matches = regexXY.Matches(ExternalWidth.Text);
                                if (!matches.Any())
                                {

                                    errorMessage += "\n External Width: Invalid input";

                                }

                            }
                            break;
                       
                        case "DateOfLastSurvey":
                            if (DateOfLastSurvey.Text == "")
                            {
                                errorMessage += "\n Date of Last Survey value cannot be empty";
                            }

                            break;
                        case "SurveyedBy":
                            if (SurveyedBy.Text == "")
                            {
                                errorMessage += "\n Surveyer name cannot be empty";
                            }
                            else
                            {
                                var matches = alphaRegex.Matches(SurveyedBy.Text);
                                if (!matches.Any())
                                {
                                    errorMessage += "\n Surveyer name is invalid";
                                }

                            }

                            break;
                        case "StatusCmb":
                            if (StatusValue.SelectedIndex == -1)
                            {
                                errorMessage += "\n Status value cannot be empty";
                            }

                            break;
                       
                        case "DateOfInstallation":
                            if (DateOfInstallation.Text == "")
                            {
                                errorMessage += "\n Installation Date cannot be empty";
                            }

                            break;
                        
                        default:
                            //what you want when nothing is selected
                            break;
                    }

                }
                if (errorMessage.Length != 0) { MessageBox.Show(errorMessage); }
                else
                {
                    float xValue = float.Parse(Xtb.Text);
                    float yValue = float.Parse(Ytb.Text);
                    double shd = double.Parse(ShdValue.Text);
                    double topLevel = double.Parse(TopLevel.Text);
                    double height = double.Parse(HeightValue.Text);
                    double externalLength = double.Parse(ExternalLength.Text);
                    double externalWidth = double.Parse(ExternalWidth.Text);
                    string structureType = ((XyzModel)StructureType.SelectedItem).Code ?? string.Empty;
                    short quality = short.Parse(((XyzModel)QualityValue.SelectedItem).Code ?? "0");
                    DateTime dateOfLastSurvey = DateTime.Parse(DateOfLastSurvey.Text);
                    string surveyedBy = SurveyedBy.Text;
                    string status = ((XyzModel)StatusValue.SelectedItem).Code ?? string.Empty;
                    DateTime dateOfInstallation = DateTime.Parse(DateOfInstallation.Text);

                    IReadOnlyList<GroupLayer> groupLayers = MapView.Active.Map.Layers.OfType<GroupLayer>().ToList();

				  // Loop for each group layer, and create survey to expected feature layer.
				  foreach (GroupLayer gLayer1 in groupLayers)
				{
					if (gLayer1.Name == _layer1ElectricNetwork)
					{
						IReadOnlyList<GroupLayer> groupLayers2 = gLayer1.Layers.OfType<GroupLayer>().ToList();
						foreach (GroupLayer gLayer2 in groupLayers2)
						{
							if (gLayer2.Name == _layer2Distribution)
							{
								IReadOnlyList<GroupLayer> groupLayers3 = gLayer2.Layers.OfType<GroupLayer>().ToList();
								foreach (GroupLayer gLayer3 in groupLayers3)
								{
									if (gLayer3.Name == _layer3Xyz)
									{
										IReadOnlyList<FeatureLayer> fLayers =
											gLayer3.Layers.OfType<FeatureLayer>().ToList();
										try
										{
											foreach (FeatureLayer feaLayer in fLayers)
											{
												if (feaLayer.Name == _xyzElectricStructure)
												{
													await QueuedTask.Run(() =>
													{
														var createOperation = new EditOperation
														{
															Name = "Generate points",
															SelectNewFeatures = false
														};
														//var featureClass = feaLayer.GetTable() as FeatureClass;
														MapPoint newMapPoint = MapPointBuilderEx.CreateMapPoint(xValue,
															yValue, 0,
															SpatialReferenceBuilder.CreateSpatialReference(3414));
														// Retrieve the class definition of the point feature class.
														//var classDefinition = featureClass?.GetDefinition() as FeatureClassDefinition;
														// Store the spatial reference as its own variable.
														//var spatialReference = classDefinition?.GetSpatialReference();

														// Define attributes to be created.
														var attributes = new Dictionary<string, object>
														{
															{ _shape, newMapPoint },
															{ _shd, shd },
															{ _assetGroupField, _assetGroup },
															{ _assetTypeField, _assetType },
															{ _operatingVoltageField, _operatingVoltage },
															{ _topLevel, topLevel },
															{ _height, height },
															{ _externalLength, externalLength },
															{ _externalWidth, externalWidth },
															{ _structureType, structureType },
															{ _quality, quality },
															{ _dateOfLastSurvey, dateOfLastSurvey },
															{ _surveyedBy, surveyedBy },
															{ _status, status },
															{ _installationDate, dateOfInstallation },
                                                            { _externalSourceField, _externalSource },
                                                        };
														createOperation.Create(feaLayer, attributes);
														createOperation.Execute();
                                                       
                                                        var completed = createOperation.IsSucceeded;
														if (completed)
														{
                                                            ZoomToPoint(xValue, yValue, SpatialReferenceBuilder.CreateSpatialReference(3414));
                                                            AppLogger._log.Info("Survey creation was successful.");
															MessageBox.Show("Survey creation was successful.");
                                                            this.Dispatcher.Invoke(() =>
                                                            {
                                                                PlaceElectricStructure.Close();
                                                            });

                                                        }
														else
														{
															AppLogger._log.Error("Survey creation error:\n" +
																createOperation.ErrorMessage);
															MessageBox.Show("Survey creation was failed. It might be input data was incorrect.");
														}
													});
												}
											}
										}
										catch (Exception ex)
										{
											AppLogger._log.Error("Survey creation exception:\n" + ex.Message);
											MessageBox.Show("Survey point creation was failed.");
										}
										finally
										{
                                            AppLogger._log.Info("Feature layer creation ended.");
                                            //PlaceElectricStructure.Close();
                                            }
									}
								}
							}
						}
					}
				}
                }

            }
            catch (Exception ex)
			{
				AppLogger._log.Error("Survey creation exception:\n" + ex.Message);
				MessageBox.Show("Survey creation was failed.");
			}
			finally
			{
				AppLogger._log.Info(nameof(AttributeOK_Click) + "ended.");
             
            }
		}
		internal void GetValue(string v)
		{
			throw new NotImplementedException();
		}
        private Task<bool> ZoomToPoint(double x, double y, ArcGIS.Core.Geometry.SpatialReference spatialReference)
        {
            //Get the active map view.
            var mapView = MapView.Active;
            if (mapView == null)
                return Task.FromResult(false);

            return QueuedTask.Run(() =>
            {
                //Note: Run within QueuedTask
                //Create a point
                var pt = MapPointBuilderEx.CreateMapPoint(x, y, spatialReference);
                //Buffer it - for purpose of zoom
                var poly = GeometryEngine.Instance.Buffer(pt, 10);

                //do we need to project the buffer polygon?
                if (!MapView.Active.Map.SpatialReference.IsEqual(poly.SpatialReference))
                {
                    //project the polygon
                    poly = GeometryEngine.Instance.Project(poly, MapView.Active.Map.SpatialReference);
                }

                //Zoom - add in a delay for animation effect
                return mapView.ZoomTo(poly, new TimeSpan(0, 0, 0, 2));
            });
        }

        private void AttributeCancel_Click(object sender, RoutedEventArgs e)
		{
			PlaceElectricStructure.Close();
		}

		private void NoOfDucts_TextChanged(object sender, TextChangedEventArgs e)
		{

		}
	}
}
