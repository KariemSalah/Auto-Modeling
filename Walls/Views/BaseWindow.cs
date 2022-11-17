#region Namespaces
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
#endregion

namespace CadToBim.Views
{
    public class BaseWindow : Window
    {
        public BaseWindow()
        {
            InitializeStyle();
            this.Loaded += delegate
            {
                InitializeEvent();
            };
        }
        private void InitializeEvent()
        {
            var resourceDict = new ResourceDictionary
            {
                Source = new Uri("D:/Codes/Walls/Walls/Views/BaseWindowStyle.xaml", UriKind.Absolute)
            };
            ControlTemplate baseTemplate = resourceDict["BaseWindowControlTemplate"] as ControlTemplate;

            Button minBtn = this.Template.FindName("MinimizeButton", this) as Button;
            minBtn.Click += delegate
            {
                this.WindowState = WindowState.Minimized;
            };

            Button closeBtn = this.Template.FindName("CloseButton", this) as Button;
            closeBtn.Click += delegate
            {
                this.Close();
            };

            Border mainHeader = this.Template.FindName("MainWindowBorder", this) as Border;
            mainHeader.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            };
           
            TextBlock titleBar = this.Template.FindName("txtTitle", this) as TextBlock;
            titleBar.Text = "One Click to Build";
          
        }

        private void InitializeStyle()
        {
            var resourceDict = new ResourceDictionary
            {
                Source = new Uri("D:/Codes/Walls/Walls/Views/BaseWindowStyle.xaml", UriKind.Absolute)
            };
            this.Style = resourceDict["BaseWindowStyle"] as Style;
        }

    }
}

