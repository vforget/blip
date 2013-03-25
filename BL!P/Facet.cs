using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Blip
{
    /// <summary>
    /// Represents a Pivot Facet. 
    /// <remarks>
    /// Each Facet has an associated FacetCategory (not directly implemented), and contains
    /// a list of FacetValue. The name and type must match the name and type of the FacetCategory.
    /// Rank is used to store the rank of the BLAST hit for a query sequence.
    /// </remarks>
    /// </summary>
    public class Facet
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Rank { set; get; }
        private IList<FacetValue> Values { get; set; }
        
        /// <summary>
        /// Instatiate an empty facet.
        /// </summary>
        /// <param name="name">Name of the Facet. Should match a Name already used by a FacetCategory</param>
        /// <param name="type">Type of the Facet. Should match a Type already used the same named FacetCategory</param>
        public Facet(string name, string type)
        {
            Name = name;
            Type = type;
            Values = new List<FacetValue>();
            Rank = 0;
        }

        /// <summary>
        /// Instatiate a facet not of type Link with an initial value.
        /// </summary>
        /// <param name="name">Name of the Facet. Should match a Name already used by a FacetCategory</param>
        /// <param name="type">Type of the Facet. Should match a Type already used the same named FacetCategory</param>
        /// <param name="value">Value of the Facet. Value of the facet. Converted to string.</param>
        public Facet(string name, string type, object value){
            if (type == "Link")
            {
                throw new Exception("Invalid type for this facet value contructor");
            }
            Name = name;
            Type = type;
            Rank = 0;
            Values = new List<FacetValue>();
            Add(new FacetValue(type, value.ToString()));
        }
        /// <summary>
        /// Instatiate a facet of type Link with an initial value.
        /// </summary>
        /// <param name="name">Name of the Facet.</param>
        /// <param name="type">Type of the Facet.</param>
        /// <param name="value">Value of the Facet.</param>
        /// <param name="href">Hyperlink of the Facet.</param>
        public Facet(string name, string type, object value, string href)
        {
            if (type != "Link")
            {
                throw new Exception("Invalid type for this facet value contructor");
            }
            Name = name;
            Type = type;
            Rank = 0;
            Values = new List<FacetValue>();
            Add(new FacetValue(type, value.ToString(), href));
        }

        /// <summary>
        /// Instantiate an empty Facet
        /// </summary>
        public Facet() {}
        
        /// <summary>
        /// Convert the Facet to Pivot CXML format.
        /// </summary>
        /// <returns>CXML string.</returns>
        public string ToXml()
        {
            string xml = "";
            if (Values.Count() > 0)
            {
                xml += "            <Facet Name=\"" + Name + "\">\n";
                foreach (FacetValue fv in Values)
                {
                    xml += fv.ToXml();
                }
                xml += "            </Facet>\r\n";
            }
            return xml;
        }

        /// <summary>
        /// Custom indexer for the Values of this Facet.
        /// </summary>
        /// <param name="idx">The index of the FacetValue</param>
        /// <returns>If exsists, the FacetValue at the index. Otherwise, returns a FacetValue with Value set to "N/A";</returns>
        public FacetValue this [int idx]{
            get
            {
                if (idx < Values.Count())
                {
                    return Values[idx];
                }
                else
                {
                    return new FacetValue(Type, "N/A");
                }
            }
        }
        /// <summary>
        /// Add a FacetValue to the Facet
        /// </summary>
        /// <param name="fv">A FacetValue to add to the list of Values.</param>
        public void Add(FacetValue fv)
        {
            Values.Add(fv);
        }
    }
}
