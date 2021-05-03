using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Hull myHull;

        //public bool AllowBulkheadMoves;

        public MainWindow()
        {
            InitializeComponent();
            myHull = new Hull();
            myHull.PropertyChanged += hull_PropertyChanged;

            Title = "AVS Hull";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void openClick(object sender, RoutedEventArgs e)
        {
            // destroy any previous hull
            myHull = null;

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
                    myHull = tempHull;
                    myHull.PropertyChanged += hull_PropertyChanged;
                    myHull.SetBulkheadHandler();

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

            PerspectiveView.InvalidateVisual();
        }

        private void UpdateViews()
        {
            EditableHull topView = new EditableHull(myHull);
            topView.Rotate(0, 90, 90);
            TopView.editableHull = topView;

            EditableHull sideView = new EditableHull(myHull);
            sideView.Rotate(0, 90, 180);
            SideView.editableHull = sideView;

            EditableHull frontView = new EditableHull(myHull);
            frontView.Rotate(0, 0, 180);
            FrontView.editableHull = frontView;

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
            Debug.WriteLine("PropertyChanged: " + e.PropertyName);
            if (e.PropertyName == "Bulkhead" || e.PropertyName == "HullData")
            {
                Debug.WriteLine("Update chines");
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

        private void MoveChecked(object sender, RoutedEventArgs e)
        {
            if (BulkheadMoveCheckbox.IsChecked == true)
                Debug.WriteLine("Allow moves");
            else
                Debug.WriteLine("Don't allow moves");

            UI_Params ui_params;
            ui_params = (UI_Params)this.FindResource("Curr_UI_Params");
            Debug.WriteLine("Resource: {0}", ui_params.AllowBulkheadMoves);
        }

        private void ChangeChinesClick(object sender, RoutedEventArgs e)
        {
            UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            myHull.ChangeChines(values.NumChines);
        }

        private void createClick(object sender, RoutedEventArgs e)
        {
            CreateHullDialog createHullDialog = new CreateHullDialog();

            if (createHullDialog.ShowDialog() == true)
            {
                CreateHullData data = (CreateHullData)this.FindResource("CreateHullData");
                if (data != null)
                {
                    myHull = new Hull(data);
                    myHull.PropertyChanged += hull_PropertyChanged;
                    myHull.SetBulkheadHandler();

                    UpdateViews();
                }
            }

        }
    }
}
