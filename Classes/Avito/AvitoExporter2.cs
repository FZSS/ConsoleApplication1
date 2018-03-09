using CsvHelper;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.Exporters;
using SneakerIcon.Classes.Stocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Avito
{
    public class AvitoExporter2 : Exporter
    {
        public static readonly string DIRECTORY_PATH = Exporter.DIRECTORY_PATH + @"Avito2\";
        public static readonly string FILENAME = DIRECTORY_PATH + "Avito.csv";
        public static readonly string POST_LIST_FILENAME = DIRECTORY_PATH + "post_list.csv";
        public const string IMAGES_FOLDER_NAME = @"C:\Users\Администратор\YandexDisk\sneaker-icon\Parsing\Images\";
        public const int marzha = 1500;
        public const string login = "info@samarawater.ru";
        public const string pass = "happysun3827";
        public const string phone = "88002001121";
        public Dictionary<string, string> post_list { get; set; }
        public List<AvitoRecord> Records { get; set; }
        public Catalog avitoCatalog { get; set; }

        public AvitoExporter2()
            : base()
        {
            Records = new List<AvitoRecord>();
            post_list = new Dictionary<string, string>();
            avitoCatalog = new Catalog();
        }

        public void Run()
        {
            ReadPostList();
            CreateCatalog();
            ExportToCSV(FILENAME);
        }

        private void ReadPostList()
        {
            //string JSON_FILENAME = @"C:\Users\Администратор\YandexDisk\fullCatalogSneaker-icon\Export\Avito\post_list.csv";
            string filename = POST_LIST_FILENAME;
            string[] stockArray = File.ReadAllLines(filename);
            //List<StockRecord> stockList = new List<StockRecord>();
            for (int i = 1; i < stockArray.Length; i++)
            {
                string[] stockStringRecords = stockArray[i].Split(';');

                if (stockStringRecords[0] != String.Empty)
                {
                    string sku = stockStringRecords[0].Trim();
                    string date = stockStringRecords[1].Trim();
                    post_list.Add(sku, date);
                }
            }
        }

        private void CreateCatalog()
        {
            //nashstock
            //foreach (var stockRecord in NashStock1.records)
            //{
            //    if (stockRecord.quantity > 0)
            //    {
            //        var sneaker = @GetSneaker(stockRecord);
            //        if (sneaker != null)
            //        {
            //            //проверим, есть ли уже этот размер в списке размеров этих кросс
            //            var record = @sneaker.GetStockRecord(stockRecord);
            //            if (record != null)
            //            {
            //                Program.logger.Warn("Два одинаковых размера в нашем стоке. sku2:" + stockRecord.sku + "-" + stockRecord.sizeUS);
            //            }
            //            else
            //            {
            //                if (stockRecord.sellPrice != 0)
            //                {
            //                    stockRecord.price = stockRecord.sellPrice;
            //                }
            //                else
            //                {
            //                    stockRecord.price += marzha;
            //                }
            //                sneaker.StockRecords.Add(stockRecord);
            //            }
            //            if (avitoCatalog.isNotExistSneakerInCatalog(sneaker))
            //            {
            //                avitoCatalog.sneakers.Add(sneaker);
            //            }
            //        }

            //    }
            //}

            //discont
            foreach (var Discont in DiscontStocks)
            {
                foreach (var stockRecord in Discont.records)
                {
                    if (stockRecord.quantity > 0)
                    {
                        var sneaker = @GetSneaker(stockRecord);
                        if (sneaker != null)
                        {
                            //проверим, есть ли уже этот размер в списке размеров этих кросс
                            var record = @sneaker.GetStockRecord(stockRecord);
                            if (record == null)
                            {
                                stockRecord.price += marzha;
                                sneaker.StockRecords.Add(stockRecord);
                            }
                            else
                            {
                                //если размер этот уже есть, то проверяем цену, если цена нового размера меньше, ставим ее
                                double price = stockRecord.price + marzha;
                                if (record.price > price)
                                {
                                    record.price = price;
                                }
                            }
                            if (avitoCatalog.isNotExistSneakerInCatalog(sneaker))
                            {
                                avitoCatalog.sneakers.Add(sneaker);
                            }
                        }
                    }
                }
            }

            //shops
            foreach (var shopStock in ShopStocks)
            {
                foreach (var stockRecord in shopStock.records)
                {
                    //price
                    int price = 100000;
                    if (shopStock.Currency == "EUR")
                    {
                        double doublePrice = (stockRecord.price + 40) * Settings.EURO_BUY; //40 евро доставка, маржу позже добавляю еще
                        price = (int)doublePrice;
                    }
                    else if (shopStock.Currency == "RUB")
                    {
                        price = (int)stockRecord.price;
                    }
                    else
                    {
                        throw new Exception("Неверная валюта");
                    }
                    int sellPrice = price + marzha;

                    var sneaker = @GetSneaker(stockRecord);
                    if (sneaker != null)
                    {
                        var record = @sneaker.GetStockRecord(stockRecord);
                        if (record == null)
                        {
                            stockRecord.price = sellPrice;
                            sneaker.StockRecords.Add(stockRecord);
                        }
                        else
                        {
                            if (sellPrice < record.price)
                            {
                                record.price = sellPrice;
                            }
                        }
                        if (avitoCatalog.isNotExistSneakerInCatalog(sneaker))
                        {
                            avitoCatalog.sneakers.Add(sneaker);
                        }
                    }

                }
            } //end shops

            foreach (var sneaker in avitoCatalog.sneakers)
            {
                var avitoRecord = SetParameters(sneaker);
                if (avitoRecord != null)
                {
                    Records.Add(avitoRecord);
                }
            }
        }

        private Sneaker GetSneaker(StockRecord stockRecord)
        {
            //провекра на существование в постинг листе
            bool isPostList = this.post_list.ContainsKey(stockRecord.sku);
            if (isPostList)
            {
                return null;
            }

            //проверяем есть ли этот артикул в списке
            var sneaker = avitoCatalog.GetSneakerFromSKU(stockRecord.sku);
            if (sneaker == null) //добавляем новый кросс
            {
                sneaker = catalog.GetSneakerFromSKU(stockRecord.sku);
                if (sneaker == null)
                {
                    Program.Logger.Info("is not exist in catalog");
                    return null;
                }
            }

            return sneaker;
        }

        //private void CreateRecords()
        //{
        //    //nashstock
        //    foreach (var stockRecord in NashStock1.records)
        //    {
        //        if (stockRecord.quantity > 0)
        //        {
        //            var tiuRecord = SetParameters(stockRecord);
        //            if (tiuRecord != null)
        //            {
        //                if (stockRecord.sellPrice != 0)
        //                {
        //                    tiuRecord.price = stockRecord.sellPrice.ToString();
        //                }
        //                else
        //                {
        //                    int price = (int)stockRecord.price + marzha;
        //                    tiuRecord.price = price.ToString();
        //                }
        //                Records.Add(tiuRecord);
        //            }
        //        }
        //    }

        //    //discont
        //    foreach (var Discont in DiscontStocks)
        //    {
        //        foreach (var stockRecord in Discont.records)
        //        {
        //            if (stockRecord.quantity > 0)
        //            {
        //                var tiuRecord = @GetRecord(stockRecord.sku);
        //                if (tiuRecord == null)
        //                {
        //                    tiuRecord = SetParameters(stockRecord);
        //                    if (tiuRecord != null)
        //                    {
        //                        int price = (int)stockRecord.price + marzha;
        //                        tiuRecord.price = price.ToString();
        //                        Records.Add(tiuRecord);
        //                    }                            
        //                }
        //                else //если уже есть то добавляем только нужное
        //                {
        //                    int price = (int)stockRecord.price + marzha;
        //                    if (price < Int32.Parse(tiuRecord.price))
        //                        tiuRecord.price = price.ToString();
        //                }
        //            }
        //        }
        //    }

        //    //shops
        //    foreach (var shopStock in ShopStocks)
        //    {
        //        foreach (var stockRecord in shopStock.records)
        //        {
        //            //price
        //            int price = 100000;
        //            if (shopStock.Currency == "EUR")
        //            {
        //                double doublePrice = (stockRecord.price + 40) * Settings.EURO_BUY; //40 евро доставка, маржу позже добавляю еще
        //                price = (int)doublePrice;
        //            }
        //            else if (shopStock.Currency == "RUB")
        //            {
        //                price = (int)stockRecord.price;
        //            }
        //            else
        //            {
        //                throw new Exception("Неверная валюта");
        //            }
        //            int sellPrice = price + marzha;


        //            var tiuRecord = @GetRecord(stockRecord.sku);
        //            if (tiuRecord == null)
        //            {
        //                tiuRecord = SetParameters(stockRecord);
        //                if (tiuRecord != null)
        //                {
        //                    tiuRecord.price = sellPrice.ToString();
        //                    Records.Add(tiuRecord);
        //                }                       
        //            }
        //            else //если уже есть то добавляем только нужное
        //            {
        //                if (sellPrice < Int32.Parse(tiuRecord.price))
        //                    tiuRecord.price = price.ToString();
        //            }
        //        }
        //    } //end shops


        //}

        //public AvitoRecord GetRecord(string sku)
        //{
        //    for (int i = 0; i < this.Records.Count; i++)
        //    {
        //        if (this.Records[i].sku == sku) return this.Records[i];
        //    }
        //    return null;
        //}

        public AvitoRecord SetParameters(Sneaker sneaker)
        {
            AvitoRecord avitoRecord = new AvitoRecord(login,pass,phone);

            //categorySneakerFullCatalog
            if (sneaker.category == "Мужская")
            {
                avitoRecord.category = "Самара -> Одежда, обувь, аксессуары -> Мужская одежда -> Обувь -> 42р";
            }
            else if (sneaker.category == "Женская")
            {
                avitoRecord.category = "Самара -> Одежда, обувь, аксессуары -> Женская одежда -> Обувь -> 37р";
            }
            else if (sneaker.category == Settings.CATEGORY_KIDS)
            {
                avitoRecord.category = "Самара -> Детская одежда и обувь -> Для мальчиков -> Обувь -> 30р";
            }
            else
            {
                Program.Logger.Warn("Wrond category. sku:" + sneaker.sku);
                return null;
            }

            //title 
            avitoRecord.title = "Новые оригинальные " + sneaker.title;

            //description
            //description
            string desc = "Новые оригинальные " + sneaker.type.ToLower() + " " + sneaker.title + "\r\n";
            desc += "Артикул: " + sneaker.sku + "\r\n";
            desc += "Размеры:\r\n";

            if (sneaker.StockRecords.Count == 0)
            {
                return null;
            }
            else
            {
                avitoRecord.price = ((int)sneaker.StockRecords[0].price).ToString();
            }

            foreach (var stockRecord in sneaker.StockRecords)
            {
                var size = new SneakerSize(sneaker, stockRecord.size);
                if (size.GetAllSizesFromUS(sneaker, size.sizeUS))
                {
                    desc += "- " + size.allSize + " / " + size.sizeRU + " RUS. Стоимость: " + stockRecord.price + "\r\n";
                }
                else
                {
                    Program.Logger.Warn("wrong size. sku:" + sneaker.sku + " category: " + sneaker.category + " size: " + size.sizeUS);
                    return null;
                }
            }
            desc += "По поводу других размеров звоните или пишите в сообщения Авито. Возможно они также есть в наличии\r\n";
            desc += "------------------------\r\n";
            desc += "Вся обувь оригинальная, новая, в упаковке\r\n";
            desc += "------------------------\r\n";
            desc += "БЕСПЛАТНАЯ доставка по Москве в день заказа или на следующий рабочий день.\r\n";
            desc += "БЕСПЛАТНАЯ доставка по Самаре в день заказа или на следующий рабочий день.\r\n";
            desc += "------------------------\r\n";
            desc += "Доставка по России 3-5 дней компанией СДЭК.\r\n";
            desc += "Стоимость доставки по РФ - 300 рублей.\r\n";
            desc += "------------------------\r\n";
            desc += "Доставка в регионы только по 100% предоплате, наложенным платежом не отправляем\r\n";
            desc += "Звоните или пишите в сообщения Авито\r\n";
            avitoRecord.description = desc;

            //images
            var images = new List<string>();
            for (int i = 1; i <= 10; i++)
            {
                string imageFileName = sneaker.sku + "-" + i + ".jpg";
                if (File.Exists(IMAGES_FOLDER_NAME + imageFileName))
                {
                    images.Add(imageFileName);
                }
            }
            if (images.Count > 0)
            {
                avitoRecord.images = String.Join(",", images);
            }
            else {
                return null;
            }

            return avitoRecord;
        }

        private void ExportToCSV(string FILENAME)
        {
            using (var sw = new StreamWriter(FILENAME, false, Encoding.GetEncoding(1251)))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                writer.WriteRecords(Records);
            }
        }


    }
}
