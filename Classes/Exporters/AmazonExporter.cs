using CsvHelper;
using SneakerIcon.Classes.Stocks;
using System;
using System.Collections.Generic;
using System.IO;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Exporters
{
    public class AmazonExporter : Exporter
    {
        public static readonly string FILENAME = Config.GetConfig().DirectoryPathExport + @"\Amazon\Amazon.csv";
        DiscontStock dSamara { get; set; }

        List<DiscontStockRecord> Records { get; set; }

        public double dollar { get; set; }

        public AmazonExporter() : base()
        {
            dSamara = new DiscontStock();
            Records = new List<DiscontStockRecord>();
            //todo перевести на нормальный источник курса
            dollar = 56;

        }

        public void run()
        {
            //string FileName = @"C:\Users\Администратор\YandexDisk\fullCatalogSneaker-icon\Parsing\discont\Stock On Hand Samara.csv";
            //dSamara.ReadStandFromCSV(Settings.DISCONT_SAMARA_FILENAME);
            //dSamara.AddSaleToRecords();
            string StockDiscont2 = Config.GetConfig().DirectoryPathParsing + @"discont\StockDiscont2.csv";
            dSamara.ReadStockFromCSV2(StockDiscont2);
            //string stockFileName = @"C:\Users\Администратор\YandexDisk\fullCatalogSneaker-icon\Export\Amazon\stock.csv";
            //dSamara.SaveStockToCSV(stockFileName);
            CreateAmazonList();

            string exportFileName = Config.GetConfig().DirectoryPathExport + @"Amazon\Amazon.csv";
            SaveStockToCSV(exportFileName);


        }

        public void run2()
        {
            dSamara.ReadStockFromCSV();
            dSamara.AddSaleToRecords();
            CreateAmazonList2();
            SaveStockToCSV(FILENAME);
        }

        public void run3()
        {
            var stockDiscont2 = Config.GetConfig().DirectoryPath + @"Parsing\discont\StockDiscont2.csv";
            dSamara.ReadStockFromCSV2(stockDiscont2);
            dSamara.AddStandtoRecords();

            NashStock1 = new NashStock();
            NashStock1.ReadStockFromCSV();

            CreateAmazonList3();
            var exportFileName = Config.GetConfig().DirectoryPath + @"Export\Amazon\Amazon.csv";
            SaveStockToCSV(exportFileName);
        }

        private void CreateAmazonList3()
        {
            foreach (var record in dSamara.Records2)
            {
                if (record.quantity > 3 && record.isBackWall == false)
                {
                    DiscontStockRecord amazonRecord = new DiscontStockRecord();
                    amazonRecord = record;
                    amazonRecord.price = GetPriceDiskont(record.price);
                    double quantity = record.quantity / 2;
                    quantity = Math.Ceiling(quantity);
                    if (quantity > 5) quantity = 5;
                    amazonRecord.quantity = (int)quantity;
                    amazonRecord.sku = record.sku + "-" + record.size;
                    //amazonRecord.upc = "\"" + record.upc + "\"";
                    Records.Add(amazonRecord);
                }
            }

            foreach (var record in NashStock1.records)
            {
                if (record.quantity > 0 && record.condition.ToLower() == "new with box")
                {
                    if (record.brand.ToLower() == "nike" || record.brand.ToLower() == "jordan")
                    {
                        if (!String.IsNullOrWhiteSpace(record.upc))
                        {
                            string sku = record.sku + "-" + record.size;
                            DiscontStockRecord amazonRecord = @GetRecordFromId(sku);
                            if (amazonRecord == null)
                            {
                                amazonRecord = new DiscontStockRecord();
                                amazonRecord.sku = sku;
                                amazonRecord.upc = record.upc;
                                amazonRecord.quantity = record.quantity;
                                amazonRecord.title = record.title;
                                amazonRecord.price = GetPriceNashStock(record.price);
                                Records.Add(amazonRecord);
                            }
                            else //если уже есть то добавляем только нужное
                            {
                                if (String.IsNullOrWhiteSpace(amazonRecord.upc) && !String.IsNullOrWhiteSpace(record.upc))
                                    amazonRecord.upc = record.upc;
                                double price = GetPrice(record.price);
                                if (amazonRecord.price > price)
                                {
                                    amazonRecord.price = price;
                                    amazonRecord.quantity = record.quantity;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CreateAmazonList2()
        {
            foreach (var sale in dSamara.sale)
            {
                foreach (var record in dSamara.records)
                {
                    if (sale.Key == record.sku)
                    {
                        if (record.quantity > 3)
                        {
                            DiscontStockRecord amazonRecord = new DiscontStockRecord();

                            amazonRecord.price = GetPrice(record.price);
                            double quantity = record.quantity / 2;
                            quantity = Math.Ceiling(quantity);
                            if (quantity > 5) quantity = 5;
                            amazonRecord.quantity = (int)quantity;
                            amazonRecord.sku = record.sku + "-" + record.size;
                            amazonRecord.upc = record.upc;
                            amazonRecord.title = record.title;

                            Records.Add(amazonRecord);
                        }
                    }
                }
            }
        }

        private void CreateAmazonList()
        {
            foreach (var record in dSamara.Records2)
            {              
                if (record.quantity > 3 && record.isBackWall == false)
                {
                    DiscontStockRecord amazonRecord = new DiscontStockRecord();
                    amazonRecord = record;
                    if (String.IsNullOrWhiteSpace(amazonRecord.upc))
                    {
                        bool test = true;
                    }
                    //double popravka = 300;
                    //double price = 54.99 + ( (record.price - popravka) / dollar );
                    //if (price < 54.99) price = 54.99;
                    //amazonRecord.price = Math.Round(price,2);
                    amazonRecord.price = GetPrice(record);
                    double quantity = record.quantity / 2;
                    quantity = Math.Ceiling(quantity);
                    if (quantity > 5) quantity = 5;
                    amazonRecord.quantity = (int)quantity;            
                    amazonRecord.sku = record.sku + "-" + record.size;
                    //amazonRecord.upc = "\"" + record.upc + "\"";
                    Records.Add(amazonRecord);
                }
            }

            //foreach (var record in NashStock1.records)
            //{
            //    if (record.quantity > 0)
            //    {
            //        string sku = record.sku + "-" + record.sizeUS;
            //        DiscontStockRecord amazonRecord = @GetRecordFromId(sku);
            //        if (amazonRecord == null)
            //        {
            //            amazonRecord = record;
            //            amazonRecord.sku = sku;
            //            amazonRecord.price = GetPrice(record);
            //            Records.Add(amazonRecord);
            //        }
            //        else //если уже есть то добавляем только нужное
            //        {
            //            if (String.IsNullOrWhiteSpace(amazonRecord.upc) && !String.IsNullOrWhiteSpace(record.upc))
            //                amazonRecord.upc = record.upc;
            //            double price = GetPrice(record);
            //            if (amazonRecord.price > price)
            //            {
            //                amazonRecord.price = price;
            //                amazonRecord.quantity = record.quantity;
            //            }
                            
            //        }
            //    }
            //}
            //throw new NotImplementedException();
        }

        public double GetPrice(DiscontStockRecord record)
        {
            double popravka = 700; //изначально было -1300, потом я сделал наценку 1000 (то есть стало -300). Теперь еще 1000 накидываю, и будет уже +700
            double price = 54.99 + ((record.price + popravka) / dollar);
            if (price < 54.99) price = 54.99;
            return Math.Round(price, 2);
        }

        public double GetPriceNashStock(double price)
        {
            double popravka = 700; //изначально было -1300, потом я сделал наценку 1000 (то есть стало -300). Теперь еще 1000 накидываю, и будет уже +700
            price = 54.99 + ((price + popravka) / dollar);
            if (price < 54.99) price = 54.99;
            return Math.Round(price, 2);
        }

        public double GetPriceDiskont(double price)
        {
            double popravka = 1200; //изначально было -1300, потом я сделал наценку 1000 (то есть стало -300). Теперь еще 1000 накидываю, и будет уже +700
            price = 54.99 + ((price + popravka) / dollar);
            if (price < 54.99) price = 54.99;
            return Math.Round(price, 2);
        }

        public double GetPrice(double price)
        {
            double popravka = 700; //изначально было -1300, потом я сделал наценку 1000 (то есть стало -300). Теперь еще 1000 накидываю, и будет уже +700
            price = 54.99 + ((price + popravka) / dollar);
            if (price < 54.99) price = 54.99;
            return Math.Round(price, 2);
        }

        public DiscontStockRecord GetRecordFromId(string sku)
        {
            for (int i = 0; i < this.Records.Count; i++)
            {
                if (this.Records[i].sku == sku) return this.Records[i];
            }
            return null;
        }

        public void SaveStockToCSV(string FileName)
        {
            using (var sw = new StreamWriter(FileName))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                //List<StockRecord> records2 = (List<StockRecord>)records;
                writer.WriteRecords(Records);
            }
        }
    }
}
