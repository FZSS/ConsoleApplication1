using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace SneakerIcon.Controller.Upwork
{
    public class LarisaLodki : Classes.Parsing.Parser
    {
        public string StartPage = "https://vodomotorika.ru/products/lodki_pod_motor?p=1";
        public string Host = "https://vodomotorika.ru/products/lodki_pod_motor";
        public List<Lodka> titles { get; set; }

        public List<Lodka> ParseLodki()
        {
            titles = new List<Lodka>();

            ParseLodkiFromPage(titles, StartPage);

            SaveToCSV("lodki.csv");

            return titles;
        }

        public void SaveToCSV(string filename)
        {
            string filenameCatalog = filename;

            using (var sw = new StreamWriter(filenameCatalog, false, Encoding.GetEncoding(1251)))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                writer.WriteRecords(titles);
            }
        }

        private void ParseLodkiFromPage(List<Lodka> titles, string link)
        {
            Console.WriteLine("titlesCount = " + titles.Count + " url: " + link );

            var document = GetHtmlPage(link);

            var titleListHtml = document.QuerySelectorAll("div.goodsPreview");

            foreach (var titleHtml in titleListHtml)
            {
                var title = titleHtml.QuerySelector("a.name").InnerHtml.Trim();
                var lodka = new Lodka();
                lodka.Title = title;
                titles.Add(lodka);
            }

            //next page
            var nextPages = document.QuerySelector("div.paginator").QuerySelectorAll("a.text");
            foreach (var nextPage in nextPages)
            {
                if (nextPage.InnerHtml == "cледующая")
                {
                    string nextPageLink = nextPage.GetAttribute("href");
                    ParseLodkiFromPage(titles, Host + nextPageLink);
                }
            }
        }
    }

    public class Lodka
    {
        public string Title { get; set; }
    }
}
