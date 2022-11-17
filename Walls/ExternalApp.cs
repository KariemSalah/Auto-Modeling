#region NameSpaces
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media.Imaging;
#endregion

namespace CadToBim
{
    public class ExternalApp : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.CreateRibbonTab("OCB");
            RibbonPanel panel = application.CreateRibbonPanel("OCB", "Create Model");

            string path = Assembly.GetExecutingAssembly().Location;

            //Column button
            PushButtonData column = new PushButtonData("Button 1", "Column", path, "CadToBim.CmdCreateColumn");
            //PushButton column = panel.AddItem(button1) as PushButton;
            column.ToolTip = "Create Columns. Link DWG with COLUMN layer";
            Uri imagpath = new Uri(@"D:\Codes\Walls\Walls\Recources\Images\Column.ico");
            column.Image = new BitmapImage(imagpath);

            //Wall button
            PushButtonData wall = new PushButtonData("Button 2", "Wall", path, "CadToBim.CmdCreateWall");
            //PushButton wall = panel.AddItem(button2) as PushButton;
            wall.ToolTip = "Create Walls. Link DWG with WALL layer";
            Uri imgpath = new Uri(@"D:\Codes\Walls\Walls\Recources\Images\Wall.ico");
            wall.Image = new BitmapImage(imgpath);

            //Opening button
            PushButtonData opening = new PushButtonData("Button 3", "Opening", path, "CadToBim.CmdCreateOpening");
            //PushButton opening = panel.AddItem(button3) as PushButton;
            opening.ToolTip = "Insert Openings. Need layer DOOR, WINDOW & WALL";
            Uri imgpth = new Uri(@"D:\Codes\Walls\Walls\Recources\Images\Opening.ico");
            opening.Image = new BitmapImage(imgpth);

            //2nd panel
            //RibbonPanel panel2 = application.CreateRibbonPanel("Modeling", "Settings");

            PushButtonData sttng = new PushButtonData("Button", "Settings", path, "CadToBim.Views.CmdConfig");
            PushButton setting = panel.AddItem(sttng) as PushButton;
            setting.ToolTip = "Default and preference settings.";
            Uri imagepth = new Uri(@"D:\Codes\Walls\Walls\Recources\Images\Settings-icon.png");
            setting.LargeImage = new BitmapImage(imagepth);

           
            panel.AddStackedItems(column, wall, opening);


            RibbonPanel ribbonpanel = application.CreateRibbonPanel("OCB", "Schedules");
            RibbonPanel ribbonpanel2 = application.CreateRibbonPanel("OCB", "Export");

            PushButtonData button3 = new PushButtonData("Button 3", "Columns Schedule", path, "CadToBim.Columns");
            PushButtonData button4 = new PushButtonData("Button 4", "Walls Schedule", path, "CadToBim.Walls");
            PushButtonData button5 = new PushButtonData("Button 5", "Doors Schedule", path, "CadToBim.Doors");
            PushButtonData button6 = new PushButtonData("Button 6", "Windows Schedule", path, "CadToBim.Windows");            
            PushButtonData button8 = new PushButtonData("Button 7", "Export Schedules", path, "CadToBim.ExportAllSchedules");

            PushButton pushButton8 = ribbonpanel2.AddItem(button8) as PushButton;

            button3.Image = new BitmapImage(new Uri(@"D:\Codes\Walls\Walls\Recources\Images\Column-icon.png"));
            button4.Image = new BitmapImage(new Uri(@"D:\Codes\Walls\Walls\Recources\Images\wall-brick-icon.png"));
            button5.Image = new BitmapImage(new Uri(@"D:\Codes\Walls\Walls\Recources\Images\door-open-icon.png"));
            button6.Image = new BitmapImage(new Uri(@"D:\Codes\Walls\Walls\Recources\Images\Window-icon.png"));

            pushButton8.LargeImage = new BitmapImage(new Uri(@"D:\Codes\Walls\Walls\Recources\Images\excel-icon.png"));

            button3.ToolTip = "Create Column Scheduling";
            button4.ToolTip = "Create Wall Scheduling";
            button5.ToolTip = "Create Door Scheduling";
            button6.ToolTip = "Create Window Scheduling";            
            pushButton8.ToolTip = "Export All Created Schedules to CSV";

            ribbonpanel.AddStackedItems(button3, button4);
            ribbonpanel.AddStackedItems(button5, button6);

            return Result.Succeeded;
        }
    }
}
