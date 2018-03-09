using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace SneakerIcon.Controller.Upwork
{
    public class Wikidata : Classes.Parsing.Parser
    {
        public static void Run()
        {
            var items = GetItems();
            CsvExport(items);
        }

        private static void CsvExport(List<TennisPlayer> items)
        {
            string filenameCatalog = "upworkTennisPlayers.csv";
            using (var sw = new StreamWriter(filenameCatalog, false, Encoding.GetEncoding(1251)))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ",";
                writer.WriteRecords(items);
            }
        }

        private static List<TennisPlayer> GetItems()
        {
            throw new NotImplementedException();
        }
    }

    public class TennisPlayer
    {
        public string Name { get; set; }
    }
}
