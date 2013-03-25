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
using DropDownCustomColorPicker;
//using System.Drawing;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Media.Effects;


namespace Blip
{
    /// <summary>
    /// Interaction logic for UserControl8.xaml
    /// </summary>
    public partial class UserControl8 : UserControl
    {
        private UIParameters Up = new UIParameters();
        private bool FirstTimeLoaded = true;
        private FacetCategory CurrentFacetCategory { get; set; }
        private int CurrentItemIndex { get; set; }
        private FacetCategory CurrentBackgroundFacetCategory { get; set; }
        private string DefaultStatusLine = "";
        private Brushes ImagePreviewBackground { get; set; }
        private int MaxZIndex = Image.MaxZIndex;
        private int initialZIndex = Image.MaxZIndex;
        private int initialX = Image.Width/2;
        private int initialY = Image.Height/2;
        private int MinZIndex = Image.MinZIndex;
        private int MaxTextWidth = Image.Width;
        private IList<Color> BgColors { get; set; }
        private Point LastMouseDownPosition { get; set; }
        private float[,] bgScRgb = { 
{ 1f, 1f, 1f, 1f }, // 0
{ 1f, 1f, 0.75f, 0.5f },     // 1
{ 1f, 1f, 0.5f, 0f },    // 2
{ 1f, 1f, 1f, 0.6f },        // 3
{ 1f, 1f, 1f, 0.2f },      // 4
{ 1f, 0.7f, 1f, 0.55f },     // 5
{ 1f, 0.2f, 1f, 0f },      // 6
{ 1f, 0.65f, 0.93f, 1f },  // 7
{ 1f, 0.1f, 0.7f, 1f },    // 8
{ 1f, 0.8f, 0.75f, 1f },     // 9
{ 1f, 0.4f, 0.3f, 1f },    // 10
{ 1f, 1f, 0.6f, 0.75f },     // 11
{ 1f, 0.9f, 0.1f, 0.2f }}; // 12
        /*
        private float[,] bgScRgb = { 
{ 1f, 1f, 0.75f, 0.5f },     // 1
{ 0.7f, 0.8f, 0.5f, 0f },    // 2
{ 1f, 1f, 1f, 0.6f },        // 3
{ 1f, 1f, 1f, 0.2f },      // 4
{ 1f, 0.7f, 1f, 0.55f },     // 5
{ 0.7f, 0.2f, 1f, 0f },      // 6
{ 1f, 0.65f, 0.93f, 1f },  // 7
{ 0.7f, 0.1f, 0.7f, 1f },    // 8
{ 1f, 0.8f, 0.75f, 1f },     // 9
{ 0.7f, 0.4f, 0.3f, 1f },    // 10
{ 1f, 1f, 0.6f, 0.75f },     // 11
{ 0.7f, 0.9f, 0.1f, 0.2f }}; // 12
        */
        public UserControl8(UIParameters up)
        {
            InitializeComponent();
            this.KeyUp += new KeyEventHandler(FacetCategoryKeyUp);
            Up = up;
            Name = "UserControl8";
            // Create Colors from the RGB values.
            BgColors = new List<Color>();
            for (int i = 0; i < bgScRgb.GetLength(0); i++)
            {
                Color bgC = Color.FromScRgb(bgScRgb[i, 0], bgScRgb[i, 1], bgScRgb[i, 2], bgScRgb[i, 3]);
                BgColors.Add(bgC);
            }

            Up.BackgroundColorScheme = BgColors; // Set the scheme to the above colors.
            ForeColorPicker.SelectedColor += SetForeColor;
            //BackColorPicker.SelectedColor += SetBackColor;
            foreach (FacetCategory f in Up.FacetCategories)
            {
                if ((f.Name == "Species") || (f.Name == "Genus"))
                {
                    BackgroundFacet.Items.Add(f);
                }
                AvailableFacets.Items.Add(f);
            }
            
            // Set the scale factor for translating the coordinates from the final image to the preview image
            Image.ScaleFactor = ImagePreview.Width / Image.Width;
            ImagePreview.Background = Brushes.White; // Set the BG color to a default color.
            CurrentBackgroundFacetCategory = (FacetCategory)BackgroundFacet.SelectedItem; // Set the current BG FacetCat to the selected item.
            // Load FacetCategories for BG color that are of type String and are FilterVisible.
            CurrentItemIndex = 0; // Set the current item to the first one in the collection.
            CurrentItemTextBox.Text = "1";
            FontFamilyBox.ItemsSource = Fonts.SystemFontFamilies; // Load fonts into pulldown
        }
        
