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

namespace Blip
{
    /// <summary>
    /// Interaction logic for UserControl10.xaml
    /// </summary>
    public partial class UserControl10 : UserControl
    {
        UIParameters Up = new UIParameters();
        public UserControl10(UIParameters up)
        {
            InitializeComponent();
            Name = "UserControl10";
            Up = up;
            Up.CxmlFile = "";
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".*"; // Default file extension
            dlg.Filter = "CXML File (*.cxml)|*.cxml| All files (*.*)|*.*"; // Filter files by extension

            Nullable<bool> result = dlg.ShowDialog();

            if ((result == true))
            {
                Up.CxmlFile = dlg.FileName;
                cxmlFileTextBox.Text = Up.CxmlFile;
                Up.CxmlDir = System.IO.Path.GetDirectoryName(Up.CxmlFile);
                Up.Log += "CXML file of gene sequences: " + Up.CxmlFile + "\n";
                RaiseRunCompletedEvent();
            }
        }
        
        // Contributed by Xin-Yi Chau
        // Raises event so the Next button can be enabled to proceed to the next step.
        public static readonly RoutedEvent RunCompletedEvent =
            EventManager.RegisterRoutedEvent("RunCompleted", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(UserControl10));

        public event RoutedEventHandler RunCompleted
        {
            add { AddHandler(RunCompletedEvent, value); }
            remove { RemoveHandler(RunCompletedEvent, value); }
        }
        void RaiseRunCompletedEvent()
        {
            RoutedEventArgs args = new RoutedEventArgs(UserControl10.RunCompletedEvent);
            RaiseEvent(args);
        }

        private void UserControl10_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
