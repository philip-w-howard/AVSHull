using System.Reflection;
using System.Windows;

namespace AVSHull
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();

            Version.Content = Assembly.GetExecutingAssembly().GetName().Version;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
