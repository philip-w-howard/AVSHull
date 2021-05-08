using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace AVSHull
{
    class OffsetWriterSetup
    {
        public int OutputType { get; set; }
        public int SpacingStyle { get; set; }
        public int HorizontalSpacing { get; set; }
        public double MaxAngle { get; set; }
    }
    class OffsetWriter : ILayoutWriter
    {
        private double MaxAngle = 178 * Math.PI / 180.0;

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
                    OffsetWriterSetup settings = (OffsetWriterSetup)setup.FindResource("OffsetSetup");
                    if (settings == null) return false;

                    MaxAngle = (180 - settings.MaxAngle) * Math.PI / 180.0;
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
                                int spacing = settings.HorizontalSpacing;

                                Point next;
                                Point curr;
                                Point prev;

                                int index = 0;
                                while (index < panel.Points.Count)
                                {
                                    // Always output the first and last points
                                    if (index == 0 || index == panel.Points.Count - 1)
                                    {
                                        output.WriteLine("   {0} {1} *", FormatPoint(panel.Points[index], setup.OutputType), index);
                                        index++;
                                    }
                                    else
                                    {
                                        prev = panel.Points[index - 1];
                                        curr = panel.Points[index];
                                        next = panel.Points[index + 1];

                                        double leftAngle = 0;
                                        double rightAngle = 0;
                                        GeometryOperations.ComputeAngle(prev, curr, next, ref leftAngle, ref rightAngle);
                                        double angle = Math.Min(leftAngle, rightAngle);
                                        if (angle < MaxAngle)
                                        {
                                            output.WriteLine("   {0} {1} ***", FormatPoint(curr, setup.OutputType), index);
                                            output.WriteLine("   {0} {1} ***", FormatPoint(next, setup.OutputType), index);
                                            index += 2;
                                        }
                                        else if ((int)curr.X / spacing != (int)next.X / spacing)
                                        {
                                            if (curr.X < next.X)
                                                curr = GeometryOperations.InterpolateFromX(curr, next, spacing * ((int)next.X / spacing));
                                            else
                                                curr = GeometryOperations.InterpolateFromX(next, curr, spacing * ((int)curr.X / spacing));

                                            output.WriteLine("   {0} {1}", FormatPoint(curr, setup.OutputType), index);
                                            index++;
                                        }
                                        else
                                        {
                                            index++;
                                        }
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
