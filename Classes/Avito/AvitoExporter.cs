using CsvHelper;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.Stocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Avito
{
    public class AvitoExporter
    {
        public const string IMAGES_FOLDER_NAME = @"C:\Users\Администратор\YandexDisk\sneaker-icon\Parsing\Images";
        public FullCatalog fullCatalog { get; set; }
        public List<StockRecord> records { get; set; }
        public Catalog catalog { get; set; }
        public Dictionary<string,string> post_list { get; set; }

        public AvitoExporter()
        {
            fullCatalog = new FullCatalog();
            //records = new List<StockRecord>();
            catalog = new Catalog();
            string manFileName = @"C:\Users\Администратор\YandexDisk\sneaker-icon\Parsing\discont_msk_kuzminki\man.csv";
            string womanFileName = @"C:\Users\Администратор\YandexDisk\sneaker-icon\Parsing\discont_msk_kuzminki\woman.csv";

            post_list = new Dictionary<string,string>();
            ReadPostList();
            
            ReadStockFromCSV("Мужская", manFileName);
            ReadStockFromCSV("Женская", womanFileName);
            LoadImages();
            SaveToCSV();
        }

        private void ReadPostList()
        {


            string filename = @"C:\Users\Администратор\YandexDisk\sneaker-icon\Export\Avito\post_list.csv";
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

        private void SaveToCSV()
        {
            string filename = @"C:\SneakerIcon\avito.csv";
            string filenameCatalog = filename;
            //string filenameCatalog = @"C:\SneakerIcon\CSV\StreetBeat\CatalogStreetBeat.csv";
            var sneakers = this.catalog.sneakers;
            int count = sneakers.Count;
            List<AvitoRecord> avitoRecordList = new List<AvitoRecord>();
            for (int i = 0; i < count; i++)
            {
                
                var sneaker = sneakers[i];
                var fullCatalogSneaker = fullCatalog.GetSneakerFromSKU(sneaker.sku);


                //title
                
                if (fullCatalogSneaker == null)
                {
                    Program.Logger.Info("is not exist in catalog");
                    //record.title = fullCatalogSneaker.title + " " + fullCatalogSneaker.sku;
                }
                else
                {
                    //работаем только с объявлениями которые есть в каталоге

                    AvitoRecord record = new AvitoRecord();
                    record.login = "info@samarawater.ru";
                    record.pass = "happysun3827";
                    record.phone = "88002001121";
                    if (fullCatalogSneaker.category == "Мужская")
                    {
                        record.category = "Москва -> Одежда, обувь, аксессуары -> Мужская одежда -> Обувь -> 42р";
                    }
                    else if (fullCatalogSneaker.category == "Женская")
                    {
                        record.category = "Москва -> Одежда, обувь, аксессуары -> Женская одежда -> Обувь -> 37р";
                    }
                    else
                    {
                        Program.Logger.Warn("Wrond category. sku:" + sneaker.sku);
                    }

                    //title
                    record.title = fullCatalogSneaker.title + " " + sneaker.sku;
                    
                    
                    //sku
                    record.sku = sneaker.sku;

                    //price
                    record.price = (sneaker.price + 1000).ToString();

                    //description
                    string desc = fullCatalogSneaker.type + " " + fullCatalogSneaker.title + "\r\n";
                    desc += "Артикул: " + sneaker.sku + "\r\n";
                    desc += "Размеры:\r\n";

                    foreach (var size in sneaker.sizes)
                    {
                        size.GetAllSizesFromUS(fullCatalogSneaker,size.sizeUS);
                        desc += "- " + size.allSize + " / " + size.sizeRU + " RUS\r\n";
                    }
                    //desc += "\r\n";
                    desc += "По поводу других размеров звоните или пишите в сообщения Авито. Возможно они также есть в наличии\r\n";
                    desc += "------------------------\r\n";
                    desc += "Вся обувь оригинальная, новая, в упаковке\r\n";
                    desc += "------------------------\r\n";
                    desc += "БЕСПЛАТНАЯ доставка по Москве в день заказа или на следующий рабочий день.\r\n";
                    desc += "------------------------\r\n";
                    desc += "Доставка по России 2-3 дня компанией СДЭК.\r\n";
                    desc += "Стоимость доставки по РФ - 300 рублей.\r\n";
                    desc += "Отправка в регионы только по 100% предоплате. Наложенным платежом не отправляем\r\n";

                    record.description += desc;

                    record.images = String.Join(",", sneaker.images);

                    bool isPostList = this.post_list.ContainsKey(sneaker.sku);

                    if (!String.IsNullOrWhiteSpace(record.images) && !isPostList)
                        avitoRecordList.Add(record);
                }
               

            }
            //var streamWriter = new Stream(filenameCatalog);
            using (var sw = new StreamWriter(filenameCatalog, false, Encoding.GetEncoding(1251)))
            {
                //sw.Encoding = Encoding.GetEncoding(1251);
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                //writer.Configuration.Encoding = Encoding.GetEncoding(1251);
                writer.WriteRecords(avitoRecordList);
            }
        }

        private void LoadImages()
        {
            foreach (var sneaker in catalog.sneakers)
            {
                for (int i = 1; i <= 10; i++)
                {
                    string imageFileName = sneaker.sku + "-" + i + ".jpg";
                    if (File.Exists(IMAGES_FOLDER_NAME + imageFileName))
                    {
                        sneaker.images.Add(imageFileName);
                    }
                }
            }

        }

        public void ReadStockFromCSV(string category, string filename)
        {


            //string JSON_FILENAME = @"C:\SneakerIcon\CSV\NashStock.csv";
            string[] stockArray = File.ReadAllLines(filename);
            //List<StockRecord> stockList = new List<StockRecord>();
            for (int i = 1; i < stockArray.Length; i++)
            {
                string[] stockStringRecords = stockArray[i].Split(';');
                StockRecord stockRecord = new StockRecord();
                
                //sku
                stockRecord.sku = stockStringRecords[0].Trim() + "-" + stockStringRecords[1].Trim();

                if (stockStringRecords[0] != String.Empty)
                {
                    //title
                    stockRecord.title = stockStringRecords[4].Trim();

                    //sizeUS
                    stockRecord.size = stockStringRecords[2].Trim();

                    //upc
                    string upc = stockStringRecords[3].Trim();
                    if (!String.IsNullOrEmpty(upc))
                    {
                        if (upc.Count() == 11)
                        {
                            upc = "0" + upc;
                        }
                        else if (upc.Count() == 12)
                        {
                            stockRecord.upc = upc;
                        }
                        else if (upc.Count() == 14)
                        {
                            stockRecord.upc = upc.Substring(2);
                        }
                        else throw new Exception("Wrong UPC");
                    }

                    

                    //quantity
                    int result;
                    string quantity = stockStringRecords[6].Replace("(", "").Replace(")", "");
                    bool istruequantity = Int32.TryParse(quantity, out result);
                    if (istruequantity)
                    {
                        //price
                        stockRecord.price = Double.Parse(stockStringRecords[5]);

                        stockRecord.quantity = result;
                        if (result > 0)
                        {
                            //Если количество больше 0, то тогда добавляем размер в каталог
                            AddToCatalog(stockRecord, category);
                        }
                    }
                    else
                    {
                        Program.Logger.Warn("Wrong Quantity. Sku: " + stockRecord.sku);
                        //stockRecord.quantity = 1;
                    }



                    //stockList.Add(stockRecord);
                }
            }
            //records = stockList;
        }

        private bool AddToCatalog(StockRecord stockRecord, string category)
        {
            Sneaker sneaker = catalog.GetSneakerFromSKU(stockRecord.sku);
            if (sneaker == null)
            {
                sneaker = new Sneaker();
                sneaker.sku = stockRecord.sku;
                sneaker.category = category;
                sneaker.title = stockRecord.title;
                sneaker.price = stockRecord.price;

                SneakerSize size = new SneakerSize();
                size.sizeUS = stockRecord.size;
                size.GetAllSizesFromUS(sneaker, stockRecord.size);
                size.upc = stockRecord.upc;
                sneaker.sizes.Add(size);

                catalog.sneakers.Add(sneaker);
            }
            else {
                if (sneaker.GetSize(stockRecord.size) != null)
                {
                    Program.Logger.Warn("Duplicate size. sku: " + stockRecord.sku + " size: " + stockRecord.size);
                    return false;
                }

                SneakerSize size = new SneakerSize();
                size.sizeUS = stockRecord.size;
                size.GetAllSizesFromUS(sneaker, stockRecord.size);
                size.upc = stockRecord.upc;
                sneaker.sizes.Add(size);
            }
            return true;
        }
    }
}
