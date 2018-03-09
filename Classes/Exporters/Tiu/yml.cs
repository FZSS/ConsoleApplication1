using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SneakerIcon.Classes.Exporters.Tiu
{
    public class YML
    {
        public YML() {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(@"AdditionalFiles/yml.yml");
            // получим корневой элемент
            XmlElement Catalog = xDoc.DocumentElement;
            var Shop = Catalog.LastChild;
            var Offers = Shop.LastChild;
            var Offer = Offers.FirstChild;
        }
        
    }
}
