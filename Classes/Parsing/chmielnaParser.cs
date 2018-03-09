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
    public class ChmielnaParser : Parser
    {
        public const string SITEURL = "https://chmielna20.pl/"; //поменять
        public static readonly string FOLDER = DIRECTORY_PATH + NAME + @"\";
        public static readonly string IMAGE_FOLDER = FOLDER + Config.GetConfig().ImageFolger;
        public static readonly string CATALOG_FILENAME = FOLDER + "Catalog.csv";
        public static readonly string STOCK_FILENAME = FOLDER + "Stock.csv";
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "en";
        public const string CURRENCY = "PLN";
        public const string NAME = "chmielna20.pl";
        public const int MARZHA = DELIVERY_TO_USA + 100; //доставка 30, 30 маржа
        public const int DELIVERY_TO_USA = 80;
        //public const bool IS_RETURN_VAT = false;
        public const double VAT_VALUE = 0; //Если 15% ват, то пишем 0.15, если вычета нет, то 0
        //public const double VAT_VALUE = 0.16;

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

            //nike men
            ParseSneakersFromPage(catalog, "Nike", Settings.CATEGORY_MEN, "https://chmielna20.pl/en/menu/obuwie/meskie/nike/page,30");

            //nike women 
            ParseSneakersFromPage(catalog, "Nike", Settings.CATEGORY_WOMEN, "https://chmielna20.pl/en/menu/obuwie/damskie/nike/page,30");

            //nike kids 
            ParseSneakersFromPage(catalog, "Nike", Settings.CATEGORY_KIDS, "https://chmielna20.pl/en/menu/obuwie/junior/nike/page,20");

            //jordan men
            ParseSneakersFromPage(catalog, "Jordan", Settings.CATEGORY_MEN, "https://chmielna20.pl/en/menu/obuwie/meskie/jordan-brand/page,30");

            //jordan kids 
            ParseSneakersFromPage(catalog, "Jordan", Settings.CATEGORY_KIDS, "https://chmielna20.pl/en/menu/obuwie/junior/jordan-brand/page,30");

            return catalog;
        }

        public void ParseSneakersFromPage(Catalog catalog, string brand, string category, string link)
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
            var items = document.QuerySelectorAll("div.col-sm-4.col-md-3.col-xs-6.products__item");
            foreach (var item in items)
            {
                var sneaker = new Sneaker();

                //categorySneakerFullCatalog
                sneaker.category = category;

                //brand
                sneaker.brand = brand;

                //link
                var linkHTML = item.QuerySelector("a");
                sneaker.link = linkHTML.GetAttribute("href");

                //prices
                bool isSale = false;
                var spans = item.QuerySelector("p.products__item-price").QuerySelectorAll("span");
                if (spans.Count() > 1) isSale = true;
                
                //var salePriceHTML = item.QuerySelector("span.price.price--sale");
                string priceString = String.Empty;
                //если товар идет по сейлу
                if (isSale)
                {
                    //price         
                    priceString = spans[1].InnerHtml;
                    priceString = priceString.Replace("PLN", "").Replace("&nbsp;", "").Replace(".", ",").Replace(",", "").Trim();
                    sneaker.price = double.Parse(priceString)/100;

                    //old price
                    priceString = spans[0].InnerHtml;
                    priceString = priceString.Replace("PLN", "").Replace("&nbsp;", "").Replace(".", ",").Replace(",", "").Trim();
                    sneaker.oldPrice = double.Parse(priceString)/100;
                }
                else
                {
                    priceString = spans[0].InnerHtml;
                    priceString = priceString.Replace("PLN", "").Replace("&nbsp;", "").Replace(".", ",").Replace(",", "").Trim();
                    sneaker.price = double.Parse(priceString)/100;
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

            //next page
            //var nextPage = document.QuerySelector("a.pagination__link.pagination__link--next.next");
            //if (nextPage != null)
            //{
            //    string nextPageLink = nextPage.GetAttribute("href");
            //    //System.Threading.Thread.Sleep(5000);

            //    ParseSneakersFromPage(catalog, brand, nextPageLink);
            //}
        }

        private Catalog ParseAllSneakers(Catalog catalog)
        {
            var newCatalog = new Catalog();
            int count = catalog.sneakers.Count;
            for (int i = 0; i < count; i++)
            {
                //Console.WriteLine(i + " " + catalog.sneakers[i].link);
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
            //try
            //{
                //sku      
                var link = sneaker.link;
                var startIndex = link.Length - 15;
                var endIndex = link.Length - 5;
                var sku = link.Substring(startIndex, endIndex - startIndex);
                sneaker.sku = sku;
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

                //title
                var title = document.QuerySelector("div.product__name").QuerySelector("p").InnerHtml;
                startIndex = title.IndexOf("(") + 1;
                if (startIndex > 0)
                {
                    endIndex = title.IndexOf(")");
                    //title = title.Substring(startIndex, endIndex - startIndex);
                    title = title.Substring(0, startIndex - 1);
                }
                sneaker.title = title.Replace("\"", "").Trim().ToUpper().Replace("  "," ");
            //}
            //catch (ArgumentOutOfRangeException e)
            //{
            //    Program.logger.Warn("wrong sku. link:" + sneaker.link);
            //    Program.logger.Warn(e.StackTrace);
            //    return null;
            //}

            //у найка у некоторых позиций нет бренда, добавляем
            if (!sneaker.title.Contains(sneaker.brand.ToUpper()))
            {
                sneaker.title = sneaker.brand.ToUpper() + " " + sneaker.title;
            }

            //images
            //var imageContainer = document.All.Where(m => m.LocalName == "div" && m.Id == "thumbnail-wrapper").First();
            try
            {
                var imageHTML = document.QuerySelector("div.owl-carousel-thumb");
                if (imageHTML != null)
                {
                    var images = imageHTML.QuerySelectorAll("img");
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
                }
            }
            catch (NullReferenceException e)
            {
                Program.Logger.Warn(e.StackTrace);
            }
            if (sneaker.images.Count == 0)
            {
                Program.Logger.Warn("no images " + sneaker.sku);
            }

            //categorySneakerFullCatalog and sex
            if (sneaker.category == Settings.CATEGORY_MEN)
            {
                sneaker.sex = Settings.GENDER_MAN;
            }
            else if (sneaker.category == Settings.CATEGORY_WOMEN)
            {
                sneaker.sex = Settings.GENDER_WOMAN;
            }
            else if (sneaker.category != Settings.CATEGORY_KIDS)
            {
                Program.Logger.Warn("wrong category sku:" + sneaker.sku + " link: " + sneaker.link);
                return null;
            }

            //sizes               
            var sizesHTML = document.QuerySelector("div.selector");
            //var separator = new String[] { "\"label\":\"Footwear-" };
            var sizes = sizesHTML.QuerySelectorAll("li");
            for (int i = 0; i < sizes.Count(); i++)
            {
                var size = sizes[i].GetAttribute("data-value");
                if (size != null)
                {
                    try
                    {
                        SneakerSize snsize = new SneakerSize(size);
                        sneaker.sizes.Add(snsize);
                    }
                    catch (Exception e)
                    {
                        Program.Logger.Warn("Размер отсутствует в таблице размеров");
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
