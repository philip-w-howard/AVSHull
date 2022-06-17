using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;

namespace AVSHull
{
    public class OffsetsParameters : INotifyPropertyChanged
    {
        //************************************************************
        public enum OutputTypeEnum { EIGHTHS, SIXTEENTHS, THIRTYSECONDTHS, DECIMAL_2DIGIT, DECIMAL_3DIGIT, DECIMAL_4DIGIT }
        
        private OutputTypeEnum _outputType = OutputTypeEnum.EIGHTHS;
        public OutputTypeEnum OutputType
        {
            get { return _outputType; }
            set 
            {
                _outputType = value;
                _outputTypeIndex = (int)_outputType;
                Notify("OutputType"); 
            }
        }

        private int _outputTypeIndex = 0;
        public int OutputTypeIndex
        {
            get { return _outputTypeIndex;  }
            set
            {
                _outputTypeIndex = value;
                OutputType = (OutputTypeEnum)_outputTypeIndex;
            }
        }

        private List<string> _outputTypeNames = new List<String>() { "Eighths", "Sixteenths", "Thirtysecondths", "Decimal 2-digits", "Decimal 3-digits", "Decimal 4-digits" };
        public List<string> OutputTypeNames
        {
            get { return _outputTypeNames; }
            set { _outputTypeNames = value; Notify("OutputTypeStrings"); }
        }

         //***************************************************************************
        public enum SpacingStyleEnum { EVERY_POINT, FIXED_SPACING }
        
        private SpacingStyleEnum _spacingStyle = SpacingStyleEnum.EVERY_POINT;
        public SpacingStyleEnum SpacingStyle
        {
            get { return _spacingStyle; }
            set 
            { 
                _spacingStyle = value;
                _spacingStyleIndex = (int)_spacingStyle;
                Notify("SpacingStyle"); 
            }
        }

        private List<string> _spacingStyleNames = new List<string>() { "Every point", "Fixed spacing" };
        public List<string> SpacingStyleNames
        {
            get { return _spacingStyleNames; }
            set { _spacingStyleNames = value; Notify("SpacingStyleStrings"); }
        }

        private int _spacingStyleIndex = 0;
        public int SpacingStyleIndex
        {
            get { return _spacingStyleIndex; }
            set
            {
                _spacingStyleIndex = value;
                SpacingStyle = (SpacingStyleEnum)_spacingStyleIndex;
                Notify("SpacingStyleIndex");
            }
        }
    
         //*******************************************************************
        private int m_Spacing = 12;
        public int Spacing
        {
            get { return m_Spacing; }
            set 
            { 
                m_Spacing = value; 
                switch (m_Spacing)
                {
                    case 6: _spacingIndex = 0; break;
                    case 12: _spacingIndex = 1; break;
                    case 24: _spacingIndex = 2; break;
                }
                Notify("Spacing"); 
            }
        }

        private List<string> _spacingNames = new List<string>() { "6 Inches", "12 Inches", "24 Inches" };
        public List<string> SpacingNames
        {
            get { return _spacingNames; }
            set { _spacingNames = value; Notify("SpacingStrings"); }
        }

        private int _spacingIndex = 0;
        public int SpacingIndex
        {
            get { return _spacingIndex; }
            set
            {
                _spacingIndex = value;
                switch (_spacingIndex)
                {
                    case 0: Spacing = 6; break;
                    case 1: Spacing = 12; break;
                    case 2: Spacing = 24; break;
                }
                Notify("SpacingIndex");
            }
        }

        //*******************************************************************
        public enum OriginEnum { UPPER_LEFT, LOWER_LEFT, CENTER }
        
        private OriginEnum _Origin = OriginEnum.CENTER;
        public OriginEnum Origin
        {
            get { return _Origin; }
            set 
            { 
                _Origin = value;
                _originIndex = (int)_Origin;
                Notify("Origin"); 
            }
        }

        private int _originIndex = 0;
        public int OriginIndex
        {
            get { return _originIndex; }
            set
            {
                _originIndex = value;
                Origin = (OriginEnum)_originIndex;
            }
        }

        private List<string> _originNames = new List<String>() { "Upper Left", "Lower Left", "Center" };
        public List<string> OriginNames
        {
            get { return _originNames; }
            set { _originNames = value; Notify("OriginStrings"); }
        }

        //**************************************************
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

    }

    class OffsetWriter : ILayoutWriter
    {
        private OffsetsParameters parameters;
        private PanelsLayoutSetup layout;

        public OffsetWriter()
        {
            parameters = (OffsetsParameters)Application.Current.FindResource("OffsetParameters");
            layout = (PanelsLayoutSetup)Application.Current.FindResource("LayoutSetup");
        }

