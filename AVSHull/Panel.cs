using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Diagnostics;

namespace AVSHull
{
    public class Panel : INotifyPropertyChanged, ICloneable
    {
        private const double MIN_EDGE_LENGTH = 0.25;

        public string name { get; set; }

        private Point m_origin = new Point(0, 0);
        public Point Origin
        {
            get { return m_origin; }
            set { m_origin = value; Notify("Panel.Origin"); }
        }

        protected PointCollection m_panelPoints;
        public PointCollection Points
        { 
            get { return m_panelPoints; } 
            set { m_panelPoints = value; }
        }


        public Panel()
        {
            m_panelPoints = new PointCollection();
        }

        // Develop the panel from two chines
        public Panel(Point3DCollection chine1, Point3DCollection chine2)
        {
            m_origin = new Point(0, 0);
            Panelize(chine1, chine2);
            HorizontalizePanel();
            Center();
        }

         public Panel(Bulkhead bulk)
        {
            double scaleFactor = 1;
            if (bulk.Type == Bulkhead.BulkheadType.TRANSOM) scaleFactor = Math.Sin(bulk.TransomAngle);

            m_panelPoints = new PointCollection();

            foreach (Point3D point in bulk.Points)
            {
                m_panelPoints.Add(new Point(point.X, point.Y/scaleFactor));
            }
            Center();
        }
        protected Panel(PointCollection points)
        {
            m_panelPoints = points.Clone();
            Center();
        }

