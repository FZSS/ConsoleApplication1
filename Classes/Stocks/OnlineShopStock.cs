using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Stocks
{
    
    public class OnlineShopStock : Stock
    {
        public string Currency { get; set; }
        public int DeliveryToUSA { get; set; }
        public double VatValue { get; set; }
        public List<StockRecord> records { get; set; }
        public int Marzha { get; set; }

        public OnlineShopStock()
            : base()
        {
            records = new List<StockRecord>();
        }

        public OnlineShopStock(string FileName) : base(FileName) {
            ReadStockFromCSV(FileName);
        }

        public OnlineShopStock(string Name, string FileName)
            : base(Name, FileName)
        {
            ReadStockFromCSV(FileName);
        }

        public OnlineShopStock(string Name, string FileName, string Currency)
            : base(Name, FileName)
        {
            this.Name = Name;
            this.Currency = Currency;
            ReadStockFromCSV(FileName);
        }

        public override void ReadStockFromCSV(string fileName)
        {
            using (var sr = new StreamReader(fileName))
            {
                var reader = new CsvReader(sr);
                reader.Configuration.Delimiter = ";";
                IEnumerable<StockRecord> records = reader.GetRecords<StockRecord>();
                this.records = records.ToList();
            }
        }

        public void SaveStockToCSV(string FileName)
        {
            using (var sw = new StreamWriter(FileName))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                //List<StockRecord> records2 = (List<StockRecord>)records;
                writer.WriteRecords(records);
            }
        }
    }
}
