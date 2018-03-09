using CsvHelper;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.Parsing;
using SneakerIcon.Classes.SizeConverters;
using SneakerIcon.Classes.SizeConverters.Model;
using SneakerIcon.Classes.Stocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Exporters
{
    public class BonanzaExporter : Exporter
    {
        public static readonly string DIRECTORY_PATH = Exporter.DIRECTORY_PATH + @"Bonanza\";
        public static readonly string FILENAME = DIRECTORY_PATH + "Bonanza.csv";
        List<BonanzaRecord> Records { get; set; }
        public const int marzha = 1500;
        public int dostavkaFromRussia = 2000;
        public int dostavkaEUR_EUR = 10;
        public int dostavkaEUR_USA = 33;
        //public int eurBuy = 71;
        //public int usdSell = 62;
        public double VAT = 0.17;

        public BonanzaExporter() : base()
        {
            Records = new List<BonanzaRecord>();
        }

        public void Run()
        {
            CreateRecords();
            CheckNumRecordWithUPC();
            LoadUPC();
            CheckNumRecordWithUPC();
            ExportToCSV(FILENAME);
        }

        private void CheckNumRecordWithUPC()
        {
            int i = 0;
            foreach (var record in Records)
            {
                if (!String.IsNullOrWhiteSpace(record.upc)) i++;
            }
            Program.Logger.Info("Записей с UPC: " + i);
        }

        private void LoadUPC()
        {
            //подгружаем базу юпс с фтп
            var upcdb = new UPCDB.UPCDB2();
            upcdb.Initialize();

            int i = 0;
            foreach (var bonanzaRecord in Records)
            {
                if (String.IsNullOrWhiteSpace(bonanzaRecord.upc))
                {
                    string upc = AddUpcToBonanzaRecord(bonanzaRecord, upcdb);
                    if (upc != null)
                    {
                        bonanzaRecord.upc = upc;
                        i++;
                    }
                }
            }

            //после прохода нужно сохранить файл с базой юпс и залить на фтп (потому что наверняка новые юпс туда подгрузились

            Program.Logger.Info(i + " UPC заполнили из базы UPCDB");
        }

        private string AddUpcToBonanzaRecord(BonanzaRecord bonanzaRecord, UPCDB.UPCDB2 upcdb)
        {           
            //определяем артикул данной бонанза записи
            var separator = new String[] { "-" };
            var id = bonanzaRecord.id.Split(separator, StringSplitOptions.None);
            var sku = id[0] + "-" + id[1];
            var sizeUsBonanzaRecord = id[2];

            //ищем этот кросс в базе юпс
            var upcdbSneakers = upcdb.myUPCDB.sneakers;
            var sneakerUpcdb = upcdbSneakers.Find(x => x.sku == sku);

            if (sneakerUpcdb == null)
            {
                //дописать, чтобы подгружались юпс для этого артикула из онлайн базы через краулеру
                //sneakerUpcdb = upcdb.AddSneakerToUpcdb(string sku);
                return null;
            }
            else
            {
                if (sneakerUpcdb.sizes != null)
                {//надо найти размер нужный, что может быть не так уж просто
                    string upc = GetUpcFromSneakerUpcdb(sneakerUpcdb, sizeUsBonanzaRecord);
                    return upc;
                }
                else
                {
                    return null;
                }
            }
            //throw new NotImplementedException();
        }

        private string GetUpcFromSneakerUpcdb(UPCDB.SneakerJson sneakerUpcdb, string sizeUsBonanzaRecord)
        {
            var category = sneakerUpcdb.category;
            
            //проходим по всем размерам кроссовка из базы юпс и ищем размер бонанзы рекорд         
            foreach (var size in sneakerUpcdb.sizes)
            {
                var sizeUpcdbSneaker = size.size;
                if (String.IsNullOrWhiteSpace(sizeUpcdbSneaker))
                {
                    //если размер пустой, скорее всего ничего уже не сделать, 
                    //но может попозже какие-то мысли появятся 
                    //по парсингу размера из названия или еще как-то
                }
                else
                {
                    //если размер не пуст, то скорее всего смогу его определить
                    if (String.IsNullOrWhiteSpace(category))
                    {
                        //если категория пустая, то это жопа какая-то, как этот кросс тут оказался?
                        throw new Exception("если категория пустая, то это жопа какая-то, как этот кросс тут оказался?");
                    }
                    else
                    {
                        if (sizeUpcdbSneaker.Contains(sizeUsBonanzaRecord))
                        {
                            //если этот размер кроссовка в базе юпс содержит размер бонанзы рекорд, значит это потенциально нужный нам размер
                            //но может быть так что во взрослый размер попадет детский, или же в размер 6 попадет юпс размера 6.5
                            
                            bool test = true;

                            if (category == Settings.CATEGORY_MEN)
                            {
                                bool test2 = true;
                                sizeUpcdbSneaker = sizeUpcdbSneaker.Replace("D(M) US", "").Trim();          
                                sizeUpcdbSneaker = sizeUpcdbSneaker.Replace("D - Medium", "").Trim();
                                sizeUpcdbSneaker = sizeUpcdbSneaker.Replace("M US", "").Trim();
                                sizeUpcdbSneaker = sizeUpcdbSneaker.Replace("US", "").Trim();
                                bool isValidate = ValidateSize(sizeUpcdbSneaker,sneakerUpcdb.category);
                                if (isValidate)
                                {
                                    if (sizeUsBonanzaRecord == sizeUpcdbSneaker)
                                    {
                                        return size.upc;
                                    }
                                }
                                else
                                {
                                    bool test10 = true;
                                }

                            }
                            else if (category == Settings.CATEGORY_WOMEN)
                            {
                                bool test2 = true;
                                sizeUpcdbSneaker = sizeUpcdbSneaker.Replace("B(M) US", "").Trim();
                                sizeUpcdbSneaker = sizeUpcdbSneaker.Replace("B - Medium", "").Trim();
                                bool isValidate = ValidateSize(sizeUpcdbSneaker,sneakerUpcdb.category);
                                if (isValidate)
                                {
                                    if (sizeUsBonanzaRecord == sizeUpcdbSneaker)
                                    {
                                        return size.upc;
                                    }
                                }
                                else
                                {
                                    bool test10 = true;
                                }
                            }
                            else if (category == Settings.CATEGORY_KIDS)
                            {
                                bool test2 = true;
                                sizeUpcdbSneaker = sizeUpcdbSneaker.Replace("B(M) US", "").Trim();
                                sizeUpcdbSneaker = sizeUpcdbSneaker.Replace("B - Medium", "").Trim();
                                bool isValidate = ValidateSize(sizeUpcdbSneaker, sneakerUpcdb.category);
                                if (isValidate)
                                {
                                    if (sizeUsBonanzaRecord == sizeUpcdbSneaker)
                                    {
                                        return size.upc;
                                    }
                                }
                                else
                                {
                                    bool test10 = true;
                                }
                            }
                            else
                            {
                                //если ни в одну из категорий не попали, то это жопа, какая ж тогда у кросса категория??
                                throw new Exception("wrong category");
                            }
                        }
                    }
                }
            }
            return null; //если до сюда дошел процесс значит не нашли нужный размер, соотв. и юпс тоже
            //throw new NotImplementedException();
        }

        private bool ValidateSize(string sizeUS, string category)
        {
            category = Helper.ConvertCategoryRusToEng(category);
            SizeChart sizeChart = SizeConverter.ReadSizeChartJson();
            var menSizes = sizeChart.sizes.FindAll(x => x.category == category);
            var size = menSizes.Find(x => x.us == sizeUS);
            if (size == null)
            {
                return false;
            }
            else
            {
                return true;
            }
            //throw new NotImplementedException();
        }

        public void CreateRecords()
        {
            //Program.logger.Info("Nashstock exporting...");
            //foreach (var stockRecord in NashStock1.records)
            //{
            //    if (stockRecord.quantity > 0)
            //    {
            //        BonanzaRecord bRecord = new BonanzaRecord();
            //        bRecord.id = stockRecord.sku + "-" + stockRecord.size;
            //        bRecord.size = stockRecord.size;
            //        bRecord.upc = stockRecord.upc;
            //        //double sellPrice = stockRecord.sellPrice;
            //        //if (sellPrice == 0)
            //        //    sellPrice = stockRecord.price + marzha;
            //        //double price = (sellPrice + dostavkaFromRussia) / Settings.USD_SELL;
            //        double price = Exporter.GetPrice2(stockRecord.price, NashStockParser.VAT_VALUE, NashStockParser.DELIVERY_TO_USA, NashStockParser.MARZHA, NashStockParser.CURRENCY, "USD");

            //        bRecord.price = Math.Round(price, 2).ToString();
            //        Records.Add(bRecord);
            //    }
            //}

            //Program.logger.Info("Discont Samara exporting...");
            //foreach (var Discont in DiscontStocks)
            //{
            //    foreach (var stockRecord in Discont.records)
            //    {
            //        if (stockRecord.quantity > 3)
            //        {
            //            string id = stockRecord.sku + "-" + stockRecord.size;
            //            //double price = GetPrice(stockRecord, "RUB", marzha);
            //            double price = Exporter.GetPrice2(stockRecord.price, DiscontSamaraParser.VAT_VALUE, DiscontSamaraParser.DELIVERY_TO_USA, DiscontSamaraParser.MARZHA, DiscontSamaraParser.CURRENCY, "USD");
            //            BonanzaRecord bRecord = @GetRecordFromId(id);
            //            if (bRecord == null)
            //            {
            //                bRecord = new BonanzaRecord();
            //                bRecord.id = id;
            //                bRecord.size = stockRecord.size;
            //                bRecord.upc = stockRecord.upc;
            //                bRecord.price = Math.Round(price, 2).ToString();
            //                Records.Add(bRecord);
            //            }
            //            else //если уже есть то добавляем только нужное
            //            {
            //                if (String.IsNullOrWhiteSpace(bRecord.upc) && !String.IsNullOrWhiteSpace(stockRecord.upc))
            //                    bRecord.upc = stockRecord.upc;
            //                if (Double.Parse(bRecord.price) > price)
            //                    bRecord.price = price.ToString();
            //            }
            //        }
            //    }
            //}
            

            foreach (var shopStock in ShopStocks)
            {
                if (shopStock.Currency == "RUB")
                {
                    //пропускаем этот магазин, работаем только с зарубежными магазами
                    bool test = true;
                }
                else
                {
                    Program.Logger.Info(shopStock.Name + " exporting...");
                    foreach (var stockRecord in shopStock.records)
                    {
                        //price
                        //double price = GetPrice(stockRecord, shopStock.Currency, shopStock.Marzha);
                        double price = Exporter.GetPrice2(stockRecord.price, shopStock.VatValue, shopStock.DeliveryToUSA, shopStock.Marzha, shopStock.Currency, "USD");
                        //double price = Exporter.GetPrice(stockRecord, shopStock.Currency, "USD", shopStock.Marzha);
                        string id = stockRecord.sku + "-" + stockRecord.size;

                        BonanzaRecord bRecord = @GetRecordFromId(id);
                        if (bRecord == null)
                        {
                            bRecord = new BonanzaRecord();
                            bRecord.id = id;
                            bRecord.size = stockRecord.size;
                            bRecord.upc = stockRecord.upc;
                            bRecord.price = Math.Round(price, 2).ToString();
                            Records.Add(bRecord);
                        }
                        else //если уже есть то добавляем только нужное
                        {
                            //double price = stockRecord.price + marzha;
                            if (Double.Parse(bRecord.price) > price)
                                bRecord.price = Math.Round(price, 2).ToString();
                        }
                    }
                }
            }

            //стоки объединили теперь добавляем данные из каталога

            List<BonanzaRecord> newRecords = new List<BonanzaRecord>();
            foreach (var bRecord in Records)
            {
                var sneaker = GetCatalogRecordFromId(bRecord.id);
                if (sneaker != null) {
                    //sneaker.brand = "Nike";
                    bool result = bRecord.run(sneaker.brand, sneaker.title, sneaker.sku, bRecord.size, bRecord.upc, sneaker.sex, sneaker.category, double.Parse(bRecord.price), sneaker.images);
                    if (result)
                    {
                        newRecords.Add(bRecord);
                    }
                }
                else
                {
                    Console.WriteLine("bRecord is not exist in catalog. id: " + bRecord.id);
                }
            }

            //добавляем расширенное описание Description2
            int i = 0;
            foreach (var record in newRecords)
            {
                i++;
                var sneaker = GetCatalogRecordFromId(record.id);
                var sizes = BonanzaExporter.GetAllSizesFromSku(sneaker.sku,newRecords);
                var sizeString = BonanzaRecord.CreateSizeString(record.size,sneaker.category);
                //record.description = BonanzaRecord.SetDescription2(record.title, sneaker.sku, sizeString, sneaker.brand, record.condition, sizes);
                record.description = BonanzaRecord.CreateDescription2Small(record.title, sneaker.sku, sizeString, sizes);
            }

            Records = newRecords;
        }

        public static List<string> GetAllSizesFromSku(string sku, List<BonanzaRecord> records) {
            var sizes = new List<string>();

            var skuRecords = records.FindAll(x => x.id.Contains(sku));
            if (skuRecords != null)
            {
                if (skuRecords.Count > 0)
                {
                    foreach (var record in skuRecords)
                    {
                        sizes.Add(record.size);
                    }
                }
            }
            sizes.Sort();

            return sizes;
        }

        public Sneaker GetCatalogRecordFromId(string id)
        {
            string sku = id.Split('-')[0] + "-" + id.Split('-')[1];
            foreach (var sneaker in catalog.sneakers)
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
        public double GetPrice(StockRecord record, string currency, int Marzha = 0)
        {
            if (currency == "RUB" && Marzha == 0)
            {
                Marzha = marzha; //это для дисконта, там без маржи вызов функции, тут исправил а в других экспортерах возможно это осталось
                //но возможно у каких-то магазинов тоже это осталось
            }
            if (currency == "RUB")
            {
                Marzha = marzha + dostavkaFromRussia;
            }
            double price = Exporter.GetPrice(record, currency, "USD", Marzha);
            return price;


            //double resultPrice;
            //if (currency == "USD")
            //{
            //    if (record.sellPrice > 0)
            //    {
            //        resultPrice = record.sellPrice;
            //        return resultPrice;
            //    }
            //    else
            //    {
            //        resultPrice = record.price + Marzha;
            //        return resultPrice;
            //        //throw new Exception("нет цены продажи sku:" + record.sku);
            //    }
            //}
            //if (currency == "EUR")
            //{
            //    if (record.sellPrice > 0)
            //    {
            //        resultPrice = record.sellPrice * Settings.EURO_BUY;
            //        return resultPrice / Settings.USD_SELL;
            //    }
            //    else
            //    {
            //        if (Marzha == 0)
            //            throw new Exception("marzha = 0. sku: " + record.sku);
            //        record.sellPrice = record.price + Marzha;
            //        resultPrice = record.sellPrice * Settings.EURO_BUY;
            //        return resultPrice / Settings.USD_SELL;
            //    }
            //}
            //if (currency == "CHF")
            //{
            //    if (record.sellPrice > 0)
            //    {
            //        resultPrice = record.sellPrice * Settings.CHF_BUY;
            //        return resultPrice / Settings.USD_SELL;
            //    }
            //    else
            //    {
            //        throw new Exception("нет цены продажи sku:" + record.sku);
            //    }
            //}
            //else if (currency == "RUB")
            //{
            //    var sneaker = catalog.GetSneakerFromSKU(record.sku);
            //    if (sneaker != null)
            //    {
            //        if (sneaker.type == "Сланцы")
            //        {
            //            resultPrice = record.price + marzha + 200;
            //            return resultPrice / Settings.USD_SELL;
            //        }
            //    }
            //    resultPrice = record.price + marzha + dostavkaFromRussia;
            //    return resultPrice / Settings.USD_SELL;
            //}
            //else
            //{
            //    resultPrice = 99999;
            //    return resultPrice;
            //    //throw new Exception("Неверная валюта");
            //}
        }

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
