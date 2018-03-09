using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Stocks
{
    public abstract class Stock
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public List<StockRecord> records { get; set; } 

        public Stock()
        {
            records = new List<StockRecord>();
        }

        public Stock(string FileName)
        {
            records = new List<StockRecord>();
            this.FileName = FileName;
            //ReadStockFromCSV(FileNameCatalog);
        }

        public Stock(string Name, string FileName)
        {
            records = new List<StockRecord>();
            this.FileName = FileName;
            //ReadStockFromCSV(FileNameCatalog);
        }

        public abstract void ReadStockFromCSV(string fileName);


    }
}
