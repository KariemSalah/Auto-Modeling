#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion

namespace CadToBim
{
    [Transaction(TransactionMode.Manual)]
    public class RegionDetect : IExternalCommand
    {
        // Print list for debug
        public static string printo(List<int> list)
        {
            string fusion = "";
            for (int index = 0; index < list.Count(); index++)
            {
                fusion = fusion + list[index].ToString() + " ";
            }
            return fusion;
        }

        public static string printb(List<bool> list)
        {
            string fusion = "";
            for (int index = 0; index < list.Count(); index++)
            {
                fusion = fusion + list[index].ToString() + " ";
            }
            return fusion;
        }

        public static string printd(Dictionary<int, List<Curve>> dictionary)
        {
            string fusion = "";
            foreach (KeyValuePair<int, List<Curve>> kvp in dictionary)
            {
                fusion = fusion + kvp.Key.ToString() + "-" + kvp.Value.Count().ToString() + " ";
            }
            return fusion;
        }

        public static List<Curve> SplitCrv(Curve parent, List<double> parameters)
        {
            double threshold = 0.00001;
            List<Curve> segments = new List<Curve>();
            parameters.Add(parent.GetEndParameter(1));
            parameters.Insert(0, parent.GetEndParameter(0));
            double[] params_ordered = parameters.ToArray();
            Array.Sort(params_ordered);
            List<double> params_rectified = new List<double>();
            params_rectified.Add(params_ordered[0]);

            for (int paraId = 1; paraId < params_ordered.Length; paraId++)
            {
                if (params_ordered[paraId] - params_rectified.Last() > threshold)
                {
                    params_rectified.Add(params_ordered[paraId]);
                }
            }
            for (int index = 0; index < params_rectified.Count - 1; index++)
            {
                Curve segment = parent.Clone();
                segment.MakeBound(params_rectified[index], params_rectified[index + 1]);
                segments.Add(segment);
            }

            return segments;
        }

        // Absolute angle from curve to curve only on basic horizontal plan
        public static double AngleBetweenCrv(Curve crv1, Curve crv2, XYZ axis)
        {
            XYZ pt_origin = new XYZ();
            Line line1 = crv1 as Line;
            Line line2 = crv2 as Line;
            XYZ vec1 = line1.Direction.Normalize();
            XYZ vec2 = line2.Direction.Normalize();
            XYZ testVec = vec1.CrossProduct(vec2).Normalize();
            double testAngle = vec1.AngleTo(vec2);
            if (testVec.IsAlmostEqualTo(axis.Normalize()))
            {
                return testAngle;
            }
            else
            {
                return 2 * Math.PI - testAngle;
            }
        }

        // Shatter a bunch of curves according to their intersections 
        public static List<Curve> ExplodeCrv(List<Curve> C)
        {
            List<Curve> shatters = new List<Curve>();
            for (int CStart = 0; CStart <= C.Count - 1; CStart++)
            {
                List<double> breakParams = new List<double>();
                for (int CCut = 0; CCut <= C.Count - 1; CCut++)
                {
                    if (CStart != CCut)
                    {
                        SetComparisonResult result = C[CStart].Intersect(C[CCut], out IntersectionResultArray results);
                        if (result != SetComparisonResult.Disjoint)
                        {
                            double breakParam = results.get_Item(0).UVPoint.U;
                            breakParams.Add(breakParam);

                        }
                    }
                }
                shatters.AddRange(SplitCrv(C[CStart], breakParams));
            }
            return shatters;
        }

