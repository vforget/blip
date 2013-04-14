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
using System.IO;
using MBF;
using MBF.IO.Fasta;

namespace Blip
{
    /// <summary>
    /// Interaction logic for UserControl2.xaml
    /// </summary>
    public partial class UserControl2 : UserControl
    {
        UIParameters Up = new UIParameters();
        public bool validFile { get; set; }

        public UserControl2(UIParameters up)
        {
            InitializeComponent();
            Name = "UserControl2";
            Up = up;
            Up.FastaFile = "";

        }

        // Select a Fasta file using the File Browser.
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".*"; // Default file extension
            dlg.Filter = "Fasta File (*.fna, *.fa,*.fasta)|*.fna;*.fa;*.fasta| All files (*.*)|*.*"; // Filter files by extension
            
            Nullable<bool> result = dlg.ShowDialog();
            
            if ((result == true))
            {
                Up.FastaFile = dlg.FileName;
                fastaFileTextBox.Text = Up.FastaFile;
                Up.Log += "FASTA file of gene sequences: " + Up.FastaFile + "\n";
                RaiseRunCompletedEvent();
            }
            
        }

        // Contributed by Xin-Yi Chau
        // Raises event so the Next button can be enabled to proceed to the next step.
        public static readonly RoutedEvent RunCompletedEvent = 
            EventManager.RegisterRoutedEvent("RunCompleted", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(UserControl2));

        public event RoutedEventHandler RunCompleted
        {
            add { AddHandler(RunCompletedEvent, value); }
            remove { RemoveHandler(RunCompletedEvent, value); }
        }
        void RaiseRunCompletedEvent()
        {
            RoutedEventArgs args = new RoutedEventArgs(UserControl2.RunCompletedEvent);
            RaiseEvent(args);
        }
    }
}
