using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AVSHull
{
    public class HullControl : Control, INotifyPropertyChanged
    {
        public enum PerspectiveType { FRONT, TOP, SIDE, PERSPECTIVE };

        private EditableHull m_editableHull;
        private double m_scale = 1.0;
        public bool IsEditable = false;
        public PerspectiveType perspective = PerspectiveType.PERSPECTIVE;

        public HullControl()
        {
            m_editableHull = null;
        }

        public EditableHull editableHull
        {
            get { return m_editableHull; }
            set { m_editableHull = value; }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return availableSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (m_editableHull != null)
            {
                Geometry chines = m_editableHull.GetChineGeometry();
                Rect bounds = chines.Bounds;

                double scale_x = 0.9 * finalSize.Width / bounds.Width;
                double scale_y = 0.9 * finalSize.Height / bounds.Height;

                m_scale = Math.Min(scale_x, scale_y);

                return new Size(m_scale * bounds.Width, m_scale * bounds.Height);
            }
            else
            {
                return finalSize;
            }
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect background = new Rect(new Point(0, 0), new Point(ActualWidth, ActualHeight));
            drawingContext.DrawRectangle(this.Background, null, background);

            if (m_editableHull == null) return;

            Pen bulkheadPen = new Pen(System.Windows.Media.Brushes.Black, 1.0);

            Geometry bulkheads = m_editableHull.GetBulkheadGeometry();

            Pen chinePen = new Pen(System.Windows.Media.Brushes.Gray, 1.0);
            Geometry chines = m_editableHull.GetChineGeometry();
            
            ScaleTransform scaleXform = new ScaleTransform(m_scale, m_scale);
            
            bulkheads.Transform = scaleXform;
            chines.Transform = scaleXform;

            drawingContext.DrawGeometry(null, bulkheadPen, bulkheads);
            drawingContext.DrawGeometry(null, chinePen, chines);

            //DrawHandles(drawingContext);
        }

        //*******************************************************
        // INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
