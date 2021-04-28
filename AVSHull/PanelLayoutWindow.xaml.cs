using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

            m_panels = new List<Panel>();

            // Initialize the panels
            if (myHull != null && myHull.Bulkheads.Count != 0)
            {
                EditableHull eHull = new EditableHull(myHull);
                for (int index = 0; index < eHull.Chines.Count / 2; index++)
                {
                    Panel p = new Panel(eHull.Chines[index], eHull.Chines[index + 1]);
                    p.name = "Panel " + (index+1);
                    m_panels.Add(p);
                }

                int bulkheadIndex = 1;

                foreach (Bulkhead bulk in eHull.Bulkheads)
                {
                    if (bulk.type != Bulkhead.BulkheadType.BOW)
                    {
                        Panel p = new Panel(bulk.Points);
                        p.name = "Bulkhead " + bulkheadIndex;
                        bulkheadIndex++;
                        m_panels.Add(p);
                    }
                }
            }
        }

        private List<Panel> m_panels;

        private void GetLayoutSetup()
        {
            //Get the layout setup
            PanelLayoutSetup setup = new PanelLayoutSetup();
            setup.Owner = this;

            bool? result = setup.ShowDialog();

            if (result == true)
            {
                LayoutSetupData parameters = new LayoutSetupData();

                //LayoutSetupData params = new LayoutSetupData();
                parameters = (LayoutSetupData)Application.Current.FindResource("LayoutSetup");
                if (parameters != null)
                {
                    LayoutControl.SheetWidth = parameters.sheetWidth;
                    LayoutControl.SheetHeight = parameters.sheetHeight;
                    LayoutControl.SheetsWide = parameters.numSheetsHorizontal;
                    LayoutControl.SheetsHigh = parameters.numSheetsVertical;
                    LayoutControl.WindowWidth = Width;
                    LayoutControl.WindowHeight = Height;
                }
            }
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

        private void PanelSelected(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine(e);
        }

        //*****************************************************************
        // Serialize/Deserialize
        //*****************************************************************
        public class AllPanelData
        {
            public List<List<Panel>> panelList { get; set; }
            public PanelLayoutControl.PanelLayoutSetup panelLayout { get; set; }
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
                        LayoutControl.Panels = panelData.panelList[1];
                    }
                    LayoutControl.LayoutSetup = panelData.panelLayout;
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
                panelData.panelList.Add(LayoutControl.Panels);

                //panelData.panelLayout = new PanelLayoutControl.PanelLayoutSetup();
                panelData.panelLayout = LayoutControl.LayoutSetup;
                System.Xml.Serialization.XmlSerializer panelWriter = new System.Xml.Serialization.XmlSerializer(typeof(AllPanelData));

                using (FileStream output = new FileStream(saveDlg.FileName, FileMode.Create))
                {
                    panelWriter.Serialize(output, panelData);
                }
            }
        }

        private void outputGcode(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.Filter = "GCode files (*.nc)|*.nc|All files (*.*)|*.*";
            saveDlg.FilterIndex = 1;
            saveDlg.RestoreDirectory = true;

            Nullable<bool> result = saveDlg.ShowDialog();
            if (result == true)
            {
                GCodeSetup setup = new GCodeSetup();
                setup.ShowDialog();
                Point gcodeOrigin = new Point(0, 0);
                if (setup.OK)
                {
                    GCodeParameters parameters = new GCodeParameters();
                    parameters = (GCodeParameters)Application.Current.FindResource("GCodeSetup");
                    if (parameters.OriginTypes[parameters.Origin] == "Panels Bottom Left")
                    {
                        double minX = Double.MaxValue;
                        double maxY = Double.MinValue;
                        foreach (Panel panel in LayoutControl.Panels)
                        {
                            double x, y;
                            PointCollection points = panel.Points;
                            GeometryOperations.TopLeft(points, out x, out y);
                            minX = Math.Min(minX, x);
                            maxY = Math.Max(maxY, y);
                        }
                        gcodeOrigin = new Point(minX, maxY);
                    }
                    else if (parameters.OriginTypes[parameters.Origin] == "Sheet Bottom Left")
                    {
                        gcodeOrigin = new Point(0, 0);
                    }
                    else if (parameters.OriginTypes[parameters.Origin] == "Sheet Center")
                    {
                        gcodeOrigin = new Point(LayoutControl.SheetWidth / 2, LayoutControl.SheetHeight/ 2);
                    }
                    GCodeWriter output = new GCodeWriter(saveDlg.FileName);
                    foreach (Panel panel in LayoutControl.Panels)
                    {
                        output.Write(panel, gcodeOrigin);
                    }

                    output.Close();
                }
            }
        }

        private void outputOffsets(object sender, RoutedEventArgs e)
        {
            // DOES NOT WORK
            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveDlg.FilterIndex = 2;
            saveDlg.RestoreDirectory = true;

            Nullable<bool> result = saveDlg.ShowDialog();
            if (result == true)
            {
                using (System.IO.StreamWriter output = new System.IO.StreamWriter(saveDlg.FileName))
                {
                    foreach (Panel panel in LayoutControl.Panels)
                    {
                        output.WriteLine(panel.name);
                        foreach (Point point in panel.Points)
                        output.WriteLine("   {0} {1}", point.X, point.Y);
                    }
                }
            }
        }

        private void outputSTL(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.Filter = "STL files (*.stl)|*.stl|All files (*.*)|*.*";
            saveDlg.FilterIndex = 1;
            saveDlg.RestoreDirectory = true;

            Nullable<bool> result = saveDlg.ShowDialog();
            if (result == true)
            {
                STLWriter output = new STLWriter(saveDlg.FileName);
                foreach (Panel panel in LayoutControl.Panels)
                {
                    output.Write(panel);
                }

                output.Close();
            }
        }
        private void outputSVG(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.Filter = "SVG files (*.svg)|*.svg|All files (*.*)|*.*";
            saveDlg.FilterIndex = 1;
            saveDlg.RestoreDirectory = true;

            Nullable<bool> result = saveDlg.ShowDialog();
            if (result == true)
            {
                SVGWriter output = new SVGWriter(saveDlg.FileName);
                foreach (Panel panel in LayoutControl.Panels)
                {
                    output.Write(panel);
                }

                output.Close();

            }
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
    }
}
