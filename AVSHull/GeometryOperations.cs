using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace AVSHull
{
    // Class with static methods for performing geometry calculations
    class GeometryOperations
    {
        // Compute the two solutions to the quadradic forumula.
        // a,b,c have the normal meaning for the quadradic formula.
        static public double QuadradicSolution(double a, double b, double c, out double x1, out double x2)
        {
            x1 = Double.NaN;
            x2 = Double.NaN;
            if (b * b - 4 * a * c < 0)
            {
                return Double.NaN;
            }

            double root = Math.Sqrt(b * b - 4 * a * c);
            x1 = (-b + root) / (2 * a);
            x2 = (-b - root) / (2 * a);

            return 0;
        }

        // Compute the intersection of two circles
        // p1, r1 specify the center and radius of one circle
        // p2, r2 specify the center and radius of the other circle
        // intersection1 and intersection2 are the two intersecting points.
        static public double Intersection(Point p1, double r1, Point p2, double r2, out Point intersection1, out Point intersection2)
        {
            intersection1 = new Point();
            intersection2 = new Point();

            if (p1.X != p2.X)
            {
                //double A = (r1 * r1 - r2 * r2 - p1.X * p1.X + p2.X * p2.X - p1.Y * p1.Y + p2.Y * p2.Y) / (2 * p2.X - 2 * p1.X);
                double A = (r1 * r1 - r2 * r2 - p1.X * p1.X + p2.X * p2.X - p1.Y * p1.Y + p2.Y * p2.Y) / (2 * p2.X - 2 * p1.X);
                double B = (p1.Y - p2.Y) / (p2.X - p1.X);
                double a = B * B + 1;
                double b = 2 * A * B - 2 * p1.X * B - 2 * p1.Y;
                double c = A * A - 2 * p1.X * A + p1.X * p1.X + p1.Y * p1.Y - r1 * r1;

                double y1, y2;

                if (QuadradicSolution(a, b, c, out y1, out y2) != 0) return Double.NaN; ;

                if (y1 == Double.NaN || y2 == Double.NaN) return Double.NaN;  //<<<<<<<<<<<<<<<<<<<<<<<<<

                intersection1.Y = y1;
                intersection1.X = A + B * intersection1.Y;

                intersection2.Y = y2;
                intersection2.X = A + B * intersection2.Y;
            }
            else
            {
                double A = (r1 * r1 - r2 * r2 - p1.Y * p1.Y + p2.Y * p2.Y - p1.X * p1.X + p2.X * p2.X) / (2 * p2.Y - 2 * p1.Y);
                double B = (p1.X - p2.X) / (p2.Y - p1.Y);
                double a = B * B + 1;
                double b = 2 * A * B - 2 * p1.Y * B - 2 * p1.X;
                double c = A * A - 2 * p1.Y * A + p1.Y * p1.Y + p1.X * p1.X - r1 * r1;
                double x1, x2;

                if (QuadradicSolution(a, b, c, out x1, out x2) != 0) return Double.NaN;
                intersection1.X = x1;
                intersection1.Y = A + B * intersection1.X;

                intersection2.X = x2;
                intersection2.Y = A + B * intersection2.X;
            }

            return 0;
        }

        // Find the intersection of two lines
        // Line 1 is specified by Pa1, Pa2
        // Line 2 is specified by Pb1, Pb2
        static public Point Intersection(Point Pa1, Point Pa2, Point Pb1, Point Pb2)
        {
            double rise1, run1, rise2, run2;
            double m1, m2, b1, b2;

            rise1 = Pa1.Y - Pa2.Y;
            run1 = Pa1.X - Pa2.X;

            rise2 = Pb1.Y - Pb2.Y;
            run2 = Pb1.X - Pb2.X;

            if (run1 != 0)
            {
                m1 = rise1 / run1;
                b1 = Pa1.Y - Pa1.X * m1;
            }
            else
            {
                m1 = Double.NaN;
                b1 = Double.NaN;
            }

            if (run2 != 0)
            {
                m2 = rise2 / run2;
                b2 = Pb1.Y - Pb1.X * m2;
            }
            else
            {
                m2 = Double.NaN;
                b2 = Double.NaN;
            }

            // parallel lines
            if (m1 == m2)
                return new Point(Double.NaN, Double.NaN);
            else if (run1 == 0)
                // L1 is vertical
                return new Point(Pa1.X, m2 * Pa1.X + b2);
            else if (run2 == 0)
                // L2 is vertical
                return new Point(Pb1.X, m1 * Pb1.X + b1);
            else
            {
                // 2 normal lines
                double x = (b2 - b1) / (m1 - m2);
                double y = m1 * x + b1;
                return new Point(x, y);
            }
        }

        // Compute angle P1, P2, P3
        public static void ComputeAngle(Point p1, Point p2, Point p3, ref double leftAngle, ref double rightAngle)
        {
            double run1, run2, rise1, rise2;
            double angle1, angle2;

            run1 = p1.X - p2.X;
            run2 = p3.X - p2.X;
            rise1 = p1.Y - p2.Y;
            rise2 = p3.Y - p2.Y;

            angle1 = Math.Atan2(rise1, run1);
            angle2 = Math.Atan2(rise2, run2);
            rightAngle = angle2 - angle1;
            if (rightAngle < 0) rightAngle += 2 * Math.PI;
            leftAngle = 2 * Math.PI - rightAngle;
        }

        // Compute angle P1, P2, P3
        public static double ComputeAngle(Point3D p1, Point3D p2, Point3D p3)
        {
            Vector3D ab = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            Vector3D bc = new Vector3D(p3.X - p2.X, p3.Y - p2.Y, p3.Z - p2.Z);

            return Vector3D.AngleBetween(ab, bc);
        }

        // Compute the angles going all the way around a closed shape defined by points.
        // leftAngle is the sum of the angles on the left hand side
        // rightAngle is the sum of the angles on the right hand side
        // NOTE: This algorithm assumes a closed shape with the first and last points in the colleciton being the same.
        static public void ComputeAngles(PointCollection points, ref double leftAngle, ref double rightAngle)
        {
            double left = 0, right = 0;
            leftAngle = 0;
            rightAngle = 0;

            Point p1, p2;

            // Prime the pipeline with the last non-duplicated point
            p2 = points[points.Count - 2];

            // NaN is a flag to indicate the pipeline isn't full yet
            p1 = new Point(Double.NaN, Double.NaN);

            foreach (Point p in points)
            {
                if (!Double.IsNaN(p1.X))
                {
                    ComputeAngle(p1, p2, p, ref left, ref right);
                    leftAngle += left;
                    rightAngle += right;
                }
                p1 = p2;
                p2 = p;
            }
        }

        public static void OffsetLine(Point p1, Point p2, double offset, ref Point linePoint1, ref Point linePoint2)
        {
            double angle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            angle += Math.PI / 2;

            linePoint1 = new Point(p1.X + offset * Math.Cos(angle), p1.Y + offset * Math.Sin(angle));
            linePoint2 = new Point(p2.X + offset * Math.Cos(angle), p2.Y + offset * Math.Sin(angle));
        }

        static public PointCollection ParallelShape(PointCollection points, double offset, bool inside = false)
        {
            double leftAngle = 0, rightAngle = 0;
            GeometryOperations.ComputeAngles(points, ref leftAngle, ref rightAngle);
            bool doLeft = false;

            if (leftAngle < rightAngle && inside)
                doLeft = true;
            else if (leftAngle > rightAngle && !inside)
                doLeft = true;

            if (!doLeft) offset = -offset;

            PointCollection result = new PointCollection();

            // Prime the pipeline with the last two non-duplicated points
            Point p1 = points[points.Count - 2];
            Point p2 = points[points.Count - 3];

            Point l1a = new Point();
            Point l1b = new Point();
            Point l2a = new Point();
            Point l2b = new Point();
            Point newPoint = new Point();

            foreach (Point p in points)
            {
                if (!Double.IsNaN(p1.X))
                {
                    GeometryOperations.OffsetLine(p2, p1, offset, ref l1a, ref l1b);
                    GeometryOperations.OffsetLine(p1, p, offset, ref l2a, ref l2b);

                    newPoint = GeometryOperations.Intersection(l1a, l1b, l2a, l2b);
                    result.Add(newPoint);
                }

                p2 = p1;
                p1 = p;
            }

            return result;
        }
        // Determine if the point (p3_x,p3_y) is near the line defined by (p1_x, p1_y) and (p2_x, p2_y)
        static public bool IsNearLine(double p1_x, double p1_y, double p2_x, double p2_y, double p3_x, double p3_y, double delta)
        {
            if (p1_x == p2_x) // vertical line
            {
                // is point along segment?
                if ((p1_y <= p3_y && p2_y >= p3_y) || (p1_y >= p3_y && p2_y <= p3_y))
                {
                    if (Math.Abs(p1_x - p3_x) <= delta) return true;
                }

                return false;
            }
            else if (p1_y == p2_y) // horizontal line
            {
                // is point along segment?
                if ((p1_x <= p3_x && p2_x >= p3_x) || (p1_x >= p3_x && p2_x <= p3_x))
                {
                    if (Math.Abs(p1_y - p3_y) <= delta) return true;
                }

                return false;
            }
            else // sloped line
            {
                double m1, m2;
                double b1, b2;
                double x, y;

                // compute slope between first two points:
                m1 = (p2_y - p1_y) / (p2_x - p1_x);

                // y intercept for first line
                b1 = -m1 * p1_x + p1_y;

                // compute slope of second (perpendicular) line
                m2 = -1 / m1;

                // y intercept for second (perpendicular) line
                b2 = -m2 * p3_x + p3_y;

                // Itersection of the two lines
                x = (b2 - b1) / (m1 - m2);
                y = m1 * x + b1;

                // is the intersection NOT within the line segment?
                if ((x <= p1_x && x <= p2_x) || (x >= p1_x && x >= p2_x)) return false;
                if ((y <= p1_y && y <= p2_y) || (y >= p1_y && y >= p2_y)) return false;

                // Is the intersection within delta of the point?
                double distance = Math.Sqrt((x - p3_x) * (x - p3_x) + (y - p3_y) * (y - p3_y));
                if (distance <= delta) return true;

                return false;
            }
        }

        // Create a matrix for doing point rotations. The angle around the x,y,z axis are specified.
        public static double[,] CreateRotateMatrix(double x, double y, double z)
        {
            double angle;
            double[,] rotate_all, rotate_1;
            rotate_all = new double[3, 3];

            // order is: Z, X, Y

            angle = z * Math.PI / 180.0;

            rotate_all[2, 2] = 1.0;
            rotate_all[0, 0] = Math.Cos(angle);
            rotate_all[1, 1] = Math.Cos(angle);
            rotate_all[0, 1] = Math.Sin(angle);
            rotate_all[1, 0] = -Math.Sin(angle);

            angle = x * Math.PI / 180.0;

            rotate_1 = new double[3, 3];
            rotate_1[0, 0] = 1.0;
            rotate_1[1, 1] = Math.Cos(angle);
            rotate_1[2, 2] = Math.Cos(angle);
            rotate_1[1, 2] = Math.Sin(angle);
            rotate_1[2, 1] = -Math.Sin(angle);

            Matrix.Multiply(rotate_all, rotate_1, out rotate_all);

            angle = y * Math.PI / 180.0;

            rotate_1 = new double[3, 3];
            rotate_1[1, 1] = 1.0;
            rotate_1[0, 0] = Math.Cos(angle);
            rotate_1[2, 2] = Math.Cos(angle);
            rotate_1[2, 0] = Math.Sin(angle);
            rotate_1[0, 2] = -Math.Sin(angle);

            Matrix.Multiply(rotate_all, rotate_1, out rotate_all);

            return rotate_all;
        }

        // Compute the size of an object specified as a collection of points.
        static public void ComputeSize(Point3DCollection points, out double size_x, out double size_y)
        {
            size_x = Double.NaN;
            size_y = Double.NaN;

            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double max_x = double.MinValue;
            double max_y = double.MinValue;

            foreach (Point3D point in points)
            {
                max_x = Math.Max(max_x, point.X);
                max_y = Math.Max(max_y, point.Y);
                min_x = Math.Min(min_x, point.X);
                min_y = Math.Min(min_y, point.Y);
            }

            size_x = max_x - min_x;
            size_y = max_y - min_y;
        }

        // Compute the size of an array of shapes.
        static public void ComputeSize(Point3DCollection[] shape, out double size_x, out double size_y)
        {
            size_x = Double.NaN;
            size_y = Double.NaN;

            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double max_x = double.MinValue;
            double max_y = double.MinValue;

            foreach (Point3DCollection points in shape)
            {
                foreach (Point3D point in points)
                {
                    max_x = Math.Max(max_x, point.X);
                    max_y = Math.Max(max_y, point.Y);
                    min_x = Math.Min(min_x, point.X);
                    min_y = Math.Min(min_y, point.Y);
                }
            }
            size_x = max_x - min_x;
            size_y = max_y - min_y;
        }

        // Compute the size of a 2D shape
        static public void ComputeSize(PointCollection points, out double size_x, out double size_y)
        {
            size_x = Double.NaN;
            size_y = Double.NaN;

            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double max_x = double.MinValue;
            double max_y = double.MinValue;

            foreach (Point point in points)
            {
                max_x = Math.Max(max_x, point.X);
                max_y = Math.Max(max_y, point.Y);
                min_x = Math.Min(min_x, point.X);
                min_y = Math.Min(min_y, point.Y);
            }

            size_x = max_x - min_x;
            size_y = max_y - min_y;
        }

        // Compute the size of an array of 2D shapes
        static public void ComputeSize(PointCollection[] shape, out double size_x, out double size_y)
        {
            size_x = Double.NaN;
            size_y = Double.NaN;

            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double max_x = double.MinValue;
            double max_y = double.MinValue;

            foreach (PointCollection points in shape)
            {
                foreach (Point point in points)
                {
                    max_x = Math.Max(max_x, point.X);
                    max_y = Math.Max(max_y, point.Y);
                    min_x = Math.Min(min_x, point.X);
                    min_y = Math.Min(min_y, point.Y);
                }
            }
            size_x = max_x - min_x;
            size_y = max_y - min_y;
        }

        // Find the bottom left corner of a shape defined as a collection of points.
        static public void ComputeMin(PointCollection points, out double min_x, out double min_y)
        {
            min_x = double.MaxValue;
            min_y = double.MaxValue;

            foreach (Point point in points)
            {
                min_x = Math.Min(min_x, point.X);
                min_y = Math.Min(min_y, point.Y);
            }
        }

        static public Point ComputeMidPoint(PointCollection points)
        {
            double min_x;
            double min_y;
            double size_x;
            double size_y;

            ComputeMin(points, out min_x, out min_y);
            ComputeSize(points, out size_x, out size_y);

            return new Point(min_x + size_x/2, min_y + size_y/2);
        }

        // Find the bottom left corner of a shape defined as a collection of points.
        static public void TopLeft(PointCollection points, out double min_x, out double max_y)
        {
            min_x = double.MaxValue;
            max_y = double.MinValue;

            foreach (Point point in points)
            {
                min_x = Math.Min(min_x, point.X);
                max_y = Math.Max(max_y, point.Y);
            }
        }

        // Change the size of a shape
        static public void ResizeShape(Point3DCollection[] shape, double scale)
        {
            double x, y, z;
            foreach (Point3DCollection points in shape)
            {
                for (int ii = 0; ii < points.Count; ii++)
                {
                    x = points[ii].X * scale;
                    y = points[ii].Y * scale;
                    z = points[ii].Z * scale;
                    points[ii] = new Point3D(x, y, z);
                }
            }
        }

        // Change the size of an array of shapes
        static public void ResizeShape(PointCollection[] shape, double scale)
        {
            double x, y;
            foreach (PointCollection points in shape)
            {
                for (int ii = 0; ii < points.Count; ii++)
                {
                    x = points[ii].X * scale;
                    y = points[ii].Y * scale;
                    points[ii] = new Point(x, y);
                }
            }
        }

        // Shift the location of a shape
        static public void TranslateShape(PointCollection points, double move_x, double move_y)
        {
            double x, y;
            for (int ii = 0; ii < points.Count; ii++)
            {
                x = points[ii].X + move_x;
                y = points[ii].Y + move_y;
                points[ii] = new Point(x, y);
            }
        }

        // Simulate an arc with a collection of straight lines
        // The lines are added to the end of the specified points, with a new collection being created if points is null
        static public void CreateArc(PointCollection points, double radius, Point center, double startAngle, double endAngle, int numPoints)
        {
            // if points already exists, we will append points to it.
            // Otherwise, create a new list
            if (points == null) points = new PointCollection();

            double delta = (endAngle - startAngle) / numPoints;
            double angle = startAngle;

            for (int ii = 0; ii < numPoints - 1; ii++)
            {
                points.Add(new Point(center.X + Math.Cos(angle) * radius, center.Y + Math.Sin(angle) * radius));
                angle += delta;
            }

            // by removing the last point from the loop, we are guaranteed to end in the right place.
            // No round-off error
            points.Add(new Point(center.X + Math.Cos(endAngle) * radius, center.Y + Math.Sin(endAngle) * radius));
        }

        static public Point3D? InterpolateFromZ(Point3DCollection points, double Z)
        {
            // FIX THIS: need to have at least two points
            bool found = false;

            Point3D left = points[0];
            Point3D right = points[1];

            if (left.Z < Z)
            {
                // assume points are increasing
                foreach (Point3D point in points)
                {
                    if (point.Z >= Z)
                    {
                        right = point;
                        found = true;
                        break;
                    }
                    else
                    {
                        left = point;
                    }
                }
            }
            else if (left.Z > Z)
            {
                // assume points are decreasing
                foreach (Point3D point in points)
                {
                    if (point.Z <= Z)
                    {
                        right = point;
                        found = true;
                        break;
                    }
                    else
                    {
                        left = point;
                    }
                }
            }
            else
            {
                // landed exactly on the point
                return points[0];
            }

            if (!found) return null;

            // Did we land exactly on a point?
            if (left.Z == Z) return left;
            if (right.Z == Z) return right;

            // If the Zs are the same, can't interpolate
            if (left.Z == right.Z) return left;

            // Do the interpolation
            double ratio = (Z - left.Z) / (right.Z - left.Z);

            double X = left.X + ratio * (right.X - left.X);
            double Y = left.Y + ratio * (right.Y - left.Y);

            return new Point3D(X, Y, Z);
        }

        public static bool IsKnee(Point p1, Point p2, Point p3, double angle_in_degrees)
        {
            double angle = Math.PI - angle_in_degrees * Math.PI / 180.0;
            double leftAngle = 0, rightAngle = 0;

            ComputeAngle(p1, p2, p3, ref leftAngle, ref rightAngle);

            if (angle > Math.Min(leftAngle, rightAngle))
                return true;
            else
                return false;
        }

        public static Point ComputeSpacingPoint(Point p1, Point p2, int fixed_offset)
        {
            double x;
            if (Math.Abs(p1.X) > Math.Abs(p2.X))
                x = p1.X;
            else
                x = p2.X;

            int steps = (int)(x / fixed_offset);

            double interest_x = steps * fixed_offset;
            double delta_x = p2.X - p1.X;
            double offset = interest_x - p1.X;

            double interest_y = p1.Y;

            if (offset != 0)
            {
                double delta_y = p2.Y - p1.Y;
                interest_y = p1.Y + delta_y * offset / delta_x;
            }


            return new Point(interest_x, interest_y);
        }

        // Interpolate a point on a line between two points given the desired Y value of the point
        public static Point3D InterpolateFromY(Point3D p1, Point3D p2, double y)
        {
            if (p1.Y == y)
                return p1;
            else if (p2.Y == y)
                return p2;
            else
            {
                double delta = (p1.Y - y) / (p1.Y - p2.Y);
                double x = (p1.X - p2.X) * delta;
                double z = (p1.Z - p2.Z) * delta;

                return new Point3D(x, y, z);
            }
        }
        // Interpolate a point on a line between two points given the desired Z value of the point
        public static Point3D InterpolateFromZ(Point3D p1, Point3D p2, double z)
        {
            if (p1.Z == z)
                return p1;
            else if (p2.Z == z)
                return p2;
            else
            {
                double delta = (p1.Z - z) / (p1.Z - p2.Z);
                double x = (p1.X - p2.X) * delta;
                double y = (p1.Y - p2.Y) * delta;

                return new Point3D(x, y, z);
            }
        }
        public static bool SpansX(Point p1, Point p2, int fixed_offset)
        {
            double x1 = Math.Abs(p1.X);
            double x2 = Math.Abs(p2.X);

            if ((int)(x1 / fixed_offset) != (int)(x2 / fixed_offset)) return true;

            return false;
        }
    }
}
