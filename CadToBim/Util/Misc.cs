
#region Namespaces
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
#endregion

namespace CadToBim
{
    public static class Misc
    {
        #region Resource
        public static Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            Debug.Print("########" + assemblyPath);
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }
        #endregion


        #region Text Processing
        public static int ExtractIndex(string str)
        {
            int index = -1;
            string numStr = "";
            for (int i = 0; i < str.Length; i++)
            {
                if (Char.IsNumber(str, i))
                {
                    numStr += str[i];
                }

            }
            if (numStr != "")
            {
                index = Convert.ToInt32(numStr);
            }
            return index;
        }

        public static List<string> GetLayerNames(string layerChain)
        {
            List<string> names = new List<string>();
            string[] split = layerChain.Split(new string[] { ",", "." }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string name in split)
            {
                names.Add(name.Trim());
            }
            Debug.Print("##### " + ListString(names));
            return names;
        }
        #endregion 


        #region Selection
        public static int GetLevel(string label, string key)
        {
            int level = -1;
            if (label.Contains(key))
            {
                level = ExtractIndex(label);
            }
            return level;
        }


        // Return the first element of the given type and name.
        public static Element GetFirstElementOfTypeNamed(Document doc, Type type, string name)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(type);

            Func<Element, bool> nameEquals = e => e.Name.Equals(name);

