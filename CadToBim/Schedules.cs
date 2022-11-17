using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CadToBim
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class Columns : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            using (Transaction t = new Transaction(document, "Create Columns Schedule"))
            {
                t.Start();

                ViewSchedule viewSchedule = ViewSchedule.CreateSchedule(document, new ElementId(BuiltInCategory.OST_Columns));

                document.Regenerate();

                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ELEM_FAMILY_PARAM));
                // Setting up the grouping field to be used
                ScheduleField field = Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ELEM_TYPE_PARAM));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.INSTANCE_LENGTH_PARAM));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ALL_MODEL_COST));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS));
                // Turning on Header and adding it to the grouping
                ScheduleSortGroupField sortGroupField = new ScheduleSortGroupField(field.FieldId);
                sortGroupField.ShowHeader = true;
                viewSchedule.Definition.AddSortGroupField(sortGroupField);

                t.Commit();

                // Switching to the newly created Schedule
                uidoc.ActiveView = viewSchedule;
            }

            return Result.Succeeded;
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    internal class Walls : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            using (Transaction t = new Transaction(document, "Create Walls Schedule"))
            {
                t.Start();

                ViewSchedule viewSchedule = ViewSchedule.CreateSchedule(document, new ElementId(BuiltInCategory.OST_Walls));

                document.Regenerate();

                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.WALL_BASE_CONSTRAINT));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.WALL_BASE_OFFSET));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.WALL_HEIGHT_TYPE));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.WALL_TOP_OFFSET));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ELEM_FAMILY_PARAM));
                // Setting up the grouping field to be used
                ScheduleField field = Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ELEM_TYPE_PARAM));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.INSTANCE_LENGTH_PARAM));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.HOST_AREA_COMPUTED));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ALL_MODEL_COST));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS));
                // Turning on Header and adding it to the grouping
                ScheduleSortGroupField sortGroupField = new ScheduleSortGroupField(field.FieldId);
                sortGroupField.ShowHeader = true;
                viewSchedule.Definition.AddSortGroupField(sortGroupField);

                t.Commit();

                // Switching to the newly created Schedule
                uidoc.ActiveView = viewSchedule;
            }

            return Result.Succeeded;
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    internal class Doors : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            using (Transaction t = new Transaction(document, "Create Doors Schedule"))
            {
                t.Start();

                ViewSchedule viewSchedule = ViewSchedule.CreateSchedule(document, new ElementId(BuiltInCategory.OST_Doors));

                document.Regenerate();

                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ALL_MODEL_MARK));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ELEM_FAMILY_PARAM));
                // Setting up the grouping field to be used
                ScheduleField field = Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ELEM_TYPE_PARAM));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.CASEWORK_HEIGHT));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.CASEWORK_WIDTH));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ALL_MODEL_COST));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS));
                // Turning on Header and adding it to the grouping
                ScheduleSortGroupField sortGroupField = new ScheduleSortGroupField(field.FieldId);
                sortGroupField.ShowHeader = true;
                viewSchedule.Definition.AddSortGroupField(sortGroupField);

                t.Commit();

                // Switching to the newly created Schedule
                uidoc.ActiveView = viewSchedule;
            }

            return Result.Succeeded;
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    internal class Windows : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            using (Transaction t = new Transaction(document, "Create Windows Schedule"))
            {
                t.Start();

                ViewSchedule viewSchedule = ViewSchedule.CreateSchedule(document, new ElementId(BuiltInCategory.OST_Windows));

                document.Regenerate();

                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ALL_MODEL_MARK));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ELEM_FAMILY_PARAM));
                // Setting up the grouping field to be used
                ScheduleField field = Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ELEM_TYPE_PARAM));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.CASEWORK_HEIGHT));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.CASEWORK_WIDTH));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ALL_MODEL_COST));
                Helpers.AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS));
                // Turning on Header and adding it to the grouping
                ScheduleSortGroupField sortGroupField = new ScheduleSortGroupField(field.FieldId);
                sortGroupField.ShowHeader = true;
                viewSchedule.Definition.AddSortGroupField(sortGroupField);

                t.Commit();

                // Switching to the newly created Schedule
                uidoc.ActiveView = viewSchedule;
            }

            return Result.Succeeded;
        }
    }

    

    [TransactionAttribute(TransactionMode.Manual)]
    internal class ExportAllSchedules : IExternalCommand
    {
        public static string path;
        public static bool[] boolList;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            FilteredElementCollector allSchedules = new FilteredElementCollector(document).OfClass(typeof(ViewSchedule));

            ViewScheduleExportOptions exporter = new ViewScheduleExportOptions();

            if (allSchedules.Count() == 0)
            {
                TaskDialog.Show("Error", "The Project Contains no Scheduling Files");
                return Result.Succeeded;
            }

            //SaveFileDialog saveFileDialog = new SaveFileDialog();
            //saveFileDialog.FileName = "Save Here";
            //saveFileDialog.FilterIndex = 1;
            //saveFileDialog.RestoreDirectory = true;
            //saveFileDialog.ShowDialog();

            var ExportUI = new Export_UI.UI().ShowDialog();

            string[] listofSchedules = { "Column", "Wall", "Door", "Window" };
            List<string> schedules = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                if (boolList[i])
                {
                    schedules.Add(listofSchedules[i]);
                }
            }

            var ConvertedSchedules = new FilteredElementCollector(document).OfClass(typeof(ViewSchedule)).Where(vs => schedules.Any(schedule => vs.Name.Contains(schedule)));

            foreach (ViewSchedule vs in ConvertedSchedules)
            {
                vs.Export(path, vs.Name + ".csv", exporter);
            }

            return Result.Succeeded;
        }
    }

    internal class Helpers
    {
        public static ScheduleField AddRegularFieldToSchedule(ViewSchedule schedule, ElementId paramId)
        {
            ScheduleDefinition definition = schedule.Definition;

            // Find the matching SchedulableField
            SchedulableField schedulableField = definition.GetSchedulableFields().FirstOrDefault(sf => sf.ParameterId == paramId);

            if (schedulableField != null)
            {
                // Add the found field and return it for use in Filting
                return definition.AddField(schedulableField);
            }
            else return null;
        }
        public static void setPath(string path)
        {
            ExportAllSchedules.path = path;
        }
        public static void setBool(bool[] boolList)
        {
            ExportAllSchedules.boolList = boolList;
        }
    }

}
