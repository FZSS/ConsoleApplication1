using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Stocks
{
    public class DiscontStock : Stock
    {
        public static readonly string FILENAME = Config.GetConfig().DiscontStockFilename;
        public static readonly string SALE_FILENAME = Config.GetConfig().DiscontStockSaleFilename;
        public Dictionary<string, double> sale { get; set; }
        public List<DiscontStockRecord> Records2 { get; set; }
        public DiscontStock()
            : base()
        {
            sale = new Dictionary<string, double>();
            Records2 = new List<DiscontStockRecord>();
            //ReadStockFromCSV();
        }

        public DiscontStock(string FileName) : base()
        {
            sale = new Dictionary<string, double>();
            //Records2 = new List<DiscontStockRecord>();
            ReadStockFromCSV(FileName);
        }

        /// <summary>
        /// добавляем ко всем записям скидки
        /// </summary>
        public void AddSaleToRecords()
        {
            bool result = ReadSaleFromCSV();
            if (!result) return;
            foreach (var stockRecord in records)
            {    
                if (this.sale.ContainsKey(stockRecord.sku))
                {
                    double salePrice = this.sale[stockRecord.sku];
                    if (salePrice < stockRecord.price)
                    {
                        stockRecord.oldPrice = stockRecord.price;
                        stockRecord.price = salePrice;
                    }
                }
            }
        }


        public void AddSaleToRecords2()
        {
            bool result = ReadSaleFromCSV();
            if (!result) return;
            foreach (var stockRecord in Records2)
            {
                if (this.sale.ContainsKey(stockRecord.sku))
                {
                    double salePrice = this.sale[stockRecord.sku];
                    if (salePrice < stockRecord.price)
                    {
                        stockRecord.oldPrice = stockRecord.price;
                        stockRecord.price = salePrice;
                    }
                }
            }
            //throw new NotImplementedException();
        }

        /// <summary>
        /// ко всем записям добавляем скидки, а также помечаем все записи, которых нет в файле sale.csv как бэквол
        /// </summary>
        /// <returns></returns>
        public bool AddStandtoRecords()
        {
            bool result = ReadSaleFromCSV();
            if (!result) return false;

            foreach (var stockRecord in Records2)
            {
                if (this.sale.ContainsKey(stockRecord.sku))
                {
                    stockRecord.isBackWall = false;
                    double salePrice = this.sale[stockRecord.sku];
                    if (salePrice < stockRecord.price)
                    {
                        stockRecord.oldPrice = stockRecord.price;
                        stockRecord.price = salePrice;
                    }
                }
                else
                {
                    stockRecord.isBackWall = true;
                }
            }

            return true;
        }

        public bool ReadSaleFromCSV(string fileName = null)
        {
            //todo проверить
            if (fileName == null)
            {
                fileName = SALE_FILENAME;
            }

            if (!File.Exists(fileName)) return false;
            string[] stockArray = File.ReadAllLines(fileName);
            for (int i = 1; i < stockArray.Length; i++)
            {
                string[] stockStringRecords = stockArray[i].Split(';');
                if (!String.IsNullOrWhiteSpace(stockStringRecords[0].Trim()) && !String.IsNullOrWhiteSpace(stockStringRecords[1].Trim()))
                {                 
                    string sku = stockStringRecords[0].Trim();
                    double price = Double.Parse(stockStringRecords[1].Trim());
                    if (!sale.Keys.Contains(sku))
                    {
                        sale.Add(sku, price);
                    }
                    else
                    {
                        bool test = true;
                    }
                }
            }
            return true;
        }

        public override void ReadStockFromCSV(string fileName = null)
        {
            //todo проверить
            if (fileName == null)
            {
                fileName = FILENAME;
            }
            string[] stockArray = File.ReadAllLines(fileName);
            List<StockRecord> stockList = new List<StockRecord>();
            for (int i = 1; i < stockArray.Length; i++)
            {
                string[] stockStringRecords = stockArray[i].Split(';');
                StockRecord stockRecord = new StockRecord();
                //stockRecord.stockName = "discont";
                stockRecord.sku = stockStringRecords[0].Trim();
                stockRecord.size = stockStringRecords[1].Trim();
                stockRecord.upc = stockStringRecords[2].Trim();
                stockRecord.title = stockStringRecords[4].Trim();
                if (!String.IsNullOrEmpty(stockRecord.upc))
                {
                    if (stockRecord.upc.Length == 14)
                    {
                        stockRecord.upc = stockRecord.upc.Substring(2);
                    }
                    //stockRecord.upc = stockStringRecords[4].Substring(2);
                }
                stockRecord.quantity = Int32.Parse(stockStringRecords[3].Replace("(", "").Replace(")", "")); //в файле дисконта иногда кол-во встречается в формате (1)
                stockRecord.price = Double.Parse(stockStringRecords[5]);
                stockRecord.oldPrice = Double.Parse(stockStringRecords[6]);
                //stockRecord.price = stockRecord.price + 1500;
                stockList.Add(stockRecord);
            }
            records = stockList;
        }

        public void ReadStockFromCSV2(string FileName)
        {
            using (var sr = new StreamReader(FileName))
            {
                var reader = new CsvReader(sr);
                reader.Configuration.Delimiter = ";";
                IEnumerable<DiscontStockRecord> records = reader.GetRecords<DiscontStockRecord>();
                this.Records2 = records.ToList();
            }
        }

        public void ParseStockOnHand(string FileName, char separator = ';') {
            //string[] stockArray = File.ReadAllLines(FileName,Encoding.GetEncoding(1251));
            string[] stockArray = File.ReadAllLines(FileName);
            List<StockRecord> stockList = new List<StockRecord>();

            int startIndex = 1;
            for (int i = 1; i < stockArray.Length; i++)
            {
                string[] stockStringRecords = stockArray[i].Split(separator);
                if (stockStringRecords[0] == "FOOTWEAR")
                {
                    startIndex = i;
                    break;
                }
            }
            

            for (int i = startIndex; i < stockArray.Length; i++)
            {
                string[] stockStringRecords = stockArray[i].Split(';');

                if (stockStringRecords[0].Length == 6 && !stockStringRecords[0].Contains("SX")) //если длина 6 значит это артикул и есть левые sx артикулы
                {
                    StockRecord stockRecord = new StockRecord();
                    //stockRecord.stockName = "discont";
                    stockRecord.sku = stockStringRecords[0].Trim() + "-" + stockStringRecords[3].Trim();
                    stockRecord.size = stockStringRecords[6].Trim();
                    stockRecord.upc = stockStringRecords[11].Trim();
                    if (!String.IsNullOrEmpty(stockRecord.upc))
                    {
                        if (stockRecord.upc.Length == 14)
                        {
                            stockRecord.upc = stockRecord.upc.Substring(2);
                        }
                        //stockRecord.upc = stockStringRecords[4].Substring(2);
                    }
                    stockRecord.title = stockStringRecords[16].Trim();
                    stockRecord.quantity = Int32.Parse(stockStringRecords[24].Replace("(", "").Replace(")", "")); //в файле дисконта иногда кол-во встречается в формате (1)

                    //double price = 0;
                    //if (Double.TryParse(stockStringRecords[19], out price))
                    //{
                    //    stockRecord.price = price;
                    //    stockList.Add(stockRecord);
                    //}
                    if (!String.IsNullOrWhiteSpace(stockStringRecords[19]))
                    {
                        var price = stockStringRecords[19].Replace(".00", "").Replace(",","");
                        stockRecord.price = Double.Parse(price);
                        stockList.Add(stockRecord);
                    }
                }


            }
            records = stockList;
        }

        public void ParseStockOnHand2(string FileName)
        {
            char separator = ';';
            //string FileName = @"C:\Users\Администратор\YandexDisk\fullCatalogSneaker-icon\Parsing\discont\Stock On Hand Samara.csv";
            //string[] stockArray = File.ReadAllLines(FileName, Encoding.GetEncoding(1251));
            string[] stockArray = File.ReadAllLines(FileName);
            List<DiscontStockRecord> stockList = new List<DiscontStockRecord>();

            int startIndex = 1;
            for (int i = 1; i < stockArray.Length; i++)
            {
                if (i == 3483)
                {
                    bool test = true;
                }
                string[] stockStringRecords = stockArray[i].Split(separator);
                if (stockStringRecords[0] == "FOOTWEAR")
                {
                    startIndex = i;
                    break;                    
                }
            }
            if (startIndex == 1) throw new Exception("FOOTWEAR не найдено. скорее всего неверный формат файла");

            string category = null;
            for (int i = startIndex; i < stockArray.Length; i++)
            {
                string[] stockStringRecords = stockArray[i].Split(separator);

                //categorySneakerFullCatalog                
                if (stockStringRecords[6].Contains("KIDS")) {
                    category = Settings.CATEGORY_KIDS;
                }
                else if (stockStringRecords[6].Contains("WOMENS"))
                {
                    category = Settings.CATEGORY_WOMEN;
                }
                else if (stockStringRecords[6].Contains("MENS")) {
                    category = Settings.CATEGORY_MEN;
                }


                if (stockStringRecords[0].Length == 6 && !stockStringRecords[0].Contains("SX")) //если длина 6 значит это артикул
                {
                    DiscontStockRecord stockRecord = new DiscontStockRecord();
                    //stockRecord.stockName = "discont";
                    stockRecord.sku = stockStringRecords[0].Trim() + "-" + stockStringRecords[3].Trim();
                    stockRecord.size = stockStringRecords[6].Trim();
                    stockRecord.upc = stockStringRecords[11].Trim();
                    stockRecord.category = category;

                    //isBackWall
                    if (stockStringRecords[21].Contains("BW")) {
                        stockRecord.isBackWall = true;
                    }
                    else {
                        stockRecord.isBackWall = false;
                    }

                    //upc
                    if (!String.IsNullOrEmpty(stockRecord.upc))
                    {
                        if (stockRecord.upc.Length == 14)
                        {
                            stockRecord.upc = stockRecord.upc.Substring(2);
                        }
                    }

                    stockRecord.title = stockStringRecords[16].Trim();
                    int quantity = 0;
                    var quantString = stockStringRecords[24].Replace("(", "").Replace(")", "");
                    bool isQuantity = Int32.TryParse(quantString, out quantity);
                    //stockRecord.quantity = Int32.Parse(); //в файле дисконта иногда кол-во встречается в формате (1)
                    if (quantity > 0)
                    {
                        stockRecord.quantity = quantity;
                        if (!String.IsNullOrWhiteSpace(stockStringRecords[19]))
                        {
                            var price = stockStringRecords[19].Replace(".00", "").Replace(",", "");
                            stockRecord.price = Double.Parse(price);
                            stockList.Add(stockRecord);
                        }
                    }
                    else
                    {
                        bool test = true;
                    }

                    //double price = 0;
                    //if (Double.TryParse(stockStringRecords[19], out price))
                    //{
                    //    stockRecord.price = price;
                    //    stockList.Add(stockRecord);
                    //}
                }


            }
            Records2 = stockList;
        }

        public void ReadStandFromCSV(string FileName)
        {
            //string FileName = @"C:\Users\Администратор\YandexDisk\fullCatalogSneaker-icon\Parsing\discont\Stock On Hand Samara.csv";
            string[] stockArray = File.ReadAllLines(FileName, Encoding.GetEncoding(1251));
            List<StockRecord> stockList = new List<StockRecord>();

            //Находим откуда начинается обувь
            int startIndex = 1;
            for (int i = 1; i < stockArray.Length; i++)
            {
                string[] stockStringRecords = stockArray[i].Split(';');
                if (stockStringRecords[0] == "FOOTWEAR")
                {
                    startIndex = i;
                    break;
                }
            }

            for (int i = startIndex; i < stockArray.Length; i++)
            {
                string[] stockStringRecords = stockArray[i].Split(';');

                if (stockStringRecords[0].Length == 6 && !stockStringRecords[0].Contains("SX")) //если длина 6 значит это артикул
                {
                    if (!stockStringRecords[21].Contains("BW")) { //берем только кроссовки со стенда, не с беквола
                        StockRecord stockRecord = new StockRecord();
                        stockRecord.sku = stockStringRecords[0].Trim() + "-" + stockStringRecords[3].Trim();
                        stockRecord.size = stockStringRecords[6].Trim();
                        stockRecord.upc = stockStringRecords[11].Trim();
                        if (!String.IsNullOrEmpty(stockRecord.upc))
                        {
                            if (stockRecord.upc.Length == 14)
                            {
                                stockRecord.upc = stockRecord.upc.Substring(2);
                            }
                            //stockRecord.upc = stockStringRecords[4].Substring(2);
                        }
                        stockRecord.title = stockStringRecords[16].Trim();
                        stockRecord.quantity = Int32.Parse(stockStringRecords[24].Replace("(", "").Replace(")", "")); //в файле дисконта иногда кол-во встречается в формате (1)
                        stockRecord.price = Double.Parse(stockStringRecords[19]);
                        //stockRecord.oldPrice = Double.Parse(stockStringRecords[6]);
                        //stockRecord.price = stockRecord.price + 1500;
                        stockList.Add(stockRecord);
                    }
                    else {
                        bool test = true;
                    }

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

        public void SaveStockToCSV2(string fileName = null)
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
                writer.WriteRecords(Records2);
            }
        }
    }
}
