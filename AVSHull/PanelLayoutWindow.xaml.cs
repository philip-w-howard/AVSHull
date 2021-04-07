using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AVSHull
{
    /// <summary>
    /// Interaction logic for PanelLayoutWindow.xaml
    /// </summary>
    public partial class PanelLayoutWindow : Window
    {
        public PanelLayoutWindow(Hull myHull)
        {
            InitializeComponent();
            if (myHull != null)
            {
                EditableHull eHull = new EditableHull(myHull);
                for (int index = 0; index < eHull.Chines.Count / 2; index++)
                {
                    m_panels.Add(new Panel(eHull.Chines[index], eHull.Chines[index + 1]));
                }

                foreach (Bulkhead bulk in eHull.Bulkheads)
                {
                    m_panels.Add(new Panel(bulk.Points));
                }
            }

            m_panels = new List<Panel>();
        }

        //private PanelLayoutControl LayoutControl = new PanelLayoutControl();

        private List<Panel> m_panels;

        private void AddAllClick(object sender, RoutedEventArgs e)
        {
            double x = 10;
            double y = 10;

            foreach (Panel p in m_panels)
            {
                Panel panel = (Panel)p.Clone();
                panel.Origin = new Point(x, y);
                y += 10;
                LayoutControl.AddPanel(panel);
            }
        }

        private void AddPanelClick(object sender, RoutedEventArgs e)
        {

        }

    }
}
