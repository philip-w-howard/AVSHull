using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AVSHull
{
    /// <summary>
    /// Interaction logic for PanelLayoutControl.xaml
    /// </summary>
    public partial class PanelLayoutControl : UserControl
    {
        public class PanelLayoutSetup
        {
            public double WindowWidth { get; set; }
            public double WindowHeight { get; set; }
            public double SheetWidth { get; set; }
            public double SheetHeight { get; set; }
            public int SheetsWide { get; set; }
            public int SheetsHigh { get; set; }
            public double Scale { get; set; }
        }
        private const double CLICK_WIDTH = 1.0;
        private const int NOT_SELECTED = -1;
        private double MIN_ROTATE_DRAG = 3;
        private double ROTATE_STEP = Math.PI / 180;
        private const double SCALE_FACTOR = 1.1;
        private int m_selectedPanel = NOT_SELECTED;
        private Point m_currentDragLoc = new Point(0, 0);
        private Point m_startDragLoc = new Point(-1, -1);
        private bool m_dragging = false;
        private bool m_doUnselect = false;

        private PanelLayoutSetup m_panelSetup;
        public PanelLayoutSetup LayoutSetup 
        { get { return m_panelSetup; }
          set 
          { 
                m_panelSetup = value;
                InvalidateMeasure(); 
                InvalidateVisual();
          }
        }

        public double WindowWidth
        {
            get { return LayoutSetup.WindowWidth; }
            set
            {
                LayoutSetup.WindowWidth = value;
                RecomputeScale();
            }
        }
        public double WindowHeight 
        {
            get { return LayoutSetup.WindowHeight; }
            set
            {
                LayoutSetup.WindowHeight = value;
                RecomputeScale();
            }
        }

        public int SheetsWide 
        {
            get { return LayoutSetup.SheetsWide; }
            set { LayoutSetup.SheetsWide = value; RecomputeScale(); } 
        }

        public int SheetsHigh
        {
            get { return LayoutSetup.SheetsHigh; }
            set { LayoutSetup.SheetsHigh = value; RecomputeScale(); }
        }
        public double SheetWidth
        {
            get { return LayoutSetup.SheetWidth; }
            set { LayoutSetup.SheetWidth = value; RecomputeScale(); }
        }
        public double SheetHeight
        {
            get { return LayoutSetup.SheetHeight; }
            set { LayoutSetup.SheetHeight = value; RecomputeScale(); }
        }

        private List<Panel> m_panels;
        public List<Panel> Panels
        {
            get { return m_panels; }
            set
            {
                m_panels = value;
                InvalidateVisual();
            }
        }

        public double Scale
        {
            get { return LayoutSetup.Scale; }
            set
            {
                LayoutSetup.Scale = value;
                InvalidateMeasure();
                InvalidateVisual();
            }
        }

        public PanelLayoutControl()
        {
            InitializeComponent();
            m_panels = new List<Panel>();
            MouseWheel += OnMouseWheel;
            PreviewMouseDown += OnPreviewMouseDown;
            PreviewMouseMove += OnPreviewMouseMove;
            PreviewMouseUp += OnPreviewMouseUp;

            LayoutSetup = new PanelLayoutSetup();
            LayoutSetup.SheetHeight = 48;
            LayoutSetup.SheetWidth = 96;
            LayoutSetup.SheetsHigh = 1;
            LayoutSetup.SheetsWide = 1;
            LayoutSetup.Scale = 1;
            LayoutSetup.WindowHeight = 400;
            LayoutSetup.WindowWidth = 600;
        }

        public void AddPanel(Panel p)
        {
            m_panels.Add(p);
            InvalidateVisual();
        }

        protected void RecomputeScale()
        {
            double horScale = Double.MaxValue;
            double vertScale = Double.MaxValue;
            double width = SCALE_FACTOR * SheetsWide * SheetWidth;
            double height = SCALE_FACTOR * SheetsHigh * SheetHeight;
            if (WindowWidth > 0) horScale = WindowWidth / width;
            if (WindowHeight > 0) vertScale = WindowHeight / height;
            Scale = Math.Min(horScale, vertScale);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double width = SheetsWide * SheetWidth * Scale;
            double height = SheetsHigh * SheetHeight * Scale;

            return new Size(width, height);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            double width = SheetsWide * SheetWidth * Scale;
            double height = SheetsHigh * SheetHeight * Scale;

            return new Size(width, height);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect background = new Rect(new Point(0, 0), new Point(ActualWidth, ActualHeight));
            drawingContext.DrawRectangle(this.Background, null, background);

            ScaleTransform scale = new ScaleTransform(LayoutSetup.Scale, LayoutSetup.Scale);

            Pen sheetPen = new Pen(System.Windows.Media.Brushes.Black, 1.0);

            for (int row=0; row<SheetsWide; row++)
            {
                for (int col=0; col<SheetsHigh; col++)
                {
                    double x = row * SheetWidth;
                    double y = col * SheetHeight;
                    Rect rect = new Rect(new Point(x, y), new Point(x + SheetWidth, y + SheetHeight));
                    RectangleGeometry sheet = new RectangleGeometry(rect);
                    sheet.Transform = scale;
                    drawingContext.DrawGeometry(this.Background, sheetPen, sheet);
                }
            }

            Pen panelPen = new Pen(System.Windows.Media.Brushes.Black, 1.0);
            Pen selectedPen = new Pen(System.Windows.Media.Brushes.Blue, 2.0);

            for (int index=0; index < m_panels.Count; index++)
            {
                Geometry geom = m_panels[index].GetGeometry();
                geom.Transform = scale;
                if (index == m_selectedPanel)
                    drawingContext.DrawGeometry(null, selectedPen, geom);
                else
                    drawingContext.DrawGeometry(null, panelPen, geom);

            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                Scale *= 1.1;
            else if (e.Delta < 0)
                Scale /= 1.1;
            Debug.WriteLine("Delta: {0} Scale: {1}", e.Delta, LayoutSetup.Scale);
        }

        private int PanelClicked(Point loc)
        {
            //Pen pen = new Pen(Brushes.Black, CLICK_WIDTH);
            ScaleTransform scale = new ScaleTransform(LayoutSetup.Scale, LayoutSetup.Scale);

            for (int index=m_panels.Count-1; index >= 0; index--)
            {
                Geometry geom = m_panels[index].GetGeometry();
                geom.Transform = scale;

                Debug.WriteLine("Panel {0}: {1} loc: {2}", index, geom.Bounds, loc);
                if (geom.FillContains(loc))
                {
                    return index;
                }
            }
            return NOT_SELECTED;
        }
        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point loc = e.GetPosition(this);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                int panel = PanelClicked(loc);
                if (panel != NOT_SELECTED)
                {
                    m_selectedPanel = panel;
                    m_currentDragLoc = loc;
                    m_dragging = true;
                    InvalidateVisual();
                }
                else
                {
                    m_doUnselect = true;
                }
                Debug.WriteLine("Layout.MouseDown: {0} {1}", loc, m_selectedPanel);
            }
            else if (e.RightButton == MouseButtonState.Pressed && m_selectedPanel != NOT_SELECTED)
            {
                ContextMenu cm = this.FindResource("panelMenu") as ContextMenu;
                if (cm != null)
                {
                    cm.IsOpen = true;
                }
            }
        }
        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Point loc = e.GetPosition(this);
            if (m_selectedPanel != NOT_SELECTED && m_doUnselect)
            {
                m_selectedPanel = NOT_SELECTED;
                InvalidateVisual();
            }
            m_dragging = false;
            m_doUnselect = false;

        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point loc = e.GetPosition(this);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                m_doUnselect = false;

                if (m_dragging && m_selectedPanel != NOT_SELECTED)
                {
                    double deltaX = (loc.X - m_currentDragLoc.X) / LayoutSetup.Scale;
                    double deltaY = (loc.Y - m_currentDragLoc.Y) / LayoutSetup.Scale;
                    Point currLoc = m_panels[m_selectedPanel].Origin;
                    currLoc.X += deltaX;
                    currLoc.Y += deltaY;
                    m_panels[m_selectedPanel].Origin = currLoc;
                    m_currentDragLoc = loc;
                    InvalidateVisual();
                }
                else if (m_selectedPanel != NOT_SELECTED)
                {
                    // do rotation
                    double distance = loc.X - m_currentDragLoc.X;
                    Debug.WriteLine("Rotate: {0}", distance);

                    if (Math.Abs(distance) > MIN_ROTATE_DRAG)
                    {
                        m_currentDragLoc = loc;

                        if (distance > 0)
                            m_panels[m_selectedPanel].Rotate(ROTATE_STEP);

                        else
                            m_panels[m_selectedPanel].Rotate(-ROTATE_STEP);

                        InvalidateVisual();
                    }
                }
            }
        }
        private void HorizontalFlipClick(object sender, RoutedEventArgs e)
        {
            if (m_selectedPanel != NOT_SELECTED)
            {
                m_panels[m_selectedPanel].HorizontalFlip();
                InvalidateVisual();
            }
        }

        private void VerticalFlipClick(object sender, RoutedEventArgs e)
        {
            if (m_selectedPanel != NOT_SELECTED)
            {
                m_panels[m_selectedPanel].VerticalFlip();
                InvalidateVisual();
            }

        }

        private void CopyClick(object sender, RoutedEventArgs e)
        {
            if (m_selectedPanel != NOT_SELECTED)
            {
                m_panels.Add((Panel)m_panels[m_selectedPanel].Clone());
                m_selectedPanel = m_panels.Count - 1;
                InvalidateVisual();
            }

        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (m_selectedPanel != NOT_SELECTED)
            {
                m_panels.RemoveAt(m_selectedPanel);
                InvalidateVisual();
            }

        }

        private void SplitClick(object sender, RoutedEventArgs e)
        {

        }

    }
}
