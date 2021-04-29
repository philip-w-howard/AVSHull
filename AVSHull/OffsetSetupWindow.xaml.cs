﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AVSHull
{
    /// <summary>
    /// Interaction logic for OffsetSetupWindow.xaml
    /// </summary>
    public partial class OffsetSetupWindow : Window
    {
        public OffsetSetupWindow()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public String OutputType
        {
            get { return OutputType_Input.Text; }
        }
        public String SpacingStyle
        {
            get { return SpacingStyle_Input.Text; }
        }
        public String Spacing
        {
            get { return Spacing_Input.Text; }
        }
    }
}
