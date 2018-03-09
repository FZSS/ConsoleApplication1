using CsvHelper;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.Stocks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes
{
    public class SiteExporter
    {
        /*
        public List<Sneaker> sneakers { get; set; }
        public List<StockRecord> queensStockList { get; set; }
        public List<StockRecord> discontStockList { get; set; }
        public List<StockRecord> streetbeatStockList { get; set; }
        public List<StockRecord> nashStockList { get; set; }
        public List<StockRecord> basketShopStockList { get; set; }
        public List<StockRecord> einhalbStockList { get; set; }
        public List<AllStockRecord> allStockList { get; set; }
        public List<SiteExportRecord> siteExportList { get; set; }
        public List<SiteExportRecord> siteExportList_ver2 { get; set; }
        public List<BonanzaRecord> bonanzaList { get; set; }
        public FullCatalog fullCatalog { get; set; }

        public SiteExporter()
        {
            sneakers = new List<Sneaker>();
            queensStockList = new List<StockRecord>();
            discontStockList = new List<StockRecord>();
            streetbeatStockList = new List<StockRecord>();
            nashStockList = new List<StockRecord>();
            allStockList = new List<AllStockRecord>();
            siteExportList = new List<SiteExportRecord>();
            siteExportList_ver2 = new List<SiteExportRecord>();
            bonanzaList = new List<BonanzaRecord>();
            basketShopStockList = new List<StockRecord>();
            einhalbStockList = new List<StockRecord>();
            fullCatalog = new FullCatalog();
            sneakers = fullCatalog.sneakers;

            Run();
        }

        public void Run()
        {
            //this.ReadCatalogFromCSV(@"C:\Users\Администратор\YandexDisk\stockSneaker-icon\Excel\CSV\Catalogs.csv");
            this.ReadQueensStockFromCSV(@"C:\Users\Администратор\YandexDisk\sneaker-icon\Excel\CSV\QueensStock.csv");
            this.ReadDiscontStockFromCSV(@"C:\Users\Администратор\YandexDisk\sneaker-icon\Excel\CSV\DiscontStock.csv");
            this.ReadNashStockFromCSV(@"C:\Users\Администратор\YandexDisk\sneaker-icon\Excel\CSV\NashStock.csv");
            this.ReadStreetBeatStockFromCSV(@"C:\Users\Администратор\YandexDisk\sneaker-icon\Excel\CSV\StreetBeatStock.csv");
            basketShopStockList = ReadStockFromCSV(Settings.DIRECTORY_PATH_PARSING + @"basketshop.ru\StockBasketshop.ru.csv");
            //einhalbStockList = ReadStockFromCSV(Settings.DIRECTORY_PATH_PARSING + @"43einhalb.com\Stock43einhalb.com.csv");
            ReadEinhalbStockFromCSV(Settings.DIRECTORY_PATH_PARSING + @"43einhalb.com\Stock43einhalb.com.csv");

            this.CreateAllStockList(Settings.DIRECTORY_PATH_PARSING + @"AllStock.csv");
            Console.WriteLine("CreateAllStockList done");

            //this.CreateSiteExportList(@"C:\Users\Администратор\YandexDisk\stockSneaker-icon\Excel\CSV\SneakerIconExport.csv");
            //Console.WriteLine("CreateSiteExportList done");

            //this.CreateSneakerSizeExportCSV_ver2(@"C:\Users\Администратор\YandexDisk\stockSneaker-icon\Excel\CSV\SneakerIconExportWithParent.csv");
            //Console.WriteLine("CreateSiteExportListWithParent done");

            //this.CreateBonanzaExportFile(@"C:\Users\Администратор\YandexDisk\stockSneaker-icon\Excel\CSV\BonanzaExport.csv");
            //Console.WriteLine("CreateBonanzaExportFile done");

            Console.WriteLine("Выполнение программы завершено. Чтобы закрыть нажмите любую клавишу");
            Console.ReadLine();
        }

        public List<StockRecord> ReadStockFromCSV(string FileNameCatalog)
        {
            using (var sr = new StreamReader(FileNameCatalog))
            {
                var reader = new CsvReader(sr);
                reader.Configuration.Delimiter = ";";
                IEnumerable<StockRecord> records = reader.GetRecords<StockRecord>();
                return records.ToList();
            }
        }

        public void ReadCatalogFromCSV(string JSON_FILENAME) {
            //string JSON_FILENAME = @"C:\SneakerIcon\CSV\Catalogs.csv";
            using (var sr = new StreamReader(JSON_FILENAME, Encoding.GetEncoding(1251)))
            {
                sneakers = new FullCatalog().sneakers;
                //var reader = new CsvReader(sr);

                ////reader.Configuration.Encoding = Encoding.GetEncoding(1251);
                //reader.Configuration.Delimiter = ";";

                ////CSVReader will now read the whole file into an enumerable
                //IEnumerable<CatalogRecord> records = reader.GetRecords<CatalogRecord>();

                ////var links = records.ToArray();

                //foreach (var record in records)
                //{
                //    Sneaker stockSneaker = new Sneaker();
                //    stockSneaker.sku = record.sku.Trim();
                //    TextInfo ti = new CultureInfo("en-us", false).TextInfo;
                //    stockSneaker.title = ti.ToTitleCase(record.title.ToLower()); //перевожу название В Верхние Регистр Первая Буква Слова Для Яндекс Маркета
                //    stockSneaker.color = record.color;
                //    stockSneaker.brand = record.brand;
                //    stockSneaker.collection = record.collection;
                //    stockSneaker.type = record.type;
                //    stockSneaker.destination = record.destination;
                //    stockSneaker.description = record.description;
                //    stockSneaker.categorySneakerFullCatalog = record.categorySneakerFullCatalog;
                //    stockSneaker.sex = record.sex;
                //    stockSneaker.images = record.images.Split('|').ToList();
                //    //stockSneaker.price = Int32.Parse(record.streetbeat);
                //    stockSneaker.queensLink = record.queensLink;
                //    sneakers.Add(stockSneaker);
                //}
            }
        }

        public void ReadQueensStockFromCSV(string JSON_FILENAME)
        {
            //string JSON_FILENAME = @"C:\SneakerIcon\CSV\QueensStock.csv";
            string[] stockArray = File.ReadAllLines(JSON_FILENAME);
            List<StockRecord> stockList = new List<StockRecord>();
            for (int i = 1; i < stockArray.Length; i++)
            {
                string[] stockStringRecords = stockArray[i].Split(';');
                StockRecord stockRecord = new StockRecord();
                stockRecord.stockName = "queens";
                stockRecord.sku = stockStringRecords[0].Trim();
                stockRecord.sizeUS = stockStringRecords[1].Trim();
                stockRecord.quantity = Int32.Parse(stockStringRecords[2]);
                stockRecord.price = Int32.Parse(stockStringRecords[3]);
                stockRecord.price = stockRecord.price + 1500;
                stockList.Add(stockRecord);
            }
            queensStockList = stockList;
        }

        public void ReadEinhalbStockFromCSV(string JSON_FILENAME)
        {
            //string JSON_FILENAME = @"C:\SneakerIcon\CSV\QueensStock.csv";
            string[] stockArray = File.ReadAllLines(JSON_FILENAME);
            List<StockRecord> stockList = new List<StockRecord>();
            for (int i = 1; i < stockArray.Length; i++)
            {
                string[] stockStringRecords = stockArray[i].Split(';');
                StockRecord stockRecord = new StockRecord();
                stockRecord.stockName = "einhalb";
                stockRecord.sku = stockStringRecords[1].Trim();
                stockRecord.sizeUS = stockStringRecords[2].Trim();
                stockRecord.quantity = Int32.Parse(stockStringRecords[4]);
                stockRecord.price = Double.Parse(stockStringRecords[5]);
                stockRecord.price = Math.Round(((stockRecord.price * 0.82468) + 33.11) * 74);
                stockRecord.price = stockRecord.price + 1500;

                stockList.Add(stockRecord);
            }
            einhalbStockList = stockList;
        }

        public void ReadDiscontStockFromCSV(string JSON_FILENAME)
        {
            //string JSON_FILENAME = @"C:\SneakerIcon\CSV\DiscontStock.csv";
            string[] stockArray = File.ReadAllLines(JSON_FILENAME);
            List<StockRecord> stockList = new List<StockRecord>();
            for (int i = 1; i < stockArray.Length; i++)
            {
                string[] stockStringRecords = stockArray[i].Split(';');
                StockRecord stockRecord = new StockRecord();
                stockRecord.stockName = "discont";
                stockRecord.sku = stockStringRecords[2].Trim();
                stockRecord.sizeUS = stockStringRecords[3].Trim();
                if (!String.IsNullOrEmpty(stockStringRecords[4]))
                {
                    stockRecord.upc = stockStringRecords[4].Substring(2);
                }
                stockRecord.quantity = Int32.Parse(stockStringRecords[7].Replace("(","").Replace(")","")); //в файле дисконта иногда кол-во встречается в формате (1)
                stockRecord.price = Int32.Parse(stockStringRecords[6]);
                stockRecord.price = stockRecord.price + 1500;
                stockList.Add(stockRecord);
            }
            discontStockList = stockList;
        }

        public void ReadNashStockFromCSV(string JSON_FILENAME)
        {
            //string JSON_FILENAME = @"C:\SneakerIcon\CSV\NashStock.csv";
            string[] stockArray = File.ReadAllLines(JSON_FILENAME);
            List<StockRecord> stockList = new List<StockRecord>();
            for (int i = 1; i < stockArray.Length; i++)
            {
                string[] stockStringRecords = stockArray[i].Split(';');
                StockRecord stockRecord = new StockRecord();
                stockRecord.stockName = "nashstock";
                stockRecord.sku = stockStringRecords[2].Trim();
                if (stockRecord.sku != String.Empty)
                {
                    stockRecord.sizeUS = stockStringRecords[3].Trim();
                    string upc = stockStringRecords[4];
                    if (!String.IsNullOrEmpty(upc))
                    {
                        if (upc.Count() == 11)
                        {
                            upc = "0" + upc;
                        }
                        if (upc.Count() == 12)
                        {
                            stockRecord.upc = upc;
                        }
                    }
                    int result;
                    bool istruequantity = Int32.TryParse(stockStringRecords[7], out result);
                    if (istruequantity)
                    {
                        stockRecord.quantity = result;
                    }
                    else
                    {
                        stockRecord.quantity = 1;
                    }
                    stockRecord.price = Int32.Parse(stockStringRecords[5]);
                    bool isSalePrice = Int32.TryParse(stockStringRecords[6],out result);
                    if (isSalePrice)
                    {
                        stockRecord.price = Int32.Parse(stockStringRecords[6]);
                    }
                    else {
                        stockRecord.price = stockRecord.price + 1500;
                    }
                    stockList.Add(stockRecord);
                }  
            }
            nashStockList = stockList;
        }

        public void ReadStreetBeatStockFromCSV(string JSON_FILENAME)
        {
            //string JSON_FILENAME = @"C:\SneakerIcon\CSV\StreetBeatStock.csv";
            string[] stockArray = File.ReadAllLines(JSON_FILENAME);
            List<StockRecord> stockList = new List<StockRecord>();
            for (int i = 1; i < stockArray.Length; i++)
            {
                string[] stockStringRecords = stockArray[i].Split(';');
                StockRecord stockRecord = new StockRecord();
                stockRecord.stockName = "nashstock";
                stockRecord.sku = stockStringRecords[0].Trim();
                stockRecord.sizeUS = stockStringRecords[1].Trim();
                stockRecord.quantity = Int32.Parse(stockStringRecords[2]);
                stockRecord.price = Int32.Parse(stockStringRecords[3]) + 1500;
                //stockRecord.sale_price = stockRecord.price + 1500;
                stockList.Add(stockRecord);
            }
            streetbeatStockList = stockList;
        }

        public void CreateAllStockList(string JSON_FILENAME)
        {
            for (int i = 0; i < queensStockList.Count; i++)
            {
                var stockListRecord = queensStockList[i];
                int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.sizeUS);
                if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                {
                    AllStockRecord allStockRecord = new AllStockRecord();
                    allStockRecord.sku = stockListRecord.sku;
                    int index = FindSneakerSKU(allStockRecord.sku);
                    if (index != -1)
                    {
                        allStockRecord.brand = sneakers[index].brand;
                        allStockRecord.title = sneakers[index].title;
                    }
                    else
                    {
                        throw new Exception("Артикула " + allStockRecord.sku + " нет в файле каталога");
                    }
                    allStockRecord.sizeUS = stockListRecord.sizeUS;
                    //allStockRecord.queens_quantity = stockListRecord.quantity;
                    allStockRecord.queens_price = stockListRecord.price;
                    allStockList.Add(allStockRecord);
                }
                else
                {
                    Console.WriteLine("Дубликат в стоке квинса: " + stockListRecord.sku + "-" + stockListRecord.sizeUS);
                }
            }

            for (int i = 0; i < nashStockList.Count; i++)
            {
                var stockListRecord = nashStockList[i];
                int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.sizeUS);

                if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                {
                    AllStockRecord allStockRecord = new AllStockRecord();
                    allStockRecord.sku = stockListRecord.sku;
                    allStockRecord.sizeUS = stockListRecord.sizeUS;
                    allStockRecord.nash_quantity = stockListRecord.quantity;
                    allStockRecord.nash_price = stockListRecord.price;
                    allStockRecord.nashSellPrice = stockListRecord.price;

                    int index = FindSneakerSKU(allStockRecord.sku);
                    if (index != -1)
                    {
                        allStockRecord.brand = sneakers[index].brand;
                        allStockRecord.title = sneakers[index].title;
                        allStockList.Add(allStockRecord);
                    }
                    else
                    {
                        Console.WriteLine("Артикула " + allStockRecord.sku + " нет в файле каталога");
                        //throw new Exception("Артикула " + allStockRecord.sku + " нет в файле каталога");
                    }

                    
                }
                else { //если данный артикул и размер есть в другом стоке и уже добавлен в общий список
                    allStockList[indexAllStockListRecord].nash_quantity = stockListRecord.quantity;
                    allStockList[indexAllStockListRecord].nash_price = stockListRecord.price;
                }
            }

            for (int i = 0; i < discontStockList.Count; i++)
            {
                var stockListRecord = discontStockList[i];

                if (stockListRecord.quantity > 0)
                {
                    int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.sizeUS);

                    if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                    {
                        AllStockRecord allStockRecord = new AllStockRecord();
                        allStockRecord.sku = stockListRecord.sku;
                        allStockRecord.sizeUS = stockListRecord.sizeUS;
                        allStockRecord.discont_quantity = stockListRecord.quantity;
                        allStockRecord.discont_price = stockListRecord.price;

                        int index = FindSneakerSKU(allStockRecord.sku);
                        if (index != -1)
                        {
                            allStockRecord.brand = sneakers[index].brand;
                            allStockRecord.title = sneakers[index].title;
                        }
                        else
                        {
                            Console.WriteLine("Артикула " + allStockRecord.sku + " нет в файле каталога");
                            //throw new Exception("Артикула " + allStockRecord.sku + " нет в файле каталога");
                        }

                        allStockList.Add(allStockRecord);
                    }
                    else
                    { //если данный артикул и размер есть в другом стоке и уже добавлен в общий список
                        allStockList[indexAllStockListRecord].discont_quantity = stockListRecord.quantity;
                        allStockList[indexAllStockListRecord].discont_price = stockListRecord.price;
                    }
                }   
            }

            for (int i = 0; i < streetbeatStockList.Count; i++)
            {
                var stockListRecord = streetbeatStockList[i];
                int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.sizeUS);

                if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                {
                    AllStockRecord allStockRecord = new AllStockRecord();
                    allStockRecord.sku = stockListRecord.sku;
                    allStockRecord.sizeUS = stockListRecord.sizeUS;
                    //allStockRecord.streetbeat_quantity = stockListRecord.quantity;
                    allStockRecord.streetbeat_price = stockListRecord.price;

                    int index = FindSneakerSKU(allStockRecord.sku);
                    if (index != -1)
                    {
                        allStockRecord.brand = sneakers[index].brand;
                        allStockRecord.title = sneakers[index].title;
                    }
                    else
                    {
                        Console.WriteLine("Артикула " + allStockRecord.sku + " нет в файле каталога");
                        //throw new Exception("Артикула " + allStockRecord.sku + " нет в файле каталога");
                    }

                    allStockList.Add(allStockRecord);
                }
                else
                { //если данный артикул и размер есть в другом стоке и уже добавлен в общий список
                    //allStockList[indexAllStockListRecord].streetbeat_quantity = stockListRecord.quantity;
                    allStockList[indexAllStockListRecord].streetbeat_price = stockListRecord.price;
                }
            }

            for (int i = 0; i < basketShopStockList.Count; i++)
            {
                var stockListRecord = basketShopStockList[i];
                int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.sizeUS);

                if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                {
                    AllStockRecord allStockRecord = new AllStockRecord();
                    allStockRecord.sku = stockListRecord.sku;
                    allStockRecord.sizeUS = stockListRecord.sizeUS;
                    //allStockRecord.streetbeat_quantity = stockListRecord.quantity;
                    allStockRecord.basketshop_price = stockListRecord.price + 1500;

                    int index = FindSneakerSKU(allStockRecord.sku);
                    if (index != -1)
                    {
                        allStockRecord.brand = sneakers[index].brand;
                        allStockRecord.title = sneakers[index].title;
                    }
                    else
                    {
                        Console.WriteLine("Артикула " + allStockRecord.sku + " нет в файле каталога");
                        //throw new Exception("Артикула " + allStockRecord.sku + " нет в файле каталога");
                    }

                    allStockList.Add(allStockRecord);
                }
                else
                { //если данный артикул и размер есть в другом стоке и уже добавлен в общий список
                    //allStockList[indexAllStockListRecord].streetbeat_quantity = stockListRecord.quantity;
                    allStockList[indexAllStockListRecord].basketshop_price = stockListRecord.price;
                }
            }

            for (int i = 0; i < einhalbStockList.Count; i++)
            {
                var stockListRecord = einhalbStockList[i];
                int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.sizeUS);

                if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                {
                    AllStockRecord allStockRecord = new AllStockRecord();
                    allStockRecord.sku = stockListRecord.sku;
                    allStockRecord.sizeUS = stockListRecord.sizeUS;
                    //allStockRecord.streetbeat_quantity = stockListRecord.quantity;
                    allStockRecord.einhalb_price = stockListRecord.price;

                    int index = FindSneakerSKU(allStockRecord.sku);
                    if (index != -1)
                    {
                        allStockRecord.brand = sneakers[index].brand;
                        allStockRecord.title = sneakers[index].title;
                    }
                    else
                    {
                        Console.WriteLine("Артикула " + allStockRecord.sku + " нет в файле каталога");
                        //throw new Exception("Артикула " + allStockRecord.sku + " нет в файле каталога");
                    }

                    allStockList.Add(allStockRecord);
                }
                else
                { //если данный артикул и размер есть в другом стоке и уже добавлен в общий список
                    //allStockList[indexAllStockListRecord].streetbeat_quantity = stockListRecord.quantity;
                    allStockList[indexAllStockListRecord].einhalb_price = stockListRecord.price;
                }
            }

            AddSizesToSneakersFromAllStockList(); //в массиве sneakers к каждому stockSneaker добавляю размеры и склады, на которых эти размеры есть
            //SortSizes();

            SaveAllStockListToCSV(JSON_FILENAME);
        }

        /// <summary>
        /// Сортирует в каждом артикуле размеры по возрастанию (на основе евро размеров)
        /// </summary>
        private void SortSizes()
        {
            for (int i = 0; i < sneakers.Count; i++)
            {
                var sneaker = sneakers[i];
                SneakerSize minSize;
                for (int ii = 0; ii < sneaker.sizes.Count; ii++)
                {
                    for (int jj = ii+1; jj < sneaker.sizes.Count; jj++)
                    {
                        Double sizeI = 0;
                        Double sizeJ = 0;

                        sizeI = Double.Parse(sneaker.sizes[ii].sizeEU.Replace('.', ','));
                        sizeJ = Double.Parse(sneaker.sizes[jj].sizeEU.Replace('.', ','));

                        
                        if (sizeJ < sizeI)
                        {
                            minSize = sneaker.sizes[jj];
                            sneaker.sizes[jj] = sneaker.sizes[ii];
                            sneaker.sizes[ii] = minSize;
                        } 
                    }
                }
            }
        }

        /// <summary>
        /// в массиве sneakers к каждому stockSneaker добавляю размеры и склады, на которых эти размеры есть
        /// </summary>
        public void AddSizesToSneakersFromAllStockList()
        {
            for (int i = 0; i < allStockList.Count; i++)
            {
                
                var allStockRecord = allStockList[i];
                if (!String.IsNullOrWhiteSpace(allStockRecord.sizeUS))
                {
                    int index = FindSneakerSKU(allStockRecord.sku);
                    var sneaker = @sneakers[index];
                    var sneakerSize = new SneakerSize(sneaker, allStockRecord.sizeUS);
                    //sneakerSize.sizeUS = allStockRecord.sizeUS;
                    sneakerSize.upc = FindUPC(allStockRecord.sku, allStockRecord.sizeUS);

                    if (allStockRecord.nash_quantity > 0)
                    {
                        sneakerSize.stock.Add(new SneakerSizeStock("nashstock", allStockRecord.nash_quantity, allStockRecord.nash_price, allStockRecord.nashSellPrice));
                    }

                    if (allStockRecord.queens_quantity > 0)
                    {
                        sneakerSize.stock.Add(new SneakerSizeStock("queens", allStockRecord.queens_quantity, allStockRecord.queens_price, 0));
                    }

                    if (allStockRecord.discont_quantity > 0)
                    {
                        sneakerSize.stock.Add(new SneakerSizeStock("discont", allStockRecord.discont_quantity, allStockRecord.discont_price, 0));
                    }

                    if (allStockRecord.streetbeat_quantity > 0)
                    {
                        sneakerSize.stock.Add(new SneakerSizeStock("streetbeat", allStockRecord.streetbeat_quantity, allStockRecord.streetbeat_price, 0));
                    }

                    sneaker.sizes.Add(sneakerSize);
                }
                else
                {
                    Console.WriteLine("У кроссовок нет размера sku:" + allStockRecord.sku);
                }
            }
        }

        public void CreateSiteExportList(string JSON_FILENAME)
        {
            for (int i = 0; i < allStockList.Count; i++)
            {
                var stockRecord = allStockList[i];
                SiteExportRecord siteExportRecord = new SiteExportRecord();
                siteExportRecord.sku = stockRecord.sku;
                siteExportRecord.brand = stockRecord.brand;
                siteExportRecord.title = stockRecord.title;
                
                siteExportRecord.sku2 = stockRecord.sku + "-" + stockRecord.sizeUS;
                siteExportRecord.upc = FindUPC(stockRecord.sku, stockRecord.sizeUS);

                int index = FindSneakerSKU(siteExportRecord.sku);
                if (index != -1)
                {
                    var sneaker = sneakers[index];
                    siteExportRecord.color = sneaker.color;
                    siteExportRecord.sex = sneaker.sex;
                    siteExportRecord.type = sneaker.type;
                    siteExportRecord.collection = sneaker.collection;
                    siteExportRecord.categorySneakerFullCatalog = sneaker.categorySneakerFullCatalog;
                    siteExportRecord.destination = sneaker.destination;
                    siteExportRecord.description = sneaker.description.Replace("\r", "<br>");

                    //размера для вариаций заносим
                    //var converterMan = new SizeConverter(stockRecord.sizeUS, stockSneaker.categorySneakerFullCatalog);
                    try
                    {
                        var converterMan = new SizeConverter(stockRecord.sizeUS, sneaker.categorySneakerFullCatalog);
                        siteExportRecord.sizeUS = converterMan.allSize;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Размер и категория не совпадают. sku:" + siteExportRecord.sku + " sizeUS:" + stockRecord.sizeUS);
                    }
                    

                    //размеры все заносим
                    //List<string> sizeListAll = new List<string>();
                    List<string> sizeListUS = SizeList(siteExportRecord);
                    List<string> sizeListUK = new List<string>();
                    List<string> sizeListEUR = new List<string>();
                    List<string> sizeListCM = new List<string>();
                    List<string> sizeListRUS = new List<string>();
                    foreach (var sizeUS in sizeListUS)
                    {
                        try
                        {
                            var converterMan = new SizeConverter(sizeUS, sneaker.categorySneakerFullCatalog);
                            //sizeListAll.Add(converterMan.allSize);
                            sizeListUK.Add(converterMan.sizeUK);
                            sizeListEUR.Add(converterMan.sizeEUR);
                            sizeListCM.Add(converterMan.sizeCM);
                            sizeListRUS.Add(converterMan.sizeRUS);

                            //siteExportRecord.sizeUS = converterMan.allSize;
                            //siteExportRecord.sizeUS = converterMan.sizeUS;
                            //siteExportRecord.sizeUK = converterMan.sizeUK;
                            //siteExportRecord.sizeEUR = converterMan.sizeEUR;
                            //siteExportRecord.sizeCM = converterMan.sizeCM;
                            //siteExportRecord.sizeRUS = converterMan.sizeRUS;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Размер и категория не совпадают. sku:" + siteExportRecord.sku + " sizeUS:" + stockRecord.sizeUS);
                        }
                    }
                    //sizeListAll.Sort();
                    sizeListUS.Sort();
                    sizeListEUR.Sort();
                    sizeListUK.Sort();
                    sizeListCM.Sort();
                    sizeListRUS.Sort();
                    //siteExportRecord.sizeUS = String.Join("|", sizeListAll);
                    siteExportRecord.sizeUS = String.Join("|", sizeListUS);
                    siteExportRecord.sizeUK = String.Join("|", sizeListUK);
                    siteExportRecord.sizeEU = String.Join("|", sizeListEUR);
                    siteExportRecord.sizeCM = String.Join("|", sizeListCM);
                    siteExportRecord.sizeRU = String.Join("|", sizeListRUS);
                }
                else
                {
                    throw new Exception("Артикула " + siteExportRecord.sku + " нет в файле каталога");
                }
                
                //выбираем склад с самой низкой ценой
                double minPrice = 1000000;
                int quantity = -1;
                string stockName = String.Empty;
                if (stockRecord.nash_quantity > 0)
                {
                    minPrice = stockRecord.nash_price;
                    quantity = stockRecord.nash_quantity;
                    stockName = "nashstock";
                }
                if (stockRecord.queens_quantity > 0 && stockRecord.queens_price < minPrice)
                {
                    minPrice = stockRecord.queens_price;
                    quantity = stockRecord.queens_quantity;
                    stockName = "queens";
                }
                if (stockRecord.discont_quantity > 0 && stockRecord.discont_price < minPrice)
                {
                    minPrice = stockRecord.discont_price;
                    quantity = stockRecord.discont_quantity;
                    stockName = "discont";
                }
                if (stockRecord.streetbeat_quantity > 0 && stockRecord.streetbeat_price < minPrice)
                {
                    minPrice = stockRecord.streetbeat_price;
                    quantity = stockRecord.streetbeat_quantity;
                    stockName = "streetbeat";
                }
                if (quantity == -1)
                {
                    throw new Exception("Артикул " + stockRecord.sku + " размер " + stockRecord.sizeUS + " не найдена мин цена");
                }
                siteExportRecord.price = minPrice.ToString();
                siteExportRecord.quantity = quantity.ToString();
                siteExportRecord.stockName = stockName;
                siteExportList.Add(siteExportRecord);
            }
            
            SaveSiteExportListToCSV(JSON_FILENAME);
        }

        public void CreateSneakerSizeExportCSV_ver2(string JSON_FILENAME)
        {
            for (int i = 0; i < sneakers.Count; i++)
            {
                var sneaker = sneakers[i];

                //добавляем родительский продукт
                SiteExportRecord parentRecord = new SiteExportRecord();
                //siteExportRecord.sku2 = stockSneaker.sku;
                parentRecord.sku = sneaker.sku;
                parentRecord.brand = sneaker.brand;
                parentRecord.title = sneaker.title;
                parentRecord.sex = sneaker.sex;
                parentRecord.categorySneakerFullCatalog = sneaker.categorySneakerFullCatalog;
                parentRecord.color = sneaker.color;
                parentRecord.type = sneaker.type;
                parentRecord.collection = sneaker.collection;
                parentRecord.description = sneaker.description;
                parentRecord.destination = sneaker.destination;
                sneaker.SetSizesList();
                parentRecord.sizeUS = String.Join("|", sneaker.sizeListUS);
                parentRecord.sizeUK = String.Join("|", sneaker.sizeListUK);
                parentRecord.sizeEU = String.Join("|", sneaker.sizeListEU);
                parentRecord.sizeCM = String.Join("|", sneaker.sizeListCM);
                parentRecord.sizeRU = String.Join("|", sneaker.sizeListRU);
                parentRecord.images = String.Join("|", sneaker.GetImagesStringForSneakerIcon());
                //parentRecord.market_category = "Все товары/Одежда, обувь и аксессуары/Обувь";

                siteExportList_ver2.Add(parentRecord);

                for (int j = 0; j < sneaker.sizes.Count; j++)
                {
                    SiteExportRecord ChildRecord = new SiteExportRecord();
                    SneakerSize sneakerSize = sneaker.sizes[j];
                    ChildRecord.sku = sneakerSize.sku; //артикул размера
                    ChildRecord.sku2 = sneaker.sku; //родительский артикул   
                    ChildRecord.sizeUS = sneakerSize.allSize;
                    ChildRecord.upc = sneakerSize.upc;                    
                    var minPriceStock = sneakerSize.GetMinPriceStock();
                    ChildRecord.price = minPriceStock.price.ToString();
                    ChildRecord.quantity = minPriceStock.quantity.ToString();
                    ChildRecord.stockName = minPriceStock.stockName;

                    siteExportList_ver2.Add(ChildRecord);
                }
            }
            SaveSiteExportListToCSV_ver2(JSON_FILENAME);
        }

        public void CreateBonanzaExportFile(string JSON_FILENAME)
        {
            for (int i = 0; i < allStockList.Count; i++)
            {
                var stockRecord = allStockList[i];

                string sex;
                string categorySneakerFullCatalog;
                string sku = stockRecord.sku;
                string brand = stockRecord.brand;
                string title = stockRecord.title;
                string sizeUS = stockRecord.sizeUS;
                string upc = FindUPC(stockRecord.sku, stockRecord.sizeUS);

                int index = FindSneakerSKU(sku);
                if (index != -1)
                {
                    var sneaker = sneakers[index];

                    sex = sneaker.sex;
                    categorySneakerFullCatalog = sneaker.categorySneakerFullCatalog;    
                }
                else
                {
                    throw new Exception("Артикула " + sku + " нет в файле каталога");
                }

                //выбираем склад с самой низкой ценой
                double price = stockRecord.getMinPrice();

                BonanzaRecord bonanzaRecord = new BonanzaRecord(brand, title, sku, sizeUS, upc, sex, categorySneakerFullCatalog, price);
                bonanzaList.Add(bonanzaRecord);
            }
            SaveBonanzaCSV(JSON_FILENAME);
        }
        
        private void SaveBonanzaCSV(string JSON_FILENAME)
        {
            //string JSON_FILENAME = @"C:\Users\Администратор\YandexDisk\stockSneaker-icon\Excel\CSV\BonanzaExport.csv";
            using (var sw = new StreamWriter(JSON_FILENAME))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ",";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                writer.WriteRecords(bonanzaList);
            }
        }

        public List<string> SizeList(SiteExportRecord siteExportRecord)
        {
            List<string> sizeList = new List<string>();

            for (int i = 0; i < allStockList.Count; i++)
            {
                AllStockRecord record = allStockList[i];
                if (siteExportRecord.sku == record.sku)
                {
                    sizeList.Add(record.sizeUS);
                }
            }
            return sizeList;
        }

        public void SaveSiteExportListToCSV(string JSON_FILENAME) {
            //string JSON_FILENAME = @"C:\Users\Администратор\YandexDisk\stockSneaker-icon\Excel\CSV\SneakerIconExport.csv";
            //string JSON_FILENAME = @"C:\SneakerIcon\CSV\SneakerIconExport.csv";
            using (var sw = new StreamWriter(JSON_FILENAME))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                writer.WriteRecords(siteExportList);
            }
        }

        public void SaveSiteExportListToCSV_ver2(string JSON_FILENAME)
        {
            //string JSON_FILENAME = @"C:\Users\Администратор\YandexDisk\stockSneaker-icon\Excel\CSV\SneakerIconExport.csv";
            //string JSON_FILENAME = @"C:\SneakerIcon\CSV\SneakerIconExport.csv";
            using (var sw = new StreamWriter(JSON_FILENAME))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                writer.WriteRecords(siteExportList_ver2);
            }
        }

        public string FindUPC(string sku, string sizeUS) {
            for (int i = 0; i < discontStockList.Count; i++)
            {
                if (discontStockList[i].sku == sku && discontStockList[i].sizeUS == sizeUS)
                {
                    return discontStockList[i].upc;
                }
            }
            for (int i = 0; i < nashStockList.Count; i++)
            {
                if (nashStockList[i].sku == sku && nashStockList[i].sizeUS == sizeUS)
                {
                    return nashStockList[i].upc;
                }
            }
            return null;
        }

        public void SaveAllStockListToCSV(string JSON_FILENAME)
        {
            //string JSON_FILENAME = @"C:\SneakerIcon\CSV\AllStock.csv";
            using (var sw = new StreamWriter(JSON_FILENAME))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                writer.WriteRecords(allStockList);
            }
        }

        public int FindAllStockListRecord(string sku, string sizeUS)
        {
            for (int i = 0; i < allStockList.Count; i++)
            {
                if (allStockList[i].sku == sku && allStockList[i].sizeUS == sizeUS)
                {
                    return i;
                }
            }
            return -1;
        }

        public int FindSneakerSKU(string sku)
        {
            for (int i = 0; i < sneakers.Count; i++)
            {
                if (sku == sneakers[i].sku)
                {
                    return i;
                }
            }
            return -1;
        }
        */
    }
}
