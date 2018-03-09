using CsvHelper;
using SneakerIcon.Classes.Stocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Exporters
{
    public class SneakerIconExporter : Exporter
    {
        public static readonly string DIRECTORY_PATH = Exporter.DIRECTORY_PATH + @"Sneaker-icon.ru\";
        public static readonly string FILENAME = DIRECTORY_PATH + "Sneaker-icon.ru.csv";
        public const int marzha = 1500;
        List<SneakerIconRecord> Records { get; set; }

        public SneakerIconExporter()
            : base()
        {
            Records = new List<SneakerIconRecord>();
        }

        public void Run()
        {
            CreateRecords();
            ExportToCSV(FILENAME);
            GoToFTP(FILENAME);
        }

        public void CreateRecords()
        {
            foreach (var stockRecord in NashStock1.records)
            {
                if (stockRecord.quantity > 0)
                {
                    string sku2 = stockRecord.sku + "-" + stockRecord.size;
                    //double price = GetPrice(stockRecord, "RUB");
                    var record = @GetRecordFromId(sku2);
                    if (record == null)
                    {
                        record = new SneakerIconRecord();
                        //record.SetParameters(stockRecord);
                        record.sku = stockRecord.sku;
                        record.sku2 = sku2;
                        record.sizeUS = stockRecord.size;
                        int price = (int)stockRecord.price + marzha;
                        record.price = price.ToString();
                        Records.Add(record);
                    }
                    else //если уже есть то добавляем только нужное
                    {
                        //if (String.IsNullOrWhiteSpace(tiuRecord.upc) && !String.IsNullOrWhiteSpace(stockRecord.upc))
                        //    tiuRecord.upc = stockRecord.upc;
                        int price = (int)stockRecord.price + marzha;
                        if (price < Int32.Parse(record.price))
                            record.price = price.ToString();
                    }
                }
            }

            foreach (var Discont in DiscontStocks)
            {
                foreach (var stockRecord in Discont.records)
                {
                    if (stockRecord.quantity > 0)
                    {
                        string sku2 = stockRecord.sku + "-" + stockRecord.size;
                        //double price = GetPrice(stockRecord, "RUB");
                        var record = @GetRecordFromId(sku2);
                        if (record == null)
                        {
                            record = new SneakerIconRecord();
                            //record.SetParameters(stockRecord);
                            record.sku = stockRecord.sku;
                            record.sku2 = sku2;
                            record.sizeUS = stockRecord.size;
                            int price = (int)stockRecord.price + marzha;
                            record.price = price.ToString();
                            Records.Add(record);
                        }
                        else //если уже есть то добавляем только нужное
                        {
                            //if (String.IsNullOrWhiteSpace(tiuRecord.upc) && !String.IsNullOrWhiteSpace(stockRecord.upc))
                            //    tiuRecord.upc = stockRecord.upc;
                            int price = (int)stockRecord.price + marzha;
                            if (price < Int32.Parse(record.price))
                                record.price = price.ToString();
                        }
                    }
                }
            }

            foreach (var shopStock in ShopStocks)
            {
                foreach (var stockRecord in shopStock.records)
                {
                    double price = GetPrice(stockRecord, shopStock.Currency, shopStock.Marzha);


                    string sku2 = stockRecord.sku + "-" + stockRecord.size;
                    var record = @GetRecordFromId(sku2);

                    if (record == null)
                    {
                        record = new SneakerIconRecord();
                        //tiuRecord.SetParameters(stockRecord);
                        record.sku = stockRecord.sku;
                        record.sku2 = sku2;
                        record.sizeUS = stockRecord.size;
                        record.price = Math.Round(price, 0).ToString();
                        record.images = record.images;
                        Records.Add(record);
                    }
                    else //если уже есть то добавляем только нужное
                    {
                        if (price < Double.Parse(record.price))
                            record.price = Math.Round(price, 0).ToString();
                    }
                }
            }

            //составляем новый список, отсеив сомнительные записи (которых нет в фулкаталоге и у которых категория неверная) и добавив новые данные из фулкаталога
            List<Sneaker> sneakers = new List<Sneaker>();
            //List<SneakerIconRecord> newRecords = new List<SneakerIconRecord>();
            foreach (var record in Records)
            {
                var fullCatalogSneaker = GetCatalogRecordFromId(record.sku2);
                if (fullCatalogSneaker != null)
                {
                    Sneaker sneaker = GetSneakerFromSKU(fullCatalogSneaker.sku, sneakers);
                    if (sneaker == null)
                    {
                        Sneaker newSneaker = new Sneaker();
                        newSneaker.sku = fullCatalogSneaker.sku;
                        newSneaker.title = fullCatalogSneaker.title;
                        newSneaker.type = fullCatalogSneaker.type;
                        newSneaker.sex = fullCatalogSneaker.sex;
                        newSneaker.category = fullCatalogSneaker.category;
                        newSneaker.brand = fullCatalogSneaker.brand;                        
                        newSneaker.destination = fullCatalogSneaker.destination;
                        newSneaker.height = fullCatalogSneaker.height;
                        newSneaker.description = fullCatalogSneaker.description;
                        newSneaker.color = fullCatalogSneaker.color;
                        newSneaker.collection = fullCatalogSneaker.collection;
                        newSneaker.images = fullCatalogSneaker.images;
                        
                        //проверяем на пустые изображения
                        if (newSneaker.images != null)
                        {
                            if (newSneaker.images.Count > 1 || !String.IsNullOrWhiteSpace(newSneaker.images[0]))
                            {
                                var snSize = new SneakerSize();
                                bool result = snSize.GetAllSizesFromUS(fullCatalogSneaker, record.sizeUS);
                                if (result)
                                {
                                    snSize.price = Convert.ToDouble(record.price);
                                    //record.sizeUS = snSize.allSize + " / " + snSize.sizeRU + " RUS";
                                    newSneaker.sizes.Add(snSize);
                                }
                                else
                                {
                                    Program.Logger.Warn("wrong size. sku:" + record.sku + " category: " + record.category + " size: " + record.sizeUS);
                                }

                                sneakers.Add(newSneaker);
                            }
                        }


                    }
                    else
                    {
                        var snSize = new SneakerSize();
                        bool result = snSize.GetAllSizesFromUS(sneaker, record.sizeUS);
                        if (result)
                        {
                            snSize.price = Convert.ToDouble(record.price);
                            //record.sizeUS = snSize.allSize + " / " + snSize.sizeRU + " RUS";
                            sneaker.sizes.Add(snSize);
                        }
                        else
                        {
                            Program.Logger.Warn("wrong size. sku:" + sneaker.sku + " category: " + sneaker.category + " size: " + record.sizeUS);
                        }

                    }
                }
                else
                {
                    Console.WriteLine("record is not exist in catalog. id: " + record.sku2);
                }
            }
            //Records = newRecords;

            //теперь когда сделали новый каталог кроссовок и их размеров можно делать новый список выходных данных
            List<SneakerIconRecord> newRecords = new List<SneakerIconRecord>();
            foreach (Sneaker sneaker in sneakers)
            {
                //добавляем родительский елемент
                SneakerIconRecord parentRecord = new SneakerIconRecord();
                parentRecord.sku = sneaker.sku;
                parentRecord.brand = sneaker.brand;
                parentRecord.title = sneaker.title;
                parentRecord.sex = sneaker.sex;
                parentRecord.category = sneaker.category;
                parentRecord.color = sneaker.color;
                parentRecord.type = sneaker.type;
                parentRecord.collection = sneaker.collection;
                parentRecord.description = sneaker.description;
                parentRecord.destination = sneaker.destination;
                sneaker.SortSizes();
                sneaker.SetSizesList();
                parentRecord.sizeUS = String.Join("|", sneaker.sizeListUS);
                parentRecord.sizeUK = String.Join("|", sneaker.sizeListUK);
                parentRecord.sizeEU = String.Join("|", sneaker.sizeListEU);
                parentRecord.sizeCM = String.Join("|", sneaker.sizeListCM);
                parentRecord.sizeRU = String.Join("|", sneaker.sizeListRU);
                parentRecord.images = String.Join("|", sneaker.images);

                newRecords.Add(parentRecord);

                //добавляем теперь дочерние элементы
                foreach (SneakerSize sneakerSize in sneaker.sizes)
                {
                    SneakerIconRecord ChildRecord = new SneakerIconRecord();
                    ChildRecord.sku = sneaker.sku + "-" + sneakerSize.sizeUS; //артикул размера
                    ChildRecord.sku2 = sneaker.sku; //родительский артикул   
                    ChildRecord.size = sneakerSize.allSize;
                    ChildRecord.upc = sneakerSize.upc;
                    ChildRecord.price = sneakerSize.price.ToString();
                    ChildRecord.quantity = "1";

                    newRecords.Add(ChildRecord);
                }
            }
            Records = newRecords;
        }

        public Sneaker GetSneakerFromSKU(string sku, List<Sneaker> sneakers)
        {
            if (sku == null) throw new Exception("sku = null");
            for (int i = 0; i < sneakers.Count; i++)
            {
                if (sku == sneakers[i].sku)
                {
                    return @sneakers[i];
                }
            }
            return null;
        }

        /// <summary>
        /// получаем конечную цену (маржа и доставка уже включены)
        /// </summary>
        /// <param ftpFolder="price">себестоимость</param>
        /// <param ftpFolder="currency">валюта</param>
        /// <returns>возвращает конечную цену продажи</returns>
        public double GetPrice(StockRecord record, string currency, int Marzha = 0)
        {
            if (currency == "RUB" && Marzha == 0)
            {
                Marzha = marzha; //это для дисконта, там без маржи вызов функции, тут исправил а в других экспортерах возможно это осталось
                //но возможно у каких-то магазинов тоже это осталось
            }
            if (currency == "RUB")
            {
                Marzha = marzha;
            }
            double price = Exporter.GetPrice(record, currency, "RUB", Marzha);
            return price;


            //double resultPrice;
            //if (currency == "USD")
            //{
            //    if (record.sellPrice > 0)
            //    {
            //        resultPrice = record.sellPrice;
            //        return resultPrice * Settings.USD_SELL;
            //    }
            //    else
            //    {
            //        resultPrice = record.price + Marzha;
            //        return resultPrice * Settings.USD_BUY;
            //        //throw new Exception("нет цены продажи sku:" + record.sku);
            //    }
            //}
            //if (currency == "EUR")
            //{
            //    if (record.sellPrice > 0)
            //    {
            //        resultPrice = record.sellPrice * Settings.EURO_BUY;
            //        return resultPrice;
            //    }
            //    else
            //    {
            //        if (Marzha == 0)
            //            throw new Exception("marzha = 0. sku: " + record.sku);
            //        record.sellPrice = record.price + Marzha;
            //        resultPrice = record.sellPrice * Settings.EURO_BUY;
            //        return resultPrice;
            //    }
            //}
            //if (currency == "CHF")
            //{
            //    if (record.sellPrice > 0)
            //    {
            //        resultPrice = record.sellPrice * Settings.CHF_BUY;
            //        return resultPrice;
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
            //            resultPrice = record.price + marzha;
            //            return resultPrice;
            //        }
            //    }
            //    resultPrice = record.price + marzha;
            //    return resultPrice;
            //}
            //throw new NotImplementedException();
        }

        public SneakerIconRecord GetRecordFromId(string sku2)
        {
            for (int i = 0; i < this.Records.Count; i++)
            {
                if (this.Records[i].sku2 == sku2) return this.Records[i];
            }
            return null;
        }

        public void ExportToCSV(string filename)
        {
            using (var sw = new StreamWriter(filename, false, Encoding.UTF8))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                writer.WriteRecords(Records);
            }
        }

        private void GoToFTP(string FILENAME)
        {
            // Создаем объект FtpWebRequest - он указывает на файл, который будет создан
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://s07.webhost1.ru/sneaker-icon.ru.csv");
            // устанавливаем метод на загрузку файлов
            request.Method = WebRequestMethods.Ftp.UploadFile;
            //login pass
            request.Credentials = new NetworkCredential("amaro_export", "38273827");

            // создаем поток для загрузки файла
            FileStream fs = new FileStream(FILENAME, FileMode.Open);
            byte[] fileContents = new byte[fs.Length];
            fs.Read(fileContents, 0, fileContents.Length);
            fs.Close();
            request.ContentLength = fileContents.Length;

            // пишем считанный в массив байтов файл в выходной поток
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            // получаем ответ от сервера в виде объекта FtpWebResponse
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }
    }
}
