using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.SizeConverters.Model
{
    public class Size
    {
        public string brand { get; set; }
        /// <summary>
        /// men, women, kids
        /// </summary>
        public string category { get; set; }
        public string us { get; set; }
        public string eu { get; set; }
        public string cm { get; set; }
        public string uk { get; set; }
        public string ru { get; set; }

        public Size(string brand, string category, string us, string eu, string uk, string cm, string ru)
        {
            this.brand = brand;
            this.category = category;
            this.us = us;
            this.eu = eu;
            this.uk = uk;
            this.cm = cm;
            this.ru = ru;
        }

        public string GetAllSizeString()
        {
            return us + " US / " + eu + " EU / " + cm + " CM / " + uk + " UK / " + ru + " RU";
        }

        public string GetSizeStringUsEuUkCm()
        {
            return us + " US / " + eu + " EU / " + uk + " UK / " + cm + " CM";
        }
    }
}