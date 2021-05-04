using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace AVSHull
{
    class PanelSplitter
    {
        public static PointCollection Line(Point start, Point end)
        {
            PointCollection splitter = new PointCollection();
            splitter.Add(start);
            splitter.Add(end);

            return splitter;
        }
        public static PointCollection SquareTongues(Point start, Point end, double height, double depth)
        {
            double verticalDir;
            double horizontalDir = 1;

            Point current;
            PointCollection splitter = new PointCollection();

            double panelHeight = Math.Abs(start.Y - end.Y);
            int tongues = (int)(panelHeight / height);
            double indent = (panelHeight - tongues * height) / 2;
            if (start.Y > end.Y)
                verticalDir = -1;
            else
                verticalDir = 1;

            splitter.Add(start);

            current = new Point(start.X, start.Y + verticalDir * indent);
            splitter.Add(current);

            current.X += horizontalDir * depth / 2;
            splitter.Add(current);
            horizontalDir *= -1;

            for (int ii = 0; ii < tongues - 1; ii++)
            {
                current.Y += verticalDir * height;
                splitter.Add(current);
                current.X += horizontalDir * depth;
                splitter.Add(current);
                horizontalDir *= -1;
            }
            current.Y += verticalDir * height;
            splitter.Add(current);
            current.X += horizontalDir * depth / 2;
            splitter.Add(current);
            horizontalDir *= -1;

            splitter.Add(end);

            return splitter;
        }
        public static PointCollection SquareEqualTongues(Point start, Point end, int numTongues, double depth)
        {
            double verticalDir;
            double horizontalDir = 1;

            Point current;
            PointCollection splitter = new PointCollection();

            double panelHeight = Math.Abs(start.Y - end.Y);
            double tongueHeight = panelHeight / (numTongues + 1); ;

            if (start.Y > end.Y)
                verticalDir = -1;
            else
                verticalDir = 1;

            splitter.Add(start);

            current = new Point(start.X, start.Y + verticalDir * tongueHeight / 2);
            splitter.Add(current);

            current.X += horizontalDir * depth / 2;
            splitter.Add(current);
            horizontalDir *= -1;

            for (int ii = 0; ii < numTongues - 1; ii++)
            {
                current.Y += verticalDir * tongueHeight;
                splitter.Add(current);
                current.X += horizontalDir * depth;
                splitter.Add(current);
                horizontalDir *= -1;
            }
            current.Y += verticalDir * tongueHeight;
            splitter.Add(current);
            current.X += horizontalDir * depth / 2;
            splitter.Add(current);
            horizontalDir *= -1;

            splitter.Add(end);

            return splitter;
        }

        private static double startHalfAngle(double verticalDir)
        {
            if (verticalDir == -1)
                return Math.PI;
            else
                return 0;
        }
        private static double endHalfAngle(double verticalDir)
        {
            if (verticalDir == -1)
                return 3 * Math.PI / 2;
            else
                return Math.PI / 2;
        }
        private static double startFullAngle(double verticalDir)
        {
            if (verticalDir == -1)
                return Math.PI / 2;
            else
                return 3 * Math.PI / 2;
        }

        private static double endFullAngle(double horizontalDir, double verticalDir)
        {
            double startAngle = startFullAngle(verticalDir);

            if (horizontalDir == 1)
                return startAngle + Math.PI;
            else
                return startAngle - Math.PI;
        }

        private static double startFinalAngle(double verticalDir)
        {
            if (verticalDir == -1)
                return Math.PI / 2;
            else
                return 3 * Math.PI / 2;
        }
        private static double endFinalAngle(double horizontalDir, double verticalDir)
        {
            double startAngle = startFinalAngle(verticalDir);

            if (horizontalDir == 1)
                return startAngle - Math.PI / 2;
            else
                return startAngle + Math.PI / 2;
        }

        public static PointCollection Tongues(Point start, Point end, int numTongues, double depth)
        {
            const int NUM_POINTS = 180;
            double verticalDir;
            double horizontalDir = 1;

            Point current;
            PointCollection splitter = new PointCollection();

            double panelHeight = Math.Abs(start.Y - end.Y);
            double radius = panelHeight / (numTongues + 1) / 2;

            if (start.Y > end.Y)
                verticalDir = -1;
            else
                verticalDir = 1;

            splitter.Add(start);
            GeometryOperations.CreateArc(splitter, radius, new Point(start.X + horizontalDir * radius, start.Y), startHalfAngle(verticalDir), endHalfAngle(verticalDir), NUM_POINTS / 2);

            current = new Point(start.X + horizontalDir * (depth / 2 - radius), start.Y + verticalDir * radius);
            splitter.Add(current);

            horizontalDir *= -1;

            for (int ii = 0; ii < numTongues - 1; ii++)
            {
                GeometryOperations.CreateArc(splitter, radius, new Point(current.X, current.Y + verticalDir * radius), startFullAngle(verticalDir), endFullAngle(horizontalDir, verticalDir), NUM_POINTS);
                current.Y += verticalDir * 2 * radius;
                //splitter.Add(current); //*******************
                current.X += horizontalDir * (depth - 2 * radius);
                //splitter.Add(current);
                horizontalDir *= -1;
            }

            GeometryOperations.CreateArc(splitter, radius, new Point(current.X, current.Y + verticalDir * radius), startFullAngle(verticalDir), endFullAngle(horizontalDir, verticalDir), NUM_POINTS);
            current.Y += verticalDir * 2 * radius;
            splitter.Add(current); //*******************
            current.X = end.X - horizontalDir * radius;
            //splitter.Add(current);

            GeometryOperations.CreateArc(splitter, radius, new Point(end.X - horizontalDir * radius, end.Y), startFinalAngle(verticalDir), endFinalAngle(horizontalDir, verticalDir), NUM_POINTS / 2);

            splitter.Add(end); //********************

            return splitter;
        }

    }
}
