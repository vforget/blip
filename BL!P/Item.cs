using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blip
{
    /// <summary>
    /// Represents a Pivot Item.
    /// </summary>
    public class Item
    {
        public string Img { get; set; }
        public string Id { get; set; }
        public string Href { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<Facet> Facets { get; set; }
        
        /// <summary>
        /// Instantiate an Item
        /// </summary>
        /// <param name="id">Id of the item. Typically a number.</param>
        /// <param name="img">Path to the Image for the item. Typically a DeepZoom collection.</param>
        public Item(object id, string img)
        {
            Description = "Description not available.";
            Facets = new List<Facet>();
            Id = id.ToString();
            Img = img;
            
        }

        /// <summary>
        /// Convert to CXML
        /// </summary>
        /// <returns>A CXML string</returns>
        public string ToXml()
        {
            string xml = "";
            xml += "    <Item Img=\"" + Img + "\" ";
            xml += "Id=\"" + Id.ToString() + "\" ";
            xml += "Href=\"" + Href + "\" ";
            xml += "Name=\"" + Name + "\" ";
            xml += ">\r\n";
            xml += "        <Description>" + Description + "</Description>\n";
            if (Facets.Count() > 0)
            {
                xml += "       <Facets>\r\n";
                foreach (Facet f in Facets)
                {
                    xml += f.ToXml();
                }
                xml += "       </Facets>\r\n";
            }
            xml += "    </Item>\r\n";
            return xml;
        }
    }
}