        private void SetForeColor(object sender, RoutedEventArgs args)
        {
            if (CurrentFacetCategory != null)
            {
                CurrentFacetCategory.tb.Foreground = ForeColorPicker.CurrentColor;
            }
        }
        /*
        private void SetBackColor(object sender, RoutedEventArgs args)
        {
            if (CurrentFacetCategory != null)
            {
                CurrentFacetCategory.tb.Background = BackColorPicker.CurrentColor;
            }
        }
        */

        /// <summary>
        /// Runs when the UI is loaded (i.e. visible).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl8_Loaded(object sender, RoutedEventArgs e)
        {

            AvailableFacets.Focus();
            TotalItemCount.Text = "of " + Up.Collection.Items.Count();
            SetImagePreviewBackground(); // Set the background to the currently selected FacetCat. 
            FacetCategory rank = new FacetCategory();
            FacetCategory gene = new FacetCategory();
            FacetCategory queryName = new FacetCategory();
            FacetCategory species = new FacetCategory();
            FacetCategory product = new FacetCategory();
            FacetCategory identity = new FacetCategory();
            if (FirstTimeLoaded)
            {
                foreach (FacetCategory fc in Up.FacetCategories)
                {
                    
                    switch (fc.Name)
                    {
                        //case "Rank":
                        //    rank = fc;
                        //    break;
                        case "Gene":
                            gene = fc;
                            break;
                        case "QueryName":
                            queryName = fc;
                            break;
                        case "Species":
                            species = fc;
                            break;
                        case "Product":
                            product = fc;
                            break;
                        case "Identity":
                            identity = fc;
                            break;
                        
                        default:
                            break;
                    }
                }
                //rank.IsSelected = true;
                //SelectedFacets.Items.Add(rank);
                //AvailableFacets.Items.Remove(rank);
                //LoadItemInPreview(rank,      Color.FromArgb(255, 150, 150, 150), 150, -10, MinZIndex, 192);
                
                gene.IsSelected = true;
                SelectedFacets.Items.Add(gene);
                AvailableFacets.Items.Remove(gene);
                CurrentFacetCategory = gene;
                StatusBlock.Text = "Editing " + gene.DisplayName;
                LoadItemInPreview(gene,      Color.FromArgb(255, 0, 165, 0), 120, 120, MaxZIndex, 72);
                LoadItemInPreview(species,   Color.FromArgb(255, 165, 0, 0), 100, 275, MaxZIndex, 18);
                LoadItemInPreview(product, Color.FromArgb(255, 0, 0, 0), 20, 420, MaxZIndex, 10);
                LoadItemInPreview(queryName, Color.FromArgb(255, 0, 0, 0), 10, 10, MaxZIndex, 14);
                LoadItemInPreview(identity, Color.FromArgb(255, 0, 0, 0), 380, 10, MaxZIndex, 24);
            }
        }

        private void LoadItemInPreview(FacetCategory fc, Color c, double x, double y, int z, double fontSize)
        {
            fc.tb.FontSize = fontSize;
            SolidColorBrush b = new SolidColorBrush(c);
            fc.tb.Foreground = b;
            fc.tb.SetValue(Canvas.LeftProperty, x * Image.ScaleFactor);
            fc.tb.SetValue(Canvas.TopProperty, y * Image.ScaleFactor);
            fc.tb.SetValue(Canvas.ZIndexProperty, z);
            AddFacetCategoryToImage2(fc);
            AvailableFacets.Items.Remove(fc);
        }

