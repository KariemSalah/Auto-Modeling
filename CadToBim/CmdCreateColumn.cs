#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
#endregion

namespace CadToBim
{
    [Transaction(TransactionMode.Manual)]
    public class CmdCreateColumn : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            double tolerance = app.ShortCurveTolerance;


            // Check if the families are ready
            if (Properties.Settings.Default.name_columnRect == null ||
                Properties.Settings.Default.name_columnRound == null)
            {
                System.Windows.MessageBox.Show("Please select the column type in settings", "Tips");
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


            // Fetch baselines
            List<Curve> columnCrvs = new List<Curve>();
            var columnLayers = Misc.GetLayerNames(Properties.Settings.Default.layerColumn);
            try
            {
                foreach (string columnLayer in columnLayers)
                {
                    columnCrvs.AddRange(Util.Geometry.ShatterCADGeometry(uidoc, import, columnLayer, tolerance));
                }

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "Tips");
                return Result.Cancelled;
            }
            if (columnCrvs == null || columnCrvs.Count == 0)
            {
                System.Windows.MessageBox.Show("Baseline not found", "Tips");
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


            // Start Creating
            TransactionGroup tg = new TransactionGroup(doc, "Create columns");
            try
            {
                tg.Start();
                CreateColumn.Execute(app, doc, columnCrvs,
                    Properties.Settings.Default.name_columnRect,
                    Properties.Settings.Default.name_columnRound,
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
