#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace CadToBim
{
    [Transaction(TransactionMode.Manual)]
    public class CmdCreateOpening : IExternalCommand
    {

        // Main execution
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(Misc.LoadFromSameFolder);

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            View view = doc.ActiveView;

            Selection sel = uidoc.Selection;

            double tolerance = app.ShortCurveTolerance;


            // Check if the families are ready
            if (Properties.Settings.Default.name_door == null ||
                Properties.Settings.Default.name_window == null)
            {
                System.Windows.MessageBox.Show("Please select the door/window type in settings", "Tips");
                return Result.Cancelled;
            }

            // Pick Import Instance
            ImportInstance import = null;
            try
            {
                Reference r = uidoc.Selection.PickObject(ObjectType.Element, new Util.ElementSelectionFilter<ImportInstance>());
                import = doc.GetElement(r) as ImportInstance;
            }
            catch
            {
                return Result.Cancelled;
            }
            if (import == null)
            {
                System.Windows.MessageBox.Show("CAD not found", "Tips");
                return Result.Cancelled;
            }


            //baselines
            List<Curve> doorCrvs = new List<Curve>();
            List<Curve> windowCrvs = new List<Curve>();
            List<Curve> wallCrvs = new List<Curve>();
            var wallLayers = Misc.GetLayerNames(Properties.Settings.Default.layerWall);
            var doorLayers = Misc.GetLayerNames(Properties.Settings.Default.layerDoor);
            var windowLayers = Misc.GetLayerNames(Properties.Settings.Default.layerWindow);
            try
            {
                foreach (string doorLayer in doorLayers)
                {
                    doorCrvs.AddRange(Util.Geometry.ShatterCADGeometry(uidoc, import, doorLayer, tolerance));
                }
                foreach (string windowLayer in windowLayers)
                {
                    windowCrvs.AddRange(Util.Geometry.ShatterCADGeometry(uidoc, import, windowLayer, tolerance));
                }
                foreach (string wallLayer in wallLayers)
                {
                    wallCrvs.AddRange(Util.Geometry.ShatterCADGeometry(uidoc, import, wallLayer, tolerance));
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "Tips");
                return Result.Cancelled;
            }
            if (doorCrvs == null || windowCrvs == null || wallCrvs == null || doorCrvs.Count * windowCrvs.Count * wallCrvs.Count == 0)
            {
                System.Windows.MessageBox.Show("Baselines not found", "Tips");
                return Result.Cancelled;
            }


            //current building level
            FilteredElementCollector docLevels = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.INVALID)
                .OfClass(typeof(Level));
            ICollection<Element> levels = docLevels.OfClass(typeof(Level)).ToElements();
            Level defaultLevel = null;
            foreach (Level level in levels)
            {
                if (level.Id == import.LevelId)
                {
                    defaultLevel = level;
                }
            }
            if (defaultLevel == null)
            {
                System.Windows.MessageBox.Show("Please make sure there's a base level in current view", "Tips");
                return Result.Cancelled;
            }

            string path = Util.Text.GetCADPath(uidoc, import);
            List<Util.Text.CADTextModel> labels = Util.Text.GetCADText(path);

            Debug.Print("The path of linked DWG file is: " + path);
            Debug.Print("Lables in total: " + labels.Count.ToString());
            foreach (Util.Text.CADTextModel label in labels)
            {
                Debug.Print(label.Text);
            }


            TransactionGroup tg = new TransactionGroup(doc, "Create openings");
            try
            {
                tg.Start();
                CreateOpening.Execute(doc, doorCrvs, windowCrvs, wallCrvs, labels,
                    Properties.Settings.Default.name_door,
                    Properties.Settings.Default.name_window,
                    defaultLevel, false);
                tg.Assimilate();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
                tg.RollBack();
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }
    }
}
