using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AVSHull
{
    public class Panel : INotifyPropertyChanged, ICloneable
    {
        private const double MIN_EDGE_LENGTH = 0.1;

        public string name { get; set; }

        private Point m_origin = new Point(0, 0);
        public Point Origin
        {
            get { return m_origin; }
            set { m_origin = value; Notify("Panel.Origin"); }
        }

        protected PointCollection m_panelPoints;

        public Panel()
        {
        }

        public Panel(Panel.SerializablePanel panel)
        {
            name = panel.name;
            m_panelPoints = panel.points.Clone();
            Origin = panel.origin;

            ShiftTo(0, 0);
        }

        // Develop the panel from two chines
        public Panel(Point3DCollection chine1, Point3DCollection chine2)
        {
            Panelize(chine1, chine2);
            HorizontalizePanel();
            ShiftTo(0, 0);
        }

        // Project a bulkhead onto its 2D shape.
        // NOTE: This assumes the bulkhead is within a plane.
        public Panel(Point3DCollection points)
        {
            m_panelPoints = new PointCollection();
            foreach (Point3D point in points)
            {
                // FIXTHIS: only works for VERTICAL bulkheads
                m_panelPoints.Add(new Point(point.X, point.Y));
            }
            ShiftTo(0, 0);
        }

         protected void Panelize(Point3DCollection chine1, Point3DCollection chine2)
        {
            double r1, r2;
            Point intersection_a1, intersection_a2;
            Point intersection_b1, intersection_b2;
            PointCollection edge2 = new PointCollection();

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
                // Make the edge the first segment in edge2
                m_panelPoints.Add(new Point(0, 0));
                edge2.Add(new Point(0, 0));

                r1 = (chine1[0] - chine2[0]).Length;
                edge2.Add(new Point(0, -r1));

                // Compute next point, and favor positive X direction
                // advance edge1 by one point
                r1 = (chine1[0] - chine1[1]).Length;
                r2 = (chine2[0] - chine1[1]).Length;
                GeometryOperations.Intersection(m_panelPoints[m_panelPoints.Count - 1], r1, edge2[edge2.Count - 1], r2, out intersection_a1, out intersection_a2);

                // advance edge2 by one point
                r1 = (chine2[0] - chine2[1]).Length;
                r2 = (chine1[0] - chine2[1]).Length;
                GeometryOperations.Intersection(edge2[edge2.Count - 1], r1, m_panelPoints[m_panelPoints.Count - 1], r2, out intersection_b1, out intersection_b2);

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

                // advance edge2 by one point
                r1 = (chine2[ii - 1] - chine2[ii]).Length;
                r2 = (chine1[ii - 1] - chine2[ii]).Length;
                GeometryOperations.Intersection(edge2[edge2.Count - 1], r1, m_panelPoints[m_panelPoints.Count - 1], r2, out intersection_b1, out intersection_b2);

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

        public void HorizontalizePanel()
        {
            double x = m_panelPoints[m_panelPoints.Count / 2].X - m_panelPoints[0].X;
            double y = m_panelPoints[m_panelPoints.Count / 2].Y - m_panelPoints[0].Y;

            double angle;

            angle = Math.Atan2(y, x);
            Rotate(new Point(0, 0), -angle);

            ShiftTo(0, 0);
        }

        public void Rotate(Point origin, double angle)
        {
            double[,] rotate = new double[2, 2];

            Shift(-origin.X, -origin.Y);

            rotate[0, 0] = Math.Cos(angle);
            rotate[1, 1] = Math.Cos(angle);
            rotate[0, 1] = Math.Sin(angle);
            rotate[1, 0] = -Math.Sin(angle);

            Matrix.Multiply(m_panelPoints, rotate, out m_panelPoints);

            ShiftTo(0, 0);
            Notify("Panel.Rotate");
        }
        private void ShiftTo(double x, double y)
        {
            double min_x, min_y;
            GeometryOperations.ComputeMin(m_panelPoints, out min_x, out min_y);

            x -= min_x;
            y -= min_y;

            if (x != 0 || y != 0) Shift(x, y);
        }
        private void Shift(double x, double y)
        {
            GeometryOperations.TranslateShape(m_panelPoints, x + m_origin.X, y + m_origin.Y);
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
            ShiftTo(0, 0);
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
            ShiftTo(0, 0);
            Notify("panel.flip");
        }

        public Geometry GetGeometry()
        {
            GeometryGroup geom = new GeometryGroup();
            
            if (m_panelPoints.Count < 2) return geom;

            Point p2 = m_panelPoints[m_panelPoints.Count - 1];

            foreach (Point p1 in  m_panelPoints)
            {
                geom.Children.Add(new LineGeometry(p2, p1));
                p2 = p1;
            }
  
            return geom;
        }

        public class SerializablePanel
        {
            public string name { get; set; }

            public PointCollection points;

            public Point origin;

            public SerializablePanel()
            { }

            public SerializablePanel(Panel panel)
            {
                name = panel.name;
                points = panel.m_panelPoints.Clone();
                origin = panel.Origin;
            }
        }

        //***************************************************
        // INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        //**************************************************
        // IClonable Interface
        public object Clone()
        {
            Panel newPanel = new Panel();

            newPanel.m_panelPoints = this.m_panelPoints.Clone();
            newPanel.name = this.name;

            return newPanel;
        }
    }
}