        private void AddFacetCategoryToImage2(FacetCategory f)
        {
            // Load item data of the currently viewed item
            if (ImagePreview.Children.Contains(f.tb))
            {
                //MessageBox.Show("Image already contains this category.");
                return;
            }

            Item item = Up.Collection.Items[CurrentItemIndex];
            // Set the name of the image preview item and it's value to that of the facet.
            f.tb.Name = f.Name;
            foreach (Facet facet in item.Facets)
            {
                if (facet.Name == f.Name)
                {
                    if (facet.Type == "DateTime")
                    {
                        try
                        {
                            DateTime dt = new DateTime();
                            dt = DateTime.ParseExact(facet[0].Value, "o", null);
                            f.tb.Text = dt.ToString("dd-MMM-yyyy");

                        }
                        catch
                        {
                            f.tb.Text = "N/A";
                        }
                    }
                    else
                    {
                        f.tb.Text = facet[0].Value;
                    }
                }
            }

            // Set the various text attributes.
            //f.tb.FontSize = GetFontSize();
            //f.tb.FontFamily = GetFontFamily();
            //f.tb.Foreground = ForeColorPicker.CurrentColor;
            //f.tb.SetValue(Canvas.LeftProperty, initialX * Image.ScaleFactor);
            //f.tb.SetValue(Canvas.TopProperty, initialY * Image.ScaleFactor);
            //f.tb.SetValue(Canvas.ZIndexProperty, initialZIndex);
            

            f.tb.Background = GetFontColor(Color.FromArgb(0, 0, 0, 0));
            f.tb.FontWeight = ToggleBold();
            f.tb.FontStyle = ToggleItalic();
            f.tb.Width = GetTextWidth(f.tb);
            f.tb.Height = GetTextHeight(f.tb);
            // Set the sliders to the location of the item
            // Add event handlers for mouse over and out events.
            f.tb.MouseEnter += new MouseEventHandler(FacetCategoryMouseEnter);
            f.tb.MouseLeftButtonDown += new MouseButtonEventHandler(FacetCategoryMouseLeftButtonDown);
            f.tb.MouseLeftButtonUp += new MouseButtonEventHandler(FacetCategoryMouseLeftButtonUp);
            f.tb.MouseLeave += new MouseEventHandler(FacetCategoryMouseLeave);
            
            f.tb.Style = App.Current.Resources["DraggableObject"] as Style;

            // Add the item to the image preview and setup other UI elements.

            SetBlur(f.tb);
            ImagePreview.Children.Add(f.tb);

            if (CurrentFacetCategory != null)
            {
                int j = ImagePreview.Children.IndexOf(CurrentFacetCategory.tb);
                TextBlock tb2 = (TextBlock)ImagePreview.Children[j];
                RemoveBlur(tb2);
            }

            CurrentFacetCategory = f;
            StatusBlock.Text = "Editing " + CurrentFacetCategory.DisplayName;
            //Xslider.Value = initialX * Image.ScaleFactor;
            //Yslider.Value = initialY * Image.ScaleFactor;
            SetImagePreviewBackground();
        }
        

        /// <summary>
        /// Save the state of how the items appear in the preview to the properties in each FacetCategory
        /// </summary>
        public void SaveImagePreviewState()
        {
            foreach (UIElement element in ImagePreview.Children)
            {
                TextBlock tb = (TextBlock)element;
                
                if (tb.Name != "Watermark")
                 {
                     foreach (FacetCategory f in Up.FacetCategories){
                         if (f.Name == tb.Name){
                             f.IsSelected = true;
                             f.FontFamily = tb.FontFamily;
                             f.FontSize = (float)tb.FontSize;
                             f.FontWeight = tb.FontWeight;
                             f.FontStyle = tb.FontStyle;
                             f.FontForegroundColor = (tb.Foreground as SolidColorBrush).Color;
                             f.FontBackgroundColor = (tb.Background as SolidColorBrush).Color;
                             f.X = (double)tb.GetValue(Canvas.LeftProperty);
                             f.Y = (double)tb.GetValue(Canvas.TopProperty);
                             f.Z = (int)tb.GetValue(Canvas.ZIndexProperty);
                         }
                     }
                 }
            }
        }

