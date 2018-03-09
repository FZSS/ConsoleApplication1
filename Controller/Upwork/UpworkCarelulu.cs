using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Model.Enum;

namespace SneakerIcon.Controller.Upwork
{
    public class UpworkCarelulu : Classes.Parsing.Parser
    {



        public void Run()
        {
            GetItem("https://www.carelulu.com/childcare-in-seattle-wa-98101/seattle-kindercare/215281");
        }

        public CareluluItem GetItem(string url)
        {
            var item = new CareluluItem();

            var doc = GetHtmlPageCrawlera(url, ERegion.US);
                
            item.post_name = doc.QuerySelector("h1").InnerHtml;
            item.post_title = item.post_name;



            return item;
        }
    }

    public class CareluluItem
    {
        public string post_name { get; set; }
        public string post_title { get; set; }
        public string telephone { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string post_content { get; set; }
        public string openingHours { get; set; }
        public string minimumAge { get; set; }
        public string maximumAge { get; set; }
    }
}
