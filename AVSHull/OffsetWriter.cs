using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace AVSHull
{
    class OffsetWriter : ILayoutWriter
    {
        public OffsetWriter()
        {
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
                OffsetSetupWindow setup = new OffsetSetupWindow();
                result = setup.ShowDialog();

                if (result == true)
                {
                    using (System.IO.StreamWriter output = new System.IO.StreamWriter(saveDlg.FileName))
                    {
                        foreach (Panel panel in Layout.Panels)
                        {
                            output.WriteLine(panel.name);

                            if (setup.SpacingStyle_Input.Text == "Every point")
                            {
                                foreach (Point point in panel.Points)
                                    output.WriteLine("   {0}", FormatPoint(point, setup.OutputType));
                            }
                            else
                            {
                                for (int ii = 0; ii < panel.Points.Count; ii++)
                                {
                                    if (ii == 0 || ii == panel.Points.Count / 2 - 1 || ii == panel.Points.Count / 2 || ii == panel.Points.Count - 2 || ii == panel.Points.Count - 1)
                                    {
                                        output.WriteLine("   {0} *", FormatPoint(panel.Points[ii], setup.OutputType));
                                    }
                                    else if (ii % 10 == 0)
                                    {
                                        output.WriteLine("   {0}", FormatPoint(panel.Points[ii], setup.OutputType));
                                    }
                                }
                            }
                        }
                    }

                    return true;
                }
            }
            
            return result;
        }

        private PointCollection GetPoints(Panel panel, OffsetSetupWindow setup)
        {
            if (setup.SpacingStyle_Input.Text == "Every point") 
                return panel.Points;
            else 
            {
                PointCollection points = new PointCollection();
                for (int ii=0; ii<panel.Points.Count; ii++)
                {
                    if (ii==0 || ii==1 || ii==points.Count/2 || ii==points.Count/2 + 1 || ii % 10 == 0)
                    {
                        points.Add(panel.Points[ii]);
                    }
                }
                return points;
            }
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
        private String FormatPoint(Point point, String format)
        {
            String result = new String("");
            switch (format)
            {
                case "Eighths":
                    result = Fraction(point.X, 8) + " " + Fraction(point.Y, 8);
                    break;
                case "Sixteenths":
                    result = Fraction(point.X, 16) + " " + Fraction(point.Y, 16);
                    break;
                case "Thirtysecondths":
                    result = Fraction(point.X, 32) + " " + Fraction(point.Y, 32);
                    break;
                case "Decimal 2-digits":
                    result = Decimal(point.X, 2) + " " + Decimal(point.Y, 2);
                    break;
                case "Decimal 3-digits":
                    result = Decimal(point.X, 3) + " " + Decimal(point.Y, 3);
                    break;
                case "Decimal 4-digits":
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
