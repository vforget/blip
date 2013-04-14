using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Blip
{
    /// <summary>
    /// Interaction logic for UserControl11.xaml
    /// </summary>
    public partial class UserControl11 : UserControl
    {
        UIParameters Up = new UIParameters();
        public UserControl11(UIParameters up)
        {
            InitializeComponent();
            Name = "UserControl10";
            Up = up;
        }

        private void UserControl11_Loaded(object sender, RoutedEventArgs e)
        {

        }
        public void RedirectToBrowser(object sender, RoutedEventArgs e)
        {
            string url = (sender as Hyperlink).NavigateUri.ToString(); ;
            Process.Start(new ProcessStartInfo(url));
            e.Handled = true;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string url = @"http://127.0.0.1:8085/blip.html?t=" + DateTime.Now.Ticks.ToString();
            Process.Start(new ProcessStartInfo(url));
            // e.Handled = true;
        }
    }
}
