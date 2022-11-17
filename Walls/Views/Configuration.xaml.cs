#region Namespaces
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
#endregion

namespace CadToBim.Views
{
    public partial class Configuration : BaseWindow
    {
        //field
        public ExternalEvent ExEvent;
        public ExtPickLayer extPickLayer;

        //constructor
        public Configuration(UIApplication uiapp)
        {
            InitializeComponent();

            extPickLayer = new ExtPickLayer(uiapp);
            ExEvent = ExternalEvent.Create(extPickLayer);
        }


        private void apply_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.url_columnRect = url_columnRect.Text;
            Properties.Settings.Default.url_columnRound = url_columnRound.Text;
            Properties.Settings.Default.url_door = url_door.Text;
            Properties.Settings.Default.url_window = url_window.Text;

            Properties.Settings.Default.name_columnRect = url_columnRect.Text;
            Properties.Settings.Default.name_columnRound = url_columnRound.Text;
            Properties.Settings.Default.name_door = url_door.Text;
            Properties.Settings.Default.name_window = url_window.Text;

            //Properties.Settings.Default.floorHeight = double.Parse(floorHeight.Text);
            Properties.Settings.Default.sillHeight = double.Parse(sillHeight.Text);           
            Properties.Settings.Default.layerColumn = layerColumn.Text;
            Properties.Settings.Default.layerWall = layerWall.Text;
            Properties.Settings.Default.layerWindow = layerWindow.Text;            
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            string thisAssemblyFolderPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            Properties.Settings.Default.url_columnRect = "M_Rectangular Column";
            Properties.Settings.Default.url_columnRound = "M_Round Column";
            Properties.Settings.Default.url_door = "M_Single-Flush";
            Properties.Settings.Default.url_window = "M_Window-Casement-Double";          
            Properties.Settings.Default.floorHeight = 4000;
            Properties.Settings.Default.sillHeight = 1200;          
            Properties.Settings.Default.minLength = 0.001;
            Properties.Settings.Default.jointRadius = 50;
            Properties.Settings.Default.layerColumn = "COLUMN";
            Properties.Settings.Default.layerWall = "WALL";
            Properties.Settings.Default.layerWindow = "WINDOW";
            Properties.Settings.Default.layerDoor = "DOOR";            
            Properties.Settings.Default.layerCache = "";
            Properties.Settings.Default.Save();
        }


        public string strrRfaName()
        {
            char[] ext = {'.','r','f','a'};
            string strName = string.Empty;
            System.Windows.Forms.OpenFileDialog ofd = null;
            ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Title = "One Click To Build";
            ofd.FileName = "";
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            ofd.Filter = "Revit Files(*.rfa)|*.rfa";
            ofd.ValidateNames = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.FilterIndex = 1;
            //string strName = string.Empty;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                strName = ofd.SafeFileName.TrimEnd(ext);
            }
            return strName;
        }

        private void BtnBrowseDoor(object sender, RoutedEventArgs e)
        {
            string strA = strrRfaName();
            if (!string.IsNullOrEmpty(strA))
            {
                url_door.Text = strA;
            }
        }

        private void BtnBrowseWin(object sender, RoutedEventArgs e)
        {
            string strA = strrRfaName();
            if (!string.IsNullOrEmpty(strA))
            {
                url_window.Text = strA;
            }
        }

        private void BtnBrowseColRect(object sender, RoutedEventArgs e)
        {
            string strA = strrRfaName();
            if (!string.IsNullOrEmpty(strA))
            {
                url_columnRect.Text = strA;
            }
        }

        private void BtnBrowseColRound(object sender, RoutedEventArgs e)
        {
            string strA = strrRfaName();
            if (!string.IsNullOrEmpty(strA))
            {
                url_columnRound.Text = strA;
            }
        }

        private void BtnPickWall(object sender, RoutedEventArgs e)
        {
            extPickLayer.targetValue = "layerWall";
            ExEvent.Raise();
        }
        private void BtnPickColumn(object sender, RoutedEventArgs e)
        {
            extPickLayer.targetValue = "layerColumn";
            ExEvent.Raise();
        }
        private void BtnPickWindow(object sender, RoutedEventArgs e)
        {
            extPickLayer.targetValue = "layerWindow";
            ExEvent.Raise();
        }
        private void BtnPickDoor(object sender, RoutedEventArgs e)
        {
            extPickLayer.targetValue = "layerDoor";
            ExEvent.Raise();
        }
        

    }
}