        /// <summary>
        /// Adds a FacetCategory to the image preview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddFacetToImage_Click(object sender, RoutedEventArgs e)
        {
            Object o = AvailableFacets.SelectedItem;
            if (o == null)
            {
                //MessageBox.Show("To add a facet category select it from the Available Categories.");
            }
            else
            {
                FacetCategory f = (FacetCategory)o;
                f.IsSelected = true;
                SelectedFacets.Items.Add(f);
                AvailableFacets.Items.Remove(f);
                AddFacetCategoryToImage(f);
            }
        }

        /// <summary>
        /// Delete a facet from the image preview
        /// </summary>
        
        private void DeleteFacetFromImage_Click(object sender, RoutedEventArgs e)
        {
            Object o = SelectedFacets.SelectedItem;
            if (o == null)
            {
                MessageBox.Show("To remove a facet category select it from the Image Facets.");
            }
            else
            {
                FacetCategory f = (FacetCategory)SelectedFacets.SelectedItem;
                DeleteFacetCategory(f);
                //Xslider.Value = initialX * Image.ScaleFactor;
                //Yslider.Value = initialY * Image.ScaleFactor;
            }
        }

        private void DeleteFacetCategory(FacetCategory f)
        {
            f.IsSelected = false;
            AvailableFacets.Items.Add(f);
            SelectedFacets.Items.Remove(f);
            ImagePreview.Children.Remove(CurrentFacetCategory.tb);
            StatusBlock.Text = DefaultStatusLine;
            CurrentFacetCategory = null;
        }

        /// <summary>
        /// Does the grunt work of adding the FacetCategory to the image preview.
        /// </summary>
        /// <param name="f"></param>
        private void AddFacetCategoryToImage(FacetCategory f)
        {
            // Load item data of the currently viewed item
            if (ImagePreview.Children.Contains(f.tb))
            {
                //MessageBox.Show("Image already contains this category.");
                return;
            }

            Item item = Up.Collection.Items[CurrentItemIndex];
            // Set the name of the image preview item and it's value to that of the facet.
            f.tb.Name = f.Name;
            foreach (Facet facet in item.Facets)
            {
                if (facet.Name == f.Name)
                {
                    if (facet.Type == "DateTime")
                    {
                        try
                        {
                            DateTime dt = new DateTime();
                            dt = DateTime.ParseExact(facet[0].Value, "o", null);
                            f.tb.Text = dt.ToString("dd-MMM-yyyy");

                        }
                        catch
                        {
                            f.tb.Text = "N/A";
                        }
                    }
                    else
                    {
                        f.tb.Text = facet[0].Value;
                    }
                }
            }

            // Set the various text attributes.
            f.tb.FontSize = GetFontSize();
            f.tb.FontWeight = ToggleBold();
            f.tb.FontStyle = ToggleItalic();
            f.tb.FontFamily = GetFontFamily();
            f.tb.Foreground = ForeColorPicker.CurrentColor;
            f.tb.Background = GetFontColor(Color.FromArgb(0, 0, 0, 0));
            //f.tb.Background = BackColorPicker.CurrentColor;
            
            //f.tb.Foreground = GetFontColor(customForegroundCP.SelectedColor);
            //f.tb.Background = GetFontColor(customBackgroundCP.SelectedColor);
            
            f.tb.Width = GetTextWidth(f.tb);
            f.tb.Height = GetTextHeight(f.tb);

            // Set the sliders to the location of the item
            f.tb.SetValue(Canvas.LeftProperty, initialX * Image.ScaleFactor);
            f.tb.SetValue(Canvas.TopProperty, initialY * Image.ScaleFactor);
            f.tb.SetValue(Canvas.ZIndexProperty, initialZIndex);
            // Add event handlers for mouse over and out events.

            
            f.tb.MouseEnter += new MouseEventHandler(FacetCategoryMouseEnter);
            f.tb.MouseLeftButtonDown += new MouseButtonEventHandler(FacetCategoryMouseLeftButtonDown);
            f.tb.MouseLeftButtonUp += new MouseButtonEventHandler(FacetCategoryMouseLeftButtonUp);
            f.tb.MouseLeave += new MouseEventHandler(FacetCategoryMouseLeave);
            f.tb.Style = App.Current.Resources["DraggableObject"] as Style;

            // Add the item to the image preview and setup other UI elements.

            SetBlur(f.tb);
            ImagePreview.Children.Add(f.tb);
            
            if (CurrentFacetCategory != null)
            {
                int j = ImagePreview.Children.IndexOf(CurrentFacetCategory.tb);
                TextBlock tb2 = (TextBlock)ImagePreview.Children[j];
                RemoveBlur(tb2);
            }

            CurrentFacetCategory = f;
            StatusBlock.Text = "Editing " + CurrentFacetCategory.DisplayName;
            //Xslider.Value = initialX * Image.ScaleFactor;
            //Yslider.Value = initialY * Image.ScaleFactor;
            SetImagePreviewBackground();
        }

