using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace AVSHull
{
    public class GCodeParameters : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        private double m_PlungeSpeed = 5;
        private double m_CutSpeed = 30;
        private double m_CutterDiameter = 0.25;
        private bool m_OffsetInside = false;
        private double m_MaterialThickness = 0.25;
        private double m_CutthroughDepth = 0.05;
        private double m_DepthPerCut = 0.15;
        private int m_NumTabs = 4;
        private double m_TabDepth = 0.0625;
        private double m_TabWidth = 0.375;
        private int m_Origin = 0;
        private List<string> m_OriginTypes = new List<string> { "Panels Bottom Left", "Sheet Bottom Left", "Sheet Center" };
        /*
                <ComboBoxItem>Panels Bottom Left</ComboBoxItem>
                <ComboBoxItem>Sheet Bottom Left</ComboBoxItem>
                <ComboBoxItem>Sheet Center</ComboBoxItem>
        */
        public List<string> OriginTypes
        {
            get { return m_OriginTypes; }
            /*
            set
            {
                m_OriginTypes = value;
                Notify("OriginTypes");
            }
            */
        }
        public double PlungeSpeed
        {
            get { return m_PlungeSpeed; }
            set
            {
                m_PlungeSpeed = value;
                Notify("PlungeSpeed");
            }
        }

        public double CutSpeed
        {
            get { return m_CutSpeed; }
            set
            {
                m_CutSpeed = value;
                Notify("CutSpeed");
            }
        }

        public double CutterDiameter
        {
            get { return m_CutterDiameter; }
            set
            {
                m_CutterDiameter = value;
                Notify("CutterDiamater");
            }
        }

        public bool OffsetInside
        {
            get { return m_OffsetInside; }
            set
            {
                m_OffsetInside = value;
                Notify("OffsetInside");
            }
        }

        public double MaterialThickness
        {
            get { return m_MaterialThickness; }
            set
            {
                m_MaterialThickness = value;
                Notify("MaterialThickness");
            }
        }

        public double CutthroughDepth
        {
            get { return m_CutthroughDepth; }
            set
            {
                m_CutthroughDepth = value;
                Notify("CutthroughDepth");
            }
        }

        public double DepthPerCut
        {
            get { return m_DepthPerCut; }
            set
            {
                m_DepthPerCut = value;
                Notify("DepthPerCut");
            }
        }

        public int NumTabs
        {
            get { return m_NumTabs; }
            set
            {
                m_NumTabs = value;
                Notify("NumTabs");
            }
        }

        public double TabDepth
        {
            get { return m_TabDepth; }
            set
            {
                m_TabDepth = value;
                Notify("TabDepth");
            }
        }

        public double TabWidth
        {
            get { return m_TabWidth; }
            set
            {
                m_TabWidth = value;
                Notify("TabWidth");
            }
        }

        public int Origin
        {
            get { return m_Origin; }
            set
            {
                m_Origin = value;
                Notify("Origin");
            }
        }

    }
    class GCodeWriter
    {
        // Parameters:
        //      Cut speed
        //      plunge speed
        //      cutter diameter
        //      Inside/Outside
        //      Depth of material
        //      Depth per cut
        //      tabs: number, placement, depth, width

        GCodeParameters parameters = new GCodeParameters();

        private System.IO.StreamWriter gCodeFile;

        public GCodeWriter(string filename)
        {
            parameters = (GCodeParameters)Application.Current.FindResource("GCodeSetup");

            gCodeFile = new System.IO.StreamWriter(filename);
            gCodeFile.WriteLine("%");
            gCodeFile.WriteLine("O1234 ({0})", filename);
            gCodeFile.WriteLine("G20 G90 G40 M05");     // inches, absolute, no offset, Spindle ON. Could add G17: XY plane, but not supported
            gCodeFile.WriteLine("G00 Z0.25 M03");           // 0.25 inches above the surface
        }

        public void Close()
        {
            if (gCodeFile != null)
            {
                gCodeFile.WriteLine("G01 Z0.25 M05");        // lift out of material and turn spindle off
                gCodeFile.WriteLine("G91 G28 X0");          // return to zero
                gCodeFile.WriteLine("G91 G28 Z0");
                gCodeFile.WriteLine("G90");                 // absolute
                gCodeFile.WriteLine("G40");                 // no offset
                gCodeFile.WriteLine("M30");                 // end of program
                gCodeFile.Close();
            }
        }

        public void Write(Panel panel, Point origin)
        {
            double offset = parameters.CutterDiameter / 2;
            PointCollection points = panel.Points.Clone();
            //GeometryOperations.TranslateShape(points, panel.Origin.X, panel.Origin.Y);

            Boolean first = true;
            Point firstPoint = new Point(0, 0);

            // Get the tool path, which is an offset from the panel points
            PointCollection path;
            if (offset != 0)
                path = GeometryOperations.ParallelShape(points, offset, parameters.OffsetInside);
            else
                path = points;

            double currentDepth = 0;
            while (currentDepth < parameters.MaterialThickness)
            {
                currentDepth += parameters.DepthPerCut;
                if (currentDepth > parameters.MaterialThickness) currentDepth = parameters.MaterialThickness + parameters.CutthroughDepth;
                foreach (Point p in path)
                {
                    Point spot = new Point(p.X - origin.X, -(p.Y - origin.Y));

                    if (first)
                    {
                        firstPoint = spot;
                        gCodeFile.WriteLine("G01 Z0.25 F{0}", parameters.PlungeSpeed);               // Lift cutter
                        gCodeFile.WriteLine("G00 X{0} Y{1}", spot.X, spot.Y);                     // Go to start point
                        gCodeFile.WriteLine("G01 Z-{0} F{1}", currentDepth, parameters.PlungeSpeed); // cut into surface
                        gCodeFile.WriteLine("G01 X{0} Y{1} F{2}", spot.X, spot.Y, parameters.CutSpeed);    // set cutspeed 
                        first = false;
                    }
                    else
                    {
                        gCodeFile.WriteLine("G01 X{0} Y{1}", spot.X, spot.Y);
                    }
                }
                first = true;
            }
            gCodeFile.WriteLine("G01 X{0} Y{1}", firstPoint.X, firstPoint.Y);
            gCodeFile.WriteLine("G01 Z0.25 F{0}", parameters.PlungeSpeed);  // Lift cutter
        }
    }
}
