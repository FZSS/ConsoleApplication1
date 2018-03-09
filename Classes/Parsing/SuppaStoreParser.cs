using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Classes.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SneakerIcon.Sys;

namespace SneakerIcon.Classes.Parsing
{
    public class SuppaStoreParser : Parser
    {
        public const string SITEURL = "https://www.suppastore.com/";
        public static readonly string FOLDER = DIRECTORY_PATH + @"suppastore.com\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "en";
        public const string CURRENCY = "EUR";
        public const string NAME = "suppastore.com";
        public const int MARZHA = DELIVERY_TO_USA + 25; 
        public const int DELIVERY_TO_USA = 26;
        public const double VAT_VALUE = 0.16;

        public void Run()
        {
            catalog = ParseSneakersFromAllPages(); //парсим все ссылки и общую инфу
            ParseAllSneakers(catalog); //парсим все кроссы по одному
            //SetSellPrices();
            SaveCatalogToCSV(CATALOG_FILENAME);
            SaveStockToCSV(STOCK_FILENAME);

            var market_info = new Parsing.Model.MarketInfo();
            market_info.name = NAME;
            market_info.website = SITEURL;
            market_info.currency = CURRENCY;
            market_info.start_parse_date = DateTime.Now;
            market_info.end_parse_date = DateTime.Now;
            market_info.delivery_to_usa = DELIVERY_TO_USA;
            market_info.photo_parameters.is_watermark_image = false;
            market_info.photo_parameters.background_color = "white";
            market_info.photo_parameters.first_photo_sneaker_direction = "left"; //уточнить
            market_info.currently_language = "en";
            var json = CreateJson(market_info);
            SaveJson(json, JSON_FILENAME,FOLDER,NAME);
        }


        public Catalog ParseSneakersFromAllPages()
        {
            Catalog catalog = new Catalog();

            ParseSneakersFromPage(catalog, "Nike", "https://www.suppastore.com/footwear/nike.html");

            ParseSneakersFromPage(catalog, "Jordan", "https://www.suppastore.com/footwear/air-jordan.html");

            return catalog;
        }

        public void ParseSneakersFromPage(Catalog catalog, string brand, string link)
        {
            Console.WriteLine("Парсим " + link);
            Uri uri = new Uri(link);
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;

            string source = webClient.DownloadString(uri);
            webClient.Dispose();
            var parser = new HtmlParser();
            var document = parser.Parse(source);

            //items
            var rows = document.QuerySelectorAll("div.row.clearfix.products-grid");
            foreach (var row in rows)
            {
                var items = row.QuerySelectorAll("div.col.col-md-4");
                foreach (var item in items)
                {
                    var sneaker = new Sneaker();

                    //fullCatalogSneaker.sex = sex;
                    //fullCatalogSneaker.categorySneakerFullCatalog = categorySneakerFullCatalog;

                    //brand
                    sneaker.brand = brand;

                    //link
                    var linkHTML = item.QuerySelector("a");
                    sneaker.link = linkHTML.GetAttribute("href");

                    var spanOutOfStock = item.QuerySelector("span.availability.out-of-stock.product-cover__out-of-stock");
                    if (spanOutOfStock == null)
                    {
                        //prices             
                        var salePriceHTML = item.QuerySelector("span.price.price--sale");
                        string priceString = String.Empty;
                        //если товар идет по сейлу
                        if (salePriceHTML != null)
                        {
                            //price
                            priceString = item.QuerySelector("span.price.price--sale").InnerHtml;
                            priceString = priceString.Replace("€", "").Replace("&nbsp;", "");
                            priceString = priceString.Replace(".", ",").Trim();
                            sneaker.price = double.Parse(priceString);

                            //old price
                            priceString = item.QuerySelector("span.price.price--regular.price--strike-through").InnerHtml;
                            priceString = priceString.Replace("€", "").Replace("&nbsp;", "");
                            priceString = priceString.Replace(".", ",").Trim();
                            sneaker.oldPrice = double.Parse(priceString);
                        }
                        else
                        {
                            priceString = item.QuerySelector("span.price").InnerHtml;
                            priceString = priceString.Replace("€", "").Replace("&nbsp;", "");
                            priceString = priceString.Replace(".", ",").Trim();
                            sneaker.price = double.Parse(priceString);
                        }
                        if (sneaker.price == 0)
                        {
                            throw new Exception("цена равна нулю");
                        }
                        else
                        {
                            //add to catalog
                            if (!catalog.isExistSneakerInCatalog(sneaker))
                                catalog.sneakers.Add(sneaker);
                        }
                    }
                }
            }

            //next page
            var nextPage = document.QuerySelector("a.pagination__link.pagination__link--next.next");
            if (nextPage != null)
            {
                string nextPageLink = nextPage.GetAttribute("href");
                //System.Threading.Thread.Sleep(5000);

                ParseSneakersFromPage(catalog, brand, nextPageLink);
            }
        }

