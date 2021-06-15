using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AVSHull
{
    /// <summary>
    /// Interaction logic for PanelsLayoutControl.xaml
    /// </summary>
    public partial class PanelLayoutScroller : UserControl
    {
        private List<Panel> m_panels;

        public PanelLayoutScroller()
        {
            InitializeComponent();
            SetupPanels();
        }

        private void SetupPanels()
        {
            m_panels = new List<Panel>();

            Hull myHull = BaseHull.Instance();
            // Initialize the panels
            if (myHull != null && myHull.Bulkheads.Count != 0)
            {
                EditableHull eHull = new EditableHull();
                for (int index = 0; index < eHull.Chines.Count / 2; index++)
                {
                    Panel p = new Panel(eHull.Chines[index], eHull.Chines[index + 1]);
                    p.name = "Panel " + (index + 1);
                    m_panels.Add(p);
                }

                int bulkheadIndex = 1;

                foreach (Bulkhead bulk in eHull.Bulkheads)
                {
                    if (bulk.Type != Bulkhead.BulkheadType.BOW)
                    {
                        Panel p = new Panel(bulk);
                        p.name = "Bulkhead " + bulkheadIndex;
                        bulkheadIndex++;
                        m_panels.Add(p);
                    }
                }
            }

            foreach (Panel panel in m_panels)
            {
                MenuItem item = new MenuItem();
                item.Header = panel.name;
                item.Click += AddPanelClick;
                PanelContextMenu.Items.Add(item);
            }
        }

        public void CheckPanels()
        {
            if (m_panels.Count == 0)
            {
                GetLayoutSetup();
                SetupPanels();
                LayoutControl.InvalidateVisual();
            }
        }
        private void GetLayoutSetup()
        {
            //Get the layout setup
            PanelLayoutSetup setup = new PanelLayoutSetup();

            bool? result = setup.ShowDialog();

            if (result == true)
            {
                // Data is copied over automagically through data binding
            }
        }

        private void AddAllClick(object sender, RoutedEventArgs e)
        {
            double x = 10;
            double y = 10;
            double x_size = 0;
            double y_size = 0;

            foreach (Panel p in m_panels)
            {
                Panel panel = (Panel)p.Clone();
                panel.Origin = new Point(x, y);
                GeometryOperations.ComputeSize(panel.Points, out x_size, out y_size);
                y += y_size * 1.1;
                LayoutControl.AddPanel(panel);
            }
        }

        private void AddPanelClick(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)e.Source;
            String panelName = item.Header.ToString();

            Point loc = Mouse.GetPosition(LayoutControl);
            loc.X /= LayoutControl.Layout.Scale;
            loc.Y /= LayoutControl.Layout.Scale;

            foreach (Panel panel in m_panels)
            {
                if (panelName == panel.name)
                {
                    Panel p = (Panel)panel.Clone();
                    p.Origin = loc;
                    LayoutControl.AddPanel(p);
                    break;
                }
            }
        }

        //*****************************************************************
        // Serialize/Deserialize
        //*****************************************************************
        public class AllPanelData
        {
            public List<List<Panel>> panelList { get; set; }
            public PanelsLayoutSetup panelLayout { get; set; }
        }

        private void openClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();

            openDlg.Filter = "AVS Panel files (*.avsp)|*.avsp|All files (*.*)|*.*";
            openDlg.FilterIndex = 0;
            openDlg.RestoreDirectory = true;

            Nullable<bool> result = openDlg.ShowDialog();
            if (result == true)
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(AllPanelData));

                using (Stream reader = new FileStream(openDlg.FileName, FileMode.Open))
                {
                    AllPanelData panelData;

                    // Call the Deserialize method to restore the object's state.
                    panelData = (AllPanelData)serializer.Deserialize(reader);
                    if (panelData.panelList.Count == 2)
                    {
                        m_panels = panelData.panelList[0];
                        LayoutControl.Layout.Panels = panelData.panelList[1];
                    }
                    LayoutControl.Layout.LayoutSetup = panelData.panelLayout;
                }
            }

        }

        private void saveClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.Filter = "AVS Panel files (*.avsp)|*.avsp|All files (*.*)|*.*";
            saveDlg.FilterIndex = 0;
            saveDlg.RestoreDirectory = true;

            Nullable<bool> result = saveDlg.ShowDialog();
            if (result == true)
            {
                AllPanelData panelData = new AllPanelData();
                panelData.panelList = new List<List<Panel>>();
                panelData.panelList.Add(m_panels);
                panelData.panelList.Add(LayoutControl.Layout.Panels);

                panelData.panelLayout = LayoutControl.Layout.LayoutSetup;
                System.Xml.Serialization.XmlSerializer panelWriter = new System.Xml.Serialization.XmlSerializer(typeof(AllPanelData));

                using (FileStream output = new FileStream(saveDlg.FileName, FileMode.Create))
                {
                    panelWriter.Serialize(output, panelData);
                }
            }
        }

        private void outputGcode(object sender, RoutedEventArgs e)
        {
            GCodeWriter writer = new GCodeWriter();
            writer.Layout = LayoutControl.Layout;
            writer.SaveLayout();
        }

        private void outputOffsets(object sender, RoutedEventArgs e)
        {
            OffsetWriter writer = new OffsetWriter();
            writer.Layout = LayoutControl.Layout;
            writer.SaveLayout();
        }

        private void outputSTL(object sender, RoutedEventArgs e)
        {
            STLWriter writer = new STLWriter();
            writer.Layout = LayoutControl.Layout;
            writer.SaveLayout();
        }
        private void outputSVG(object sender, RoutedEventArgs e)
        {
            SVGWriter writer = new SVGWriter();
            writer.Layout = LayoutControl.Layout;
            writer.SaveLayout();
        }

        private void setupClick(object sender, RoutedEventArgs e)
        {
            GetLayoutSetup();
            LayoutControl.InvalidateVisual();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            GetLayoutSetup();
        }

        public void WindowResized(object sender, SizeChangedEventArgs e)
        {
            InvalidateMeasure();
            InvalidateVisual();
            LayoutControl.Layout.WindowWidth = Width;
            LayoutControl.Layout.WindowHeight = Height;

            LayoutControl.InvalidateMeasure();
        }

    }
}
