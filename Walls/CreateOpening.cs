#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace CadToBim
{
    [Transaction(TransactionMode.Manual)]
    public static class CreateOpening
    {

        // Create a new opening type of the default family. Metric here.       
        private static FamilySymbol NewOpeningType(Document doc, string familyName,
            double width, double height, string type = "")
        {
            Family f = Misc.GetFirstElementOfTypeNamed(doc, typeof(Family), familyName) as Family;
            if (null == f)
            {
                // add default path and error handling here
                if (type == "Door")
                {
                    if (!doc.LoadFamily(Properties.Settings.Default.url_door, out f))
                    {
                        Debug.Print("Unable to load the default Door");
                    }
                }
                if (type == "Window")
                {
                    if (!doc.LoadFamily(Properties.Settings.Default.url_window, out f))
                    {
                        Debug.Print("Unable to load the default Window");
                    }
                }
                if (type == "")
                {
                    Debug.Print("Please specify the type to load a default setting");
                }
            }

            if (null != f)
            {
                FamilySymbol s = null;
                foreach (ElementId id in f.GetFamilySymbolIds())
                {
                    s = doc.GetElement(id) as FamilySymbol;
                    if (s.Name == width.ToString() + " x " + height.ToString() + "mm")
                    {
                        return s;
                    }
                }
                s = s.Duplicate(width.ToString() + " x " + height.ToString() + "mm") as FamilySymbol;

                // Define new dimensions for our new type;
                s.LookupParameter("Width").Set(Misc.MmToFoot(width));
                s.LookupParameter("Height").Set(Misc.MmToFoot(height));

                return s;
            }
            else
            {
                return null;
            }
        }


        public static void Execute(Document doc, List<Curve> doorCrvs, List<Curve> windowCrvs,
            List<Curve> wallCrvs, List<Util.Text.CADTextModel> labels,
            string nameDoor, string nameWindow, Level level, bool IsSilent)
        {
            View view = doc.ActiveView;

            var doorClusters = Algorithm.ClusterByIntersect(doorCrvs);
            List<List<Curve>> doorBlocks = new List<List<Curve>> { };
            foreach (List<Curve> cluster in doorClusters)
            {
                if (null != Algorithm.CreateBoundingBox2D(cluster))
                {
                    doorBlocks.Add(Algorithm.CreateBoundingBox2D(cluster));
                }
            }

            List<Curve> doorAxes = new List<Curve> { };
            foreach (List<Curve> doorBlock in doorBlocks)
            {
                List<Curve> doorFrame = new List<Curve> { };
                for (int i = 0; i < doorBlock.Count; i++)
                {
                    int sectCount = 0;
                    List<Curve> fenses = new List<Curve>();
                    foreach (Curve line in wallCrvs)
                    {
                        Curve testCrv = doorBlock[i].Clone();
                        SetComparisonResult result = RegionDetect.ExtendCrv(testCrv, 0.01).Intersect(line,
                                                   out IntersectionResultArray results);
                        if (result == SetComparisonResult.Overlap)
                        {
                            sectCount += 1;
                            fenses.Add(line);
                        }
                    }
                    if (sectCount == 2)
                    {
                        XYZ projecting = fenses[0].Evaluate(0.5, true);
                        XYZ projected = fenses[1].Project(projecting).XYZPoint;
                        if (fenses[0].Length > fenses[1].Length)
                        {
                            projecting = fenses[1].Evaluate(0.5, true);
                            projected = fenses[0].Project(projecting).XYZPoint;
                        }
                        Line doorAxis = Line.CreateBound(projecting, projected);
                        doorAxes.Add(doorAxis);

                    }
                }

            }
            Debug.Print("We got {0} door axes. ", doorAxes.Count);


            // Collect window blocks
            var windowClusters = Algorithm.ClusterByIntersect(windowCrvs);

            List<List<Curve>> windowBlocks = new List<List<Curve>> { };
            foreach (List<Curve> cluster in windowClusters)
            {
                if (null != Algorithm.CreateBoundingBox2D(cluster))
                {
                    windowBlocks.Add(Algorithm.CreateBoundingBox2D(cluster));
                }
            }


            List<Curve> windowAxes = new List<Curve> { };
            foreach (List<Curve> windowBlock in windowBlocks)
            {
                Line axis1 = Line.CreateBound((windowBlock[0].GetEndPoint(0) + windowBlock[0].GetEndPoint(1)).Divide(2),
                    (windowBlock[2].GetEndPoint(0) + windowBlock[2].GetEndPoint(1)).Divide(2));
                Line axis2 = Line.CreateBound((windowBlock[1].GetEndPoint(0) + windowBlock[1].GetEndPoint(1)).Divide(2),
                    (windowBlock[3].GetEndPoint(0) + windowBlock[3].GetEndPoint(1)).Divide(2));
                if (axis1.Length > axis2.Length)
                {
                    windowAxes.Add(axis1);
                }
                else
                {
                    windowAxes.Add(axis2);
                }
            }
            Debug.Print("We got {0} window axes. ", windowAxes.Count);


            if (IsSilent)
            {

                using (Transaction tx = new Transaction(doc, "Generate sub-surfaces & marks"))
                {
                    FailureHandlingOptions options = tx.GetFailureHandlingOptions();
                    options.SetFailuresPreprocessor(new Util.FailureProcess(false, false));

                    tx.SetFailureHandlingOptions(options);

                    tx.Start();

                    // Create door axis & instance
                    foreach (Curve doorAxis in doorAxes)
                    {
                        Wall hostWall = Wall.Create(doc, doorAxis, level.Id, true);

                        double width = Math.Round(Misc.FootToMm(doorAxis.Length), 0);
                        double height = 2000;
                        XYZ basePt = (doorAxis.GetEndPoint(0) + doorAxis.GetEndPoint(1)).Divide(2);
                        XYZ insertPt = basePt + XYZ.BasisZ * level.Elevation; // Absolute height
                        double span = Double.PositiveInfinity;
                        int labelId = -1;
                        foreach (Util.Text.CADTextModel text in labels)
                        {
                            double distance = basePt.DistanceTo(text.Location);
                            if (distance < span)
                            {
                                span = distance;
                                labelId = labels.IndexOf(text);
                            }
                        }
                        if (labelId > -1)
                        {
                            width = Util.Text.DecodeLabel(labels[labelId].Text, width, height).Item1;
                            height = Util.Text.DecodeLabel(labels[labelId].Text, width, height).Item2;

                        }

                        Debug.Print("Create new door with dimension {0}x{1}", width.ToString(), height.ToString());
                        FamilySymbol fs = NewOpeningType(doc, nameDoor, width, height, "Door");
                        if (null == fs) { continue; }
                        if (!fs.IsActive) { fs.Activate(); }

                        FamilyInstance fi = doc.Create.NewFamilyInstance(insertPt, fs, hostWall, level,
                            Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                    }

                    // Create window axis & instance
                    foreach (Curve windowAxis in windowAxes)
                    {
                        Wall hostWall = Wall.Create(doc, windowAxis, level.Id, true);

                        double width = Math.Round(Misc.FootToMm(windowAxis.Length), 0);
                        double height = 2500;

                        XYZ basePt = (windowAxis.GetEndPoint(0) + windowAxis.GetEndPoint(1)).Divide(2); // On plane
                        XYZ insertPt = basePt + XYZ.BasisZ * (Misc.MmToFoot(Properties.Settings.Default.sillHeight)); // Absolute height
                        double span = Misc.MmToFoot(2000);
                        int labelId = -1;
                        foreach (Util.Text.CADTextModel text in labels)
                        {
                            double distance = basePt.DistanceTo(text.Location);

                            if (distance < span)
                            {
                                span = distance;
                                labelId = labels.IndexOf(text);
                            }
                        }
                        if (labelId > -1)
                        {
                            width = Util.Text.DecodeLabel(labels[labelId].Text, width, height).Item1;
                            height = Util.Text.DecodeLabel(labels[labelId].Text, width, height).Item2;
                            if (height + Properties.Settings.Default.sillHeight > Properties.Settings.Default.floorHeight)
                            { height = Properties.Settings.Default.floorHeight - Properties.Settings.Default.sillHeight; }

                        }

                        Debug.Print("Create new window with dimension {0}x{1}", width.ToString(), height.ToString());
                        FamilySymbol fs = NewOpeningType(doc, nameWindow, width, height, "Window");
                        if (null == fs) { continue; }
                        if (!fs.IsActive) { fs.Activate(); }

                        FamilyInstance fi = doc.Create.NewFamilyInstance(insertPt, fs, hostWall, level,
                            Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                    }
                    tx.Commit();
                }
            }


            // UNDER PRESENTATION MODE 
            else
            {
                string caption = "Create openings";
                string task = "Creating doors...";

                Views.ProgressBar pb1 = new Views.ProgressBar(caption, task, doorAxes.Count);

                foreach (Curve doorAxis in doorAxes)
                {
                    using (Transaction tx = new Transaction(doc, "Generate doors & adherences"))
                    {

                        tx.Start();

                        Wall hostWall = Wall.Create(doc, doorAxis, level.Id, true);

                        double width = Math.Round(Misc.FootToMm(doorAxis.Length), 0);
                        double height = 2000;
                        XYZ basePt = (doorAxis.GetEndPoint(0) + doorAxis.GetEndPoint(1)).Divide(2);
                        XYZ insertPt = basePt + XYZ.BasisZ; // Absolute height
                        double span = Double.PositiveInfinity;
                        int labelId = -1;
                        foreach (Util.Text.CADTextModel text in labels)
                        {
                            double distance = basePt.DistanceTo(text.Location);
                            if (distance < span)
                            {
                                span = distance;
                                labelId = labels.IndexOf(text);
                            }
                        }
                        if (labelId > -1)
                        {
                            width = Util.Text.DecodeLabel(labels[labelId].Text, width, height).Item1;
                            height = Util.Text.DecodeLabel(labels[labelId].Text, width, height).Item2;

                        }

                        Debug.Print("Create new door with dimension {0}x{1}", width.ToString(), height.ToString());
                        FamilySymbol fs = NewOpeningType(doc, nameDoor, width, height, "Door");

                        if (null == fs) { continue; }
                        if (!fs.IsActive) { fs.Activate(); }

                        FamilyInstance fi = doc.Create.NewFamilyInstance(insertPt, fs, hostWall, level,
                            Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                        tx.Commit();
                    }
                    pb1.Increment();
                    if (pb1.ProcessCancelled) { break; }
                }
                pb1.Close();

                task = "Creating windows...";
                Views.ProgressBar pb2 = new Views.ProgressBar(caption, task, windowAxes.Count);

                // Create window axis & instance
                foreach (Curve windowAxis in windowAxes)
                {
                    using (Transaction tx = new Transaction(doc, "Generate windows & adherences"))
                    {
                        tx.Start();

                        Wall hostWall = Wall.Create(doc, windowAxis, level.Id, true);

                        double width = Math.Round(Misc.FootToMm(windowAxis.Length), 0);
                        double height = 2500;

                        XYZ basePt = (windowAxis.GetEndPoint(0) + windowAxis.GetEndPoint(1)).Divide(2); // On plane
                        XYZ insertPt = basePt + XYZ.BasisZ * (Misc.MmToFoot(Properties.Settings.Default.sillHeight)); // Absolute height
                        double span = Misc.MmToFoot(2000);
                        int labelId = -1;
                        foreach (Util.Text.CADTextModel text in labels)
                        {
                            double distance = basePt.DistanceTo(text.Location);

                            if (distance < span)
                            {
                                span = distance;
                                labelId = labels.IndexOf(text);
                            }
                        }
                        if (labelId > -1)
                        {
                            width = Util.Text.DecodeLabel(labels[labelId].Text, width, height).Item1;
                            height = Util.Text.DecodeLabel(labels[labelId].Text, width, height).Item2;
                            if (height + Properties.Settings.Default.sillHeight > Properties.Settings.Default.floorHeight)
                            { height = Properties.Settings.Default.floorHeight - Properties.Settings.Default.sillHeight; }

                        }

                        Debug.Print("Create new window with dimension {0}x{1}", width.ToString(), height.ToString());
                        FamilySymbol fs = NewOpeningType(doc, nameWindow, width, height, "Window");

                        if (null == fs) { continue; }
                        if (!fs.IsActive) { fs.Activate(); }

                        FamilyInstance fi = doc.Create.NewFamilyInstance(insertPt, fs, hostWall, level,
                            Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        tx.Commit();
                    }
                    pb2.Increment();
                    if (pb2.ProcessCancelled) { break; }
                }
                pb2.JobCompleted();

            }

        }
    }
}