        private Catalog ParseAllSneakers(Catalog catalog)
        {
            var newCatalog = new Catalog();
            int count = catalog.sneakers.Count;
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(i + " " + catalog.sneakers[i].link);
                var sneaker = ParseOneSneaker(catalog.sneakers[i]);
                if (sneaker != null) newCatalog.AddUniqueSneaker(sneaker);
                Console.WriteLine(i + ". sku: " + catalog.sneakers[i].sku + " title: " + catalog.sneakers[i].title);
                //System.Threading.Thread.Sleep(5000);
                //sneakerParser.DownloadSneakerImages(i);
            }
            catalog.sneakers = newCatalog.sneakers;
            return catalog;
        }

        public Sneaker ParseOneSneaker(Sneaker @sneaker)
        {
            string source = Parser.GetPage(sneaker.link);
            var parser = new HtmlParser();
            var document = parser.Parse(source);

            //sku //color
            try
            {
                //sku
                var trList = document.QuerySelectorAll("tr.table-row.table-row--odd");
                var tr = trList[0];
                var tdList = tr.QuerySelectorAll("td");
                var td0 = tdList[0].InnerHtml.Trim();
                sneaker.sku = tdList[1].InnerHtml.Trim();
                if (!Helper.isTrueMaskSKUForNike(sneaker.sku))
                {
                    Program.Logger.Warn("wrong sku. sku: " + sneaker.sku + ". Link: " + sneaker.link);
                    return null;
                }
                if (sneaker.sku == null)
                {
                    Program.Logger.Warn("wrong sku. sku: " + sneaker.sku + ". Link: " + sneaker.link);
                    return null;
                }

                //color отладить надо, не отлаживал, это код именно для suppastore
                //trList = document.QuerySelectorAll("tr.table-row.table-row--even");
                //foreach (var trtr in trList)
                //{
                //    var tdList2 = trtr.QuerySelectorAll("td");
                //    var tdColor = tdList2[0].InnerHtml.Trim();
                //    if (tdList[0].InnerHtml.Trim() == "COLOR")
                //    {
                //        sneaker.color = tdList[1].InnerHtml.Trim();
                //    }
                //}
            }
            catch (NullReferenceException e)
            {
                Program.Logger.Warn("wrong sku. link:" + sneaker.link);
                Program.Logger.Warn(e.StackTrace);
                return null;
            }

            //title
            var titleHTML = document.QuerySelector("div.product-detail__headline").QuerySelector("span").InnerHtml;
            titleHTML += " " + document.QuerySelector("div.product-detail__subheadline.product-name").InnerHtml;
            titleHTML = titleHTML.Replace("\"", " ").Trim();
            sneaker.title = titleHTML.ToUpper().Replace("  "," ");

            //images
            //var imageContainer = document.All.Where(m => m.LocalName == "div" && m.Id == "thumbnail-wrapper").First();
            var images = document.QuerySelector("ul.image-zoom__gallery-list").QuerySelectorAll("img");
            //List<String> listImage = new List<String>();
            foreach (var image in images)
            {
                //var imageHTML = image.QuerySelector("img");
                string imageString = image.GetAttribute("src");
                if (imageString.Contains(".jpg"))
                {
                    sneaker.images.Add(imageString);
                }
                else
                {
                    Program.Logger.Warn("wrong image " + sneaker.sku);
                }
            }
            if (sneaker.images.Count == 0) Program.Logger.Warn("no images " + sneaker.sku);

            //categorySneakerFullCatalog and sex

            var sizeDict = new Dictionary<string,string>(); //пара ключ размер евро - значение размер юс
            if (document.QuerySelectorAll("table.data-table.table").Count() < 3)
            { //это значит, что таблицы размеров нет
                //FullCatalog fullCatalog = new FullCatalog();
                //var fullCatalogSneaker = fullCatalog.GetSneakerFromSKU(sneaker.sku);
                //if (fullCatalogSneaker == null)
                //{
                //    return null; //в фул каталоге нет, таблицы размеров на странице нет(
                //}
                //else
                //{
                //    sneaker.categorySneakerFullCatalog = fullCatalogSneaker.categorySneakerFullCatalog;
                //    sneaker.sex = fullCatalogSneaker.sex;
                //}

                //надо придумать как евро размеры в юс преобразовать и допилить
                Program.Logger.Warn("Нет таблицы размеров на странице " + sneaker.sku + " " + sneaker.title);
                return null; //нет таблицы размеров на странице

            }
            else
            {
                var tableSize = document.QuerySelectorAll("table.data-table.table")[2];
                var sizeChart = tableSize.QuerySelector("tbody").QuerySelectorAll("tr");
                var tdList3 = sizeChart[0].QuerySelectorAll("td");
                var sizeUS = tdList3[0].InnerHtml.Trim();
                var sizeEU = tdList3[2].InnerHtml.Trim();
                for (int i = 0; i < sizeChart.Count(); i++)
                {
                    var tdList4 = sizeChart[i].QuerySelectorAll("td");
                    sizeDict.Add(tdList4[2].InnerHtml.Trim(), tdList4[0].InnerHtml.Trim());
                }
                string category;
                string sex;
                bool result = Helper.GetCategoryAndSex(sizeUS, sizeEU, out category, out sex);
                string testCategory = SizeConverters.SizeConverter.GetCategory(sizeUS, sizeEU, null, null);
                sneaker.category = category;
                sneaker.sex = sex;
                if (!result)
                {
                    Program.Logger.Warn("wrong category sku:" + sneaker.sku + " link: " + sneaker.link);
                    return null;
                } 
            }


            //sizes               
            var sizesHTML = document.QuerySelector("div.product-options").InnerHtml;
            var separator = new String[] { "\"label\":\"Footwear-" };
            var sizes = sizesHTML.Split(separator, StringSplitOptions.None);
            for (int i = 1; i < sizes.Count(); i++)
            {
                var size = sizes[i];
                var flagStr = "\",\"price\"";
                if (size.Contains(flagStr))
                {
                    //sizeUS = sizeUS.Replace("\\r\\n", "");
                    int index1 = size.IndexOf(flagStr);
                    //int index2 = sizeUS.IndexOf("\",\"label_uk");
                    try
                    {
                        size = size.Substring(0,index1);
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        size = null;
                        Program.Logger.Warn("wrong size: " + sneaker.sku);
                        return null;
                    }
                    if (size != null)
                    {
                        try
                        {
                            size = sizeDict[size];
                            SneakerSize snsize = new SneakerSize(size);
                            sneaker.sizes.Add(snsize);
                        }
                        catch (Exception e)
                        {
                            Program.Logger.Warn("Размер отсутствует в таблице размеров");
                        }
                    }
                }
            }                   
            //sizes

            if (sneaker.sizes.Count > 0)
            {
                return sneaker;
            }
            else
            {
                return null;
            }
                     
        }
    }
}
