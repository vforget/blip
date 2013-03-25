using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;


namespace Blip
{
    /// <summary>
    /// Represents one Value of a Facet stored in a list of Facet.Values.
    /// </summary>
    public class FacetValue
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public string Name { get; set; } // For Link type Facet.
        public string Href { get; set; } // For Link type Facet.
        
        /// <summary>
        /// Initialize a Facet Value of non-Link type.
        /// </summary>
        /// <param name="type">Type of the parent Facet. Cannot be Link.</param>
        /// <param name="value">Value for the facet value.</param>
        public FacetValue(string type, string value)
        {
            if (Type == "Link")
            {
                throw new Exception("Invalid type for this facet value contructor");
            }
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Initialize a Facet Value of Link type.
        /// </summary>
        /// <param name="type">Type of the parent Facet. Must be Link.</param>
        /// <param name="name">Name of the Hyperlink</param>
        /// <param name="href">Hyperlink</param>
        public FacetValue(string type, string name, string href)
        {
            if (type != "Link")
            {
                throw new Exception("Invalid type for this facet value contructor");
            }
            Type = type;
            Name = name;
            Href = href;
            Value = name; // For instances where Value is used to get a facet values content.
        }

        /// <summary>
        /// Initialize an empty Facet Value.
        /// </summary>
        public FacetValue() { }

        /// <summary>
        /// Convert to Pivot CXML format.
        /// </summary>
        /// <returns>CXML string.</returns>
        public string ToXml()
        {
            if (Type == "Link")
            {
                if (Href != null) 
                { 
                    return "                <" + Type + " Name=\"" + Name + "\" Href=\"" + Href + "\"/>\r\n";     
                }
                else
                {
                    return "                <" + Type + " Name=\"" + Name + "\"/>\r\n";
                }
            }
            else
            {
                if (Type != "DateTime")
                {
                    return "                <" + Type + " Value=\"" + EscapeXML(Value) + "\"/>\r\n";
                }
                else
                {
                    return "                <" + Type + " Value=\"" + Value + "\"/>\r\n";
                }
            }
        }

        private string EscapeXML(string s)
        {
            // Replace special chars in w/ space
            return s.Replace('&', '+').Replace('<', ' ').Replace('>', ' ').Replace('"', ' ').Replace('\'', ' ');
        }
    }
}
