using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AVSHull
{
    class ATabItem : TabItem
    {
        public String TabName { get; set; }

         protected override void OnSelected(System.Windows.RoutedEventArgs e)
        {
            MainWindow main = (MainWindow)Application.Current.MainWindow;

            base.OnSelected(e);
            if (TabName == "Panels")
            {
                main.CreateMenu.IsEnabled = false;
                main.ImportMenu.IsEnabled = false;
                main.ExportMenu.IsEnabled = true;
                PanelLayoutScroller layout = (PanelLayoutScroller)((ATabItem)e.Source).Content;
                layout.CheckPanels();
            }
            else if (TabName == "Design")
            {
                main.CreateMenu.IsEnabled = true;
                main.ImportMenu.IsEnabled = true;
                main.ExportMenu.IsEnabled = false;
            }
        }
        protected override void OnUnselected(System.Windows.RoutedEventArgs e)
        {
            base.OnUnselected(e);
            Debug.WriteLine("UnSelected {0} {1}", TabName, TabName.Length);
        }
    }
}
