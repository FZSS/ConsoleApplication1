using CsvHelper;
using SneakerIcon.Classes.Parsing;
using SneakerIcon.Classes.Stocks;
using SneakerIcon.Classes.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Exporters
{
    public class AllStockExporter : Exporter
    {
        private string FILENAME = Config.GetConfig().DirectoryPathParsing + @"allstock\AllStock.csv";
        public List<AllStockRecord> allStockList { get; set; }

        public AllStockExporter()
        {
            allStockList = new List<AllStockRecord>();
        }

        public void CreateAllStockList(string filename = null)
        {
            //todo проверить, нормально ли работает.

            if (filename == null)
            {
                filename = FILENAME;
            }

            var queensStockList = Queens.records;
            var sneakers = catalog.sneakers;
            var shop = Queens;
            for (int i = 0; i < queensStockList.Count; i++)
            {
                var stockListRecord = queensStockList[i];
                int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);
                if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                {
                    AllStockRecord allStockRecord = new AllStockRecord();
                    allStockRecord.queensLink = stockListRecord.link;
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
                    allStockRecord.size = stockListRecord.size;
                    //allStockRecord.queens_quantity = stockListRecord.quantity;
                    //var shop = Queens;
                    var price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                    allStockRecord.queens_price = price;
                    //allStockRecord.queens_oldPrice = stockListRecord.oldPrice;
                    allStockList.Add(allStockRecord);
                }
                else
                {
                    Console.WriteLine("Дубликат в стоке квинса: " + stockListRecord.sku + "-" + stockListRecord.size);
                }
            }

            var nashStockList = NashStock1.records;
            for (int i = 0; i < nashStockList.Count; i++)
            {
                var stockListRecord = nashStockList[i];
                int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                {
                    AllStockRecord allStockRecord = new AllStockRecord();
                    allStockRecord.sku = stockListRecord.sku;
                    allStockRecord.size = stockListRecord.size;
                    allStockRecord.nash_quantity = stockListRecord.quantity;
                    allStockRecord.nash_price = GetAllStockPrice(stockListRecord.price, NashStockParser.CURRENCY, NashStockParser.DELIVERY_TO_USA, NashStockParser.VAT_VALUE);
                    allStockRecord.nashSellPrice = stockListRecord.sellPrice;

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
                else
                { //если данный артикул и размер есть в другом стоке и уже добавлен в общий список
                    allStockList[indexAllStockListRecord].nash_quantity = stockListRecord.quantity;
                    allStockList[indexAllStockListRecord].nash_price = GetAllStockPrice(stockListRecord.price, NashStockParser.CURRENCY, NashStockParser.DELIVERY_TO_USA, NashStockParser.VAT_VALUE); ;
                    allStockList[indexAllStockListRecord].nashSellPrice = stockListRecord.sellPrice;
                }
            }

            var discontStockList = DiscontSamara.records;
            for (int i = 0; i < discontStockList.Count; i++)
            {
                var stockListRecord = discontStockList[i];

                if (stockListRecord.quantity > 0)
                {
                    int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                    if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                    {
                        AllStockRecord allStockRecord = new AllStockRecord();
                        allStockRecord.sku = stockListRecord.sku;
                        allStockRecord.size = stockListRecord.size;
                        allStockRecord.discont_quantity = stockListRecord.quantity;
                        allStockRecord.discont_price = GetAllStockPrice(stockListRecord.price, DiscontSamaraParser.CURRENCY, DiscontSamaraParser.DELIVERY_TO_USA, DiscontSamaraParser.VAT_VALUE);
                        //allStockRecord.discontOldPrice = stockListRecord.oldPrice;

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
                        allStockList[indexAllStockListRecord].discont_price = GetAllStockPrice(stockListRecord.price, DiscontSamaraParser.CURRENCY, DiscontSamaraParser.DELIVERY_TO_USA, DiscontSamaraParser.VAT_VALUE);
                        //allStockList[indexAllStockListRecord].discontOldPrice = stockListRecord.oldPrice;
                    }
                }
            }

            //discontStockList = Kuzminki.records;
            //for (int i = 0; i < discontStockList.Count; i++)
            //{
            //    var stockListRecord = discontStockList[i];

            //    if (stockListRecord.quantity > 0)
            //    {
            //        int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.sizeUS);

            //        if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
            //        {
            //            AllStockRecord allStockRecord = new AllStockRecord();
            //            allStockRecord.sku = stockListRecord.sku;
            //            allStockRecord.sizeUS = stockListRecord.sizeUS;
            //            allStockRecord.kuzminki_quantity = stockListRecord.quantity;
            //            allStockRecord.kuzminki_price = stockListRecord.price;
            //            //allStockRecord.kuzminkiOldPrice = stockListRecord.oldPrice;

            //            int index = FindSneakerSKU(allStockRecord.sku);
            //            if (index != -1)
            //            {
            //                allStockRecord.brand = sneakers[index].brand;
            //                allStockRecord.title = sneakers[index].title;
            //            }
            //            else
            //            {
            //                Console.WriteLine("Артикула " + allStockRecord.sku + " нет в файле каталога");
            //                //throw new Exception("Артикула " + allStockRecord.sku + " нет в файле каталога");
            //            }

            //            allStockList.Add(allStockRecord);
            //        }
            //        else
            //        { //если данный артикул и размер есть в другом стоке и уже добавлен в общий список
            //            allStockList[indexAllStockListRecord].kuzminki_quantity = stockListRecord.quantity;
            //            allStockList[indexAllStockListRecord].kuzminki_price = stockListRecord.price;
            //            //allStockList[indexAllStockListRecord].kuzminkiOldPrice = stockListRecord.oldPrice;

            //        }
            //    }
            //}

            shop = StreetBeat;
            var streetbeatStockList = StreetBeat.records;
            for (int i = 0; i < streetbeatStockList.Count; i++)
            {
                var stockListRecord = streetbeatStockList[i];
                int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                {
                    AllStockRecord allStockRecord = new AllStockRecord();
                    allStockRecord.sku = stockListRecord.sku;
                    allStockRecord.size = stockListRecord.size;
                    allStockRecord.strBeatLink = stockListRecord.link;
                    //allStockRecord.streetbeat_quantity = stockListRecord.quantity;
                    allStockRecord.streetbeat_price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                    //allStockRecord.streetbeatOldPrice = stockListRecord.oldPrice;

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
                    allStockList[indexAllStockListRecord].streetbeat_price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                    //allStockList[indexAllStockListRecord].streetbeatOldPrice = stockListRecord.oldPrice;
                    allStockList[indexAllStockListRecord].strBeatLink = stockListRecord.link;
                }
            }

            shop = BasketShop;
            var basketShopStockList = BasketShop.records;
            for (int i = 0; i < basketShopStockList.Count; i++)
            {
                var stockListRecord = basketShopStockList[i];
                int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                {
                    AllStockRecord allStockRecord = new AllStockRecord();
                    allStockRecord.sku = stockListRecord.sku;
                    allStockRecord.size = stockListRecord.size;
                    allStockRecord.basketShopLink = stockListRecord.link;
                    //allStockRecord.streetbeat_quantity = stockListRecord.quantity;
                    allStockRecord.basketshop_price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                    //allStockRecord.basketshopOldPrice = stockListRecord.oldPrice;

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
                    allStockList[indexAllStockListRecord].basketshop_price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                    //allStockList[indexAllStockListRecord].basketshopOldPrice = stockListRecord.oldPrice;
                    allStockList[indexAllStockListRecord].basketShopLink = stockListRecord.link;
                }
            }

            shop = Einhalb;
            var einhalbStockList = Einhalb.records;
            for (int i = 0; i < einhalbStockList.Count; i++)
            {
                var stockListRecord = einhalbStockList[i];
                int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                {
                    AllStockRecord allStockRecord = new AllStockRecord();
                    allStockRecord.sku = stockListRecord.sku;
                    allStockRecord.size = stockListRecord.size;
                    allStockRecord.einhalbLink = stockListRecord.link;
                    //allStockRecord.streetbeat_quantity = stockListRecord.quantity;
                    allStockRecord.einhalb_price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                    //allStockRecord.einhalbOldPrice = stockListRecord.oldPrice;

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
                    allStockList[indexAllStockListRecord].einhalb_price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                    //allStockList[indexAllStockListRecord].einhalbOldPrice = stockListRecord.oldPrice;
                    allStockList[indexAllStockListRecord].einhalbLink = stockListRecord.link;
                }
            }

            shop = Sivas;
            var StockList = Sivas.records;
            for (int i = 0; i < StockList.Count; i++)
            {
                var stockListRecord = StockList[i];
                int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                {
                    AllStockRecord allStockRecord = new AllStockRecord();
                    allStockRecord.sku = stockListRecord.sku;
                    allStockRecord.size = stockListRecord.size;
                    allStockRecord.sivasLink = stockListRecord.link;
                    //allStockRecord.streetbeat_quantity = stockListRecord.quantity;
                    allStockRecord.sivas_price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                    //allStockRecord.sivasOldPrice = stockListRecord.oldPrice;

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
                    allStockList[indexAllStockListRecord].sivas_price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                    //allStockList[indexAllStockListRecord].sivasOldPrice = stockListRecord.oldPrice;
                    allStockList[indexAllStockListRecord].sivasLink = stockListRecord.link;
                }
            }

            shop = Titolo;
            if (Titolo != null)
            {
                StockList = Titolo.records;
                for (int i = 0; i < StockList.Count; i++)
                {
                    var stockListRecord = StockList[i];
                    int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                    if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                    {
                        AllStockRecord allStockRecord = new AllStockRecord();
                        allStockRecord.sku = stockListRecord.sku;
                        allStockRecord.size = stockListRecord.size;
                        allStockRecord.titoloLink = stockListRecord.link;
                        //allStockRecord.streetbeat_quantity = stockListRecord.quantity;
                        allStockRecord.titolo_price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockRecord.titoloOldPrice = stockListRecord.oldPrice;

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
                        allStockList[indexAllStockListRecord].titolo_price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockList[indexAllStockListRecord].titoloOldPrice = stockListRecord.oldPrice;
                        allStockList[indexAllStockListRecord].titoloLink = stockListRecord.link;
                    }
                }
            }

            shop = Sneakersnstuff;
            if (Sneakersnstuff != null)
            {
                StockList = Sneakersnstuff.records;
                for (int i = 0; i < StockList.Count; i++)
                {
                    var stockListRecord = StockList[i];
                    int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                    if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                    {
                        AllStockRecord allStockRecord = new AllStockRecord();
                        allStockRecord.sku = stockListRecord.sku;
                        allStockRecord.size = stockListRecord.size;
                        allStockRecord.sneakerstuffLink = stockListRecord.link;
                        //allStockRecord.streetbeat_quantity = stockListRecord.quantity;
                        allStockRecord.sneakerstuff_price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockRecord.sneakerstuffOldPrice = stockListRecord.oldPrice;

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
                        allStockList[indexAllStockListRecord].sneakerstuff_price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockList[indexAllStockListRecord].sneakerstuffOldPrice = stockListRecord.oldPrice;
                        allStockList[indexAllStockListRecord].sneakerstuffLink = stockListRecord.link;
                    }
                }
            }

            shop = Overkillshop;
            if (Overkillshop != null)
            {
                StockList = Overkillshop.records;
                for (int i = 0; i < StockList.Count; i++)
                {
                    var stockListRecord = StockList[i];
                    int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                    if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                    {
                        AllStockRecord allStockRecord = new AllStockRecord();
                        allStockRecord.sku = stockListRecord.sku;
                        allStockRecord.size = stockListRecord.size;
                        allStockRecord.overkilllink = stockListRecord.link;
                        //allStockRecord.streetbeat_quantity = stockListRecord.quantity;
                        allStockRecord.overkill_price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockRecord.overkillOldPrice = stockListRecord.oldPrice;

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
                        allStockList[indexAllStockListRecord].overkill_price = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockList[indexAllStockListRecord].overkillOldPrice = stockListRecord.oldPrice;
                        allStockList[indexAllStockListRecord].overkilllink = stockListRecord.link;
                    }
                }
            }

            shop = Solehaven;
            if (Solehaven != null)
            {
                StockList = Solehaven.records;
                for (int i = 0; i < StockList.Count; i++)
                {
                    var stockListRecord = StockList[i];
                    int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                    if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                    {
                        AllStockRecord allStockRecord = new AllStockRecord();
                        allStockRecord.sku = stockListRecord.sku;
                        allStockRecord.size = stockListRecord.size;
                        allStockRecord.solehavenLink = stockListRecord.link;
                        allStockRecord.solehavenPrice = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockRecord.solehavenOldPrice = stockListRecord.oldPrice;

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
                        allStockList[indexAllStockListRecord].solehavenPrice = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockList[indexAllStockListRecord].solehavenOldPrice = stockListRecord.oldPrice;
                        allStockList[indexAllStockListRecord].solehavenLink = stockListRecord.link;
                    }
                }
            }

            shop = Afew;
            if (Afew != null)
            {
                StockList = Afew.records;
                for (int i = 0; i < StockList.Count; i++)
                {
                    var stockListRecord = StockList[i];
                    int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                    if (stockListRecord.sku == "878071-100" && stockListRecord.size == "7")
                    {
                        bool test = true;
                    }

                    if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                    {
                        AllStockRecord allStockRecord = new AllStockRecord();
                        allStockRecord.sku = stockListRecord.sku;
                        allStockRecord.size = stockListRecord.size;
                        allStockRecord.afewLink = stockListRecord.link;
                        allStockRecord.afewPrice = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockRecord.afewOldPrice = stockListRecord.oldPrice;

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
                        allStockList[indexAllStockListRecord].afewPrice = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockList[indexAllStockListRecord].afewOldPrice = stockListRecord.oldPrice;
                        allStockList[indexAllStockListRecord].afewLink = stockListRecord.link;
                    }
                }
            }

            shop = SuppaStore;
            if (SuppaStore != null)
            {
                StockList = SuppaStore.records;
                for (int i = 0; i < StockList.Count; i++)
                {
                    var stockListRecord = StockList[i];
                    int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                    if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                    {
                        AllStockRecord allStockRecord = new AllStockRecord();
                        allStockRecord.sku = stockListRecord.sku;
                        allStockRecord.size = stockListRecord.size;
                        allStockRecord.suppaLink = stockListRecord.link;
                        allStockRecord.suppaStorePrice = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockRecord.afewOldPrice = stockListRecord.oldPrice;

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
                        allStockList[indexAllStockListRecord].suppaStorePrice = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockList[indexAllStockListRecord].afewOldPrice = stockListRecord.oldPrice;
                        allStockList[indexAllStockListRecord].suppaLink = stockListRecord.link;
                    }
                }
            }

            shop = Chmielna;
            if (Chmielna != null)
            {
                StockList = Chmielna.records;
                for (int i = 0; i < StockList.Count; i++)
                {
                    var stockListRecord = StockList[i];
                    int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                    if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                    {
                        AllStockRecord allStockRecord = new AllStockRecord();
                        allStockRecord.sku = stockListRecord.sku;
                        allStockRecord.size = stockListRecord.size;
                        allStockRecord.chmielnaLink = stockListRecord.link;
                        allStockRecord.chmielnaPrice = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockRecord.afewOldPrice = stockListRecord.oldPrice;

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
                        allStockList[indexAllStockListRecord].chmielnaLink = stockListRecord.link;
                        allStockList[indexAllStockListRecord].chmielnaPrice = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockList[indexAllStockListRecord].afewOldPrice = stockListRecord.oldPrice;                  
                    }
                }
            }

            shop = BdgaStore;
            if (BdgaStore != null)
            {
                StockList = BdgaStore.records;
                for (int i = 0; i < StockList.Count; i++)
                {
                    var stockListRecord = StockList[i];
                    int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                    if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                    {
                        AllStockRecord allStockRecord = new AllStockRecord();
                        allStockRecord.sku = stockListRecord.sku;
                        allStockRecord.size = stockListRecord.size;
                        allStockRecord.bdgastoreLink = stockListRecord.link;
                        allStockRecord.bdgastorePrice = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockRecord.afewOldPrice = stockListRecord.oldPrice;

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
                        allStockList[indexAllStockListRecord].bdgastoreLink = stockListRecord.link;
                        allStockList[indexAllStockListRecord].bdgastorePrice = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockList[indexAllStockListRecord].afewOldPrice = stockListRecord.oldPrice;                  
                    }
                }
            }

            shop = AsfaltGold;
            if (shop != null)
            {
                StockList = shop.records;
                for (int i = 0; i < StockList.Count; i++)
                {
                    var stockListRecord = StockList[i];
                    var id = stockListRecord.sku + "-" + stockListRecord.size;
                    int indexAllStockListRecord = FindAllStockListRecord(stockListRecord.sku, stockListRecord.size);

                    if (indexAllStockListRecord == -1) //если этой записи еще нет в стоке
                    {
                        AllStockRecord allStockRecord = new AllStockRecord();
                        allStockRecord.sku = stockListRecord.sku;
                        allStockRecord.size = stockListRecord.size;
                        allStockRecord.asfaltgoldLink = stockListRecord.link;
                        allStockRecord.asfaltgoldPrice = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockRecord.afewOldPrice = stockListRecord.oldPrice;

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
                        allStockList[indexAllStockListRecord].asfaltgoldLink = stockListRecord.link;
                        allStockList[indexAllStockListRecord].asfaltgoldPrice = GetAllStockPrice(stockListRecord.price, shop.Currency, shop.DeliveryToUSA, shop.VatValue);
                        //allStockList[indexAllStockListRecord].afewOldPrice = stockListRecord.oldPrice;                  
                    }
                }
            }

            //AddSizesToSneakersFromAllStockList(); //в массиве sneakers к каждому stockSneaker добавляю размеры и склады, на которых эти размеры есть
            //SortSizes();

            SaveAllStockListToCSV(filename);
        }

        public static double GetAllStockPrice(double stockPrice, string currency, int deliveryToUSA, double VatValue)
        {

            var stockRecord = new StockRecord();
            var price = ( (stockPrice) * (1 - VatValue) ) + deliveryToUSA;

            if (currency == "USD")
            {
                return Math.Round(price, 2);
            }

            //stockRecord.price = (stockPrice + deliveryToUSA) * (1 - VatValue);
            //stockRecord.sellPrice = stockRecord.price;

            //var price2 = Exporter.GetPrice2(stockPrice, VatValue, deliveryToUSA, 0, currency, "USD");

            CurrencyRate currate = CurrencyRate.ReadObjectFromJsonFile();

            double curInputRate = currate.GetCurrencyRate(currency);
            double curOutputRate = currate.GetCurrencyRate("USD");

            double curInputRateBuy = curInputRate * 1.035;
            double curOutputRateSell = curOutputRate * 0.965;

            

            var resultPrice = price * curInputRateBuy / curOutputRateSell;
            var oldResultPrice = price * curInputRate / curOutputRate;
            


            resultPrice = Math.Round(resultPrice, 2);
            //var resultPrice = Exporter.GetPrice(stockRecord, currency, "USD");
            return resultPrice;
            //throw new NotImplementedException();
        }

        public int FindAllStockListRecord(string sku, string size)
        {
            for (int i = 0; i < allStockList.Count; i++)
            {
                if (allStockList[i].sku == sku && allStockList[i].size == size)
                {
                    return i;
                }
            }
            return -1;
        }

        public int FindSneakerSKU(string sku)
        {
            var sneakers = catalog.sneakers;
            for (int i = 0; i < sneakers.Count; i++)
            {
                if (sku == sneakers[i].sku)
                {
                    return i;
                }
            }
            return -1;
        }

        public void SaveAllStockListToCSV(string filename = null)
        {
            //todo проверить, нормально ли работает.

            if (filename == null)
            {
                filename = FILENAME;
            }

            //string JSON_FILENAME = @"C:\SneakerIcon\CSV\AllStock.csv";
            using (var sw = new StreamWriter(filename))
            {
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                //writer.Configuration.Encoding = Encoding.GetEncoding("UTF-8");
                writer.WriteRecords(allStockList);
            }
        }

        public void ReadStockFromCSV(string filename = null)
        {
            //todo проверить, нормально ли работает.

            if (filename == null)
            {
                filename = FILENAME;
            }

            using (var sr = new StreamReader(filename))
            {
                var reader = new CsvReader(sr);
                reader.Configuration.Delimiter = ";";
                IEnumerable<AllStockRecord> records = reader.GetRecords<AllStockRecord>();
                this.allStockList = records.ToList();
            }
        }
    }
}
