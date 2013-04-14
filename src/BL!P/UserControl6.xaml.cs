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

using System.Windows.Forms;

namespace Blip
{
    /// <summary>
    /// Interaction logic for UserControl6.xaml
    /// </summary>


    public partial class UserControl6 : System.Windows.Controls.UserControl
    {

        private UIParameters Up = new UIParameters();
        private string CollectionFileName;
        private string CollectionTitle;
        private string CollectionPath;
        private string CollectionImagePath;
        private string CollectionUrl;
        private string CollectionDeepzoomPath;

        public UserControl6(UIParameters up)
        {
            InitializeComponent();
            Name = "UserControl6";
            Up = up;
        }

        // Set Pivot collection name and path to default values
        private void UserControl6_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime now = DateTime.Now;
            //Set default values for UI fields
            CollectionFileName = "BLiP_" + now.Month.ToString() + now.Day.ToString() + now.Year.ToString();
            CollectionTitle = "BL!P -- Date created: " + now.ToShortDateString() + " " + now.ToShortTimeString();
            CollectionPath = Up.ProjectDir;

            //Set default values for non-UI fields
            CollectionUrl = @"file:///" + CollectionPath + "\\" + CollectionFileName + ".cxml";
            CollectionImagePath = CollectionPath + "\\" + CollectionFileName + "_images";
            CollectionDeepzoomPath = CollectionPath + "\\" + CollectionFileName + "_deepzoom";

            //Set values to the UI elements
            //CollectionNameBox.Text = CollectionFileName;
            CollectionTitleBox.Text = CollectionTitle;
            CollectionPathBox.Text = CollectionPath;

            //Update global UI parameters
            //Up.CollectionName = CollectionFileName;
            Up.CollectionName = "blip";
            Up.CollectionTitle = CollectionTitle;
            Up.CollectionPath = CollectionPath;
            Up.CollectionUrl = CollectionUrl;
            Up.CollectionImagePath = CollectionImagePath;
            Up.CollectionDeepzoomPath = CollectionDeepzoomPath;
        }

        // Set Pivot collection name and path to user-defined values
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new FolderBrowserDialog();
            DialogResult result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                //Get new values from the UI fields
                //CollectionFileName = CollectionNameBox.Text;
                CollectionPath = dlg.SelectedPath;
                CollectionTitle = CollectionTitleBox.Text;
                //Update dependant variables with new values from the UI fields.
                CollectionUrl = @"file:///" + CollectionPath + "\\" + CollectionFileName + ".cxml";
                CollectionImagePath = CollectionPath + "\\" + CollectionFileName + "_images";
                CollectionDeepzoomPath = CollectionPath + "\\" + CollectionFileName + "_deepzoom";
                CollectionPathBox.Text = CollectionPath;

                //Update the global UI parameters.
                //Up.CollectionName = CollectionFileName;
                Up.CollectionName = "blip";
                Up.CollectionTitle = CollectionTitle;
                Up.CollectionPath = CollectionPath;
                Up.CollectionUrl = CollectionUrl;
                Up.CollectionImagePath = CollectionImagePath;
                Up.CollectionDeepzoomPath = CollectionDeepzoomPath;
                
            }
        }
    }

    

}
