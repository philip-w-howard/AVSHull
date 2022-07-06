using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AVSHull
{
    /// <summary>
    /// Interaction logic for DesignControl.xaml
    /// </summary>
    public partial class DesignControl : UserControl
    {
        private HullLog undoLog;
        private HullLog redoLog;

        public DesignControl()
        {
            InitializeComponent();

            BaseHull.Instance().PropertyChanged += hull_PropertyChanged;

            undoLog = (HullLog)this.FindResource("UndoLog");
            undoLog.Clear();
            undoLog.Add(BaseHull.Instance());

            redoLog = (HullLog)this.FindResource("RedoLog");
            redoLog.Clear();

        }

        private void HullMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == FrontView)
            {
                PerspectiveView.Perspective = HullControl.PerspectiveType.FRONT;
                PerspectiveView.IsEditable = true;
            }
            else if (sender == TopView)
            {
                PerspectiveView.Perspective = HullControl.PerspectiveType.TOP;
                PerspectiveView.IsEditable = true;
            }
            else if (sender == SideView)
            {
                PerspectiveView.Perspective = HullControl.PerspectiveType.SIDE;
                PerspectiveView.IsEditable = true;
            }

            UpdateViews();
        }

        private void UpdateViews()
        {
            TopView.Perspective = HullControl.PerspectiveType.TOP;
            SideView.Perspective = HullControl.PerspectiveType.SIDE;
            FrontView.Perspective = HullControl.PerspectiveType.FRONT;

            // Need to invoke the setter to regenerate the hull.
            PerspectiveView.Perspective = PerspectiveView.Perspective;

            TopView.InvalidateVisual();
            FrontView.InvalidateVisual();
            SideView.InvalidateVisual();
            PerspectiveView.InvalidateVisual();
        }

        void hull_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine("DesignControl.PropertyChanged: " + e.PropertyName);
            if (e.PropertyName == "HullData" || e.PropertyName == "Bulkhead" || e.PropertyName == "HullScale" || e.PropertyName == "Bulkhead.Handle")
            {
                if (e.PropertyName != "Bulkhead.Handle") undoLog.StartSnapshot();
                undoLog.Add(BaseHull.Instance());
                redoLog.Clear();
                UpdateViews();
            }
        }

        public void openClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();

            openDlg.Filter = "AVS Hull files (*.avsh)|*.avsh|All files (*.*)|*.*";
            openDlg.FilterIndex = 0;
            openDlg.RestoreDirectory = true;

            Nullable<bool> result = openDlg.ShowDialog();
            if (result == true)
            {
                Hull tempHull;

                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Hull));

                using (Stream reader = new FileStream(openDlg.FileName, FileMode.Open))
                {
                    // Call the Deserialize method to restore the object's state.
                    tempHull = (Hull)serializer.Deserialize(reader);

                    // Make sure we have a timestamp
                    if (tempHull.Timestamp == DateTime.MinValue) tempHull.Timestamp = DateTime.Now;

                    BaseHull.Instance().Bulkheads = tempHull.Bulkheads;
                    
                    // Update to handle older .avsh files
                    BaseHull.Instance().Filename = openDlg.FileName;
                    BaseHull.Instance().SaveTimestamp = BaseHull.Instance().Timestamp;

                    undoLog.Clear();
                    undoLog.Add(BaseHull.Instance());

                    redoLog.Clear();

                    PerspectiveView.Perspective = HullControl.PerspectiveType.PERSPECTIVE;
                    UpdateViews();
                }
            }
        }

        public void saveClick(object sender, RoutedEventArgs e)
        {
            Save();
        }

        public void Save(string filename)
        {
            BaseHull.Instance().Filename = filename;
            BaseHull.Instance().SaveTimestamp = DateTime.Now;
            BaseHull.Instance().Timestamp = BaseHull.Instance().SaveTimestamp;

            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(Hull));

            using (FileStream output = new FileStream(BaseHull.Instance().Filename, FileMode.Create))
            {
                writer.Serialize(output, BaseHull.Instance());
            }
            undoLog.Snapshot();
            redoLog.Clear();
        }
        public void Save()
        {
            if (BaseHull.Instance() == null) return;

            if (BaseHull.Instance().Filename != null && BaseHull.Instance().Filename != "")
                Save(BaseHull.Instance().Filename);
            else
                SaveAs();
        }

        public void SaveAs()
        {
            if (BaseHull.Instance() == null) return;

            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.Filter = "AVS Hull files (*.avsh)|*.avsh|All files (*.*)|*.*";
            saveDlg.FilterIndex = 0;
            saveDlg.RestoreDirectory = true;

            BaseHull.Instance().SaveTimestamp = DateTime.Now;
            BaseHull.Instance().Timestamp = BaseHull.Instance().SaveTimestamp;

            Nullable<bool> result = saveDlg.ShowDialog();
            if (result == true)
            {
                Save(saveDlg.FileName);
            }
        }

        public void saveAsClick(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        public void importClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "Hull files (*.hul)|*.hul|All files (*.*)|*.*";
            openDlg.FilterIndex = 0;
            openDlg.RestoreDirectory = true;

            if (openDlg.ShowDialog() == true)
            {
                Hull tempHull = new Hull(openDlg.FileName);

                BaseHull.Instance().Bulkheads = tempHull.Bulkheads;

                PerspectiveView.Perspective = HullControl.PerspectiveType.PERSPECTIVE;

                undoLog.Clear();
                undoLog.Add(BaseHull.Instance());

                redoLog.Clear();
                UpdateViews();
            }
        }
        public void createClick(object sender, RoutedEventArgs e)
        {
            //Point loc = GetMousePosition();

            CreateHullDialog createHullDialog = new CreateHullDialog();
            createHullDialog.Owner = App.Current.MainWindow;
            //createHullDialog.Top = loc.Y;
            //createHullDialog.Left = loc.X;
            if (createHullDialog.ShowDialog() == true)
            {
                CreateHullData data = (CreateHullData)this.FindResource("CreateHullData");
                if (data != null)
                {
                    Hull tempHull = new Hull(data);
                    BaseHull.Instance().Bulkheads = tempHull.Bulkheads;

                    undoLog.Clear();
                    undoLog.Add(BaseHull.Instance());

                    redoLog.Clear();

                    UpdateViews();
                }
            }
        }
         public void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = undoLog.Count > 1;
        }

        public void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (undoLog.Count > 1)
            {
                redoLog.Add(undoLog.Pop());
                BaseHull.Instance().Bulkheads = undoLog.Peek().Bulkheads;
                UpdateViews();
            }
        }
        public void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = redoLog.Count > 0;
        }

        public void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (redoLog.Count > 0)
            {
                BaseHull.Instance().Bulkheads = redoLog.Pop().Bulkheads;
                undoLog.Add(BaseHull.Instance());
                undoLog.StartSnapshot();

                UpdateViews();
            }
        }

        public void ResizeClick(object sender, RoutedEventArgs e)
        {
            if (BaseHull.Instance().Bulkheads.Count == 0)
            {
                MessageBox.Show("Can't resize a non-existant hull.");
                return;
            }

            Resizer.Setup();
            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            values.ResizeExpanded = !values.ResizeExpanded;
        }
        private void ChangeChinesClick(object sender, RoutedEventArgs e)
        {
            if (BaseHull.Instance().Bulkheads.Count == 0)
            {
                MessageBox.Show("Can't change chines on a non-existant hull.");
                return;
            }

            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            values.NumChines = BaseHull.Instance().NumChines / 2;
            values.ChangeChinesExpanded = !values.LayoutSetupExpanded;
        }
        private void NewBulkheadClick(object sender, RoutedEventArgs e)
        {
            if (BaseHull.Instance().Bulkheads.Count == 0)
            {
                MessageBox.Show("Can't add bulkhead to a non-existant hull.");
                return;
            }

            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            values.NewBulkheadExpanded = !values.NewBulkheadExpanded;
        }

        private void WaterlinesClick(object sender, RoutedEventArgs e)
        {
            if (BaseHull.Instance().Bulkheads.Count == 0)
            {
                MessageBox.Show("Can't add bulkhead to a non-existant hull.");
                return;
            }

            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            values.WaterlinesExpanded = !values.WaterlinesExpanded;

        }
    }
}