        protected void Panelize(Point3DCollection chine1, Point3DCollection chine2)
        {
            double r1, r2;
            Point intersection_a1, intersection_a2;
            Point intersection_b1, intersection_b2;
            PointCollection edge2 = new PointCollection();

            m_panelPoints = new PointCollection();

            bool pointy_bow = (chine1[0] - chine2[0]).Length < MIN_EDGE_LENGTH;
            bool pointy_stern = (chine1[chine1.Count-1] - chine2[chine1.Count-1]).Length < MIN_EDGE_LENGTH;

            if (pointy_bow)
            {
                m_panelPoints.Add(new Point(0, 0));
                edge2.Add(new Point(0, 0));

                r1 = (chine1[0] - chine1[1]).Length;
                m_panelPoints.Add(new Point(r1 * Math.Cos(Math.PI / 4), r1 * Math.Sin(Math.PI / 4)));
            }
            else
            {
                // Start at origin
                m_panelPoints.Add(new Point(0, 0));
                edge2.Add(new Point(0, 0));

                // Make the edge the first segment in edge2
                r1 = (chine1[0] - chine2[0]).Length;
                edge2.Add(new Point(0, -r1));

                // Compute next point, and favor positive X direction
                // advance edge1 by one point
                r1 = (chine1[0] - chine1[1]).Length;
                r2 = (chine2[0] - chine1[1]).Length;
                GeometryOperations.Intersection(m_panelPoints[m_panelPoints.Count - 1], r1, edge2[edge2.Count - 1], r2, out intersection_a1, out intersection_a2);
                Debug.WriteLine("Intersection a {0}: ({1:F2}) r: {2:F2}  ({3:F2}) r: {4:F2} ::= ({5:F2}) ({6:F2})",
                    m_panelPoints.Count, m_panelPoints[m_panelPoints.Count - 1], r1, edge2[edge2.Count - 1], r2, intersection_a1, intersection_a2);

                if (intersection_a1.X >= intersection_a2.X)
                    m_panelPoints.Add(intersection_a1);
                else
                    m_panelPoints.Add(intersection_a2);
            }

            // Add next point to edge2
            r1 = (chine2[0] - chine2[1]).Length;
            r2 = (chine1[1] - chine2[1]).Length;
            GeometryOperations.Intersection(edge2[edge2.Count - 1], r1, m_panelPoints[m_panelPoints.Count - 1], r2, out intersection_b1, out intersection_b2);
            Debug.WriteLine("Intersection b {0}: ({1:F2}) r: {2:F2}  ({3:F2}) r: {4:F2} ::= ({5:F2}) ({6:F2})",
                m_panelPoints.Count, edge2[edge2.Count - 1], r1, m_panelPoints[m_panelPoints.Count - 1], r2, intersection_b1, intersection_b2);

            if (intersection_b1.X >= intersection_b2.X)
                edge2.Add(intersection_b1);
            else
                edge2.Add(intersection_b2);

            // Complete the rest of the points
            int last_point;
            if (pointy_stern)
                last_point = chine1.Count - 2;
            else
                last_point = chine1.Count - 1;

            for (int ii = 1; ii < last_point; ii++)
            {
                r1 = (chine1[ii] - chine1[ii+1]).Length;
                r2 = (chine2[ii] - chine1[ii+1]).Length;
                GeometryOperations.Intersection(m_panelPoints[m_panelPoints.Count - 1], r1, edge2[edge2.Count - 1], r2, out intersection_a1, out intersection_a2);
                Debug.WriteLine("Intersection a {0}: ({1:F2}) r: {2:F2}  ({3:F2}) r: {4:F2} ::= ({5:F2}) ({6:F2})",
                    m_panelPoints.Count, m_panelPoints[m_panelPoints.Count - 1], r1, edge2[edge2.Count - 1], r2, intersection_a1, intersection_a2);

                Vector v_1 = m_panelPoints[m_panelPoints.Count - 1] - m_panelPoints[m_panelPoints.Count - 2];
                Vector v_1a = intersection_a1 - m_panelPoints[m_panelPoints.Count - 1];
                Vector v_1b = intersection_a2 - m_panelPoints[m_panelPoints.Count - 1];

                double a1 = Math.Abs(Vector.AngleBetween(v_1, v_1a));
                double a2 = Math.Abs(Vector.AngleBetween(v_1, v_1b));

                if (a1 < a2)
                    m_panelPoints.Add(intersection_a1);
                else
                    m_panelPoints.Add(intersection_a2);
                
                // advance edge2 by one point
                r1 = (chine2[ii] - chine2[ii + 1]).Length;
                r2 = (chine1[ii + 1] - chine2[ii + 1]).Length;
                GeometryOperations.Intersection(edge2[edge2.Count - 1], r1, m_panelPoints[m_panelPoints.Count - 1], r2, out intersection_b1, out intersection_b2);
                Debug.WriteLine("Intersection b {0}: ({1:F2}) r: {2:F2}  ({3:F2}) r: {4:F2} ::= ({5:F2}) ({6:F2})",
                    m_panelPoints.Count, edge2[edge2.Count - 1], r1, m_panelPoints[m_panelPoints.Count - 1], r2, intersection_b1, intersection_b2);

                Vector v_2 = edge2[edge2.Count - 1] - edge2[edge2.Count - 2];
                Vector v_2a = intersection_b1 - edge2[edge2.Count - 1];
                Vector v_2b = intersection_b2 - edge2[edge2.Count - 1];

                double b1 = Math.Abs(Vector.AngleBetween(v_2, v_2a));
                double b2 = Math.Abs(Vector.AngleBetween(v_2, v_2b));

                if (b1 < b2)
                    edge2.Add(intersection_b1);
                else
                    edge2.Add(intersection_b2);
            }

            if (pointy_stern)
            {
                r1 = (chine1[chine1.Count - 2] - chine1[chine1.Count - 1]).Length;
                r2 = (chine2[chine2.Count - 2] - chine2[chine2.Count - 1]).Length;

                GeometryOperations.Intersection(m_panelPoints[m_panelPoints.Count - 1], r1, edge2[edge2.Count - 1], r2, out intersection_a1, out intersection_a2);
                Debug.WriteLine("Intersection a {0}: ({1:F2}) r: {2:F2}  ({3:F2}) r: {4:F2} ::= ({5:F2}) ({6:F2})",
                    m_panelPoints.Count, m_panelPoints[m_panelPoints.Count - 1], r1, edge2[edge2.Count - 1], r2, intersection_a1, intersection_a2);

                Vector v_1 = m_panelPoints[m_panelPoints.Count - 1] - m_panelPoints[m_panelPoints.Count - 2];
                Vector v_1a = intersection_a1 - m_panelPoints[m_panelPoints.Count - 1];
                Vector v_1b = intersection_a2 - m_panelPoints[m_panelPoints.Count - 1];

                double a1 = Math.Abs(Vector.AngleBetween(v_1, v_1a));
                double a2 = Math.Abs(Vector.AngleBetween(v_1, v_1b));

                if (a1 < a2)
                    m_panelPoints.Add(intersection_a1);
                else
                    m_panelPoints.Add(intersection_a2);

                // Don't need to add point to edge2 because it is the same (pointy) point and it would be a duplicate
            }

            // NOTE: Should check for closed tail?
            for (int ii = edge2.Count - 1; ii >= 0; ii--)
            {
                m_panelPoints.Add(edge2[ii]);
            }
        }
        protected void Panelize_old(Point3DCollection chine1, Point3DCollection chine2)
        {
            double r1, r2;
            Point intersection_a1, intersection_a2;
            Point intersection_b1, intersection_b2;
            PointCollection edge2 = new PointCollection();

            for (int ii = 0; ii < chine1.Count; ii++)
            {
                Debug.WriteLine("Chines {0} ({1:F2}) ({2:F2})", ii, chine1[ii], chine2[ii]);
            }

            m_panelPoints = new PointCollection();

            // See if we start at a point or an edge:
            if ((chine1[0] - chine2[0]).Length < MIN_EDGE_LENGTH)
            {
                // Not implemented yet
                throw new Exception();

                //// Start both edges at (0,0)
                //m_panelPoints.Add(new Point(0, 0));
                //edge2.Add(new Point(0, 0));

                //// Compute next point, and place it on the x axis
                //// advance edge1 by one point
                //r1 = (chine1[0] - chine1[1]).Length;
                //m_panelPoints.Add(new Point(r1, 0));


                //// advance edge2 by one point
                //r1 = (chine2[0] - chine2[1]).Length;
                //r2 = (chine1[0] - chine2[1]).Length;
                //Geometry.Intersection(edge2[edge2.Count - 1], r1, m_panelPoints[m_panelPoints.Count - 1], r2, out intersection_b1, out intersection_b2);

                //if (intersection_b1.X >= intersection_b2.X)
                //    edge2.Add(intersection_b1);
                //else
                //    edge2.Add(intersection_b2);
            }
            else
            {
                // Start at origin
                m_panelPoints.Add(new Point(0, 0));
                edge2.Add(new Point(0, 0));

                // Make the edge the first segment in edge2
                r1 = (chine1[0] - chine2[0]).Length;
                edge2.Add(new Point(0, -r1));

                // Compute next point, and favor positive X direction
                // advance edge1 by one point
                r1 = (chine1[0] - chine1[1]).Length;
                r2 = (chine2[0] - chine1[1]).Length;
                GeometryOperations.Intersection(m_panelPoints[m_panelPoints.Count - 1], r1, edge2[edge2.Count - 1], r2, out intersection_a1, out intersection_a2);
                Debug.WriteLine("Intersection a {0}: ({1:F2}) r: {2:F2}  ({3:F2}) r: {4:F2} ::= ({5:F2}) ({6:F2})",
                    m_panelPoints.Count, m_panelPoints[m_panelPoints.Count - 1], r1, edge2[edge2.Count - 1], r2, intersection_a1, intersection_a2);

                // advance edge2 by one point
                r1 = (chine2[0] - chine2[1]).Length;
                r2 = (chine1[0] - chine2[1]).Length;
                GeometryOperations.Intersection(edge2[edge2.Count - 1], r1, m_panelPoints[m_panelPoints.Count - 1], r2, out intersection_b1, out intersection_b2);
                Debug.WriteLine("Intersection b {0}: ({1:F2}) r: {2:F2}  ({3:F2}) r: {4:F2} ::= ({5:F2}) ({6:F2})",
                    m_panelPoints.Count, edge2[edge2.Count - 1], r1, m_panelPoints[m_panelPoints.Count - 1], r2, intersection_b1, intersection_b2);

                if (intersection_a1.X >= intersection_a2.X)
                    m_panelPoints.Add(intersection_a1);
                else
                    m_panelPoints.Add(intersection_a2);

                if (intersection_b1.X >= intersection_b2.X)
                    edge2.Add(intersection_b1);
                else
                    edge2.Add(intersection_b2);
            }


            for (int ii = 2; ii < chine1.Count; ii++)
            {
                // advance edge1 by one point
                r1 = (chine1[ii - 1] - chine1[ii]).Length;
                r2 = (chine2[ii - 1] - chine1[ii]).Length;
                GeometryOperations.Intersection(m_panelPoints[m_panelPoints.Count - 1], r1, edge2[edge2.Count - 1], r2, out intersection_a1, out intersection_a2);
                Debug.WriteLine("Intersection a {0}: ({1:F2}) r: {2:F2}  ({3:F2}) r: {4:F2} ::= ({5:F2}) ({6:F2})",
                    m_panelPoints.Count, m_panelPoints[m_panelPoints.Count - 1], r1, edge2[edge2.Count - 1], r2, intersection_a1, intersection_a2);

                // advance edge2 by one point
                r1 = (chine2[ii - 1] - chine2[ii]).Length;
                r2 = (chine1[ii - 1] - chine2[ii]).Length;
                GeometryOperations.Intersection(edge2[edge2.Count - 1], r1, m_panelPoints[m_panelPoints.Count - 1], r2, out intersection_b1, out intersection_b2);
                Debug.WriteLine("Intersection b {0}: ({1:F2}) r: {2:F2}  ({3:F2}) r: {4:F2} ::= ({5:F2}) ({6:F2})",
                    m_panelPoints.Count, edge2[edge2.Count - 1], r1, m_panelPoints[m_panelPoints.Count - 1], r2, intersection_b1, intersection_b2);

                Vector v_1 = m_panelPoints[m_panelPoints.Count - 1] - m_panelPoints[m_panelPoints.Count - 2];
                Vector v_1a = intersection_a1 - m_panelPoints[m_panelPoints.Count - 1];
                Vector v_1b = intersection_a2 - m_panelPoints[m_panelPoints.Count - 1];

                Vector v_2 = edge2[edge2.Count - 1] - edge2[edge2.Count - 2];
                Vector v_2a = intersection_b1 - edge2[edge2.Count - 1];
                Vector v_2b = intersection_b2 - edge2[edge2.Count - 1];

                double a1 = Math.Abs(Vector.AngleBetween(v_1, v_1a));
                double a2 = Math.Abs(Vector.AngleBetween(v_1, v_1b));
                double b1 = Math.Abs(Vector.AngleBetween(v_2, v_2a));
                double b2 = Math.Abs(Vector.AngleBetween(v_2, v_2b));

                if (a1 < a2)
                    m_panelPoints.Add(intersection_a1);
                else
                    m_panelPoints.Add(intersection_a2);

                if (b1 < b2)
                    edge2.Add(intersection_b1);
                else
                    edge2.Add(intersection_b2);
            }

            // NOTE: Should check for closed tail?
            for (int ii = edge2.Count - 1; ii >= 0; ii--)
            {
                m_panelPoints.Add(edge2[ii]);
            }
        }

