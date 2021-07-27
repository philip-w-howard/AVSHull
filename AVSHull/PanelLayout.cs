using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace AVSHull
{
    //**************************************************
    // PanelLayout implementation
    // Setup data plus list of panels
    public class PanelLayout : INotifyPropertyChanged
    {
        public PanelLayout()
        {
            m_panelSetup = (PanelsLayoutSetup)Application.Current.FindResource("LayoutSetup");
            m_panels = new List<Panel>();
        }

        private PanelsLayoutSetup m_panelSetup;
        public PanelsLayoutSetup LayoutSetup
        {
            get { return m_panelSetup; }
            set
            {
                m_panelSetup.SheetWidth = value.SheetWidth;
                m_panelSetup.SheetHeight = value.SheetHeight;
                m_panelSetup.SheetsWide = value.SheetsWide;
                m_panelSetup.SheetsHigh = value.SheetsHigh;
                Notify("LayoutSetup");
            }
        }

        private double _windowWidth;
        public double WindowWidth
        {
            get { return _windowWidth; }
            set { _windowWidth = value; Notify("WindowWidth"); }
        }

        private double _windowHeight;
        public double WindowHeight
        {
            get { return _windowHeight; }
            set { _windowHeight = value; Notify("WindowHeight"); }
        }

        public int SheetsWide
        {
            get { return LayoutSetup.SheetsWide; }
            set { LayoutSetup.SheetsWide = value; Notify("SheetsWide"); }
        }

        public int SheetsHigh
        {
            get { return LayoutSetup.SheetsHigh; }
            set { LayoutSetup.SheetsHigh = value; Notify("SheetsHigh"); }
        }
        public double SheetWidth
        {
            get { return LayoutSetup.SheetWidth; }
            set { LayoutSetup.SheetWidth = value; Notify("WheetWidth"); }
        }
        public double SheetHeight
        {
            get { return LayoutSetup.SheetHeight; }
            set { LayoutSetup.SheetHeight = value; Notify("SheetHeight"); }
        }

        private List<Panel> m_panels;

        public IEnumerable<Panel> Panels
        {
            get { return m_panels; }
            // NOTE: set should only be invoked from undo/redo code. If it is invoked elsewhere, notifications will not be properly delivered.
            set
            {
                m_panels = value as List<Panel>;
                foreach (Panel p in m_panels)
                {
                    p.PropertyChanged += panel_PropertyChanged;
                }

                // Notify("Panels"); This should only happen on Undo/Redo, so we don't need to notify
            }
        }

        public IReadOnlyCollection<Panel> ReadOnlyPanels
        {
            get { return m_panels.AsReadOnly(); }
        }

        public void Clear()
        {
            m_panels.Clear();
        }

        public void AddPanel(Panel p)
        {
            m_panels.Add(p);
            p.PropertyChanged += panel_PropertyChanged;

            Notify("Panels");
        }

        public void RemovePanel(Panel p)
        {
            m_panels.Remove(p);

            Notify("Panels");
        }

        private double _scale;
        public double Scale
        {
            get { return _scale; }
            set { _scale = value; Notify("Scale"); }
        }

        //void panel_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        //{
        //    PanelLog log = (PanelLog)Application.Current.FindResource("PanelUndoLog");
        //    if (e.Action != NotifyCollectionChangedAction.Reset) log.Add(m_panels);
        //    //if (e.PropertyName == "HullData" || e.PropertyName == "Bulkhead" || e.PropertyName == "HullScale")
        //    //{
        //    //    undoLog.Add(BaseHull.Instance());
        //    //    redoLog.Clear();
        //    //    UpdateViews();
        //    //}
        //}

        void panel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Notify("PanelLayout.Panel");
        }

        //****************************************************************
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