        public PanelLayout Layout { get; set; }

        public bool? SaveLayout()
        {
            if (Layout == null) return false;

            Panel outputPanel;

            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveDlg.FilterIndex = 1;
            saveDlg.RestoreDirectory = true;

            Nullable<bool> result = saveDlg.ShowDialog();
            if (result == true)
            {
                using (System.IO.StreamWriter output = new System.IO.StreamWriter(saveDlg.FileName))
                {
                    foreach (Panel panel in Layout.Panels)
                    {
                        output.WriteLine(panel.name);
                        if (parameters.SpacingStyle == OffsetsParameters.SpacingStyleEnum.FIXED_SPACING)
                            outputPanel = panel.FixedOffsetPanel(parameters.Spacing);
                        else
                            outputPanel = panel;

                        Point panelOrigin = outputPanel.Origin;
                        foreach (Point point in outputPanel.Points)
                        {
                            output.WriteLine("   {0}", FormatPoint(panelOrigin, point, parameters.OutputType));
                        }

                        if (panel.HasAlignmentLine)
                        {
                            output.WriteLine("Alignment Line");
                            if (parameters.SpacingStyle == OffsetsParameters.SpacingStyleEnum.FIXED_SPACING)
                            {
                                List<Point> alignment = panel.FixedOffsetAlignment(parameters.Spacing);
                                foreach (Point point in alignment)
                                {
                                    output.WriteLine("   {0}", FormatPoint(panelOrigin, point, parameters.OutputType));
                                }
                            }
                            else
                            {
                                output.WriteLine("   {0}", FormatPoint(panelOrigin, outputPanel.AlignmentLeft, parameters.OutputType));
                                output.WriteLine("   {0}", FormatPoint(panelOrigin, outputPanel.AlignmentRight, parameters.OutputType));
                            }
                        }
                    }
                }

                return true;
            }
            
            return result;
        }

        private String Fraction(double value, int denominator)
        {
            String result = new string("");

            int intPart = (int)value;
            double f_numerator = value - intPart;
            f_numerator = Math.Round(f_numerator*denominator, 0);
            int numerator = (int)f_numerator;

            result = String.Format("{0}-{1}/{2}", intPart, numerator, denominator);

            return result;
        }
        private String Decimal(double value, int digits)
        {
            NumberFormatInfo formatter = new NumberFormatInfo();
            formatter.NumberDecimalDigits = digits;

            String result = String.Format(formatter, "{0:f} ", value);

            return result;
        }
        private String FormatPoint(Point panelOrigin, Point point, OffsetsParameters.OutputTypeEnum format)
        {
            double layout_width = layout.SheetsWide * layout.SheetWidth;
            double layout_height = layout.SheetsHigh * layout.SheetHeight;
            String result = new String("");

            if (parameters.Origin == OffsetsParameters.OriginEnum.UPPER_LEFT)
            {
                point = new Point(point.X + panelOrigin.X, point.Y + panelOrigin.Y);
            }
            else if (parameters.Origin == OffsetsParameters.OriginEnum.CENTER)
            {
                point = new Point(point.X + panelOrigin.X - layout_width / 2, point.Y + panelOrigin.Y - layout_height / 2);
            }
            else if (parameters.Origin == OffsetsParameters.OriginEnum.LOWER_LEFT)
            {
                point = new Point(point.X + panelOrigin.X, -(point.Y + panelOrigin.Y - layout_height));
            }

            switch (format)
            {
                case OffsetsParameters.OutputTypeEnum.EIGHTHS:
                    result = Fraction(point.X, 8) + " " + Fraction(point.Y, 8);
                    break;
                case OffsetsParameters.OutputTypeEnum.SIXTEENTHS:
                    result = Fraction(point.X, 16) + " " + Fraction(point.Y, 16);
                    break;
                case OffsetsParameters.OutputTypeEnum.THIRTYSECONDTHS:
                    result = Fraction(point.X, 32) + " " + Fraction(point.Y, 32);
                    break;
                case OffsetsParameters.OutputTypeEnum.DECIMAL_2DIGIT:
                    result = Decimal(point.X, 2) + " " + Decimal(point.Y, 2);
                    break;
                case OffsetsParameters.OutputTypeEnum.DECIMAL_3DIGIT:
                    result = Decimal(point.X, 3) + " " + Decimal(point.Y, 3);
                    break;
                case OffsetsParameters.OutputTypeEnum.DECIMAL_4DIGIT:
                    result = Decimal(point.X, 4) + " " + Decimal(point.Y, 4);
                    break;
                default:
                    result = String.Format("{0} {1}", point.X, point.Y);
                    break;
            }

            return result;
        }
    }
}