        private void HorizontalizePanel()
        {
            double x = m_panelPoints[m_panelPoints.Count / 2].X - m_panelPoints[0].X;
            double y = m_panelPoints[m_panelPoints.Count / 2].Y - m_panelPoints[0].Y;

            double angle;

            angle = Math.Atan2(y, x);
            Rotate(-angle);
        }

        public void Rotate(double angle)
        {
            double[,] rotate = new double[2, 2];

            rotate[0, 0] = Math.Cos(angle);
            rotate[1, 1] = Math.Cos(angle);
            rotate[0, 1] = Math.Sin(angle);
            rotate[1, 0] = -Math.Sin(angle);

            Matrix.Multiply(m_panelPoints, rotate, out m_panelPoints);

            Notify("Panel.Rotate");
        }
        private void Center()
        {
            Point center = GeometryOperations.ComputeMidPoint(m_panelPoints);
            if (center.X != 0 || center.Y != 0)
            GeometryOperations.TranslateShape(m_panelPoints, -center.X, -center.Y);
            m_origin.X -= center.X;
            m_origin.Y -= center.Y;
        }
        public void VerticalFlip()
        {
            PointCollection points = new PointCollection();

            foreach (Point p in m_panelPoints)
            {
                Point newPoint = p;
                newPoint.Y = -newPoint.Y;
                points.Add(newPoint);
            }

            m_panelPoints = points;
            Center();
            Notify("panel.flip");
        }

