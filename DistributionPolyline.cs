using ActiproSoftware.Products.Logging;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace DistributionPlacement
{
    class LineMapPoints
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double Shd { get; init; }
        public double TopLevel { get; init; }
        public double Height { get; init; }
        public double Width { get; init; }
        public short NoOfColumns { get; init; }

        public short NoOfRows { get; init; }
        public short NoOfDucts { get; set; }
        public short NoOfCables { get; init; }

        public short Quality { get; set; }
        public short Haunches { get; set; }

        public string Source { get; set; }
        public string SurveyedBy { get; set; }
        public string Status { get; set; }
        public string TypeOfLine { get; set; }
        public DateTime dateOfInstallationDucts { get; set; }
        public DateTime dateOfInstallationNewestCables { get; set; }
        public DateTime dateOfLastSurvey { get; set; }
        public int Sequence { get; set; }

    }

    internal class DistributionPolyline : Button
    {
        private readonly string _shd = Configs.GetString("SHD") ?? "SHD";
        private readonly string _assetGroup = Configs.GetString("ASSETGROUP_TRENCH_SUREVEY_LINE") ?? "46";
        private readonly string _assetType = Configs.GetString("ASSETTYPE_TRENCH_SUREVEY_LINE") ?? "461";
        private readonly string _assetGroupField = Configs.GetString("ASSETGROUPFIELD") ?? "ASSETGROUP";
        private readonly string _assetTypeField = Configs.GetString("ASSETTYPEFIELD") ?? "ASSETTYPE";
        private readonly string _shape = Configs.GetString("SHAPE") ?? "SHAPE";
        private readonly string _topLevel = Configs.GetString("TopLevel") ?? "TopLevel";
        private readonly string _height = Configs.GetString("Height") ?? "Height";
        private readonly string _externalSourceField = Configs.GetString("ExternalSourceField") ?? "ExternalSource";
        private readonly string _externalSource = Configs.GetString("ExternalSource") ?? "1";
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
        private readonly string _dateOfInstallDucts = Configs.GetString("Date_of_Installation_Ducts") ?? "Date_of_Installation_Ducts";
        private readonly string _dateOfInstallCables = Configs.GetString("Date_of_Installation_Cables") ?? "Date_of_Installation_Cables";
        private readonly string _dateOfLastSurvey = Configs.GetString("Date_of_Last_Survey") ?? "Date_of_Last_Survey";


        /// <summary>
        /// 
        /// </summary>
        protected override void OnClick()
        {
            AppLogger._log.Info("Distribution survey line tool initiated");
            QueuedTask.Run(() =>
            {
                IReadOnlyList<GroupLayer> groupLayers = MapView.Active.Map.Layers.OfType<GroupLayer>().ToList();
                var featurePolylineLayer = MapView.Active?.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                    .DefaultIfEmpty(null).FirstOrDefault(lyr =>
                        (lyr.Name == "XYZ_TrenchLine" && lyr.CanEditData() == true &&
                         lyr.ShapeType == esriGeometryType.esriGeometryPolyline));
                var disSurveyPointLayer = MapView.Active?.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                    .DefaultIfEmpty(null).FirstOrDefault(lyr =>
                        (lyr.Name == "XYZ_TrenchSurveyPoint" && lyr.CanEditData() == true &&
                         lyr.ShapeType == esriGeometryType.esriGeometryPoint));
                var disStructurePointLayer = MapView.Active?.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                    .DefaultIfEmpty(null).FirstOrDefault(lyr =>
                        (lyr.Name == "XYZ_ElectricStructure" && lyr.CanEditData() == true &&
                         lyr.ShapeType == esriGeometryType.esriGeometryPoint));

                try
                {
                    List<LineMapPoints> listOfPoints = new List<LineMapPoints>();

                    var disSurveyPointFeatureClass = disSurveyPointLayer?.GetTable() as FeatureClass;
                    var electricStructurePointFeatureClass = disStructurePointLayer?.GetTable() as FeatureClass;
                    var polylineFeatureClass = featurePolylineLayer?.GetTable() as FeatureClass;

                    // retrieve the feature class schema information for the feature classes
                    var polyLineDefinition = polylineFeatureClass?.GetDefinition() as FeatureClassDefinition;
                    var disSurveyPointDefinition = disSurveyPointFeatureClass?.GetDefinition() as FeatureClassDefinition;
                    var dissStructurePointDefinition = electricStructurePointFeatureClass?.GetDefinition() as FeatureClassDefinition;


                    // set up the edit operation for the feature creation
                    var createPolylineOperation = new EditOperation()
                    {
                        Name = "Create poly lines",
                        SelectNewFeatures = false
                    };
                    QueuedTask.Run(() =>
                    {
                        var disSurveyPointSelection = disSurveyPointLayer?.GetSelection().GetObjectIDs();
                        var disStructureSelection = disStructurePointLayer?.GetSelection().GetObjectIDs();
                        var count = (disSurveyPointSelection ?? Array.Empty<long>()).Count() +
                                    (disStructureSelection ?? Array.Empty<long>())
                                    .Count();
                        if (count < 2)
                        {
                            MessageBox.Show("Must select more than one point to generate line");
                        }
                        else
                        {
                            try
                            {
                                IReadOnlyList<long> disSurveyPointSelectedOiDs = disSurveyPointSelection;
                                QueryFilter queryFilterSp = new QueryFilter
                                { ObjectIDs = disSurveyPointSelectedOiDs };
                                IReadOnlyList<long> disStructureSelectedOiDs = disStructureSelection;
                                QueryFilter queryFilterEs = new QueryFilter { ObjectIDs = disStructureSelectedOiDs };
                                int seq = 0;
                                using (RowCursor rowCursor = disSurveyPointLayer?.Search(queryFilterSp))
                                {
                                    //Loop through selected point features
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Row row = rowCursor.Current)
                                        {
                                            //insert into List of points 
                                            Feature pointFeature = row as Feature;

                                            listOfPoints.Add(new LineMapPoints()
                                            {
                                                X = ((MapPoint)pointFeature?.GetShape()).Coordinate2D.X,
                                                Y = ((MapPoint)pointFeature?.GetShape()).Coordinate2D.Y,
                                                TopLevel = double.Parse(row["TopLevel"]?.ToString() ?? "0"),
                                                Height = double.Parse(row["Height"]?.ToString() ?? "0"),
                                                Width = double.Parse(row["Width"]?.ToString() ?? "0"),
                                                NoOfColumns = short.Parse(row["No_of_Columns"]?.ToString() ?? "0"),
                                                NoOfRows = short.Parse(row["No_of_Rows"]?.ToString() ?? "0"),
                                                NoOfDucts = short.Parse(row["No_of_Ducts"]?.ToString() ?? "0"),
                                                SurveyedBy = row["Surveyed_by"]?.ToString() ?? "",
                                                NoOfCables = short.Parse(row["No_of_Cables"]?.ToString() ?? "0"),
                                                Shd = double.Parse(row["SHD"]?.ToString() ?? "0"),
                                                Haunches = short.Parse(row["Haunches"]?.ToString() ?? "0"),
                                                Quality = short.Parse(row["Quality"]?.ToString() ?? "0"),
                                                TypeOfLine = row["Type_of_Line"]?.ToString() ?? "",
                                                dateOfInstallationDucts = DateTime.Parse(row["Date_of_Installation_Ducts"]?.ToString()),
                                                dateOfInstallationNewestCables = DateTime.Parse(row["Date_of_Installation_Cables"]?.ToString()),
                                                dateOfLastSurvey = DateTime.Parse(row["Date_of_Last_Survey"]?.ToString()),
                                                Status = row["Status"]?.ToString() ?? "",
                                                Source = "survey point",
                                                Sequence = seq
                                            });
                                        }

                                        seq++;
                                    }
                                }

                                using (RowCursor rowCursor = disStructurePointLayer?.Search(queryFilterEs))
                                {
                                    //Loop through selected point features
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Row row = rowCursor.Current)
                                        {


                                            Feature pointFeature = row as Feature;
                                            listOfPoints.Add(new LineMapPoints()
                                            {
                                                X = ((ArcGIS.Core.Geometry.MapPoint)pointFeature?.GetShape())
                                                    .Coordinate2D.X,
                                                Y = ((ArcGIS.Core.Geometry.MapPoint)pointFeature?.GetShape())
                                                    .Coordinate2D.Y,
                                                TopLevel = double.Parse(row["TopLevel"]?.ToString() ?? "0"),
                                                Height = double.Parse(row["Height"]?.ToString() ?? "0"),
                                                Width = double.Parse(row["Width"]?.ToString() ?? "0"),
                                                NoOfColumns = short.Parse(row["No_of_Columns"]?.ToString() ?? "0"),
                                                NoOfRows = short.Parse(row["No_of_Rows"]?.ToString() ?? "0"),
                                                SurveyedBy = row["Surveyed_by"]?.ToString() ?? "",
                                                NoOfCables = short.Parse(row["No_of_Cables"]?.ToString() ?? "0"),
                                                Shd = double.Parse(row["SHD"]?.ToString() ?? "0"),
                                                Haunches = short.Parse(row["Haunches"]?.ToString() ?? "0"),
                                                Quality = short.Parse(row["Quality"]?.ToString() ?? "0"),
                                                TypeOfLine = row["Type_of_Line"]?.ToString() ?? "",
                                                Status = row["Status"]?.ToString() ?? "",
                                                Source = "structure",
                                                Sequence = seq
                                            });
                                        }

                                        seq++;
                                    }
                                }
                                List<LineMapPoints> sortedListOfPoints = new List<LineMapPoints>();
                               // var sortedListOfPoints = listOfPoints; //.OrderBy(p => p.X).ToList();
                                //short ptNoOfDucts = 0;
                                //short ptHaunches = 0;

                                int i = 0;
                                List<int> posList = new List<int>();
                                List<int> diff = new List<int>();
                                posList.Add(i);
                                // foreach (var pt in sortedListOfPoints)
                                sortedListOfPoints.Add(listOfPoints[0]);
                                listOfPoints.RemoveAt(0);
                                for (int q=0;q< sortedListOfPoints.Count;q++)
                                {
                                    var listPoints = new List<MapPoint>();
                                    MapPoint pt1= MapPointBuilderEx.CreateMapPoint(sortedListOfPoints[sortedListOfPoints.Count-1].X, sortedListOfPoints[sortedListOfPoints.Count - 1].Y, 0.0);
                                        for (int j = 0; j < listOfPoints.Count; j++)
                                        {
                                            var mp = MapPointBuilderEx.CreateMapPoint(listOfPoints[j].X, listOfPoints[j].Y, 0.0);
                                            listPoints.Add(mp);
                                        } 
                                    //Find nearest point to a particular point
                                    Multipoint multipoint = MultipointBuilderEx.CreateMultipoint(listPoints);
                                    var result = GeometryEngine.Instance.NearestVertex(multipoint, pt1);
                                    for (int k = 0; k < listOfPoints.Count; k++)
                                    {
                                        if (listOfPoints[k].X == result.Point.X &&
                                            listOfPoints[k].Y == result.Point.Y)
                                        {
                                            sortedListOfPoints.Add(listOfPoints[k]);
                                            listOfPoints.RemoveAt(k);

                                        }

                                    }
                                }

                                ////////////////////////////////////////////////////////////////////
                                ////foreach (int pos in posList)
                                var plCoords = new List<Coordinate3D> { };

                                for (int pos = 0; pos < sortedListOfPoints.Count - 1; pos++)
                                {
                                    
                                    var ptShd = sortedListOfPoints[pos].Shd;
                                    var ptTopLevel = sortedListOfPoints[pos].TopLevel;
                                    var ptHeight = sortedListOfPoints[pos].Height;
                                    var ptWidth = sortedListOfPoints[pos].Width;
                                    var ptNoOfRows = sortedListOfPoints[pos].NoOfRows;
                                    var ptNoOfColumns = sortedListOfPoints[pos].NoOfColumns;
                                    var ptNoOfDucts = sortedListOfPoints[pos].NoOfDucts;
                                    var ptNoOfCables = sortedListOfPoints[pos].NoOfCables;
                                    var ptSurveyedBy = sortedListOfPoints[pos].SurveyedBy;
                                    var ptStatus = sortedListOfPoints[pos].Status;
                                    var ptQuality = sortedListOfPoints[pos].Quality;
                                    var ptHaunches = sortedListOfPoints[pos].Haunches;
                                    var ptTypeOfLine = sortedListOfPoints[pos].TypeOfLine;
                                    var ptdateOfInstallationNewestCables = sortedListOfPoints[pos].dateOfInstallationNewestCables;
                                    var ptdateOfInstallationDucts = sortedListOfPoints[pos].dateOfInstallationDucts;
                                    var ptdateOfLastSurvey = sortedListOfPoints[pos].dateOfLastSurvey;

                                    if (sortedListOfPoints[pos].Height ==
                                        sortedListOfPoints[pos+1].Height
                                        && sortedListOfPoints[pos].TopLevel ==
                                        sortedListOfPoints[pos+1].TopLevel
                                        && sortedListOfPoints[pos].Width ==
                                        sortedListOfPoints[pos+1].Width
                                        && sortedListOfPoints[pos].NoOfColumns ==
                                        sortedListOfPoints[pos+1].NoOfColumns
                                        && sortedListOfPoints[pos].NoOfRows ==
                                        sortedListOfPoints[pos + 1].NoOfRows && pos != sortedListOfPoints.Count - 2
                                       && sortedListOfPoints[pos].Source == sortedListOfPoints[pos+1].Source
                                        )


                                    {

                                        plCoords.Add(new Coordinate3D(sortedListOfPoints[pos].X,
                                                sortedListOfPoints[pos].Y, 0.0));

                                    }
                                    else
                                    {
                                        plCoords.Add(new Coordinate3D(sortedListOfPoints[pos].X,
                                               sortedListOfPoints[pos].Y, 0.0));
                                        plCoords.Add(new Coordinate3D(sortedListOfPoints[pos+1].X,
                                                sortedListOfPoints[pos+1].Y, 0.0));
                                        Polyline newPolyline = PolylineBuilderEx.CreatePolyline(plCoords,
                                                polyLineDefinition?.GetSpatialReference());

                                        var attributes = new Dictionary<string, object>
                                        {
                                            { _shape, newPolyline },
                                            { _assetGroupField, _assetGroup },
                                            { _assetTypeField, _assetType },
                                            { _topLevel, ptTopLevel },
                                            { _externalSourceField, _externalSource },
                                            { _height, ptHeight },
                                            { _width, ptWidth },
                                            { _numberOfRows, ptNoOfRows },
                                            { _numberOfColumns, ptNoOfColumns },
                                            { _numberOfDucts, ptNoOfDucts },
                                            {_numberOfCables, ptNoOfCables },
                                            { _haunches, ptHaunches },
                                            { _quality, ptQuality },
                                            {_dateOfLastSurvey, ptdateOfLastSurvey},
                                            { _surveyedBy, ptSurveyedBy },
                                            { _status, ptStatus },
                                            { _typeOfLine, ptTypeOfLine },
                                            { _shd, ptShd },
                                            { _dateOfInstallCables, ptdateOfInstallationNewestCables },
                                            { _dateOfInstallDucts, ptdateOfInstallationDucts },
                                        };
                                        createPolylineOperation.Create(featurePolylineLayer, attributes);
                                        plCoords.Clear();

                                        //plCoords.Add(new Coordinate3D(sortedListOfPoints[posList[pos+1]].X,
                                        //        sortedListOfPoints[posList[pos+1]].Y, 0.0));

                                    }
                                }

                                createPolylineOperation.Execute();

                                if (createPolylineOperation.IsSucceeded)
                                {
                                    AppLogger._log.Error("Created Distribution surveyline feature successfully");
                                    MessageBox.Show("Created Distribution surveyline feature successfully");
                                   

                                }
                                else
                                {
                                    AppLogger._log.Error("Created Distribution surveyline feature error:\n" +
                                                         createPolylineOperation.ErrorMessage);
                                    MessageBox.Show("Failed to create Distribution surveyline feature");

                                }
                            }
                            catch (Exception ex)
                            {
                                AppLogger._log.Error(
                                    "Failed to create Distribution surveyline feature:\n" + ex.Message);
                                MessageBox.Show("Failed to create Distribution surveyline feature.");
                            }
                            finally
                            {
                                AppLogger._log.Info("Feature layer creation ended.");
                            }
                        }

                    });
                }
                catch (Exception ex)
                {
                    AppLogger._log.Error("A handled exception just occurred: " + ex.Message);
                    MessageBox.Show("Failed to create Distribution surveyline feature successfully");
                }
                finally
                {
                    AppLogger._log.Info("Distribution survey line tool ended.");
                }
            });
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
    }
}
