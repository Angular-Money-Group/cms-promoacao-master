using HtmlAgilityPack;
using System;
using System.Net;
using System.Linq;

namespace Bitzar.Products.Models
{
    public class Field : ICloneable
    {
        public int IdProduct { get; set; }
        public int IdLanguage { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Value { get; set; }
        public bool ReadOnly { get; set; } = false;
        public string Type { get; set; }

        public string Formatted
        {
            get
            {
                switch (this.Type.ToLower())
                {
                    case "image":
                        return (!string.IsNullOrEmpty(this.Value) ? (Plugin.CMS.Library.Object(int.Parse(this.Value))?.FullPath ?? string.Empty) : string.Empty);
                    case "html":
                        if (string.IsNullOrEmpty(this.Value))
                            return string.Empty;

                        // Decode value before strip
                        var decoded = WebUtility.HtmlDecode(this.Value);

                        // Load the document to be validated
                        var doc = new HtmlDocument();
                        doc.LoadHtml(decoded);

                        return WebUtility.HtmlDecode(doc.DocumentNode.InnerText).Replace("\r\n", " ");
                    default:
                        return string.Empty;
                }
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}