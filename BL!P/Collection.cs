using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blip
{
    /// <summary>
    /// Represent a Pivot Collection. A Pivot collection consists of a list of FacetCategories and Items.
    /// </summary>
    public class Collection
    {
        public string Name { get; set; }
        public IList<FacetCategory> FacetCategories { get; set; }
        public IList<Item> Items { get; set; }
        public string ImgBase { get; set; }

        /// <summary>
        /// Create an empty Pivot collection.
        /// </summary>
        /// <param name="name">Name of the collection. This text will appear in the top left corner of the Pivot interface.</param>
        /// <param name="imgBase">Location of the DeepZoom collection.</param>
        public Collection(string name, string imgBase)
        {
            Name = name;
            ImgBase = imgBase;
            FacetCategories = new List<FacetCategory>();
            Items = new List<Item>();
        }
        
        /// <summary>
        /// Converts the collection to an XML string (specifically Pivot CXML format).
        /// </summary>
        /// <returns>CXML string</returns>
        public string ToXml(){
            
            string xml = "";
            xml += "<?xml version=\"1.0\"?>\r\n";
            xml += "<Collection Name=\"" + Name + "\"\r\n";
            xml += "    SchemaVersion=\"1.0\"\r\n";
            // Support for these features has not been implemented.
            // xml += "Icon=\"" + icon + "\" ";
            // xml += "BrandImage=\"" + brandImage + "\" ";
            xml += "    p:AdditionalSearchText=\"__block\"\r\n";
            xml += "    xmlns=\"http://schemas.microsoft.com/collection/metadata/2009\"\r\n";
            xml += "    xmlns:p=\"http://schemas.microsoft.com/livelabs/pivot/collection/2009\"\r\n";
            xml += "    xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\r\n";
            xml += "    xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n";
            xml += "<FacetCategories>\r\n";
            
            foreach (FacetCategory fc in FacetCategories)
            {
                xml += fc.ToXml();
            }
            xml += "</FacetCategories>\r\n";
            xml += "<Items ImgBase=\"" + ImgBase + "\">\r\n";
            
            foreach (Item item in Items)
            {
                xml += item.ToXml();
            }
            xml += "</Items>\r\n";
            xml += "</Collection>\r\n";
            return xml;
        }
    }
}
