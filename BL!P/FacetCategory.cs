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

namespace Blip
{
    /// <summary>
    /// Represents a Pivot Facet Category. Extended to store attributes of the Facet Category
    /// as they are visualized in the Image for each Item.
    /// </summary>
    public class FacetCategory
    {
        //Proterties of the Pivot Facet Category
        public string Name { get; set; }
        public string Format { get; set; }
        public string Type { get; set; }
        public bool IsFilterVisible { get; set; }
        public bool IsMetaDataVisible { get; set; }
        public bool IsWordWheelVisible { get; set; }
        // Properties of this FacetCatecory in the Image for each Item.
        public string DisplayName { get; set; }
        public TextBlock tb { get; set; }
        public bool IsSelected { get; set; }
        public FontFamily FontFamily { get; set; }
        public float FontSize { get; set; }
        public FontWeight FontWeight { get; set; }
        public FontStyle FontStyle { get; set; }
        public Color FontForegroundColor { get; set; }
        public Color FontBackgroundColor { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Z { get; set; }
        public Color BackgroundColor { get; set; }
        
        /// <summary>
        /// Create a Number Type Facet Category.
        /// </summary>
        /// <param name="name">Name of the Facet Category</param>
        /// <param name="type">Type of the Facet Category</param>
        /// <param name="format">Format for the number, e.g. "0 aa" for amino acid length</param>
        /// <param name="isFilterVisible">Can this Facet Category be used as a filter in Pivot?</param>
        public FacetCategory() { }

        public FacetCategory(string name, string type, string format, bool isFilterVisible){
            if (type != "Number"){
                throw(new Exception("Incorrect type for this contructor."));
            }
            Name = name;
            Type = type;
            Format = format;
            DisplayName = Name + " (" + Type + ")";
            IsFilterVisible = isFilterVisible;
            IsMetaDataVisible = true;
            IsWordWheelVisible = false;
            tb = new TextBlock();
            tb.TextWrapping = TextWrapping.Wrap;
            IsSelected = false;
            BackgroundColor = Brushes.White.Color;
        }

        /// <summary>
        /// Create a Facet Category for non-Number Types.
        /// </summary>
        /// <param name="name">Name of the Facet Category</param>
        /// <param name="type">Type of the Facet Category</param>
        /// <param name="isFilterVisible">Can this Facet Category be used as a filter in Pivot?</param>
        public FacetCategory(string name, string type, bool isFilterVisible){
             if (type == "Number"){
                throw(new Exception("Incorrect type for this contructor."));
            }
            Name = name;
            Type = type;
            Format = Type;
            DisplayName = Name + " (" + Type + ")"; 
            IsFilterVisible = isFilterVisible;
            IsMetaDataVisible = true;
            IsWordWheelVisible = true;
            tb = new TextBlock();
            tb.TextWrapping = TextWrapping.Wrap;
            IsSelected = false;
            BackgroundColor = Brushes.White.Color;
        }

        /// <summary>
        /// Convert the Facet Category to Pivot CXML format.
        /// </summary>
        /// <returns>CXML string.</returns>
        public string ToXml()
        {
            string xml = "";
            xml += "    <FacetCategory Name=\"" + Name + "\" ";
            if (Type == "Number")
            {
                xml += "Format=\"" + Format + "\" ";
            }
            xml += "Type=\"" + Type + "\" ";
            xml += "p:IsFilterVisible=\"" + IsFilterVisible.ToString().ToLower() + "\" ";
            xml += "p:IsMetaDataVisible=\"" + IsMetaDataVisible.ToString().ToLower() + "\" ";
            xml += "p:IsWordWheelVisible=\"" + IsWordWheelVisible.ToString().ToLower() + "\" ";
            xml += "/>\r\n";
            return xml;
        }
    }
}
