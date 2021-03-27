using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Hull myHull;

        public MainWindow()
        {
            InitializeComponent();
            myHull = new Hull();
            myHull.PropertyChanged += hull_PropertyChanged;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void openClick(object sender, RoutedEventArgs e)
        {

        }

        private void saveClick(object sender, RoutedEventArgs e)
        {

        }

        private void importClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Hull files (*.hul)|*.hul|All files (*.*)|*.*";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                myHull.LoadFromHullFile(openFileDialog.FileName);

                UpdateViews();
            }

        }

        private void PanelsClick(object sender, RoutedEventArgs e)
        {

        }

        private void ResizeClick(object sender, RoutedEventArgs e)
        {

        }

        private void HullMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

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
            EditableHull topView = new EditableHull();
            topView.BaseHull = myHull;
            topView.Rotate(0, 90, 90);
            TopView.editableHull = topView;

            EditableHull sideView = new EditableHull();
            sideView.BaseHull = myHull;
            sideView.Rotate(0, 90, 180);
            SideView.editableHull = sideView;

            EditableHull frontView = new EditableHull();
            frontView.BaseHull = myHull;
            frontView.Rotate(0, 0, 180);
            FrontView.editableHull = frontView;

            // FIXTHIS: handle editable front, top, side
            EditableHull perspectiveView = new EditableHull();
            perspectiveView.BaseHull = myHull;
            perspectiveView.Rotate(10, 30, 190);
            PerspectiveView.editableHull = perspectiveView;

            TopView.InvalidateVisual();
            FrontView.InvalidateVisual();
            SideView.InvalidateVisual();
            PerspectiveView.InvalidateVisual();
        }

        void hull_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BulkheadData")
            {
                Debug.WriteLine("Update chines");
                UpdateViews();
            }
        }

    }
}
