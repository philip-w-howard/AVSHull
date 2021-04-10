using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace AVSHull
{
    class STLWriter
    {
        private System.IO.StreamWriter stlFile;
        private string m_filename = null;
        private double m_depth = 0.25;

        public STLWriter(string filename)
        {
            m_filename = filename;
            stlFile = new System.IO.StreamWriter(filename);
            stlFile.WriteLine("solid " + filename);
        }

        public void Close()
        {
            if (stlFile != null)
            {
                stlFile.WriteLine("endsolid " + m_filename);
                stlFile.Close();
            }
        }

        public void Write(Panel panel)
        {
            Point lastPoint = new Point(0, 0);
            Point firstPoint = new Point(0, 0);
            PointCollection points = panel.Points;
            Boolean first = true;

            foreach (Point p in points)
            {
                if (first)
                {
                    lastPoint = p;
                    firstPoint = p;
                    first = false;
                }
                else
                {
                    stlFile.WriteLine("facet normal 0.0E1 0.0E1 0.0E1"); // normal vector
                    stlFile.WriteLine("outer loop");
                    stlFile.WriteLine("vertex {0:E} {1:E} {2:E}", lastPoint.X, lastPoint.Y, -m_depth);
                    stlFile.WriteLine("vertex {0:E} {1:E} {2:E}", lastPoint.X, lastPoint.Y, 0);
                    stlFile.WriteLine("vertex {0:E} {1:E} {2:E}", p.X, p.Y, 0);
                    stlFile.WriteLine("endloop");
                    stlFile.WriteLine("endfacet");

                    stlFile.WriteLine("facet normal ni nj nk"); // normal vector
                    stlFile.WriteLine("outer loop");
                    stlFile.WriteLine("vertex {0:E} {1:E} {2:E}", lastPoint.X, lastPoint.Y, -m_depth);
                    stlFile.WriteLine("vertex {0:E} {1:E} {2:E}", p.X, p.Y, 0);
                    stlFile.WriteLine("vertex {0:E} {1:E} {2:E}", p.X, p.Y, -m_depth);
                    stlFile.WriteLine("endloop");
                    stlFile.WriteLine("endfacet");
                    lastPoint = p;
                }
            }
            // go back to first point
            stlFile.WriteLine("facet normal 0.0E1 0.0E1 0.0E1"); // normal vector
            stlFile.WriteLine("outer loop");
            stlFile.WriteLine("vertex {0:E} {1:E} {2:E}", lastPoint.X, lastPoint.Y, -m_depth);
            stlFile.WriteLine("vertex {0:E} {1:E} {2:E}", lastPoint.X, lastPoint.Y, 0);
            stlFile.WriteLine("vertex {0:E} {1:E} {2:E}", firstPoint.X, firstPoint.Y, 0);
            stlFile.WriteLine("endloop");
            stlFile.WriteLine("endfacet");

            stlFile.WriteLine("facet normal ni nj nk"); // normal vector
            stlFile.WriteLine("outer loop");
            stlFile.WriteLine("vertex {0:E} {1:E} {2:E}", lastPoint.X, lastPoint.Y, -m_depth);
            stlFile.WriteLine("vertex {0:E} {1:E} {2:E}", firstPoint.X, firstPoint.Y, 0);
            stlFile.WriteLine("vertex {0:E} {1:E} {2:E}", firstPoint.X, firstPoint.Y, -m_depth);
            stlFile.WriteLine("endloop");
            stlFile.WriteLine("endfacet");
        }

    }
}
