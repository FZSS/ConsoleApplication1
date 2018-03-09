using Newtonsoft.Json;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Classes.SizeConverters.Model;
using SneakerIcon.Model.AllStock;
using SneakerIcon.Model.ShopCatalogModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Exporters
{
    public class AllStockExporter2
    {
        public static AllStockRoot Run()
        {
            var jsonCatalog = ShopCatalogRoot.ReadFromFtp();
            var fullCatalog = new FullCatalog();
            AllStockRoot allstock = CreateAllStock(fullCatalog, jsonCatalog);
            //когда оллсток создан, нужно его прогнать и отсортировать все офферы, чтобы первым был с самой низкой ценой
            SortOffers(allstock);

            Program.Logger.Info("Артикулов в фулкаталог: " + fullCatalog.sneakers.Count);
            Program.Logger.Info("Артикулов в оллсток: " + allstock.sneakers.Count);
            Program.Logger.Info("Размеров в оллсток: " + allstock.GetCountSizes());
            Program.Logger.Info("Офферов в оллсток: " + allstock.GetCountOffers());


            //затем нужно прогнать весь оллсток и, там где нет юпс, добавить их из upcdb
            var folder = Config.GetConfig().DirectoryPathParsing + @"allstock\";
            SaveJson(allstock, "allstock.json", folder);
            return allstock;
        }

        private static void SortOffers(AllStockRoot allstock)
        {
            foreach (var sneaker in allstock.sneakers)
            {
                foreach (var size in sneaker.sizes)
                {
                    //List<Order> SortedList = objListOrder.OrderBy(o=>o.OrderDate).ToList();
                    size.offers = size.offers.OrderBy(x => x.price_usd_with_delivery_to_usa_and_minus_vat).ToList();
                    //size.offers.Sort();
                }
                sneaker.sizes = sneaker.sizes.OrderBy(x => x.us).ToList();
            }
            //throw new NotImplementedException();
        }

        private static void SaveJson(AllStockRoot json, string filename, string folder)
        {
            var localFileName = folder + filename;
            //сохраняем на яндекс.диск файл
            var textJson = JsonConvert.SerializeObject(json);
            System.IO.File.WriteAllText(localFileName, textJson);

            ////подгружаем из конфига данные фтп
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            var ftpHost = appSettings["ftpHostSneakerIcon"];
            var ftpUser = appSettings["ftpUserSneakerIcon"];
            var ftpPass = appSettings["ftpPassSneakerIcon"];

            ////загружаем на ftp файл
            var ftpFileName = "AllStock" + "/" + "AllStock.json";
            Helper.LoadFileToFtp(localFileName, ftpFileName, ftpHost, ftpUser, ftpPass);
        }

        public static AllStockRoot LoadLocalFile()
        {
            var folder = Config.GetConfig().DirectoryPathParsing + @"allstock\";
            var filename = "allstock.json";
            var localFileName = folder + filename;
            var textJson = System.IO.File.ReadAllText(localFileName);
            AllStockRoot allStock = JsonConvert.DeserializeObject<AllStockRoot>(textJson);
            return allStock;
        }

        private static AllStockRoot CreateAllStock(FullCatalog fullCatalog, ShopCatalogRoot jsonCatalog)
        {
            AllStockRoot allstock = new AllStockRoot();
            
            foreach (var jsonMarket in jsonCatalog.markets)
            {
                RootParsingObject market = Parsing.Model.RootParsingObject.ReadJsonMarketFromFtp(jsonMarket);
                if (market != null)
                    AddMarketToAllStock(market, fullCatalog, allstock, jsonMarket);
            }
            allstock.update_time = DateTime.Now;
            return allstock;
        }

        private static void AddMarketToAllStock(RootParsingObject market, FullCatalog fullCatalog, AllStockRoot allstock, Shop jsonMarket)
        {
            foreach (var listing in market.listings)
            {
                var sneaker = allstock.sneakers.Find(x => x.sku == listing.sku);
                //если такого кроссовка еще не существует
                if (sneaker == null)
                {
                    //добавляем новый кроссовок в оллсток
                    var fullCatalogSneaker = fullCatalog.sneakers.Find(x => x.sku == listing.sku);

                    if (fullCatalogSneaker == null)
                    {
                        Program.Logger.Warn("sku: " + listing.sku + " does not exist in fullcatalog");
                    }
                    else
                    {
                        sneaker = new AllStockSneaker();
                        sneaker.sku = listing.sku;
                        sneaker.brand = fullCatalogSneaker.brand;
                        sneaker.category = Helper.ConvertCategoryRusToEng(fullCatalogSneaker.category);
                        sneaker.title = fullCatalogSneaker.title;
                        if (sneaker.sku == "854261-106")
                        {
                            bool test = true;
                        }
                        foreach (var size in listing.sizes)
                        {

                            var sizeAllStock = CreateSize(size, sneaker, listing, market, jsonMarket);
                            if (sizeAllStock != null) sneaker.sizes.Add(sizeAllStock);
                        }

                        if (sneaker.sizes.Count > 0)
                            allstock.sneakers.Add(sneaker);
                    }
                }
                else
                {
                    /* Если кроссовок не нул, значит он уже есть в оллстоке
                     * Это значит, что нужно брать каждый размер из листинга, проверять, есть ли этот размер уже в этом кроссовке
                     * Если нет, то создавать размер и оффер
                     * Если размер есть, то к этому размеру создавать еще один оффер. Возможно тут же надо отсортировать офферы, чтобы с меньшей ценой был первым
                     * Если размер уже есть, то смотреть есть ли юпс
                     * Если юпс нет, то добавлять его, если есть проверять, одинаковый ли он, если нет, кидать варнинг
                     */
                    if (sneaker.sku == "854261-106")
                    {
                        bool test = true;
                    }
                    foreach (var size in listing.sizes)
                    {
                        if (size.us == null) {
                            size.us = SizeConverters.SizeConverter.GetSizeUs(sneaker.brand, sneaker.category, size.eu, size.uk, size.cm);
                        }
                        if (size.us == null)
                        {
                            //значит размер левый или его нет в таблице размеров
                        }
                        else
                        {
                            var sizeAllStock = sneaker.sizes.Find(x => x.us == size.us);
                            if (sizeAllStock == null)
                            {
                                //если размера нет, создаем новый размер
                                sizeAllStock = CreateSize(size, sneaker, listing, market, jsonMarket);
                                if (sizeAllStock != null) 
                                    sneaker.sizes.Add(sizeAllStock);
                            }
                            else
                            {
                                //если такой размер есть, то добавляем оффер и проверяем юпс
                                if (sizeAllStock.upc == null && size.upc != null)
                                {
                                    sizeAllStock.upc = size.upc;
                                }
                                var offer = CreateOffer(listing, market, jsonMarket);
                                sizeAllStock.offers.Add(offer);
                            }
                        }
                    }
                }
            }
            //throw new NotImplementedException();
        }

        private static AllStockSize CreateSize(ListingSize size, AllStockSneaker sneaker, Listing listing, RootParsingObject market, Shop jsonMarket)
        {
            var sizeAllStock = new AllStockSize();
            if (size.us == null)
            {
                size.us = SizeConverters.SizeConverter.GetSizeUs(sneaker.brand, sneaker.category, size.eu, size.uk, size.cm);
            }
            if (size.us == null)
            {
                //значит размер левый или его нет в таблице размеров
                return null;
            }
            else
            {
                if (listing.category != sneaker.category)
                {
                    Program.Logger.Warn("wrong category. SKU:"+listing.sku +"\n"
                        + "Listing Category: " + listing.category + ".\n" 
                        + "FullCatalog Category:" + sneaker.category + "\n"
                        + "ListingTite: " + listing.title + "\n"
                        + "Listing Link:" + listing.url);
                    return null;
                }
                var schSize = SizeChart.GetSizeStatic(new Size(sneaker.brand, sneaker.category, size.us, size.eu, size.uk, size.cm, size.ru));
                if (schSize == null)
                {
                    Program.Logger.Warn("wrong size. SKU:" + listing.sku + "\n"
                        + "Listing Category: " + listing.category + ".\n"
                        + "FullCatalog Category:" + sneaker.category + "\n"
                        + "ListingTite: " + listing.title + "\n"
                        + "Listing Link:" + listing.url);
                    return null;
                }
                sizeAllStock.us = size.us;
                sizeAllStock.sku2 = sneaker.sku + "-" + size.us;
                sizeAllStock.upc = size.upc;
                var offer = CreateOffer(listing, market, jsonMarket);
                sizeAllStock.offers.Add(offer);
                return sizeAllStock;
            }
        }



        private static AllStockOffer CreateOffer(Listing listing, RootParsingObject market, Shop jsonMarket)
        {
            if (listing.sku == "852628-001")
            {
                bool test = true;
            }

            var offer = new AllStockOffer();
            offer.url = listing.url;
            offer.price = listing.price;
            offer.old_price = listing.old_price;
            offer.currency = jsonMarket.currency;
            //offer.currency = market.market_info.currency;
            offer.stock_name = market.market_info.name;
            offer.price_usd_with_delivery_to_usa_and_minus_vat = AllStockExporter.GetAllStockPrice(offer.price, offer.currency, jsonMarket.delivery_to_usa, jsonMarket.vat_value);
            return offer;      
        }

        public static void TestForDima() {
            
            var allstock = AllStockExporter2.LoadLocalFile();

            List<AllStockOffer> offers = new List<AllStockOffer>();
            foreach (var sneaker in allstock.sneakers)
	        {
                foreach (var size in sneaker.sizes)
                {
                    offers.AddRange(size.offers);
                }	 
	        }
            offers = offers.OrderBy(x => x.price_usd_with_delivery_to_usa_and_minus_vat).ToList();

            var text = JsonConvert.SerializeObject(offers);
            var path = Config.GetConfig().DirectoryPathExport + @"VkGoods\offersTestForDima.json";
            File.WriteAllText(path,text);

            offers = JsonConvert.DeserializeObject<List<AllStockOffer>>(File.ReadAllText(path));
            
        }
    }
}
