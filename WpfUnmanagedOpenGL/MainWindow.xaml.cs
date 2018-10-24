using System;
using System.Collections.Generic;
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

namespace WpfUnmanagedOpenGL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            var glHost = new OpenGlHost(BorderHost);
            glHost.Error += GlHostOnError;
            BorderHost.Child = glHost;
        }

        private void GlHostOnError(string message)
        {
            MessageBox.Show(this, message, "OpenGL Thread Error");
            Close();
        }
    }
}
