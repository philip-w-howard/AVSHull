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
        public PanelLayoutWindow()
        {
            InitializeComponent();
            m_panels = new List<Panel>();
        }

        //private PanelLayoutControl LayoutControl = new PanelLayoutControl();

        private List<Panel> m_panels;
        public void AddPanel(Panel p)
        {
            m_panels.Add(p);
        }

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
