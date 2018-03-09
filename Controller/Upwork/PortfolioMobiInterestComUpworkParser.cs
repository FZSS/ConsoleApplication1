using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace SneakerIcon.Controller.AllBrands.Parser.ShopsParsers
{
    public class PortfolioMobiInterestComUpworkParser : Classes.Parsing.Parser
    {
        public static void Run()
        {
            var items = GetItems();
            CsvExport(items);
        }

        private static void CsvExport(List<Item> items)
        {
            string filenameCatalog = "upwork.csv";
            using (var sw = new StreamWriter(filenameCatalog, false, Encoding.GetEncoding(1251)))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                writer.WriteRecords(items);
            }
        }

        private static List<Item> GetItems()
        {
            var resultItems = new List<Item>();
            var doc = GetHtmlPage("http://portfolio.mobi-interest.com/");

            var items = doc.QuerySelectorAll("div.resource");

            foreach (var item in items)
            {
                var resItem = new Item();
                resItem.title = item.QuerySelectorAll("div")[0].InnerHtml.Trim();

                var desc = item.QuerySelector("p").InnerHtml.Trim();
                desc = WebUtility.HtmlDecode(desc);

                var searchStr = "Contact: ";
                var contactIndex = desc.IndexOf(searchStr);

                if (contactIndex > 0)
                {
                    var contact = desc.Substring(contactIndex + searchStr.Length).Trim();
                    resItem.contact = contact;

                    desc = desc.Substring(0, contactIndex);
                }

                searchStr = "Team: ";
                var teamIndex = desc.IndexOf(searchStr);

                if (teamIndex > 0)
                {
                    var team = desc.Substring(teamIndex + searchStr.Length).Trim();
                    resItem.team = team;

                    desc = desc.Substring(0, teamIndex);
                    
                }
                resItem.description = desc;
                resultItems.Add(resItem);
            }

            return resultItems;
        }
    }

    public class Item
    {
        public string title { get; set; }
        public string description { get; set; }
        public string team { get; set; }
        public string contact { get; set; }
    }
}
