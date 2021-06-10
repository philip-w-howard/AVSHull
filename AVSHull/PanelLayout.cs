using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AVSHull
{
    public class PanelLayout : INotifyPropertyChanged
    {
        public class PanelLayoutSetup : INotifyPropertyChanged
        {
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
                set { _windowHeight = value; Notify("WindowHeighth"); }
            }
            private double _sheetWidth;
            public double SheetWidth
            {
                get { return _sheetWidth; }
                set { _sheetWidth = value; Notify("SheetWidth"); }
            }
            private double _sheetHeight;
            public double SheetHeight
            {
                get { return _sheetHeight; }
                set { _sheetHeight = value; Notify("SheetHeight"); }
            }
            private int _sheetsWide;
            public int SheetsWide
            {
                get { return _sheetsWide; }
                set { _sheetsWide = value; Notify("SheetsWide"); }
            }
            private int _sheetsHigh;
            public int SheetsHigh
            {
                get { return _sheetsHigh; }
                set { _sheetsHigh = value; Notify("SheetsHigh"); }
            }
            private double _scale;
            public double Scale
            {
                get { return _scale; }
                set { _scale = value; Notify("Scale"); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            void Notify(string propName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propName));
                }
            }

        }

        //**************************************************
        // PanelLayout implementation
        public PanelLayout()
        {
            m_panelSetup = new PanelLayoutSetup();
        }

        private PanelLayoutSetup m_panelSetup;
        public PanelLayoutSetup LayoutSetup
        {
            get { return m_panelSetup; }
            set
            {
                m_panelSetup = value;
                Notify("LayoutSetup");
            }
        }

        public double WindowWidth
        {
            get { return LayoutSetup.WindowWidth; }
            set { LayoutSetup.WindowWidth = value; }
        }
        public double WindowHeight
        {
            get { return LayoutSetup.WindowHeight; }
            set { LayoutSetup.WindowHeight = value; }
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

        public double Scale
        {
            get { return LayoutSetup.Scale; }
            set { LayoutSetup.Scale = value; }
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