        public void HorizontalFlip()
        {
            PointCollection points = new PointCollection();

            foreach (Point p in m_panelPoints)
            {
                Point newPoint = p;
                newPoint.X = -newPoint.X;
                points.Add(newPoint);
            }

            m_panelPoints = points;
            Center();
            Notify("panel.flip");
        }

        public Geometry GetGeometry()
        {
            PathFigure path = new PathFigure();

            PathGeometry geom = new PathGeometry();
            
            if (m_panelPoints.Count < 2) return geom;

            Point pt = m_panelPoints[m_panelPoints.Count - 1];
            pt.X += Origin.X;
            pt.Y += Origin.Y;
            path.StartPoint = pt;
            
            foreach (Point p in  m_panelPoints)
            {
                pt = p;
                pt.X += Origin.X;
                pt.Y += Origin.Y;

                path.Segments.Add(new LineSegment(pt, true));
            }

            geom.Figures.Add(path);
            
            return geom;
        }

         //***************************************************
        // INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        //**************************************************
        // IClonable Interface
        public object Clone()
        {
            Panel newPanel = new Panel();

            newPanel.m_panelPoints = this.m_panelPoints.Clone();
            newPanel.name = this.name;
            newPanel.m_origin = this.m_origin;

            return newPanel;
        }

