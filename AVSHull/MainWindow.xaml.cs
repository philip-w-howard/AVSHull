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
        //public bool AllowBulkheadMoves;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        public static RoutedCommand UndoCommand = new RoutedCommand();

        private void openClick(object sender, RoutedEventArgs e)
        {
            DesignWindow.openClick(sender, e);
        }

        private void saveClick(object sender, RoutedEventArgs e)
        {
            DesignWindow.saveClick(sender, e);
        }

        private void importClick(object sender, RoutedEventArgs e)
        {
            DesignWindow.importClick(sender, e);
        }

        private void PanelsClick(object sender, RoutedEventArgs e)
        {
            PanelLayoutWindow layout = new PanelLayoutWindow();
            layout.Show();
        }

        private void ResizeClick(object sender, RoutedEventArgs e)
        {
            DesignWindow.ResizeClick(sender, e);
        }


        private void InsertClick(object sender, RoutedEventArgs e)
        {
            //UI_Params values = (UI_Params)this.FindResource("Curr_UI_Params");
            //PerspectiveView.InsertBulkhead(values.NewBulkheadLoc);
        }
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            //PerspectiveView.DeleteSelectedBulkhead();
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DesignWindow.Undo_CanExecute(sender, e);
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ATabItem tab = (ATabItem)MyTabs.SelectedItem;
            if (tab.TabName == "Design")
            {
                DesignWindow.Undo_Executed(sender, e);

            }
        }
        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DesignWindow.Redo_CanExecute(sender, e);
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DesignWindow.Redo_Executed(sender, e);
        }
        private void AboutClick(object sender, RoutedEventArgs e)
        {
            About setup = new About();
            setup.Owner = this;
            setup.ShowDialog();
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
            DesignWindow.createClick(sender, e);
        }

    }
}
