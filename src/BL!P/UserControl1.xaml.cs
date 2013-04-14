using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Forms;

namespace Blip
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : System.Windows.Controls.UserControl
    {
        private UIParameters Up = new UIParameters();
        
        public UserControl1(UIParameters up)
        {
            InitializeComponent();
            Name = "UserControl1";
            Up = up;
            Up.ProjectDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            projectDirTextBox.Text = Up.ProjectDir;
        }

        // Set project direcroty to default location
        private void UserControl1_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        // Set project direcroty using folder browser
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new FolderBrowserDialog();
            DialogResult result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                projectDirTextBox.Text = dlg.SelectedPath;
                Up.ProjectDir = projectDirTextBox.Text;
                //System.Windows.MessageBox.Show(String.Format("DIR: {0}", Up.ProjectDir));
            }
        }
    }
}
