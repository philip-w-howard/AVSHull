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
        private const double KNEE_ANGLE = 5;            // knee angle in degrees

        public string name { get; set; }

        private Point m_origin = new Point(0, 0);
        public Point Origin
        {
            get { return m_origin; }
            set { m_origin = value; Notify("Panel.Origin"); }
        }

        private Point m_alignmentLeft = new Point(0, 0);
        public Point AlignmentLeft
        {
            get { return m_alignmentLeft; }
            set { m_alignmentLeft = value; Notify("Panel.AlignmentLeft"); }
        }

        private Point m_alignmentRight = new Point(0, 0);
        public Point AlignmentRight
        {
            get { return m_alignmentRight; }
            set { m_alignmentRight = value; Notify("Panel.AlignmentRight"); }
        }

        public bool HasAlignmentLine
        {
            get
            {
                Point zero = new Point(0, 0);
                return (AlignmentLeft != zero || AlignmentRight != zero);
            }
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
        protected Panel(PointCollection points, Point origin)
        {
            m_panelPoints = points.Clone();
            Center(origin);
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

                if (intersection_a1.X >= intersection_a2.X)
                    m_panelPoints.Add(intersection_a1);
                else
                    m_panelPoints.Add(intersection_a2);
            }

            // Add next point to edge2
            r1 = (chine2[0] - chine2[1]).Length;
            r2 = (chine1[1] - chine2[1]).Length;
            GeometryOperations.Intersection(edge2[edge2.Count - 1], r1, m_panelPoints[m_panelPoints.Count - 1], r2, out intersection_b1, out intersection_b2);

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

            // Copy edge2 input m_panelPoints
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

            if (HasAlignmentLine)
            {
                AlignmentLeft = Matrix.Rotate(AlignmentLeft, rotate);
                AlignmentRight = Matrix.Rotate(AlignmentRight, rotate);
            }

            Notify("Panel.Rotate");
        }
        private void Center()
        {
            Point origin = new Point(0,0);
            Center(origin);
        }
        private void Center(Point origin)
        {
            m_origin = origin;

            Point center = GeometryOperations.ComputeMidPoint(m_panelPoints);
            if (center.X != 0 || center.Y != 0)
                GeometryOperations.TranslateShape(m_panelPoints, -center.X, -center.Y);
            m_origin.X += center.X;
            m_origin.Y += center.Y;
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
        public Geometry GetAlignmentGeometry()
        {
            PathFigure path = new PathFigure();

            PathGeometry geom = new PathGeometry();

            if (!HasAlignmentLine) return geom;

            Point pt = AlignmentLeft;
            pt.X += Origin.X;
            pt.Y += Origin.Y;
            path.StartPoint = pt;


            pt = AlignmentRight;
            pt.X += Origin.X;
            pt.Y += Origin.Y;
            path.Segments.Add(new LineSegment(pt, true));

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
            newPanel.m_alignmentLeft = this.m_alignmentLeft;
            newPanel.m_alignmentRight = this.m_alignmentRight;

            return newPanel;
        }

        //****************************************************
        // Split a panel into two sub-panels
        public bool Split(double start, int numTongues, double depth, bool roundEnds, bool addAlignmentPoints, out Panel panel_1, out Panel panel_2)
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

            panel_1 = new Panel(points_1, Origin);
            panel_2 = new Panel(points_2, Origin);

            panel_1.name = name + "A";
            panel_2.name = name + "B";

            if (HasAlignmentLine)
            {
                Point p1 = AlignmentLeft;
                Point p2 = AlignmentRight;

                p1.X += Origin.X;
                p1.Y += Origin.Y;

                p2.X += Origin.X;
                p2.Y += Origin.Y;

                panel_1.AddAlignment(p1, p2);
                panel_2.AddAlignment(p1, p2);
            }
            return true;
        }
        public void AddAlignment(Point p1, Point p2)
        {
            Point startLoc = p1;
            bool isVertical = p1.X == p2.X;
            double slope = 0;
            double y_intercept = 0;
            if (!isVertical)
            {
                slope = (p1.Y - p2.Y) / (p1.X - p2.X);
                y_intercept = p1.Y - p1.X * slope;
            }

            Geometry geom = GetGeometry();

            if (!geom.FillContains(startLoc)) startLoc =p2;

            if (geom.FillContains(startLoc))
            {
                Point leftLoc = startLoc;
                leftLoc.X -= 0.25;
                while (geom.FillContains(leftLoc))
                {
                    if (isVertical)
                    {
                        leftLoc.Y -= 0.25;
                    }
                    else
                    {
                        leftLoc.X -= 0.25;
                        leftLoc.Y = slope * leftLoc.X + y_intercept;
                    }
                }
                
                // backup two steps
                if (isVertical)
                {
                    leftLoc.Y += 0.5;
                }
                else
                {
                    leftLoc.X += 0.5;
                    leftLoc.Y = slope * leftLoc.X + y_intercept;
                }
                
                // Adjust for origin
                leftLoc.X -= Origin.X;
                leftLoc.Y -= Origin.Y;

                Point rightLoc = startLoc;
                rightLoc.X += 0.25;
                while (geom.FillContains(rightLoc))
                {
                    if (isVertical)
                    {
                        rightLoc.Y += 0.25;
                    }
                    else
                    {
                        rightLoc.X += 0.25;
                        rightLoc.Y = slope * rightLoc.X + y_intercept;
                    }
                }

                // backup two steps
                if (isVertical)
                {
                    rightLoc.Y -= 0.5;
                }
                else
                {
                    rightLoc.X -= 0.5;
                    rightLoc.Y = slope * rightLoc.X + y_intercept;
                }

                // Adjust for origin
                rightLoc.X -= Origin.X;
                rightLoc.Y -= Origin.Y;

                AlignmentLeft = leftLoc;
                AlignmentRight = rightLoc;
            }
            else
            {
                // FIXTHIS: Notify the user they goofed
            }
        }
        //**************************************************
        // create a panel based on fixed horizontal spaced points
        public Panel FixedOffsetPanel(int fixed_offset)
        {
            PointCollection points = new PointCollection();

            Point p1 = m_panelPoints[m_panelPoints.Count - 2];
            Point p2 = m_panelPoints[m_panelPoints.Count - 1];
            Point p3;

            p1 = new Point(p1.X + m_origin.X, p1.Y + m_origin.Y);
            p2 = new Point(p2.X + m_origin.X, p2.Y + m_origin.Y);

            for (int ii=0; ii<m_panelPoints.Count; ii++)
            {
                p3 = new Point(m_panelPoints[ii].X + m_origin.X, m_panelPoints[ii].Y + m_origin.Y);

                if (ii==0 || ii==m_panelPoints.Count-1)
                {
                    points.Add(p3);
                }
                else if (GeometryOperations.IsKnee(p1, p2, p3, KNEE_ANGLE))
                {
                    points.Add(p2);
                }
                
                if (GeometryOperations.SpansX(p2, p3, fixed_offset))
                {
                    Point temp = GeometryOperations.ComputeSpacingPoint(p2, p3, fixed_offset);
                    // transitioned across boundary
                    points.Add(temp);
                }

                p1 = p2;
                p2 = p3;
            }

            // Have to manually create the new panel in order to get it in the exact same place
            // as the original.
            Panel fixedPanel = new Panel();
            fixedPanel.m_panelPoints = points.Clone();

            fixedPanel.Origin = new Point(0,0);

            return fixedPanel;
        }
        public List<Point> FixedOffsetAlignment(int fixed_offset)
        {
            List<Point> alignment = new List<Point>();

            Point p1 = AlignmentLeft;
            Point p2 = AlignmentRight;

            p1.X += Origin.X;
            p1.Y += Origin.Y;

            p2.X += Origin.X;
            p2.Y += Origin.Y;

            if (p1.X > p2.X)
            {
                Point temp = p1;
                p1 = p2;
                p2 = temp;
            }

            if (p1.X == p2.X)
            {
                // for vertical, we can only do the endpoints
                alignment.Add(p1);
                alignment.Add(p2);
            }
            else
            {
                double slope = (p1.Y - p2.Y) / (p1.X - p2.X);
                double y_intercept = p1.Y - p1.X * slope;

                alignment.Add(p1);

                Point current = p1;
                current.X += fixed_offset;
                current.Y = slope * current.X + y_intercept;

                while (current.X < p2.X)
                {
                    alignment.Add(GeometryOperations.ComputeSpacingPoint(p1, current, fixed_offset));
                    p1 = current;
                    current.X += fixed_offset;
                    current.Y = slope * current.X + y_intercept;
                }

                alignment.Add(p2);
            }

            return alignment;
        }
    }
}
