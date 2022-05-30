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
                _outputTypeString = _outputTypeNames[(int)_outputType];  
                Notify("OutputType"); 
            }
        }

        private List<string> _outputTypeNames = new List<String>() { "Eighths", "Sixteenths", "Thirtysecondths", "Decimal 2-digits", "Decimal 3-digits", "Decimal 4-digits" };
        public List<string> OutputTypeNames
        {
            get { return _outputTypeNames; }
            set { _outputTypeNames = value; Notify("OutputTypeStrings"); }
        }

        private string _outputTypeString = "Eighths";
        public string OutputTypeString
        {
            get { return _outputTypeString; }
            set
            {
                _outputTypeString = value.ToString();
                OutputType = (OutputTypeEnum)_outputTypeNames.IndexOf(_outputTypeString);
                Notify("OutputTypeString");
            }
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
                _spacingStyleString = SpacingStyleNames[(int)_spacingStyle];
                Notify("SpacingStyle"); }
        }

        private List<string> _spacingStyleNames = new List<string>() { "Every point", "Fixed spacing" }; //  (Enum.GetNames(typeof(SpacingStyleEnum)));
        public List<string> SpacingStyleNames
        {
            get { return _spacingStyleNames; }
            set { _spacingStyleNames = value; Notify("SpacingStyleStrings"); }
        }

        private string _spacingStyleString = "Every point";
        public string SpacingStyleString
        {
            get { return _spacingStyleString; }
            set
            {
                _spacingStyleString = value;
                SpacingStyle = (SpacingStyleEnum)_spacingStyleNames.IndexOf(_spacingStyleString);
                Notify("SpacingStyleString");
            }
        }
        //*******************************************************************
        private double m_Spacing = 12;
        public double Spacing
        {
            get { return m_Spacing; }
            set { m_Spacing = value; Notify("Spacing"); }
        }



        public enum OriginEnum { UPPER_LEFT, LOWER_LEFT, CENTER }
        private OriginEnum _Origin = OriginEnum.CENTER;

        public OriginEnum Origin
        {
            get { return _Origin; }
            set { _Origin = value; Notify("Origin"); }
        }

        private string _originString = "Center";
        public string OriginString
        {
            get { return _originString; }
            set
            {
                _originString = value;
                switch (_originString)
                {
                    case "Center":
                    case "System.Windows.Controls.ComboBoxItem: Center":
                        Origin = OriginEnum.CENTER;
                        break;
                    case "Upper Left":
                    case "System.Windows.Controls.ComboBoxItem: Upper Left":
                        Origin = OriginEnum.UPPER_LEFT;
                        break;
                    case "Lower Left":
                    case "System.Windows.Controls.ComboBoxItem: Lower Left":
                        Origin = OriginEnum.LOWER_LEFT;
                        break;
                }
                Notify("Origin");
            }
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

        public OffsetWriter()
        {
            parameters = (OffsetsParameters)Application.Current.FindResource("OffsetParameters");
        }

        public PanelLayout Layout { get; set; }

        public bool? SaveLayout()
        {
            if (Layout == null) return false;

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
                        foreach (Point point in panel.Points)
                            output.WriteLine("   {0}", FormatPoint(point, parameters.OutputType));
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
        private String FormatPoint(Point point, OffsetsParameters.OutputTypeEnum format)
        {
            String result = new String("");
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
