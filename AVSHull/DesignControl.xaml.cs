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
            PerspectiveView.IsEditable = false;

            if (sender == FrontView)
            {
                PerspectiveView.perspective = HullControl.PerspectiveType.FRONT;
            }
            else if (sender == TopView)
            {
                PerspectiveView.perspective = HullControl.PerspectiveType.TOP;
            }
            else if (sender == SideView)
            {
                PerspectiveView.perspective = HullControl.PerspectiveType.SIDE;
            }

            UpdateViews();
            PerspectiveView.IsEditable = true;
            PerspectiveView.InvalidateVisual();

        }

        private void RotateClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if ((string)button.Content == "+X")
                PerspectiveView.editableHull.Rotate(5, 0, 0);
            else if ((string)button.Content == "-X")
                PerspectiveView.editableHull.Rotate(-5, 0, 0);
            else if ((string)button.Content == "+Y")
                PerspectiveView.editableHull.Rotate(0, 5, 0);
            else if ((string)button.Content == "-Y")
                PerspectiveView.editableHull.Rotate(0, -5, 0);
            else if ((string)button.Content == "+Z")
                PerspectiveView.editableHull.Rotate(0, 0, 5);
            else if ((string)button.Content == "-Z")
                PerspectiveView.editableHull.Rotate(0, 0, -5);

            PerspectiveView.perspective = HullControl.PerspectiveType.PERSPECTIVE;
            PerspectiveView.InvalidateVisual();
        }

        private void UpdateViews()
        {
            EditableHull topView = new EditableHull();
            topView.Rotate(0, 90, 90);
            TopView.editableHull = topView;
            TopView.perspective = HullControl.PerspectiveType.TOP;

            EditableHull sideView = new EditableHull();
            sideView.Rotate(0, 90, 180);
            SideView.editableHull = sideView;
            SideView.perspective = HullControl.PerspectiveType.SIDE;

            EditableHull frontView = new EditableHull();
            frontView.Rotate(0, 0, 180);
            FrontView.editableHull = frontView;
            FrontView.perspective = HullControl.PerspectiveType.FRONT;

            EditableHull perspectiveView = new EditableHull();
            switch (PerspectiveView.perspective)
            {
                case HullControl.PerspectiveType.FRONT:
                    perspectiveView.Rotate(0, 0, 180);
                    break;
                case HullControl.PerspectiveType.TOP:
                    perspectiveView.Rotate(0, 90, 90);
                    break;
                case HullControl.PerspectiveType.SIDE:
                    perspectiveView.Rotate(0, 90, 180);
                    break;
                case HullControl.PerspectiveType.PERSPECTIVE:
                    perspectiveView.Rotate(10, 30, 190);
                    break;

            }
            PerspectiveView.editableHull = perspectiveView;

            TopView.InvalidateVisual();
            FrontView.InvalidateVisual();
            SideView.InvalidateVisual();
            PerspectiveView.InvalidateVisual();
        }

        void hull_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine("MainWindow.PropertyChanged: " + e.PropertyName);
            if (e.PropertyName == "HullData" || e.PropertyName == "Bulkhead" || e.PropertyName == "HullScale")
            {
                undoLog.Add(BaseHull.Instance());
                redoLog.Clear();
                UpdateViews();
            }
        }

        //private Version GetVersion()
        //{
        //    //if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
        //    //{
        //    //    return System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;
        //    //}
        //    //try
        //    //{
        //    //    return ApplicationDeployment.CurrentDeployment.CurrentVersion;
        //    //}
        //    //catch (Exception)
        //    {
        //        return Assembly.GetExecutingAssembly().GetName().Version;
        //    }
        //}

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
                    tempHull.CheckTransom();

                    // Make sure we have a timestamp
                    if (tempHull.Timestamp == DateTime.MinValue) tempHull.Timestamp = DateTime.Now;

                    BaseHull.Instance().Bulkheads = tempHull.Bulkheads;

                    undoLog.Clear();
                    undoLog.Add(BaseHull.Instance());

                    redoLog.Clear();

                    PerspectiveView.perspective = HullControl.PerspectiveType.PERSPECTIVE;
                    PerspectiveView.IsEditable = false;
                    UpdateViews();

                    NumChines.Text = ((BaseHull.Instance().Bulkheads[0].NumChines) / 2).ToString();
                }
            }
        }

        public void saveClick(object sender, RoutedEventArgs e)
        {
            if (BaseHull.Instance() == null) return;

            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.Filter = "AVS Hull files (*.avsh)|*.avsh|All files (*.*)|*.*";
            saveDlg.FilterIndex = 0;
            saveDlg.RestoreDirectory = true;

            Nullable<bool> result = saveDlg.ShowDialog();
            if (result == true)
            {
                BaseHull.Instance().Timestamp = DateTime.Now;

                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(Hull));

                using (FileStream output = new FileStream(saveDlg.FileName, FileMode.Create))
                {
                    writer.Serialize(output, BaseHull.Instance());
                }
                undoLog.Snapshot();
                redoLog.Clear();
            }

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

                PerspectiveView.perspective = HullControl.PerspectiveType.PERSPECTIVE;
                PerspectiveView.IsEditable = false;

                undoLog.Clear();
                undoLog.Add(BaseHull.Instance());

                redoLog.Clear();
                UpdateViews();

                NumChines.Text = ((BaseHull.Instance().Bulkheads[0].NumChines) / 2).ToString();
            }
        }
        public void createClick(object sender, RoutedEventArgs e)
        {
            //Point loc = GetMousePosition();

            CreateHullDialog createHullDialog = new CreateHullDialog();
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
        public void ResizeClick(object sender, RoutedEventArgs e)
        {
            if (BaseHull.Instance().Bulkheads.Count == 0)
            {
                MessageBox.Show("Can't resize a non-existant hull.");
                return;
            }

            EditableHull hull = new EditableHull();

            Size3D originalSize = hull.GetSize();

            ResizeWindow resize = new ResizeWindow();
            resize.ShowDialog();

            if (resize.OK)
            {
                ResizeWindowData resizeData = (ResizeWindowData)resize.FindResource("ResizeData");
                double scale_x = 1.0;
                double scale_y = 1.0;
                double scale_z = 1.0;

                if (resizeData != null)
                {
                    scale_x = resizeData.Width / originalSize.X;
                    scale_y = resizeData.Height / originalSize.Y;
                    scale_z = resizeData.Length / originalSize.Z;

                    BaseHull.Instance().Scale(scale_x, scale_y, scale_z);
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

                UpdateViews();
            }
        }
    }
}