        public void FacetCategoryKeyUp(object sender, KeyEventArgs args)
        {
            if (CurrentFacetCategory != null)
            {
                if ((args.Key == Key.Back) || (args.Key == Key.Delete))
                {
                    DeleteFacetCategory(CurrentFacetCategory);
                }
            }
        }
        public void FacetCategoryKeyUp2(object sender, KeyEventArgs args)
        {
            TextBlock tb = (TextBlock)sender;
            foreach (FacetCategory fc in Up.FacetCategories)
            {
                if (fc.Name == tb.Name)
                {
                    if (CurrentFacetCategory != null)
                    {
                        if ((args.Key == Key.Back) || (args.Key == Key.Delete))
                        {
                            DeleteFacetCategory(CurrentFacetCategory);
                        }
                    }
                }
            }

        }

        public void FacetCategoryMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            TextBlock tb = (TextBlock)sender;
            LastMouseDownPosition = args.GetPosition(ImagePreview);
        }

        public void FacetCategoryMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
        {
            
                Point MouseUpPosition = args.GetPosition(ImagePreview);
                if ((MouseUpPosition.X == LastMouseDownPosition.X) && (MouseUpPosition.Y == LastMouseDownPosition.Y))
                {
                    TextBlock tb = (TextBlock)sender;
                    foreach (FacetCategory fc in Up.FacetCategories)
                    {
                        if (fc.Name == tb.Name)
                        {
                            if (CurrentFacetCategory != null)
                            {
                                RemoveBlur(CurrentFacetCategory.tb);
                            }
                            CurrentFacetCategory = fc;
                            SetBlur(CurrentFacetCategory.tb);
                            StatusBlock.Text = "Editing " + CurrentFacetCategory.DisplayName;
                        }
                    }
                }
            
        }

        public void FacetCategoryMouseEnter(object sender, RoutedEventArgs args)
        {
            TextBlock tb = (TextBlock)sender;
            StatusBlock.Text = "Mouse is over " + tb.Name;
        }

        public void FacetCategoryMouseLeave(object sender, RoutedEventArgs args)
        {
            try
            {
                try
                {
                    StatusBlock.Text = "Editing " + CurrentFacetCategory.DisplayName;
                }
                catch
                {
                    StatusBlock.Text = "Editing " + CurrentFacetCategory.Name;
                }
            }
            catch
            {
                StatusBlock.Text = "";
            }
        }

