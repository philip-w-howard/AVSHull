using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AVSHull
{
    // Class to define the setup of panel layout 
    //      Sheet size
    //      Sheet configuration
    public class PanelsLayoutSetup : INotifyPropertyChanged
    {
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

        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