            return collector.Any<Element>(nameEquals) ? collector.First<Element>(nameEquals) : null;
        }
        #endregion


        #region Conversion
        // Convert a given length in feet to milimeters.
        public static double FootToMm(double length) { return length * 304.8; }

        // Convert a given length in milimeters to feet.
        public static double MmToFoot(double length) { return length / 304.8; }

        // Convert a given point or vector from milimeters to feet.
        public static XYZ MmToFoot(XYZ v) { return v.Divide(304.8); }

        // Convert List of lines to List of curves, name="lines"
        public static List<Curve> LinesToCrvs(List<Line> lines)
        {
            List<Curve> crvs = new List<Curve>();
            foreach (Line line in lines)
            {
                crvs.Add(line as Curve);
            }
            return crvs;
        }

        // Convert List of curves to List of lines, name="crvs"
        public static List<Line> CrvsToLines(List<Curve> crvs)
        {
            List<Line> lines = new List<Line>();
            foreach (Curve crv in crvs)
            {
                lines.Add(crv as Line);
            }
            return lines;
        }

        #endregion


        #region Formatting

        // Return an English plural suffix for the given
        // number of items,'s' for zero or more
        // than one, and nothing for exactly one.
        public static string PluralSuffix(int n)
        {
            return 1 == n ? "" : "s";
        }

        // Return an English plural suffix 'ies' or
        // 'y' for the given number of items.
        public static string PluralSuffixY(int n)
        {
            return 1 == n ? "y" : "ies";
        }

        // Return a dot for zero
        // or a colon for more than zero.
        public static string DotOrColon(int n)
        {
            return 0 < n ? ":" : ".";
        }

        // Return a string for a real number
        // formatted to two decimal places.
        public static string RealString(double a)
        {
            return a.ToString("0.##");
        }

        // Return a hash string for a real number
        // formatted to nine decimal places.
        public static string HashString(double a)
        {
            return a.ToString("0.#########");
        }

        // Return a string representation in degrees
        // for an angle given in radians.
        public static string AngleString(double angle)
        {
            return RealString(angle * 180 / Math.PI) + " degrees";
        }

        // Return a string for a length in millimetres
        // formatted as an integer value.
        public static string MmString(double length)
        {
            return Math.Round(FootToMm(length)).ToString() + " mm";
        }

        // Return a string for a UV point
        // or vector with its coordinates
        // formatted to two decimal places.
        public static string PointString(UV p, bool onlySpaceSeparator = false)
        {
            string format_string = onlySpaceSeparator ? "{0} {1}" : "({0},{1})";
            return string.Format(format_string, RealString(p.U), RealString(p.V));
        }

        // Return a string for an XYZ point
        // or vector with its coordinates
        // formatted to two decimal places.
        public static string PointString(XYZ p, bool onlySpaceSeparator = false)
        {
            string format_string = onlySpaceSeparator ? "{0} {1} {2}" : "({0},{1},{2})";
            return string.Format(format_string, RealString(p.X), RealString(p.Y), RealString(p.Z));
        }

        // Return a hash string for an XYZ point
        // or vector with its coordinates
        // formatted to nine decimal places.
        public static string HashString(XYZ p)
        {
            return string.Format("({0},{1},{2})", HashString(p.X), HashString(p.Y), HashString(p.Z));
        }

        // Return a string for this bounding box
        // with its coordinates formatted to two
        // decimal places.
        public static string BoundingBoxString(BoundingBoxUV bb, bool onlySpaceSeparator = false)
        {
            string format_string = onlySpaceSeparator ? "{0} {1}" : "({0},{1})";

            return string.Format(format_string, PointString(bb.Min, onlySpaceSeparator), PointString(bb.Max, onlySpaceSeparator));
        }

        // Return a string for this bounding box
        // with its coordinates formatted to two
        // decimal places.
        public static string BoundingBoxString(BoundingBoxXYZ bb, bool onlySpaceSeparator = false)
        {
            string format_string = onlySpaceSeparator ? "{0} {1}" : "({0},{1})";

            return string.Format(format_string, PointString(bb.Min, onlySpaceSeparator), PointString(bb.Max, onlySpaceSeparator));
        }

        // Return a string for this plane
        // with its coordinates formatted to two
        // decimal places.
        public static string PlaneString(Plane p)
        {
            return string.Format("plane origin {0}, plane normal {1}", PointString(p.Origin), PointString(p.Normal));
        }

        // Return a string for this transformation
        // with its coordinates formatted to two
        // decimal places.
        public static string TransformString(Transform t)
        {
            return string.Format("({0},{1},{2},{3})", PointString(t.Origin),
              PointString(t.BasisX), PointString(t.BasisY), PointString(t.BasisZ));
        }

        // Return a string for a list of doubles 
        // formatted to two decimal places.
        public static string DoubleArrayString(IEnumerable<double> a, bool onlySpaceSeparator = false)
        {
            string separator = onlySpaceSeparator ? " " : ", ";

            return string.Join(separator, a.Select<double, string>(x => RealString(x)));
        }

        // Return a string for this point array
        // with its coordinates formatted to two
        // decimal places.
        public static string PointArrayString(IEnumerable<UV> pts, bool onlySpaceSeparator = false)
        {
            string separator = onlySpaceSeparator ? " " : ", ";

            return string.Join(separator, pts.Select<UV, string>(p => PointString(p, onlySpaceSeparator)));
        }

        // Return a string for this point array
        // with its coordinates formatted to two
        // decimal places.
        public static string PointArrayString(IEnumerable<XYZ> pts, bool onlySpaceSeparator = false)
        {
            string separator = onlySpaceSeparator ? " " : ", ";

            return string.Join(separator, pts.Select<XYZ, string>(p => PointString(p, onlySpaceSeparator)));
        }

        // Return a string representing the data of a
        // curve. Currently includes detailed data of
        // line and arc elements only.
        public static string CurveString(Curve c)
        {
            string s = c.GetType().Name.ToLower();

            XYZ p = c.GetEndPoint(0);
            XYZ q = c.GetEndPoint(1);

            s += string.Format(" {0} --> {1}", PointString(p), PointString(q));

            Arc arc = c as Arc;

            if (null != arc)
            {
                s += string.Format(" center {0} radius {1}", PointString(arc.Center), arc.Radius);
            }

            return s;
        }

        // Return a string for this curve with its
        // tessellated point coordinates formatted
        // to two decimal places.
        public static string CurveTessellateString(Curve curve)
        {
            return "curve tessellation " + PointArrayString(curve.Tessellate());
        }

        public static string ListString(List<string> list)
        {
            string fusion = "";
            for (int index = 0; index < list.Count(); index++)
            {
                fusion = fusion + list[index] + " ";
            }
            return fusion;
        }

        public static string ListString(List<bool> list)
        {
            string fusion = "";
            for (int index = 0; index < list.Count(); index++)
            {
                fusion = fusion + list[index].ToString() + " ";
            }
            return fusion;
        }

        public static string ListString(List<int> list)
        {
            string fusion = "";
            for (int index = 0; index < list.Count(); index++)
            {
                fusion = fusion + list[index].ToString() + " ";
            }
            return fusion;
        }

        public static string ListString(List<double> list)
        {
            string fusion = "";
            for (int index = 0; index < list.Count(); index++)
            {
                fusion = fusion + list[index].ToString() + " ";
            }
            return fusion;
        }

        public static string ListString(List<XYZ> list)
        {
            string fusion = "";
            for (int index = 0; index < list.Count(); index++)
            {
                fusion = fusion + PointString(list[index]) + " ";
            }
            return fusion;
        }


        #endregion 

    }
}

