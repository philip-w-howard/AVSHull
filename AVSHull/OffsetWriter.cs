using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;

namespace AVSHull
{
    public class OffsetsParameters : INotifyPropertyChanged
    {
        private string _outputTypeString = "Eighths";
        public string OutputTypeString
        {
            get { return _outputTypeString; }
            set
            {
                _outputTypeString = value.ToString();
                switch (_outputTypeString)
                {
                    case "Eights":
                    case "System.Windows.Controls.ComboBoxItem: Eighths":
                        OutputType = OutputTypeEnum.EIGHTHS;
                        break;
                    case "Sixteenths":
                    case "System.Windows.Controls.ComboBoxItem: Sixteenths":
                        OutputType = OutputTypeEnum.SIXTEENTHS;
                        break;
                    case "Thirtysecondths":
                    case "System.Windows.Controls.ComboBoxItem: Thirtysecondths":
                        OutputType = OutputTypeEnum.THIRTYSECONDTHS;
                        break;
                    case "Decimal 2-digits":
                    case "System.Windows.Controls.ComboBoxItem: Decimal 2-digits":
                        OutputType = OutputTypeEnum.DECIMAL_2DIGIT;
                        break;
                    case "Decimal 3-digits":
                    case "System.Windows.Controls.ComboBoxItem: Decimal 3-digits":
                        OutputType = OutputTypeEnum.DECIMAL_3DIGIT;
                        break;
                    case "Decimal 4-digits":
                    case "System.Windows.Controls.ComboBoxItem: Decimal 4-digits":
                        OutputType = OutputTypeEnum.DECIMAL_4DIGIT;
                        break;
                    default:
                        break;
                }
                Notify("OutputTypeString");
            }
        }

        private string _spacingStyleString = "Every point";
        public string SpacingStyleString
        {
            get { return _spacingStyleString; }
            set
            {
                _spacingStyleString = value;
                switch (_spacingStyleString)
                {
                    case "Every point":
                        SpacingStyle = SpacingStyleEnum.EVERY_POINT;
                        break;
                    case "Fixed spacing":
                        SpacingStyle = SpacingStyleEnum.FIXED_SPACING;
                        break;
                    default:
                        break;
                }
                Notify("SpacingStyleString");
            }
        }
        private double m_Spacing = 12;
        public double Spacing
        {
            get { return m_Spacing; }
            set { m_Spacing = value; Notify("Spacing"); }
        }

        public enum OutputTypeEnum { EIGHTHS, SIXTEENTHS, THIRTYSECONDTHS, DECIMAL_2DIGIT, DECIMAL_3DIGIT, DECIMAL_4DIGIT }
        private OutputTypeEnum _outputType = OutputTypeEnum.EIGHTHS;

        public OutputTypeEnum OutputType
        {
            get { return _outputType; }
            set { _outputType = value; Notify("OutputType"); }
        }

        public enum SpacingStyleEnum { EVERY_POINT, FIXED_SPACING }
        private SpacingStyleEnum _spacingStyle = SpacingStyleEnum.EVERY_POINT;

        public SpacingStyleEnum SpacingStyle
        {
            get { return _spacingStyle; }
            set { _spacingStyle = value; Notify("SpacingStyle"); }
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
