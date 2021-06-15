using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            set { _windowWidth = value; }
        }

        private double _windowHeight;
        public double WindowHeight
        {
            get { return _windowHeight; }
            set { _windowHeight = value; }
        }

        public int SheetsWide
        {
            get { return LayoutSetup.SheetsWide; }
            set { LayoutSetup.SheetsWide = value; }
        }

        public int SheetsHigh
        {
            get { return LayoutSetup.SheetsHigh; }
            set { LayoutSetup.SheetsHigh = value; }
        }
        public double SheetWidth
        {
            get { return LayoutSetup.SheetWidth; }
            set { LayoutSetup.SheetWidth = value; }
        }
        public double SheetHeight
        {
            get { return LayoutSetup.SheetHeight; }
            set { LayoutSetup.SheetHeight = value; }
        }

        private List<Panel> m_panels;
        public List<Panel> Panels
        {
            get { return m_panels; }
            set
            {
                m_panels = value;
                Notify("Panels");
            }
        }

        private double _scale;
        public double Scale
        {
            get { return _scale; }
            set { _scale = value; }
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