        // Check if 2 lines are the same by comparing endpoints
        public static bool CompareLines(Curve crv1, Curve crv2)
        {
            double bias = 0.00001;
            Line line1 = crv1 as Line;
            Line line2 = crv2 as Line;
            XYZ startPt1 = line1.GetEndPoint(0);
            XYZ endPt1 = line1.GetEndPoint(1);
            XYZ startPt2 = line2.GetEndPoint(0);
            XYZ endPt2 = line2.GetEndPoint(1);
            if ((startPt1.IsAlmostEqualTo(startPt2, bias) && endPt1.IsAlmostEqualTo(endPt2, bias))
                || (startPt1.IsAlmostEqualTo(endPt2, bias) && endPt1.IsAlmostEqualTo(startPt2, bias)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Tuple<List<Curve>, List<Curve>> FlattenLines(List<CurveArray> polys)
        {
            List<Curve> curvePool = new List<Curve>();
            List<Curve> curveMesh = new List<Curve>();
            List<Curve> curveBoundary = new List<Curve>();
            foreach (CurveArray poly in polys)
            {
                foreach (Curve polyline in poly)
                {
                    curvePool.Add(polyline);
                }
            }
            List<int> meshList = new List<int>();
            List<int> killList = new List<int>();
            for (int i = 0; i < curvePool.Count(); i++)
            {
                for (int j = i + 1; j < curvePool.Count(); j++)
                {
                    if (CompareLines(curvePool[i], curvePool[j]))
                    {
                        meshList.Add(i);
                        killList.Add(i);
                        killList.Add(j);
                    }
                }
            }
            for (int i = 0; i < curvePool.Count(); i++)
            {
                if (meshList.Contains(i) || !killList.Contains(i)) { curveMesh.Add(curvePool[i]); }
                if (!killList.Contains(i)) { curveBoundary.Add(curvePool[i]); }
            }
            return Tuple.Create(curveMesh, curveBoundary);
        }

        public static List<Curve> GetBoundary(List<CurveArray> polys)
        {
            bool IsSame(Curve crv1, Curve crv2)
            {
                XYZ start1 = crv1.GetEndPoint(0);
                XYZ start2 = crv2.GetEndPoint(0);
                XYZ end1 = crv1.GetEndPoint(1);
                XYZ end2 = crv2.GetEndPoint(1);
                if (start1.IsAlmostEqualTo(start2) && end1.IsAlmostEqualTo(end2) ||
                    start1.IsAlmostEqualTo(end2) && start2.IsAlmostEqualTo(end1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            List<Curve> shatters = new List<Curve>();
            List<Curve> boundary = new List<Curve>();
            foreach (CurveArray poly in polys)
            {
                for (int i = 0; i < poly.Size; i++)
                {
                    shatters.Add(poly.get_Item(i));
                }
            }
            Debug.Print("Shatters in all: " + shatters.Count.ToString());
            for (int i = 0; i < shatters.Count; i++)
            {
                for (int j = 0; j < shatters.Count; j++)
                {
                    if (j != i)
                    {
                        if (IsSame(shatters[i], shatters[j]))
                        {
                            goto a;
                        }
                    }
                }
                boundary.Add(shatters[i]);
            a:;
            }
            Debug.Print("Boundry size: " + boundary.Count.ToString());
            return boundary;
        }

        public static CurveArray AlignCrv(List<Curve> polylines)
        {
            List<Curve> imagelines = polylines.ToList();
            int lineNum = imagelines.Count;
            CurveArray polygon = new CurveArray();
            polygon.Append(imagelines[0]);
            imagelines.RemoveAt(0);
            while (polygon.Size < lineNum)
            {
                XYZ endPt = polygon.get_Item(polygon.Size - 1).GetEndPoint(1);
                for (int i = 0; i < imagelines.Count; i++)
                {
                    if (imagelines[i].GetEndPoint(0).IsAlmostEqualTo(endPt))
                    {
                        polygon.Append(imagelines[i]);
                        imagelines.Remove(imagelines[i]);
                        break;
                    }
                    if (imagelines[i].GetEndPoint(1).IsAlmostEqualTo(endPt))
                    {
                        polygon.Append(imagelines[i].CreateReversed());
                        imagelines.Remove(imagelines[i]);
                        break;
                    }
                }
            }
            return polygon;
        }

        public static bool PointInPoly(CurveArray crvArr, XYZ pt)
        {
            int judgement = 0;
            int counter = 0;
            foreach (Curve crv in crvArr)
            {
                XYZ ptstart = crv.GetEndPoint(0);
                XYZ ptend = crv.GetEndPoint(1);
                XYZ direction = (pt - ptstart).CrossProduct(pt - ptend).Normalize();
                if (direction.IsAlmostEqualTo(new XYZ(0, 0, 1)))
                {
                    judgement += 1;
                }
                counter += 1;
            }
            if (judgement < counter) { return false; }
            else { return true; }
        }

        public static CurveArray PolyLineToCurveArray(PolyLine poly, double tolerance)
        {
            var vertices = poly.GetCoordinates();
            CurveArray shatters = new CurveArray();
            for (int i = 0; i < vertices.Count() - 1; i++)
            {
                if ((vertices[i + 1] - vertices[i]).GetLength() >= tolerance)
                {
                    shatters.Append(Line.CreateBound(vertices[i], vertices[i + 1]) as Curve);
                }
                else
                {
                    continue;
                }
            }
            return shatters;
        }

        public static bool PolyInPoly(PolyLine test, CurveArray target)
        {
            int judgement = 0;
            int counter = 0;
            var testPts = test.GetCoordinates();
            foreach (XYZ testPt in testPts)
            {
                if (PointInPoly(target, testPt))
                {
                    judgement += 1;
                }
                counter += 1;
            }
            if (judgement < counter) { return false; }
            else { return true; }
        }

        public static XYZ PolyCentPt(PolyLine poly)
        {
            var vertices = poly.GetCoordinates();
            int counter;
            XYZ sumPt = XYZ.Zero;
            if (vertices[0].IsAlmostEqualTo(vertices.Last()))
            {
                counter = vertices.Count() - 1;
            }
            else
            {
                counter = vertices.Count();
            }

            for (int i = 0; i < counter; i++)
            {
                sumPt += vertices[i];
            }
            return sumPt.Divide(counter);
        }

        public static Curve ExtendCrv(Curve crv, double ratio)
        {
            double pstart = crv.GetEndParameter(0);
            double pend = crv.GetEndParameter(1);
            double pdelta = ratio * (pend - pstart);

            crv.MakeUnbound();
            crv.MakeBound(pstart - pdelta, pend + pdelta);
            return crv;
        }


        public static List<CurveArray> RegionCluster(List<Curve> C)
        {
            List<Curve> Crvs = new List<Curve>();

            for (int CStart = 0; CStart <= C.Count - 1; CStart++)
            {
                List<double> breakParams = new List<double>();
                for (int CCut = 0; CCut <= C.Count - 1; CCut++)
                {
                    if (CStart != CCut)
                    {
                        SetComparisonResult result = C[CStart].Intersect(C[CCut], out IntersectionResultArray results);
                        if (result != SetComparisonResult.Disjoint)
                        {
                            double breakParam = results.get_Item(0).UVPoint.U;
                            breakParams.Add(breakParam);

                        }
                    }
                }
                Crvs.AddRange(SplitCrv(C[CStart], breakParams));
            }

            List<XYZ> Vtc = new List<XYZ>();
            List<Curve> HC = new List<Curve>();
            List<int> HCI = new List<int>();
            List<int> HCO = new List<int>();
            List<int> HCN = new List<int>();
            List<int> HCV = new List<int>();
            List<int> HCF = new List<int>();
            List<bool> HCK = new List<bool>();

            Dictionary<int, List<Curve>> F = new Dictionary<int, List<Curve>>();
            Dictionary<int, List<int>> VOut = new Dictionary<int, List<int>>();

            foreach (Curve Crv in Crvs) // cycle through each curve
            {
                for (int CRun = 0; CRun <= 2; CRun += 2)
                {
                    XYZ testedPt = new XYZ();
                    if (CRun == 0)
                    {
                        HC.Add(Crv);
                        testedPt = Crv.GetEndPoint(0);
                    }
                    else
                    {
                        HC.Add(Crv.CreateReversed());
                        testedPt = Crv.GetEndPoint(1);
                    }
                    HCI.Add(HCI.Count); // count this iteration
                    HCO.Add(HCI.Count - CRun);
                    HCN.Add(-1);
                    HCF.Add(-1);
                    HCK.Add(false);

                    int VtcSet = -1;

                    for (int VtxCheck = 0; VtxCheck <= Vtc.Count - 1; VtxCheck++)
                    {
                        if (Vtc[VtxCheck].DistanceTo(testedPt) < 0.0000001)
                        {
                            VtcSet = VtxCheck; // get the vertex index, if it already exists
                            break;
                        }
                    }

                    if (VtcSet > -1)
                    {
                        HCV.Add(VtcSet); // If the vertex already exists, set the half-curve vertex
                        VOut[VtcSet].Add(HCI.Last());
                    }
                    else
                    {
                        HCV.Add(Vtc.Count); // if the vertex doesn't already exist, add a new vertex index
                        VOut.Add(Vtc.Count, new List<int>() { HCI.Last() });

                        Vtc.Add(testedPt);

                    }
                    Crv.CreateReversed();
                }
            }

            foreach (KeyValuePair<int, List<int>> path in VOut)
            {

                if (path.Value.Count == 1)
                {
                    HCK[path.Value[0]] = true;
                    HCK[HCO[path.Value[0]]] = true;
                }
            }
            Debug.Print("Elements inside Crvs are " + Crvs.Count.ToString());
            Debug.Print("Elements inside HC are " + HC.Count.ToString());
            Debug.Print("Elements inside VOut are " + VOut.Count.ToString());
            Debug.Print("Elements inside HCK are " + HCK.Count.ToString());



            foreach (int HCIdx in HCI)
            {
                int minIdx = -1;
                double minAngle = 2 * Math.PI;

                foreach (int HCOut in VOut[HCV[HCO[HCIdx]]])
                {
                    if (HCOut != HCO[HCIdx] & HCK[HCIdx] == false & HCK[HCOut] == false)
                    {
                        double testAngle = AngleBetweenCrv(HC[HCOut], HC[HCO[HCIdx]], new XYZ(0, 0, 1));

                        if (testAngle < minAngle)
                        {
                            minIdx = HCOut;
                            minAngle = testAngle;
                        }
                    }
                }
                HCN[HCIdx] = minIdx;
            }

            Debug.Print("Elements inside HCI are " + HCI.Count.ToString());
            Debug.Print("Elements inside HCN are " + HCN.Count.ToString());
            Debug.Print("Elements in HCN: " + printo(HCN));


            List<int> FaceEdges = new List<int>();
            List<int> DeleteEdges = new List<int>();

            // cycle through each half-curve
            foreach (int HCIdx in HCI)
            {
                int EmExit = 0;
                if (HCF[HCIdx] == -1)
                {
                    int EdgeCounter = 1;
                    int FaceIdx = F.Count();
                    int CurrentIdx = HCIdx;
                    F.Add(FaceIdx, new List<Curve>() { HC[CurrentIdx] });
                    HCF[CurrentIdx] = FaceIdx;
                    do
                    {
                        if (HCN[CurrentIdx] == -1)
                        {
                            DeleteEdges.Add(FaceIdx);
                            break;
                        }

                        CurrentIdx = HCN[CurrentIdx];
                        F[FaceIdx].Add(HC[CurrentIdx]);
                        EdgeCounter += 1;
                        HCF[CurrentIdx] = FaceIdx;
                        if (HCN[CurrentIdx] == HCIdx)
                            break;
                        EmExit += 1;
                        if (EmExit == Crvs.Count - 1)
                            break;
                    }
                    while (true);

                    FaceEdges.Add(EdgeCounter);
                }
            }


            // Find the perimeter by counting edges of a region
            int Perim = -1;
            int PerimCount = -1;
            for (int FE = 0; FE <= FaceEdges.Count - 1; FE++)
            {
                if (FaceEdges[FE] > PerimCount)
                {
                    Perim = FE;
                    PerimCount = FaceEdges[FE];
                }
            }
            DeleteEdges.Add(Perim);

            int NewPath = 0;
            List<CurveArray> OutputFaces = new List<CurveArray>();

            foreach (KeyValuePair<int, List<Curve>> kvp in F)
            {
                if (DeleteEdges.Contains(kvp.Key) == false)
                {
                    CurveArray tempLoop = new CurveArray();
                    foreach (Curve element in kvp.Value)
                    {
                        tempLoop.Append(element);
                    }
                    OutputFaces.Add(tempLoop);
                    NewPath += 1;
                }
            }

            return OutputFaces;
        }


        // Create walls
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;

            Autodesk.Revit.Creation.Application appCreation = app.Create;
            Autodesk.Revit.Creation.Document docCreation = doc.Create;

            // Access current selection
            Selection sel = uidoc.Selection;

            CurveElementFilter filter = new CurveElementFilter(CurveElementType.ModelCurve);
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> founds = collector.WherePasses(filter).ToElements();
            List<CurveElement> importCurves = new List<CurveElement>();
            foreach (CurveElement ce in founds)
            {
                importCurves.Add(ce);
            }
            var strayCurves = importCurves.Where(x => x.LineStyle.Name == "WALL").ToList();
            List<Curve> strayLines = new List<Curve>();
            foreach (CurveElement ce in strayCurves)
            {
                strayLines.Add(ce.GeometryCurve as Line);
            }


            // Get the current building level
            FilteredElementCollector colLevels = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.INVALID)
                .OfClass(typeof(Level));
            Level firstLevel = colLevels.FirstElement() as Level;

            // Get the building floortype
            FloorType floorType = new FilteredElementCollector(doc)
                .OfClass(typeof(FloorType))
                .First<Element>(e => e.Name.Equals("Generic 150mm")) as FloorType;

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Generate Walls");

                List<CurveArray> curveGroup = RegionCluster(strayLines);

                Plane Geomplane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
                SketchPlane sketch = SketchPlane.Create(doc, Geomplane);

                foreach (CurveArray group in curveGroup)
                {
                    foreach (Curve edge in group)
                    {
                        DetailLine axis = doc.Create.NewDetailCurve(view, edge) as DetailLine;
                        GraphicsStyle gs = axis.LineStyle as GraphicsStyle;
                        gs.GraphicsStyleCategory.LineColor = new Color(202, 51, 82);
                        gs.GraphicsStyleCategory.SetLineWeight(7, gs.GraphicsStyleType);
                    }
                }

                tx.Commit();
            }


            return Result.Succeeded;
        }

    }
}