        //****************************************************
        // Split a panel into two sub-panels
        public bool Split(double start, int numTongues, double depth, bool roundEnds, out Panel panel_1, out Panel panel_2)
        {
            bool addTo_1 = true;
            PointCollection points_1 = new PointCollection();
            PointCollection points_2 = new PointCollection();
            Point points_2_start = new Point(0, 0);
            Point top = new Point();
            Point bottom = new Point();


            for (int ii = 0; ii < Points.Count - 1; ii++)
            {
                Point first =Points[ii];
                Point second = Points[ii + 1];
                Point startPoint = new Point();
                double min = Math.Min(first.X, second.X);
                double max = Math.Max(first.X, second.X);
                if (min <= start && max >= start)
                {
                    if (first.X == start)
                    {
                        startPoint = first;
                    }
                    else if (second.X == start)
                    {
                        startPoint = second;
                    }
                    else
                    {
                        // need to find point on line between first and second
                        // NOTE: because of the above conditions, we can't have a vertical line. They are ruled out.
                        double slope = (second.Y - first.Y) / (second.X - first.X);
                        startPoint = new Point(start, first.Y + slope * (start - first.X));
                    }

                    if (addTo_1)
                    {
                        top = startPoint;
                        if (first != startPoint) points_1.Add(first);
                        points_2.Add(top);
                    }
                    else
                    {
                        bottom = startPoint;

                        if (first != startPoint) points_2.Add(first);

                        PointCollection splitter_1;
                        if (roundEnds)
                            splitter_1 = PanelSplitter.Tongues(top, bottom, numTongues, depth);
                        else
                            splitter_1 = PanelSplitter.SquareEqualTongues(top, bottom, numTongues, depth);

                        IEnumerable<Point> splitter_2 = splitter_1.Reverse();

                        foreach (Point p in splitter_1)
                        {
                            points_1.Add(p);
                        }

                        foreach (Point p in splitter_2)
                        {
                            points_2.Add(p);
                        }
                    }

                    addTo_1 = !addTo_1;
                }
                else
                {
                    if (addTo_1)
                        points_1.Add(Points[ii]);
                    else
                        points_2.Add(Points[ii]);
                }
            }

            // close the panels
            points_1.Add(points_1[0]);
            //points_2.Add(top);

            panel_1 = new Panel(points_1);
            panel_2 = new Panel(points_2);

            panel_1.name = name + "A";
            panel_2.name = name + "B";

            return true;
        }
    }
}
