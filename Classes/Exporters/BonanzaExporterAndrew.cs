using CsvHelper;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.Parsing;
using SneakerIcon.Classes.Stocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Exporters
{
    public class BonanzaExporterAndrew : Exporter
    {
        public static readonly string DIRECTORY_PATH = Exporter.DIRECTORY_PATH + @"BonanzaAndrew\";
        public static readonly string FILENAME = DIRECTORY_PATH + "Bonanza.csv";
        List<BonanzaRecord> Records { get; set; }
        public const int marzha = 40;
        public int dostavka = 40;
        Catalog TitoloCatalog { get; set; }

        public BonanzaExporterAndrew()
            : base()
        {
            Records = new List<BonanzaRecord>();
            TitoloCatalog = new Catalog(TitoloParser.CATALOG_FILENAME);
        }

        public void Run()
        {
            CreateRecords();
            ExportToCSV(FILENAME);
        }

        public void CreateRecords()
        {
            foreach (var stockRecord in Titolo.records)
            {
                if (stockRecord.quantity > 0)
                {
                    BonanzaRecord bRecord = new BonanzaRecord();
                    bRecord.id = stockRecord.sku + "-" + stockRecord.size;
                    bRecord.size = stockRecord.size;
                    double price = stockRecord.price + dostavka + marzha;
                    bRecord.price = price.ToString();
                    Records.Add(bRecord);
                }
            }

            //сток объединили теперь добавляем данные из каталога
            List<BonanzaRecord> newRecords = new List<BonanzaRecord>();
            foreach (var bRecord in Records)
            {
                var sneaker = GetCatalogRecordFromId(bRecord.id);
                if (sneaker != null) {
                    sneaker.brand = "Nike";
                    bool result = bRecord.run(sneaker.brand, sneaker.title, sneaker.sku, bRecord.size, bRecord.upc, sneaker.sex, sneaker.category, double.Parse(bRecord.price), sneaker.images);
                    if (result) newRecords.Add(bRecord);
                }
                else
                {
                    Console.WriteLine("bRecord is not exist in catalog. id: " + bRecord.id);
                }
            }
            Records = newRecords;
        }

        public Sneaker GetCatalogRecordFromId(string id)
        {
            
            string sku = id.Split('-')[0] + "-" + id.Split('-')[1];
            foreach (var sneaker in TitoloCatalog.sneakers)
            {
                if (sku == sneaker.sku)
                    return sneaker;
            }
            Program.Logger.Warn("sku does not exist in catalog " + sku);
            return null;
        }

        /// <summary>
        /// получаем конечную цену (маржа и доставка уже включены)
        /// </summary>
        /// <param ftpFolder="price">себестоимость</param>
        /// <param ftpFolder="currency">валюта</param>
        /// <returns>возвращает конечную цену, которая указана на бонанзе</returns>
        //public double GetPrice(StockRecord record, string currency)
        //{
        //    double resultPrice;
        //    if (currency == "EUR") {
        //        if (record.sellPrice > 0) 
        //        {
        //            resultPrice = record.sellPrice * Settings.EURO_BUY;
        //            return resultPrice  / Settings.USD_SELL;
        //        }
        //        else 
        //        {
        //            throw new Exception("нет цены продажи sku:" + record.sku);
        //        }
        //    }
        //    else if (currency == "RUB")
        //    {
        //        var fullCatalogSneaker = catalog.GetSneakerFromSKU(record.sku);
        //        if (fullCatalogSneaker != null)
        //        {
        //            if (fullCatalogSneaker.type == "Сланцы")
        //            {
        //                resultPrice = record.price + marzha + 200;
        //                return resultPrice / Settings.USD_SELL;
        //            }
        //        }
        //        //resultPrice = record.price + marzha + dostavkaFromRussia;
        //        return resultPrice / Settings.USD_SELL;
        //    }
        //    throw new NotImplementedException();
        //}

        public BonanzaRecord GetRecordFromId(string id)
        {
            for (int i = 0; i < this.Records.Count; i++)
            {
                if (this.Records[i].id == id) return this.Records[i];
            }
            return null;
        }

        private void ExportToCSV(string filename)
        {
            //string JSON_FILENAME = @"C:\Users\Администратор\YandexDisk\stockSneaker-icon\Excel\CSV\BonanzaExport.csv";
            using (var sw = new StreamWriter(filename))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ",";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                writer.WriteRecords(Records);
            }

            filename = filename.Replace("Bonanza.csv", "BonanzaExcel.csv");
            using (var sw = new StreamWriter(filename))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                writer.WriteRecords(Records);
            }
        }
    } 
}
