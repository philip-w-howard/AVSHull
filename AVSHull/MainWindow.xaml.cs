using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace AVSHull
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Hull myHull;
        private HullLog undoLog;
        private HullLog redoLog;

        //public bool AllowBulkheadMoves;

        public MainWindow()
        {
            InitializeComponent();
            myHull = (Hull)this.FindResource("MyHull");
            myHull.PropertyChanged += hull_PropertyChanged;
            myHull.SetBulkheadHandler();

            undoLog = (HullLog)this.FindResource("UndoLog");
            undoLog.Clear();
            undoLog.Add(myHull);

            redoLog = (HullLog)this.FindResource("RedoLog");
            redoLog.Clear();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        public static RoutedCommand UndoCommand = new RoutedCommand();

        private void openClick(object sender, RoutedEventArgs e)
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
                    myHull.Bulkheads = tempHull.Bulkheads;
                    undoLog.Clear();
                    undoLog.Add(myHull);

                    redoLog.Clear();

                    PerspectiveView.perspective = HullControl.PerspectiveType.PERSPECTIVE;
                    PerspectiveView.IsEditable = false;
                    UpdateViews();

                    PanelsMenu.IsEnabled = true;
                    NumChines.Text = ((myHull.Bulkheads[0].NumChines)/2).ToString();
                }
            }
        }

        private void saveClick(object sender, RoutedEventArgs e)
        {
            if (myHull == null) return;

            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.Filter = "AVS Hull files (*.avsh)|*.avsh|All files (*.*)|*.*";
            saveDlg.FilterIndex = 0;
            saveDlg.RestoreDirectory = true;

            Nullable<bool> result = saveDlg.ShowDialog();
            if (result == true)
            {
                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(Hull));

                using (FileStream output = new FileStream(saveDlg.FileName, FileMode.Create))
                {
                    writer.Serialize(output, myHull);
                }
                undoLog.Snapshot();
                redoLog.Clear();
            }

        }

        private void importClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Hull files (*.hul)|*.hul|All files (*.*)|*.*";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                myHull.LoadFromHullFile(openFileDialog.FileName);

                NumChines.Text = ((myHull.Bulkheads[0].NumChines) / 2).ToString();
                undoLog.Clear();
                undoLog.Add(myHull);

                redoLog.Clear();
                UpdateViews();
            }
        }

        private void PanelsClick(object sender, RoutedEventArgs e)
        {
            PanelLayoutWindow layout = new PanelLayoutWindow(myHull);
            layout.Show();
        }

        private void ResizeClick(object sender, RoutedEventArgs e)
        {
            if (myHull.Bulkheads.Count == 0)
            {
                MessageBox.Show("Can't resize a non-existant hull.");
                return;
            }

            EditableHull hull = new EditableHull(myHull);

            Size3D originalSize = hull.GetSize();

            ResizeWindow resize = new ResizeWindow(hull);
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

                    myHull.Scale(scale_x, scale_y, scale_z);
                    UpdateViews();
                }
            }

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

        private int UpdateCount = 0;
        private void UpdateViews()
        {
            Debug.WriteLine("UpdateViews: {0}", ++UpdateCount);

            EditableHull topView = new EditableHull(myHull);
            topView.Rotate(0, 90, 90);
            TopView.editableHull = topView;
            TopView.perspective = HullControl.PerspectiveType.TOP;

            EditableHull sideView = new EditableHull(myHull);
            sideView.Rotate(0, 90, 180);
            SideView.editableHull = sideView;
            SideView.perspective = HullControl.PerspectiveType.SIDE;

            EditableHull frontView = new EditableHull(myHull);
            frontView.Rotate(0, 0, 180);
            FrontView.editableHull = frontView;
            FrontView.perspective = HullControl.PerspectiveType.FRONT;

            EditableHull perspectiveView = new EditableHull(myHull);
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
            if (e.PropertyName == "Bulkhead" || e.PropertyName == "HullData")
            {
                undoLog.Add(myHull);
                redoLog.Clear();
                UpdateViews();
            }
        }

        private void AboutClick(object sender, RoutedEventArgs e)
        {
            About setup = new About();
            setup.Owner = this;
            setup.ShowDialog();
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

        private void ChangeChinesClick(object sender, RoutedEventArgs e)
        {
            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            myHull.ChangeChines(values.NumChines);
        }

        private Point GetMousePosition()
        {
            // Position of the mouse relative to the window
            var position = Mouse.GetPosition(this);

            // Add the window position
            return new Point(position.X + this.Left, position.Y + this.Top);
        }
        private void createClick(object sender, RoutedEventArgs e)
        {

            Point loc = GetMousePosition();

            CreateHullDialog createHullDialog = new CreateHullDialog();
            createHullDialog.Top = loc.Y;
            createHullDialog.Left = loc.X;
            if (createHullDialog.ShowDialog() == true)
            {
                CreateHullData data = (CreateHullData)this.FindResource("CreateHullData");
                if (data != null)
                {
                    Hull tempHull = new Hull(data);
                    myHull.Bulkheads = tempHull.Bulkheads;

                    undoLog.Clear();
                    undoLog.Add(myHull);

                    redoLog.Clear();

                    UpdateViews();
                }
            }

        }

        private void InsertClick(object sender, RoutedEventArgs e)
        {
            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            PerspectiveView.InsertBulkhead(values.NewBulkheadLoc);
        }
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            PerspectiveView.DeleteSelectedBulkhead();
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = undoLog.Count > 1;
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (undoLog.Count > 1)
            {
                redoLog.Add(undoLog.Pop());
                myHull.Bulkheads = undoLog.Peek().Bulkheads;
                UpdateViews();
            }
        }
        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = redoLog.Count > 0;
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (redoLog.Count > 0)
            {
                myHull.Bulkheads = redoLog.Pop().Bulkheads;
                undoLog.Add(myHull);

                UpdateViews();
            }
        }
    }
}
