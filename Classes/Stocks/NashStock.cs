using CsvHelper;
using SneakerIcon.Classes.Catalogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Stocks
{
    public class NashStock : Stock
    {
        public static readonly string FILENAME = Config.GetConfig().NashStockFilename;

        public List<NashStockRecord> records { get; set; }
        public Catalog catalog { get; set; }
        public double sellPrice { get; set; }
        
        public NashStock()
            : base()
        {
            records = new List<NashStockRecord>();
            catalog = new Catalog();
            
            //ReadStockFromCSV();
            
            //AddSaleToRecords();
        }

        public NashStock(string fileName)
        {
            records = new List<NashStockRecord>();
            ReadStockFromCSV(fileName);
        }

        public void Initialize()
        {
            ReadStockFromCSV();
            CreateSneakerList();
        }

        private void CreateSneakerList()
        {
            foreach (var record in records)
            {
                
            }
        }

        //public override void ReadStockFromCSV(string FileName = JSON_FILENAME)
        //{
        //    using (var sr = new StreamReader(FileName))
        //    {
        //        var reader = new CsvReader(sr);
        //        reader.Configuration.Delimiter = ",";
        //        IEnumerable<NashStockRecord> records = reader.GetRecords<NashStockRecord>();
        //        this.records = records.ToList();
        //    }
        //}

        public override void ReadStockFromCSV(string fileName = null)
        {
            //todo проверить
            if (fileName == null)
            {
                fileName = FILENAME;
            }

            //string JSON_FILENAME = @"C:\SneakerIcon\CSV\NashStock.csv";
            string[] stockArray = File.ReadAllLines(fileName);
            List<NashStockRecord> stockList = new List<NashStockRecord>();
            for (int i = 1; i < stockArray.Length; i++)
            {
                if (i == 188)
                {
                    bool test = true;
                }
                string[] stockStringRecords = stockArray[i].Split(',');
                NashStockRecord stockRecord = new NashStockRecord();
                stockRecord.brand = stockStringRecords[0].Trim();
                stockRecord.condition = stockStringRecords[1].Trim();
                stockRecord.sku = stockStringRecords[2].Trim();
                if (!String.IsNullOrWhiteSpace(stockRecord.sku))
                {
                    stockRecord.size = stockStringRecords[3].Trim();
                    stockRecord.sku2 = stockStringRecords[4].Trim();
                    stockRecord.upc = stockStringRecords[5].Trim();
                    stockRecord.price = Convert.ToDouble(stockStringRecords[6].Trim());
                    if (!String.IsNullOrWhiteSpace(stockStringRecords[7]))
                        stockRecord.sellPrice = Convert.ToDouble(stockStringRecords[7].Trim());
                    stockRecord.quantity = Convert.ToInt32(stockStringRecords[8].Trim());
                    stockRecord.capacity = Convert.ToDouble(stockStringRecords[9].Trim());
                    stockRecord.category = stockStringRecords[10].Trim();
                    stockRecord.title = stockStringRecords[11].Trim();
                    stockRecord.comment = stockStringRecords[12].Trim();

                    bool isValidate = stockRecord.Validate();

                    if (isValidate)
                        stockList.Add(stockRecord);
                }
            }
            records = stockList;
        }



        public void SaveStockToCSV(string fileName = null)
        {
            //todo проверить
            if (fileName == null)
            {
                fileName = FILENAME;
            }

            using (var sw = new StreamWriter(fileName))
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
