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
using System.IO;

namespace Blip
{
    /// <summary>
    /// Interaction logic for UserControl7.xaml
    /// </summary>
    public partial class UserControl7 : System.Windows.Controls.UserControl
    {
        private UIParameters Up = new UIParameters();
        
        public UserControl7(UIParameters up)
        {
            InitializeComponent();
            Up = up;
            Name = "UserControl7";
            
        }

        // Update location of the Pivot Collection
        private void UserControl7_Loaded(object sender, RoutedEventArgs e)
        {
            CollectionUrlBox.Text = Up.CollectionUrl;
        }

        // If available Lauch Pivot, otherwise provide link to get Pivot.
        private void LaunchPivotButton(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Up.PivotEXE))
            {
                Process pivot = new Process();
                pivot.StartInfo.FileName = Up.PivotEXE;
                pivot.StartInfo.Arguments = Up.CollectionUrl;
                pivot.Start();
            }
            else
            {
                Process browser = new Process();
                browser.StartInfo.FileName = @"http://www.getpivot.com";
                browser.Start();
            }

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string url = @"http://127.0.0.1:8085/blip.html?t=" + DateTime.Now.Ticks.ToString();
            ProcessStartInfo p = new ProcessStartInfo(url);
            Process.Start(p);
            //e.Handled = true;
        }
    }
}
