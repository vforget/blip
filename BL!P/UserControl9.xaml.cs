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
    /// Interaction logic for UserControl9.xaml
    /// </summary>
    public partial class UserControl9 : UserControl
    {
        private UIParameters Up = new UIParameters();
        public UserControl9(UIParameters up)
        {
            InitializeComponent();
            Up = up;
            Name = "UserControl9";
            
        }

        private void UserControl9_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
