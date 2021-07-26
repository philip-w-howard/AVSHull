﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
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
                HullView eHull = new HullView();
                List<Point3DCollection> chines = eHull.GenerateChines();

                for (int index = 0; index < chines.Count / 2; index++)
                {
                    Panel p = new Panel(chines[index], chines[index + 1]);
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
                SetupPanels();
                LayoutControl.InvalidateVisual();
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

        public void openClick(object sender, RoutedEventArgs e)
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
                        foreach (Panel p in panelData.panelList[1])
                        {
                            LayoutControl.AddPanel(p);
                        }
                    }
                    LayoutControl.Layout.LayoutSetup = panelData.panelLayout;
                }
            }

        }

        public void saveClick(object sender, RoutedEventArgs e)
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
                panelData.panelList.Add(LayoutControl.Layout.Panels as List<Panel>);

                panelData.panelLayout = LayoutControl.Layout.LayoutSetup;
                System.Xml.Serialization.XmlSerializer panelWriter = new System.Xml.Serialization.XmlSerializer(typeof(AllPanelData));

                using (FileStream output = new FileStream(saveDlg.FileName, FileMode.Create))
                {
                    panelWriter.Serialize(output, panelData);
                }
            }
        }
        public void exportClick(object sender, RoutedEventArgs e)
        {
            MenuItem menu = (MenuItem)sender;

            if (menu != null)
            {
                switch (menu.Header)
                {
                    case "_GCode":
                        outputGcode(sender, e);
                        break;
                    case "_Offsets":
                        outputOffsets(sender, e);
                        break;
                    case "S_VG":
                        outputSVG(sender, e);
                        break;
                    case "S_TL":
                        outputSTL(sender, e);
                        break;
                }
            }
        }

        private void outputGcode(object sender, RoutedEventArgs e)
        {
            //UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            //values.GCodeSetupExapnded = true;

            GCodeWriter writer = new GCodeWriter();
            writer.Layout = LayoutControl.Layout;
            writer.SaveLayout();

            //values.GCodeSetupExapnded = false;
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

        public void WindowResized(object sender, SizeChangedEventArgs e)
        {
            InvalidateMeasure();
            InvalidateVisual();
            LayoutControl.Layout.WindowWidth = Width;
            LayoutControl.Layout.WindowHeight = Height;

            LayoutControl.InvalidateMeasure();
        }

        private void GCodeClick(object sender, RoutedEventArgs e)
        {
            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            values.GCodeSetupExpanded = !values.GCodeSetupExpanded;
        }

        private void LayoutClick(object sender, RoutedEventArgs e)
        {
            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            values.LayoutSetupExpanded = !values.LayoutSetupExpanded;
        }

        private void OffsetsClick(object sender, RoutedEventArgs e)
        {
            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            values.OffsetsSetupExpanded = !values.OffsetsSetupExpanded;
        }
    }
}
