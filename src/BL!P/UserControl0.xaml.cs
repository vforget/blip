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
    /// Interaction logic for UserControl0.xaml
    /// </summary>
    public partial class UserControl0 : UserControl
    {
        private UIParameters Up = new UIParameters();
        
        public UserControl0(UIParameters up)
        {
            InitializeComponent();
            Name = "UserControl0"; 
            Up = up;
        }

        public void RedirectToBrowser(object sender, RoutedEventArgs e) 
        {
            string url = (sender as Hyperlink).NavigateUri.ToString(); ;
            Process.Start(new ProcessStartInfo(url)); 
            e.Handled = true; 
        }

    }
}
