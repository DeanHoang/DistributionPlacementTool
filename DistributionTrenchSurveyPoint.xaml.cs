using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
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
	/// Interaction logic for DistributionTrenchSurveyPoint.xaml
	/// </summary>
	public partial class DistributionTrenchSurveyPoint
	{
		private readonly string _shd = Configs.GetString("SHD") ?? "SHD";
		private readonly string _assetGroupField = Configs.GetString("ASSETGROUPFIELD") ?? "ASSETGROUP";
		private readonly string _assetTypeField = Configs.GetString("ASSETTYPEFIELD") ?? "ASSETTYPE";
		private readonly string _operatingVoltageField = Configs.GetString("OperatingVoltageField") ?? "OperatingVoltage";
		private readonly string _assetGroup = Configs.GetString("ASSETGROUP") ?? "42";
		private readonly string _assetType = Configs.GetString("ASSETTYPE") ?? "421";
		private readonly string _operatingVoltage = Configs.GetString("OperatingVoltage") ?? "150";
		private readonly string _layer1ElectricNetwork = Configs.GetString("LAYER1_ELECTRICITY_NETWORK") ?? "CONTRACTOR SUBMISSION ";
		private readonly string _layer2Distribution = Configs.GetString("LAYER2_DISTRIBUTION") ?? "DISTRIBUTION";
		private readonly string _layer3Xyz = Configs.GetString("LAYER3_XYZ") ?? "XYZ";
		private readonly string _xyzTrenchSurveyPoint = Configs.GetString("XYZ_TrenchSurveyPoint") ?? "XYZ_TrenchSurveyPoint";
		private readonly string _shape = Configs.GetString("SHAPE") ?? "SHAPE";
		private readonly string _topLevel = Configs.GetString("TopLevel") ?? "TopLevel";
		private readonly string _height = Configs.GetString("Height") ?? "Height";
		private readonly string _width = Configs.GetString("Width") ?? "Width";
		private readonly string _numberOfColumns = Configs.GetString("No_of_Columns") ?? "No_of_Columns";
		private readonly string _numberOfRows = Configs.GetString("No_of_Rows") ?? "No_of_Rows";
		private readonly string _numberOfDucts = Configs.GetString("No_of_Ducts") ?? "No_of_Ducts";
		private readonly string _numberOfCables = Configs.GetString("No_of_Cables") ?? "No_of_Cables";
		private readonly string _haunches = Configs.GetString("Haunches") ?? "Haunches";
		private readonly string _quality = Configs.GetString("Quality") ?? "Quality";
		private readonly string _surveyedBy = Configs.GetString("Surveyed_by") ?? "Surveyed_by";
		private readonly string _status = Configs.GetString("Status") ?? "Status";
		private readonly string _typeOfLine = Configs.GetString("Type_of_Line") ?? "Type_of_Line";
        private readonly string _externalSourceField = Configs.GetString("ExternalSourceField") ?? "ExternalSource";
        private readonly string _externalSource = Configs.GetString("ExternalSource") ?? "1";
        private bool _isQualityTrenchlessMethod = false;

        private ObservableCollection<XyzModel> Qualities { get; } = new ObservableCollection<XyzModel>();
		private ObservableCollection<XyzModel> Statuses { get; } = new ObservableCollection<XyzModel>();
		private ObservableCollection<XyzModel> TypeOfLines { get; } = new ObservableCollection<XyzModel>();

		public DistributionTrenchSurveyPoint()
		{
			InitializeComponent();
			InitCollections();
		}

		/// <summary>
		/// Initialize data collections from app.config
		/// </summary>
		private void InitCollections()
		{
			Qualities.Add(new XyzModel { Code = "1", Description = Configs.GetString("Quality_1") ?? "±100mm" });
			Qualities.Add(new XyzModel { Code = "2", Description = Configs.GetString("Quality_2") ?? "±300mm" });
			Qualities.Add(new XyzModel { Code = "3", Description = Configs.GetString("Quality_3") ?? "±500mm" });
			Qualities.Add(new XyzModel { Code = "4", Description = Configs.GetString("Quality_4") ?? "Unknown Accuracy" });
			Qualities.Add(new XyzModel { Code = "5", Description = Configs.GetString("Quality_5") ?? "Trenchless Method" });

			Statuses.Add(new XyzModel { Code = "ABN", Description = Configs.GetString("Status_ABN") ?? "Abandoned" });
			Statuses.Add(new XyzModel { Code = "EXT", Description = Configs.GetString("Status_EXT") ?? "Existing" });

			TypeOfLines.Add(new XyzModel { Code = "LV", Description = Configs.GetString("TypeOfLine_LV") ?? "LV" });
			TypeOfLines.Add(new XyzModel { Code = "DIST", Description = Configs.GetString("TypeOfLine_DIST") ?? "Distribution" });
			TypeOfLines.Add(new XyzModel { Code = "TRANS", Description = Configs.GetString("TypeOfLine_TRANS") ?? "Transmission" });
            RefreshCollectionViews();
        }

		private void RefreshCollectionViews()
		{
			QualityValue.ItemsSource = Qualities;
			QualityValue.Items.Refresh();
			StatusValue.ItemsSource = Statuses;
			StatusValue.Items.Refresh();
			TypeOfLine.ItemsSource = TypeOfLines;
			TypeOfLine.Items.Refresh();
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
		/// This method get input values and create survey
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void AttributeOK_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				AppLogger._log.Info(nameof(AttributeOK_Click) + " started");
                // MessageBox.Show("GeometryOK_Click");
                string regex = "^[1-9][0-9]*|[0-9]$";
                string regexDecimal = "^\\d+(\\.\\d{1,2})?$";
                Regex alphaRegex = new Regex(@"^[a-zA-Z]+$");
                Regex regex1 = new(@"^[0-9]\d*(\.\d+)?$");
                string errorMessage = "";
                foreach (Control tb in AttributesForSurveyPoint.Children)
                {
                    switch (tb.Name)
                    {
                            
					        case "Xtb":
						        if (Xtb.Text == "")
                    {
                        errorMessage += "X value cannot be empty";
                    }
                    else
                    {

                        var matches = regex1.Matches(Xtb.Text);
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

                        var matches = regex1.Matches(Ytb.Text);
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
                                var matches = regex1.Matches(ShdValue.Text);
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
                                var matches = regex1.Matches(TopLevel.Text);
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

                                var matches = regex1.Matches(HeightValue.Text);
                                if (!matches.Any())
                                {

                                    errorMessage += "\n Height: Invalid input";

                                }

                            }
                            break;
                        case "WidthValue":
                            if (WidthValue.Text == "")
                            {
                                errorMessage += "\n Width value cannot be empty";
                            }
                            else
                            {
                                var matches = regex1.Matches(WidthValue.Text);
                                if (!matches.Any())
                                {

                                    errorMessage += "\n Width: Invalid input";
                                }
                            }
                            break;
                        case "NoOfColumns":
                            if (NoOfColumns.Text == "")
                            {
                                errorMessage += "\n Number Of Columns value cannot be empty";
                            }
                            else
                            {
                                if ((XyzModel)QualityValue.SelectedItem != null && !_isQualityTrenchlessMethod && short.Parse(NoOfColumns.Text) <= 0)
                                {
                                    errorMessage += "\n Number Of Columns: Must be greater than zero because quality is trenchless method";
                                }
                                else if (!Regex.IsMatch(NoOfColumns.Text, regex))
                                {

                                    errorMessage += "\n Number Of Columns: Invalid input";

                                }
                               

                            }
                            break;
                        case "NoOfRows":
                            if (NoOfRows.Text == "")
                            {
                                errorMessage += "\n Number Of Rows value cannot be empty";
                            }
                            else
                            {
                                if ((XyzModel)QualityValue.SelectedItem != null && !_isQualityTrenchlessMethod && short.Parse(NoOfRows.Text) <= 0)
                                {
                                    errorMessage += "\n Number Of Rows: Must be greater than zero because quality is trenchless method";
                                }
                                else if (!Regex.IsMatch(NoOfRows.Text, regex))
                                {
                                    errorMessage += "\n Number Of Rows: Invalid input";
                                }
                                
                            }
                            break;
                        case "NoOfDucts":
                            if (NoOfDucts.Text == "")
                            {
                                errorMessage += "\n Number Of Ducts value cannot be empty";
                            }
                            else
                            {
                                if ((XyzModel)QualityValue.SelectedItem != null && !_isQualityTrenchlessMethod && short.Parse(NoOfDucts.Text) <= 0)
                                {
                                    errorMessage += "\n Number Of Ducts: Must be greater than zero because quality is trenchless method";
                                }
                                else if (!Regex.IsMatch(NoOfDucts.Text, regex))
                                {
                                    errorMessage += "\n Number Of Ducts: Invalid input";
                                } 
                                
                            }
                            break;
                        case "NoOfCables":
                            if (NoOfCables.Text == "")
                            {
                                errorMessage += "\n Number Of Cables value cannot be empty";
                            }
                            else
                            {
                               
                                    if (_isQualityTrenchlessMethod && short.Parse(NoOfCables.Text) <= 0)
                                    {
                                        errorMessage += "\n Number Of Cables: Must be greater than zero because quality is trenchless method";
                                    }
                                    else if(!Regex.IsMatch(NoOfCables.Text, regex))
                                    {
                                    errorMessage += "\n Number Of Cables: Invalid input";

                                    }
                                
                            }
                            break;
                        case "HaunchesValue":
                            if (HaunchesValue.Text == "")
                            {
                                errorMessage += "\n Haunches value cannot be empty";
                            }


                            break;
                        case "QualityValue":
                            if (QualityValue.Text == "")
                            {
                                errorMessage += "\n Quality value cannot be empty";
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
                        case "TypeoflineCmb":
                            if (TypeOfLine.SelectedIndex == -1)
                            {
                                errorMessage += "\n Type of line value cannot be empty";
                            }

                            break;
                        case "DateofInstallation_Ducts":
                            if (DateOfInstallationDucts.Text == "")
                            {
                                errorMessage += "\n Installation Date of Ducts cannot be empty";
                            }

                            break;
                        case "DateofInstallation_Cables":
                            if (DateOfInstallationCables.Text == "")
                            {
                                errorMessage += "\n Installation Date of Ducts(Newest cables) cannot be empty";
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
					double width = double.Parse(WidthValue.Text);
					short noOfColumns = short.Parse(NoOfColumns.Text);
					short noOfRows = short.Parse(NoOfRows.Text);
					short noOfDucts = short.Parse(NoOfDucts.Text);
					short noOfCables = short.Parse(NoOfCables.Text);
					short haunches = short.Parse(HaunchesValue.SelectedValue.ToString() ?? string.Empty);
					short quality = short.Parse(((XyzModel)QualityValue.SelectedItem).Code ?? "0");
					DateTime dateOfLastSurvey = DateTime.Parse(DateOfLastSurvey.Text);
					string surveyedBy = SurveyedBy.Text;
					string status = ((XyzModel)StatusValue.SelectedItem).Code ?? string.Empty;
					string typeOfLine = ((XyzModel)TypeOfLine.SelectedItem).Code ?? string.Empty;
					DateTime dateOfInstallationDucts = DateTime.Parse(DateOfInstallationDucts.Text);
					DateTime dateOfInstallationNewestCables = DateTime.Parse(DateOfInstallationCables.Text);

					IReadOnlyList<GroupLayer> groupLayers = MapView.Active.Map.Layers.OfType<GroupLayer>().ToList();

					//GroupLayer groupLayer = null;
					foreach (GroupLayer gLayer1 in groupLayers)
					{
						if (gLayer1.Name == _layer1ElectricNetwork)
						{
							//groupLayer = gLayer1;
							IReadOnlyList<GroupLayer> groupLayers2 = gLayer1.Layers.OfType<GroupLayer>().ToList();
							//GroupLayer groupLayer2 = null;
							foreach (GroupLayer gLayer2 in groupLayers2)
							{
								if (gLayer2.Name == _layer2Distribution)
								{
									//groupLayer = gLayer2;

									IReadOnlyList<GroupLayer> groupLayers3 = gLayer2.Layers.OfType<GroupLayer>().ToList();
									//GroupLayer groupLayer3 = null;
									foreach (GroupLayer gLayer3 in groupLayers3)
									{
										if (gLayer3.Name == _layer3Xyz)
										{
											IReadOnlyList<FeatureLayer> fLayers = gLayer3.Layers.OfType<FeatureLayer>().ToList();
											try
											{
												foreach (FeatureLayer feaLayer in fLayers)
												{
													if (feaLayer.Name == _xyzTrenchSurveyPoint)
													{
														await QueuedTask.Run(() =>
														{
															var createOperation = new EditOperation
															{
																Name = "Generate points",
																SelectNewFeatures = false
															};
															var featureClass = feaLayer.GetTable() as FeatureClass;
															MapPoint newMapPoint = MapPointBuilderEx.CreateMapPoint(xValue, yValue, 0, SpatialReferenceBuilder.CreateSpatialReference(3414));
															// Retrieve the class definition of the point feature class.
															var classDefinition = featureClass?.GetDefinition();
															// Restore the spatial reference as its own variable.
															var spatialReference = classDefinition?.GetSpatialReference();

															// Defined attributes to be inserted.
															var attributes = new Dictionary<string, object>
															{
														{ _shape, newMapPoint },
														{ _shd, shd },
														{ _assetGroupField, _assetGroup },
														{ _assetTypeField, _assetType },
														{ _operatingVoltageField, _operatingVoltage },
														{ _topLevel, topLevel },
														{ _height, height },
														{ _width, width },
														{ _numberOfColumns, noOfColumns },
														{ _numberOfRows, noOfRows },
														{ _numberOfDucts, noOfDucts },
														{ _numberOfCables, noOfCables },
														{ _haunches, haunches },
														{ _quality, quality },
														{ _externalSourceField, _externalSource },
														{"Date_of_Last_Survey", dateOfLastSurvey},
														{ _surveyedBy, surveyedBy },
														{ _status, status },
														{ _typeOfLine, typeOfLine },
															};
                                                            attributes.Add("Date_of_Installation_Ducts", dateOfInstallationDucts);
															attributes.Add("Date_of_Installation_Cables", dateOfInstallationNewestCables);
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
                                                                     PlaceTrenchPoint.Close();
                                                                });
                                                            }
                                                            else
															{
																AppLogger._log.Error("Survey creation error:\n" +
																	createOperation.ErrorMessage);
																MessageBox.Show(
																	"Survey creation was failed. It might be input data was incorrect.");
															}
														});
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
												AppLogger._log.Info("Feature layer creation ended.");
												
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

		private void AttributeCancel_Click(object sender, RoutedEventArgs e)
		{
			PlaceTrenchPoint.Close();
		}
        private void NoOfColumnsTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Calculate value for  NoOfDuctsTxt from NoOfColumnsTxt & NoOfRowsTxt

            try
            {
                if (NoOfColumns.Text != "" && NoOfRows.Text != "")
                {
                    //isNoOfColumns is false then numericNoOfColumns=0 default value
                    int.TryParse(NoOfColumns.Text, out var numericNoOfColumns);
                    //isNoOfRows is false then numericNoOfRows=0 default value
                    int.TryParse(NoOfRows.Text, out var numericNoOfRows);

                    int ductsCount = numericNoOfColumns * numericNoOfRows;
                    NoOfDucts.Text = ductsCount.ToString();
                   // NoOfCables.IsEnabled = false;
                }
                else
                {
                    NoOfDucts.Text = "";
                   

                }
            }
            catch (Exception ex)
            {
                AppLogger._log.Error(nameof(NoOfColumnsTxt_TextChanged) + " exception:\n" + ex.Message);
                MessageBox.Show("Something went wrong in data input");
            }

        }

        private void NoOfRowsTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Calculate value for  NoOfDuctsTxt from NoOfColumnsTxt & NoOfRowsTxt
            try
            {
                if (NoOfColumns.Text != "" && NoOfRows.Text != "")
                {
                    //isNoOfColumns is false then numericNoOfColumns=0 default value
                    int.TryParse(NoOfColumns.Text, out var numericNoOfColumns);
                    //isNoOfRows is false then numericNoOfRows=0 default value
                    int.TryParse(NoOfRows.Text, out var numericNoOfRows);

                    int ductsCount = numericNoOfColumns * numericNoOfRows;
                    NoOfDucts.Text = ductsCount.ToString();
                  //  NoOfCables.IsEnabled = false;
                }
                else
                {
                    NoOfDucts.Text = "";
                   

                }
            }
            catch (Exception ex)
            {
                AppLogger._log.Error(nameof(NoOfRowsTxt_TextChanged) + " exception:\n" + ex.Message);
                MessageBox.Show("Something went wrong in data input");
            }
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
        /// <summary>
        /// if quality value is trencheless then
        ///   no of coumns, rows, ducts = 0 no of cables >0
        /// other than trenchless then
        ///  no of of columns, rows, ducts > 0 no fo cables =0  
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <exception cref="NotImplementedException"></exception>
        private void QualityCmb_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {

                if (((XyzModel)QualityValue.SelectedItem).Description == "Trenchless Method")
                {

                    _isQualityTrenchlessMethod = true;
                    NoOfColumns.Text = "0";
                    NoOfColumns.IsEnabled = false;
                    NoOfColumns.IsReadOnly = true;

                    NoOfRows.Text = "0";                   
                    NoOfRows.IsEnabled = false;
                    NoOfRows.IsReadOnly = true;

                    NoOfDucts.Text = "0";                   
                    NoOfDucts.IsEnabled = false;
                    NoOfDucts.IsReadOnly = true;

                    NoOfCables.Text = "";
                    NoOfCables.IsEnabled = true;
                    NoOfCables.IsReadOnly = false;
                    
                }
                else
                {
                    _isQualityTrenchlessMethod = false;
                    NoOfColumns.Text = "";
                    NoOfColumns.IsEnabled = true;
                    NoOfColumns.IsReadOnly = false;

                    NoOfRows.Text = "";
                    NoOfRows.IsEnabled = true;
                    NoOfRows.IsReadOnly = false;

                    NoOfDucts.Text = "";
                    NoOfDucts.IsEnabled = true;
                    NoOfDucts.IsReadOnly = false;

                    NoOfCables.Text = "0";
                    NoOfCables.IsEnabled = false;
                    NoOfCables.IsReadOnly = true;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }


    }
}
