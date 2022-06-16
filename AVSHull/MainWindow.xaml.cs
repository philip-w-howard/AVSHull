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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            if (BaseHull.Instance().Timestamp != BaseHull.Instance().SaveTimestamp)
            {
                string text = "Do you want to save Hull changes?";
                string caption = "AVSH Hull file";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBoxResult result;
                result = MessageBox.Show(text, caption, button, icon, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    DesignWindow.Save();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }

            if (!e.Cancel && !LayoutWindow.IsSaved())
            {
                string text = "Do you want to save Panel changes?";
                string caption = "AVSH Panel file";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBoxResult result;
                result = MessageBox.Show(text, caption, button, icon, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    LayoutWindow.Save();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        public static RoutedCommand UndoCommand = new RoutedCommand();

        private void openClick(object sender, RoutedEventArgs e)
        {
            ATabItem tab = (ATabItem)MyTabs.SelectedItem;
            if (tab.TabName == "Design")
            {
                DesignWindow.openClick(sender, e);
            }
            else if (tab.TabName == "Panels")
            {
                LayoutWindow.openClick(sender, e);
            }
        }

        private void saveClick(object sender, RoutedEventArgs e)
        {
            ATabItem tab = (ATabItem)MyTabs.SelectedItem;
            if (tab.TabName == "Design")
            {
                DesignWindow.saveClick(sender, e);
            }
            else if (tab.TabName == "Panels")
            {
                LayoutWindow.saveClick(sender, e);
            }
        }
        private void saveAsClick(object sender, RoutedEventArgs e)
        {
            ATabItem tab = (ATabItem)MyTabs.SelectedItem;
            if (tab.TabName == "Design")
            {
                DesignWindow.saveAsClick(sender, e);
            }
            else if (tab.TabName == "Panels")
            {
                LayoutWindow.saveAsClick(sender, e);
            }
        }

        private void importClick(object sender, RoutedEventArgs e)
        {
            DesignWindow.importClick(sender, e);
        }
        private void exportClick(object sender, RoutedEventArgs e)
        {
            LayoutWindow.exportClick(sender, e);
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ATabItem tab = (ATabItem)MyTabs.SelectedItem;
            if (tab.TabName == "Design")
            {
                DesignWindow.Undo_CanExecute(sender, e);

            }
            else if (tab.TabName == "Panels")
            {
                LayoutWindow.LayoutControl.Undo_CanExecute(sender, e);
            }
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ATabItem tab = (ATabItem)MyTabs.SelectedItem;
            if (tab.TabName == "Design")
            {
                DesignWindow.Undo_Executed(sender, e);

            }
            else if (tab.TabName == "Panels")
            {
                LayoutWindow.LayoutControl.Undo_Executed(sender, e);
            }
        }
        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ATabItem tab = (ATabItem)MyTabs.SelectedItem;
            if (tab.TabName == "Design")
            {
                DesignWindow.Redo_CanExecute(sender, e);

            }
            else if (tab.TabName == "Panels")
            {
                LayoutWindow.LayoutControl.Redo_CanExecute(sender, e);
            }
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ATabItem tab = (ATabItem)MyTabs.SelectedItem;
            if (tab.TabName == "Design")
            {
                DesignWindow.Redo_Executed(sender, e);

            }
            else if (tab.TabName == "Panels")
            {
                LayoutWindow.LayoutControl.Redo_Executed(sender, e);
            }
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
