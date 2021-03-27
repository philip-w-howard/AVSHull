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

        public HullControl()
        {
            m_editableHull = null;
        }

        public EditableHull editableHull
        {
            get { return m_editableHull; }
            set { m_editableHull = value; }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect background = new Rect(new Point(0, 0), new Point(ActualWidth, ActualHeight));
            drawingContext.DrawRectangle(this.Background, null, background);

            if (m_editableHull == null) return;

            Pen pen = new Pen(System.Windows.Media.Brushes.Black, 1.0);

            Geometry bulkheads = m_editableHull.GetBulkheadGeometry();
            drawingContext.DrawGeometry(null, pen, bulkheads);

            pen = new Pen(System.Windows.Media.Brushes.Gray, 1.0);
            Geometry chines = m_editableHull.GetChineGeometry();
            drawingContext.DrawGeometry(null, pen, chines);

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
