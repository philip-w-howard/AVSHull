using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private const double CLICK_WIDTH = 1.0;
        private const int NOT_SELECTED = -1;
        private double MIN_ROTATE_DRAG = 3;
        private double ROTATE_STEP = Math.PI / 180;
        private const double SCALE_FACTOR = 1.1;
        private Panel m_selectedPanel = null;
        private Point m_currentDragLoc = new Point(0, 0);
        private Point m_startDragLoc = new Point(-1, -1);
        private bool m_dragging = false;
        private bool m_doUnselect = false;

        public PanelLayout Layout { get; set; }

        PanelCtrlLog UndoLog;
        PanelCtrlLog RedoLog;

        public PanelLayoutControl()
        {
            InitializeComponent();
            Layout = new PanelLayout();
            UndoLog = new PanelCtrlLog();
            UndoLog.Add(Layout.Panels);
            UndoLog.StartSnapshot();
            RedoLog = new PanelCtrlLog();

            Layout.LayoutSetup.PropertyChanged += layout_PropertyChanged;
            Layout.PropertyChanged += layout_PropertyChanged;

            MouseWheel += OnMouseWheel;
            PreviewMouseDown += OnPreviewMouseDown;
            PreviewMouseMove += OnPreviewMouseMove;
            PreviewMouseUp += OnPreviewMouseUp;

            Layout.Scale = 1;
            Layout.WindowHeight = 400;
            Layout.WindowWidth = 600;
        }

        public void Clear()
        {
            Layout.Clear();

            // reset undo/redo logs
            UndoLog.Clear();
            UndoLog.Add(Layout.Panels);
            UndoLog.StartSnapshot();
            RedoLog.Clear();
            InvalidateVisual();
        }
        // Initialize layout from a file Open operation
        public void Load(List<Panel> panels, PanelsLayoutSetup setup)
        {
            Layout.Clear();

            foreach (Panel p in panels)
            {
                AddPanel(p);
            }
            
            Layout.LayoutSetup = setup;

            // reset undo/redo logs
            UndoLog.Clear();
            UndoLog.Add(Layout.Panels);
            UndoLog.StartSnapshot();
            RedoLog.Clear();
            InvalidateVisual();
        }

        void layout_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "WindowWidth":
                case "WindowHeight":
                case "SheetWidth":
                case "SheetHeight":
                case "SheetsHigh":
                case "SheetsWide":
                    RecomputeScale();
                    break;
                case "Scale":   // not RecomputeScale
                case "LayoutSetup":
                    InvalidateMeasure();
                    //InvalidateVisual();
                    break;
                case "Panels":
                case "PanelLayout.Panel":
                    UndoLog.Add(Layout.Panels);
                    Debug.WriteLine("PanelCtrl Log: {0}", UndoLog.Count);
                    InvalidateVisual();
                    break;

                default:
                    break;
            }
        }

        public void AddPanel(Panel p)
        {
            Layout.AddPanel(p);
            UndoLog.StartSnapshot();
            InvalidateVisual();
        }

        protected void RecomputeScale()
        {
            double horScale = Double.MaxValue;
            double vertScale = Double.MaxValue;
            double width = SCALE_FACTOR * Layout.SheetsWide * Layout.SheetWidth;
            double height = SCALE_FACTOR * Layout.SheetsHigh * Layout.SheetHeight;
            if (Layout.WindowWidth > 0) horScale = Layout.WindowWidth / width;
            if (Layout.WindowHeight > 0) vertScale = Layout.WindowHeight / height;
            Layout.Scale = Math.Min(horScale, vertScale);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double width = Layout.SheetsWide * Layout.SheetWidth * Layout.Scale;
            double height = Layout.SheetsHigh * Layout.SheetHeight * Layout.Scale;

            return new Size(width, height);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            double width = Layout.SheetsWide * Layout.SheetWidth * Layout.Scale;
            double height = Layout.SheetsHigh * Layout.SheetHeight * Layout.Scale;

            return new Size(width, height);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect background = new Rect(new Point(0, 0), new Point(ActualWidth, ActualHeight));
            drawingContext.DrawRectangle(this.Background, null, background);

            ScaleTransform scale = new ScaleTransform(Layout.Scale, Layout.Scale);

            Pen sheetPen = new Pen(System.Windows.Media.Brushes.Black, 1.0);

            for (int row=0; row< Layout.SheetsWide; row++)
            {
                for (int col=0; col< Layout.SheetsHigh; col++)
                {
                    double x = row * Layout.SheetWidth;
                    double y = col * Layout.SheetHeight;
                    Rect rect = new Rect(new Point(x, y), new Point(x + Layout.SheetWidth, y + Layout.SheetHeight));
                    RectangleGeometry sheet = new RectangleGeometry(rect);
                    sheet.Transform = scale;
                    drawingContext.DrawGeometry(this.Background, sheetPen, sheet);
                }
            }

            Pen panelPen = new Pen(System.Windows.Media.Brushes.Black, 1.0);
            Pen selectedPen = new Pen(System.Windows.Media.Brushes.Blue, 2.0);

            foreach (Panel p in Layout.Panels)
            {
                Geometry geom = p.GetGeometry();
                geom.Transform = scale;
                if (p == m_selectedPanel)
                    drawingContext.DrawGeometry(null, selectedPen, geom);
                else
                    drawingContext.DrawGeometry(null, panelPen, geom);
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                Layout.Scale *= 1.1;
            else if (e.Delta < 0)
                Layout.Scale /= 1.1;

            InvalidateVisual();
        }

        private Panel PanelClicked(Point loc)
        {
            //Pen pen = new Pen(Brushes.Black, CLICK_WIDTH);
            ScaleTransform scale = new ScaleTransform(Layout.Scale, Layout.Scale);

            int index = 0;

            //NOTE: Used to iterate in reverse direction
            foreach (Panel p in Layout.Panels)
            {
                Geometry geom = p.GetGeometry();
                geom.Transform = scale;

                if (geom.FillContains(loc))
                {
                    return p;
                }
                index++;
            }
            return null;
        }
        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point loc = e.GetPosition(this);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Panel panel = PanelClicked(loc);
                if (panel != null)
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
            }
            else if (e.RightButton == MouseButtonState.Pressed && m_selectedPanel != null)
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
            if (m_selectedPanel != null && m_doUnselect)
            {
                m_selectedPanel = null;
                InvalidateVisual();
            }
            m_dragging = false;
            m_doUnselect = false;

            UndoLog.Snapshot();
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point loc = e.GetPosition(this);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                m_doUnselect = false;

                if (m_dragging && m_selectedPanel != null)
                {
                    double deltaX = (loc.X - m_currentDragLoc.X) / Layout.Scale;
                    double deltaY = (loc.Y - m_currentDragLoc.Y) / Layout.Scale;
                    Point currLoc = m_selectedPanel.Origin;
                    currLoc.X += deltaX;
                    currLoc.Y += deltaY;
                    m_selectedPanel.Origin = currLoc;
                    m_currentDragLoc = loc;
                    InvalidateVisual();
                }
                else if (m_selectedPanel != null)
                {
                    // do rotation
                    double distance = loc.X - m_currentDragLoc.X;

                    if (Math.Abs(distance) > MIN_ROTATE_DRAG)
                    {
                        m_currentDragLoc = loc;

                        if (distance > 0)
                            m_selectedPanel.Rotate(ROTATE_STEP);

                        else
                            m_selectedPanel.Rotate(-ROTATE_STEP);

                        InvalidateVisual();
                    }
                }
            }
        }
        private void HorizontalFlipClick(object sender, RoutedEventArgs e)
        {
            if (m_selectedPanel != null)
            {
                m_selectedPanel.HorizontalFlip();
                InvalidateVisual();
            }
        }

        private void VerticalFlipClick(object sender, RoutedEventArgs e)
        {
            if (m_selectedPanel != null)
            {
                m_selectedPanel.VerticalFlip();
                InvalidateVisual();
            }

        }

        private void CopyClick(object sender, RoutedEventArgs e)
        {
            if (m_selectedPanel != null)
            {
                Panel p = (Panel)m_selectedPanel.Clone();
                Layout.AddPanel(p);
                m_selectedPanel = p;
                UndoLog.StartSnapshot();
                InvalidateVisual();
            }

        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (m_selectedPanel != null)
            {
                Layout.RemovePanel(m_selectedPanel);
                UndoLog.StartSnapshot();
                InvalidateVisual();
            }
        }

        private void SplitClick(object sender, RoutedEventArgs e)
        {
            if (m_selectedPanel != null)
            {
                PanelSplitSetup setup = new PanelSplitSetup();
                bool? result = setup.ShowDialog();

                if (result == true)
                {
                    Panel panel_1, panel_2;
                    PanelSplitSetupValues parameters = (PanelSplitSetupValues)Application.Current.FindResource("SplitSetup");
                    if (parameters == null) return;

                    if (m_selectedPanel.Split(parameters.Start, parameters.NumTongues, parameters.Depth, parameters.RoundEnds, out panel_1, out panel_2))
                    {
                        UndoLog.StartSnapshot();
                        Point origin = m_selectedPanel.Origin;
                        panel_1.Origin = origin;
                        origin.X += parameters.Start;
                        panel_2.Origin = origin;

                        Layout.RemovePanel(m_selectedPanel);
                        Layout.AddPanel(panel_1);
                        Layout.AddPanel(panel_2);
                        m_selectedPanel = null;
                        UndoLog.Snapshot();
                        InvalidateVisual();
                    }
                }
            }
        }
        public void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            Debug.WriteLine("Can undo? {0}", UndoLog.Count);
            e.CanExecute = UndoLog.Count > 1;
        }

        public void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("Undo {0}", UndoLog.Count);
            if (UndoLog.Count > 1)
            {
                RedoLog.Add(UndoLog.Pop());
                Layout.Panels = UndoLog.Peek();
                InvalidateVisual();
            }
        }
        public void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = RedoLog.Count > 0;
        }

        public void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (RedoLog.Count > 0)
            {
                Layout.Panels = RedoLog.Pop();
                UndoLog.Add(Layout.Panels);
                UndoLog.StartSnapshot();
                InvalidateVisual();
            }
        }

    }
}
