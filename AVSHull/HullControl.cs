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
        private EditableHull m_editableHull;
        private double m_scale = 1.0;

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
                m_scale = 0.9 * Math.Min(finalSize.Width / bounds.Width, finalSize.Height / bounds.Height);
                return new Size(0.9 * finalSize.Width, 0.9 * finalSize.Height);
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
            
            Rect drawBounds = chines.GetRenderBounds(bulkheadPen);

            //double scale = 0.9 * Math.Min(ActualWidth/drawBounds.Width, ActualHeight/drawBounds.Height);
            // drawBounds.Width, Height
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