        /// <summary>
        /// When the user selects another image facet change the status text, and move the sliders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        private void SelectedFacets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Object o = SelectedFacets.SelectedItem;
            if (o != null)
            {
                if (CurrentFacetCategory != null)
                {
                    int j = ImagePreview.Children.IndexOf(CurrentFacetCategory.tb);
                    TextBlock tb2 = (TextBlock)ImagePreview.Children[j];
                    RemoveBlur(tb2);

                }
                
                CurrentFacetCategory = (FacetCategory)o;
                StatusBlock.Text = "Editing " + CurrentFacetCategory.DisplayName;
                int i = ImagePreview.Children.IndexOf(CurrentFacetCategory.tb);
                TextBlock tb = (TextBlock)ImagePreview.Children[i];
                //Xslider.Value = (double)tb.GetValue(Canvas.LeftProperty);
                //Yslider.Value = ImagePreview.Height - (double)tb.GetValue(Canvas.TopProperty);
                SetBlur(tb);
                
            }
        }

        private void RemoveBlur(TextBlock tb)
        {
            DropShadowEffect ds = new DropShadowEffect();

            ds.Direction = 0;
            ds.ShadowDepth = 0;
            ds.BlurRadius = 0;
            ds.Color = Colors.Red;
            ds.Opacity = 0;
            tb.Effect = ds;
        }

        private void SetBlur(TextBlock tb)
        {
            DropShadowEffect ds = new DropShadowEffect();
            
            ds.Direction = 0;
            ds.ShadowDepth = 0;
            ds.BlurRadius = 10;
            ds.Color = Colors.Red;
            ds.Opacity = 255;
            tb.Effect = ds;
        }
        
        private void Xslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CurrentFacetCategory != null)
            {
                //CurrentFacetCategory.tb.SetValue(Canvas.LeftProperty, Xslider.Value);
            }
        }

        private void Yslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CurrentFacetCategory != null)
            {
                //CurrentFacetCategory.tb.SetValue(Canvas.TopProperty, ImagePreview.Height - Yslider.Value);
            }
        }

        /*
         * FONT-RELATED PROPERTIES
         */ 

        private void FontFamilyBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentFacetCategory != null)
            {
                CurrentFacetCategory.tb.FontFamily = GetFontFamily();
                CurrentFacetCategory.tb.Width = GetTextWidth(CurrentFacetCategory.tb);
                CurrentFacetCategory.tb.Height = GetTextHeight(CurrentFacetCategory.tb);
            }
        }

        private void FontSizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentFacetCategory != null)
            {
                CurrentFacetCategory.tb.FontSize = GetFontSize();
                CurrentFacetCategory.tb.Width = GetTextWidth(CurrentFacetCategory.tb);
                CurrentFacetCategory.tb.Height = GetTextHeight(CurrentFacetCategory.tb);
            }
        }

        /*
        private void customForegroundCP_Loaded(object sender, RoutedEventArgs e)
        {
            byte A = 255;
            byte R = 0;
            byte G = 0;
            byte B = 0;
            customForegroundCP.SelectedColor = Color.FromArgb(A, R, G, B);
        }

        
        private void customBackgroundCP_Loaded(object sender, RoutedEventArgs e)
        {
            byte A = 0;
            byte R = 255;
            byte G = 255;
            byte B = 255;
            customBackgroundCP.SelectedColor = Color.FromArgb(A, R, G, B);
        }
        */
        public void customForegroundCP_SelectionChanged(Color c)
        {
            if (CurrentFacetCategory != null)
            {
                CurrentFacetCategory.tb.Foreground = GetFontColor(c);
            }
        }

        public void customBackgroundCP_SelectionChanged(Color c)
        {
            if (CurrentFacetCategory != null)
            {
                CurrentFacetCategory.tb.Background = GetFontColor(c);
            }
        }

        private void IsBoldToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (CurrentFacetCategory != null){
                CurrentFacetCategory.tb.FontWeight = ToggleBold();
                CurrentFacetCategory.tb.Height = GetTextHeight(CurrentFacetCategory.tb);
                CurrentFacetCategory.tb.Width = GetTextWidth(CurrentFacetCategory.tb);
            }
        }

        private void IsItalicToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (CurrentFacetCategory != null)
            {
                CurrentFacetCategory.tb.FontStyle = ToggleItalic();   
            }
        }

        private System.Windows.FontStyle ToggleItalic(){
            if (IsItalicToggle.IsChecked == true)
            {
                return FontStyles.Italic;
            }
            else
            {
                return FontStyles.Normal;
            }
        }

        private System.Windows.FontWeight ToggleBold()
        {
            
            if (IsBoldToggle.IsChecked == true)
            {
                return FontWeights.Bold;
            }
            else
            {
                return FontWeights.Regular;
            }
        }

        private double GetFontSize()
        {
            ComboBoxItem o = (ComboBoxItem)FontSizeBox.SelectedItem;
            return Convert.ToDouble(o.Content);
        }

        private FontFamily GetFontFamily()
        {
            return (FontFamily)FontFamilyBox.SelectedItem;
            //ComboBoxItem o = (ComboBoxItem)FontFamilyBox.SelectedItem;
            //return new FontFamily(o.Content.ToString());
        }

        private Typeface GetTypeface()
        {
            ComboBoxItem o = (ComboBoxItem)FontFamilyBox.SelectedItem;
            return new Typeface(o.Content.ToString());
        }

        public SolidColorBrush GetFontColor(Color c)
        {
            SolidColorBrush b = new SolidColorBrush(c);
            return b;
        }

        
        //
        // Send the item forward or backward in the image preview.
        //
        private void SendBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFacetCategory != null)
            {
                int oldZIndex = (int)CurrentFacetCategory.tb.GetValue(Canvas.ZIndexProperty);
                if (oldZIndex == MinZIndex)
                {
                    MessageBox.Show("Cannot send backward any further.");
                }
                else
                {
                    CurrentFacetCategory.tb.SetValue(Canvas.ZIndexProperty, (int)CurrentFacetCategory.tb.GetValue(Canvas.ZIndexProperty) - 1);
                }
            }
        }

        private void BringForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFacetCategory != null)
            {
                int oldZIndex = (int)CurrentFacetCategory.tb.GetValue(Canvas.ZIndexProperty);
                if (oldZIndex == MaxZIndex)
                {
                    MessageBox.Show("Cannot bring forward any further.");
                }
                else
                {
                    CurrentFacetCategory.tb.SetValue(Canvas.ZIndexProperty, oldZIndex + 1);
                }
            }
        }


        // Get the width of the text element.
        private double GetTextWidth(TextBlock tb)
        {
            FormattedText ft = new FormattedText(tb.Text,
                                                    CultureInfo.GetCultureInfo("en-us"),
                                                    FlowDirection.LeftToRight,
                                                    new Typeface(tb.FontFamily.ToString()),
                                                    tb.FontSize,
                                                    tb.Background);
                                                    //BackColorPicker.CurrentColor);
                                                    //GetFontColor(customBackgroundCP.SelectedColor));
            ft.SetFontWeight(tb.FontWeight);
            ft.SetFontStyle(tb.FontStyle);
            double widthMul = ft.Width / (MaxTextWidth * Image.ScaleFactor);
            //MessageBox.Show(String.Format("{0}", ));
            if (widthMul > 1)
            {
                return MaxTextWidth * Image.ScaleFactor;
            }
            else
            {
                return ft.Width;
            }
            
        }
        // Get the height of the text element
        private double GetTextHeight(TextBlock tb)
        {
            FormattedText ft = new FormattedText(tb.Text,
                                                 CultureInfo.GetCultureInfo("en-us"),
                                                 FlowDirection.LeftToRight,
                                                 new Typeface(tb.FontFamily.ToString()),
                                                 tb.FontSize,
                                                 tb.Background);
                                                 //BackColorPicker.CurrentColor);
                                                 //GetFontColor(customBackgroundCP.SelectedColor));
            ft.SetFontWeight(tb.FontWeight);
            ft.SetFontStyle(tb.FontStyle);
            
            double widthMul = ft.Width / (MaxTextWidth * Image.ScaleFactor);
            if (widthMul > 1)
            {
                return ft.Height * Math.Ceiling(widthMul);
            }
            else
            {
                return ft.Height;
            }
            
        }

        private void BackgroundFacet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentBackgroundFacetCategory = (FacetCategory)BackgroundFacet.SelectedItem;
            Up.BackgroundFacetCategory = CurrentBackgroundFacetCategory;
            try
            {
                SetImagePreviewBackground();
            }
            catch
            {
                // FIX: Invalid operation usually caused when the UserControl iniially loads. I should permanently find a solution for this.
            }
            
        }

        private void NextItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentItemIndex < (Up.Collection.Items.Count() - 1))
            {
                CurrentItemIndex += 1;
                CurrentItemTextBox.Text = (CurrentItemIndex + 1).ToString();
                RefreshImage();
            }
            else
            {
                //MessageBox.Show("Reached the last item.");
            }
            
        }

        private void PreviousItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentItemIndex > 0)
            {
                CurrentItemIndex -= 1;
                CurrentItemTextBox.Text = (CurrentItemIndex + 1).ToString();
                RefreshImage();
            }
            else
            {
                //MessageBox.Show("Reached the first item.");
            } 
        }

        private void CurrentItemTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int itemIndex = Convert.ToInt32(CurrentItemTextBox.Text) - 1;
                if ((itemIndex >= Up.Collection.Items.Count()) && (itemIndex < 0))
                {
                    MessageBox.Show("Invalid item number");
                }
                else
                {
                    CurrentItemIndex = itemIndex;
                    RefreshImage();
                }
            }
            catch
            {
                // Invalid integer
            }
        }
        private void RefreshImage()
        {
            try
            {
                Item item = Up.Collection.Items[CurrentItemIndex];
                SetImagePreviewBackground();
                foreach (UIElement element in ImagePreview.Children)
                {
                    TextBlock tb = (TextBlock)element;
                    if (tb.Name != "Watermark")
                    {
                        foreach (Facet facet in item.Facets)
                        {
                            if (facet.Name == tb.Name)
                            {
                                if (facet.Type == "DateTime")
                                {
                                    try
                                    {
                                        DateTime dt = new DateTime();
                                        dt = DateTime.ParseExact(facet[0].Value, "o", null);
                                        tb.Text = dt.ToString("dd-MMM-yyyy");
                                    }
                                    catch
                                    {
                                        tb.Text = "N/A";
                                    }
                                }
                                else
                                {
                                    tb.Text = facet[0].Value;
                                }
                                tb.Width = GetTextWidth(tb);
                                tb.Height = GetTextHeight(tb);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Error retreiving item: {0} {1}", CurrentItemIndex, e.Message));
            }
                //Color bgColor = Color.FromScRgb(130, item.BackgroundRgb[0], item.BackgroundRgb[1], item.BackgroundRgb[2]);
                //ImagePreview.Background = new SolidColorBrush(bgColor);
        }

        public void SetImagePreviewBackground()
        {
            ItemDetails.Text = "";
            Item item = Up.Collection.Items[CurrentItemIndex];
            FacetCategory fc = CurrentBackgroundFacetCategory;
            foreach (Facet f in item.Facets)
            {
                
                ItemDetails.Text += String.Format("{0} -- {1}\r\n", f.Name, f[0].Value);
                
                if (fc.Name == f.Name)
                {
                    int bgIndex = f.Rank;
                    if (bgIndex >= BgColors.Count())
                    {
                        bgIndex = BgColors.Count() - 1;
                    }
                    //MessageBox.Show(String.Format("{0} {1} {2} {3} {4} {5}", BgColors[bgIndex].ToString(), fc.Name, CurrentItemIndex, bgIndex, f[0].Value, f.Rank));
                    ImagePreview.Background = new SolidColorBrush(BgColors[bgIndex]);
                }
            }

        }

        private void ToTopButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFacetCategory != null)
            {
                CurrentFacetCategory.tb.SetValue(Canvas.ZIndexProperty, MaxZIndex);
            }
        }

        private void ToBottomButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFacetCategory != null)
            {
                CurrentFacetCategory.tb.SetValue(Canvas.ZIndexProperty, MinZIndex);
            }
        }

        private void ItemDetails_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void AvailableFacets_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }

        

    }
}
