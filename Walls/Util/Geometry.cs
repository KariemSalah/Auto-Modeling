
#region Namespaces
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion

namespace CadToBim.Util
{
    public static class Geometry
    {
        public static List<GeometryObject> ExtractElement(UIDocument uidoc, ImportInstance import, string layer = "*", string type = "*")
        {
            Document doc = uidoc.Document;
            View active_view = doc.ActiveView;

            List<GeometryObject> visible_dwg_geo = new List<GeometryObject>();

            // Get Geometry
            var geoElem = import.get_Geometry(new Options());
            Debug.Print("Found elements altogether: " + geoElem.Count().ToString());
            foreach (var geoObj in geoElem)
            {
                if (geoObj is GeometryInstance)
                {
                    var geoIns = geoObj as GeometryInstance;
                    var ge2 = geoIns.GetInstanceGeometry();
                    if (ge2 != null)
                    {
                        foreach (var obj in ge2)
                        {
                            // Use the GraphicsStyle to get the DWG layer linked to the Category for visibility.
                            var gStyle = doc.GetElement(obj.GraphicsStyleId) as GraphicsStyle;

                            // If an object does not have a GraphicsStyle just skip it
                            if (gStyle == null)
                            {
                                continue;
                            }

                            // Check if the layer is visible in the view.
                            if (!active_view.GetCategoryHidden(gStyle.GraphicsStyleCategory.Id))
                            {
                                if (layer == "*")
                                {
                                    if (type == "*")
                                    {
                                        visible_dwg_geo.Add(obj);
                                    }
                                    else if (obj.GetType().Name == type)
                                    {
                                        visible_dwg_geo.Add(obj);
                                    }
                                }
                                // Select a certain Linetype
                                else if (gStyle.GraphicsStyleCategory.Name == layer)
                                {
                                    if (type == "*")
                                    {
                                        visible_dwg_geo.Add(obj);
                                    }
                                    else if (obj.GetType().Name == type)
                                    {
                                        visible_dwg_geo.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Debug.Print("Geometry collected: " + visible_dwg_geo.Count().ToString());
            return visible_dwg_geo;
        }

        public static List<Curve> ShatterCADGeometry(UIDocument uidoc, ImportInstance import, string layer, double tolerance)
        {
            List<Curve> shatteredCrvs = new List<Curve>();

            List<GeometryObject> dwg_geos = Geometry.ExtractElement(uidoc, import, layer);
            if (dwg_geos.Count > 0)
            {
                foreach (var obj in dwg_geos)
                {
                    if (obj.GetType().ToString() == "Autodesk.Revit.DB.Arc")
                    {
                        Arc arc = obj as Arc;
                        Debug.Print("An arc detected");
                        shatteredCrvs.Add(arc);
                        continue;
                    }

                    Curve crv = obj as Curve;
                    PolyLine poly = obj as PolyLine;

                    if (null != crv)
                    {
                        shatteredCrvs.Add(crv);
                    }
                    if (null != poly)
                    {
                        var vertices = poly.GetCoordinates();
                        for (int i = 0; i < vertices.Count() - 1; i++)
                        {
                            if ((vertices[i + 1] - vertices[i]).GetLength() >= tolerance)
                            {
                                shatteredCrvs.Add(Line.CreateBound(vertices[i], vertices[i + 1]) as Curve);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            return shatteredCrvs;
        }
    }
}

