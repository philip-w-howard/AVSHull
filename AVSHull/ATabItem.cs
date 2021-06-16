using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;

namespace AVSHull
{
    class ATabItem : TabItem
    {
        public String TabName { get; set; }

         protected override void OnSelected(System.Windows.RoutedEventArgs e)
        {
            base.OnSelected(e);
            if (TabName == "Panels")
            {
                PanelLayoutScroller layout = (PanelLayoutScroller)((ATabItem)e.Source).Content;
                Debug.WriteLine("Updating panels layout");
                layout.CheckPanels();
            }
            Debug.WriteLine("Selected {0} {1}", TabName, TabName.Length);
        }
        protected override void OnUnselected(System.Windows.RoutedEventArgs e)
        {
            base.OnUnselected(e);
            Debug.WriteLine("UnSelected {0} {1}", TabName, TabName.Length);
        }
    }
}
