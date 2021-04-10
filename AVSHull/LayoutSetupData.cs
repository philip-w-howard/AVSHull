using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AVSHull
{
    class LayoutSetupData : INotifyPropertyChanged
    {
        private double m_sheetWidth;
        private double m_sheetHeight;
        private int m_numSheetsHorizontal;
        private int m_numSheetsVertical;

        public double sheetWidth 
        {
            get { return m_sheetWidth; }
            set { m_sheetWidth = value; Notify("sheetWidth"); }
        }
        public double sheetHeight
        {
            get { return m_sheetHeight; }
            set { m_sheetHeight = value; Notify("sheetHeight"); }
        }
        public int numSheetsHorizontal
        {
            get { return m_numSheetsHorizontal; }
            set { m_numSheetsHorizontal = value; Notify("numSheetsHorizontal"); }
        }
        public int numSheetsVertical
        {
            get { return m_numSheetsVertical; }
            set { m_numSheetsVertical = value; Notify("numSheetsVertical"); }
        }

        //******************************************
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

    };
}
