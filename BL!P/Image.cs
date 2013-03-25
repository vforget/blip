using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using Media = System.Windows.Media;
using System.Diagnostics;

namespace Blip
{
    /// <summary>
    /// A Pivot Item Image. Width and Height set to 512 px. Stores a reference to an Item and FacetCategories.
    /// <remarks>The DrawAndSaveImage method should be refactored:
    /// - Break up code into smaller methods e.g. drawFacetValue method, drawBG method, convertDateTime()
    /// - separate draw image from save image.
    /// - The scaling (* 2) should be generalized to handle any size for the image preview.
    /// </remarks>
    /// </summary>
    class Image
    {
        public readonly static  int Width = 512;
        public readonly static int Height = 512;
        public readonly static int MinZIndex = 1;
        public readonly static int MaxZIndex = 20;
        public static double ScaleFactor = 1;
        public static double FontScaleFactor = 1.5;
        private Item Item;
        private IList<FacetCategory> FacetCategories;
        
        /// <summary>
        /// Instantiate an Image with an Item and a List of FacetCategory
        /// </summary>
        /// <param name="item">Item with information to be drawn in the image</param>
        /// <param name="fc">FacetCategories with information on how the items should be drawn (color, size etc)</param>
        public Image(Item item, IList<FacetCategory> fc)
        {
            Item = item;
            FacetCategories = fc;
        }

        /// <summary>
        /// Draw and save the image to file.
        /// </summary>
        /// <param name="imagePath">Local path to save the image.</param>
        /// <param name="BackgroundFacetCategory">Facet Category that specifies the BG color.</param>
        /// <param name="BackgroundColorScheme">The color scheme used for the BG color</param>
        public void DrawAndSaveImage(string imagePath, FacetCategory BackgroundFacetCategory, IList<Media.Color> BackgroundColorScheme)
        {
            // Init the image and graphic.
            Bitmap image = new Bitmap(Width, Height);
            Graphics graphic = Graphics.FromImage(image);
            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphic.TextRenderingHint = TextRenderingHint.AntiAlias;

            // Init the BG to white, and set it to the appropriate color based on the color scheme and BackgroundFacetCategory
            Rectangle bg = new Rectangle(0, 0, Width, Height);
            Color bgc = Color.White;
            foreach (Facet f in Item.Facets)
            {
                if (f.Name == BackgroundFacetCategory.Name)
                {
                    int bgIndex = f.Rank;
                    // If Rank exceeds the total number of available colors, set it to the last one.
                    if (bgIndex >= BackgroundColorScheme.Count())
                    {
                        bgIndex = BackgroundColorScheme.Count() - 1;
                    }
                    
                    // Set the BG color.
                    bgc = Color.FromArgb(BackgroundColorScheme[bgIndex].A,
                                         BackgroundColorScheme[bgIndex].R,
                                         BackgroundColorScheme[bgIndex].G,
                                         BackgroundColorScheme[bgIndex].B);
                    SolidBrush b = new SolidBrush(bgc);
                    graphic.FillRectangle(b, bg);
                }
            }

            // From bottom-most to top-most layer ...
            for (int zIndex = MinZIndex; zIndex <= MaxZIndex; zIndex++)
            {
                foreach (FacetCategory fc in FacetCategories)
                {
                    // If the FacetCategory is selected to be drawn
                    if (fc.IsSelected && fc.Z == zIndex)
                    {
                        
                        // Iterate over all the Facet for an item ...
                        foreach (Facet facet in Item.Facets)
                        {
                            // if the facet is found ...
                            if (facet.Name == fc.Name)
                            {
                                // Set the font size, family, weight, and style
                                Font font = new Font(fc.FontFamily.ToString(), (float)(fc.FontSize * Image.FontScaleFactor));
                                if (fc.FontWeight.ToString() == "Bold" && fc.FontStyle.ToString() == "Italic")
                                {
                                    font = new Font(fc.FontFamily.ToString(), fc.FontSize, FontStyle.Bold | FontStyle.Italic);
                                }
                                if (fc.FontWeight.ToString() == "Bold" && fc.FontStyle.ToString() == "Normal")
                                {
                                    font = new Font(fc.FontFamily.ToString(), fc.FontSize, FontStyle.Bold);
                                }
                                if (fc.FontWeight.ToString() == "Normal" && fc.FontStyle.ToString() == "Italic")
                                {
                                    font = new Font(fc.FontFamily.ToString(), fc.FontSize, FontStyle.Italic);
                                }

                                // Set the font color
                                Color fontFGColor = Color.FromArgb(fc.FontForegroundColor.A, fc.FontForegroundColor.R, fc.FontForegroundColor.G, fc.FontForegroundColor.B);
                                Color fontBGColor = Color.FromArgb(fc.FontBackgroundColor.A, fc.FontBackgroundColor.R, fc.FontBackgroundColor.G, fc.FontBackgroundColor.B);
                                //Color c = new Pen(fc.FontForegroundColor).Color;
                                SolidBrush fontFGBrush = new SolidBrush(fontFGColor);
                                SolidBrush fontBGBrush = new SolidBrush(fontBGColor);
                                
                                // Set point to draw the text
                                Point p = new Point((int)Math.Round(fc.X/Image.ScaleFactor, 0), (int)Math.Round(fc.Y/Image.ScaleFactor, 0));
                                
                                // String to draw
                                string str = facet[0].Value;
                                // Convert to more human readable DateTime. Default is longish xs:DateTime (required for Pivot).
                                if (facet[0].Type == "DateTime")
                                {
                                    try
                                    {
                                        DateTime dt = new DateTime();
                                        dt = DateTime.ParseExact(facet[0].Value, "o", null);
                                        str = dt.ToString("dd-MMM-yyyy");
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Error parsing DateTime: " + facet[0].Value);
                                    }
                                }
                                
                                // Determine height and width of the string
                                SizeF stringSize = graphic.MeasureString(str, font);
                                float textWidth = stringSize.Width;
                                float textHeight = stringSize.Height;
                                
                                // If width exceeds the image width, wrap text.
                                if (stringSize.Width > Width)
                                {
                                    textWidth = Width;
                                    textHeight = stringSize.Height * (float)Math.Ceiling(stringSize.Width / Width);
                                }
                                
                                // Bounding color filled box around the text
                                Rectangle box = new Rectangle(p.X, p.Y, (int)Math.Round(textWidth,0), (int)Math.Round(textHeight,0));
                                graphic.FillRectangle(fontBGBrush, box);
                                
                                // Draw the text.
                                RectangleF textRect = new RectangleF(p.X, p.Y, textWidth, textHeight);
                                graphic.DrawString(str, font, fontFGBrush, textRect);

                                Color outlineColor = Color.LightGray;
                                Pen outlinePen = new Pen(outlineColor);
                                outlinePen.Width = 1;
                                graphic.DrawRectangle(outlinePen, box);
                                Font legendFont = new Font("Segoe UI", 10);
                                
                                if (font.Size < 10)
                                {
                                    legendFont = new Font("Segoe UI", font.Size);
                                }
                                SolidBrush legendBrush = new SolidBrush(outlineColor);
                                stringSize = graphic.MeasureString(str, legendFont);
                                Point legendPos = new Point(p.X, p.Y - (int)stringSize.Height);
                                graphic.DrawString(fc.Name, legendFont, legendBrush, legendPos);
                            }
                        }
                    }
                }
            }
            // Save the image.
            image.Save(imagePath);
        }
    }
}
