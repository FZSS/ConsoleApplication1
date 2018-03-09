using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Sys;
using System.Net;
using AngleSharp.Parser.Html;
using NLog;
using SneakerIcon.Classes;
using SneakerIcon.Model.Enum;

namespace SneakerIcon.Controller.Parser.ShopsParsers
{
    public class SneakersnstuffParserAllBrands : ParserAllBrands
    {
        public const string SITEURL = "https://www.sneakersnstuff.com";
        public const string NAME = "sneakersnstuff.com";
        public static readonly string FOLDER = DIRECTORY_PATH + ParsersFolder + @"\" + NAME + @"\";      
        public const string JSON_FILENAME = NAME + "_" + LANGUAGE + ".json";
        public const string LANGUAGE = "en";
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public void Run(int startItem = 0)
        {
            catalog = ParseSneakersFromAllPages(); //парсим все ссылки и общую инфу
            int startCount = catalog.sneakers.Count;
            ParseAllSneakers(catalog, startItem); //парсим все кроссы по одному
            int endCount = catalog.sneakers.Count;
            _logger.Info("start count: " + startCount);
            _logger.Info("valid items: " + endCount);
            var market_info = new Classes.Parsing.Model.MarketInfo();
            market_info.name = NAME;
            market_info.website = SITEURL;
            //market_info.currency = CURRENCY;
            market_info.start_parse_date = DateTime.Now;
            market_info.end_parse_date = DateTime.Now;
            //market_info.delivery_to_usa = DELIVERY_TO_USA;
            market_info.photo_parameters.is_watermark_image = false;
            market_info.photo_parameters.background_color = "white";
            market_info.photo_parameters.first_photo_sneaker_direction = "left"; //уточнить
            market_info.currently_language = "en";
            var json = CreateJson(market_info);
            SaveJson(json, JSON_FILENAME, FOLDER, NAME);
        }

        public Catalog ParseSneakersFromAllPages()
        {
            Catalog catalog = new Catalog();

            ParseSneakersFromPage(catalog, "Adidas", "https://www.sneakersnstuff.com/en/2/sneakers?p=813&orderBy=Published");
            ParseSneakersFromPage(catalog, "New Balance", "https://www.sneakersnstuff.com/en/2/sneakers?p=2412&orderBy=Published");
            ParseSneakersFromPage(catalog, "Puma", "https://www.sneakersnstuff.com/en/2/sneakers?p=853&orderBy=Published");
            ParseSneakersFromPage(catalog, "Reebok", "https://www.sneakersnstuff.com/en/2/sneakers?p=2173&orderBy=Published");
            //ParseSneakersFromPage(catalog, "Converse", "https://www.sneakersnstuff.com/en/2/sneakers?p=798&orderBy=Published"); //артикулы неправильные у конверса, без цвета
            ParseSneakersFromPage(catalog, "Vans", "https://www.sneakersnstuff.com/en/2/sneakers?p=13859&orderBy=Published");

            return catalog;
        }

        public void ParseSneakersFromPage(Catalog catalog, string brand, string link)
        {
            //Console.WriteLine("Парсим " + link);
            //Uri uri = new Uri(link);
            //WebClient webClient = new WebClient();
            //webClient.Encoding = Encoding.UTF8;

            //string source = webClient.DownloadString(uri);
            //webClient.Dispose();
            //var parser = new HtmlParser();
            //var document = parser.Parse(source);

            _logger.Info("Парсим " + link);
            //var document = GetHtmlPage(link);
            var document = GetHtmlPageCrawlera5Try(link, ERegion.US);

            //items
            var items = document.QuerySelectorAll("li.product");

            foreach (var item in items)
            {
                var sneaker = new Sneaker();

                //fullCatalogSneaker.sex = sex;
                //fullCatalogSneaker.categorySneakerFullCatalog = categorySneakerFullCatalog;

                //brand
                sneaker.brand = brand;

                //link
                var linkHTML = item.QuerySelector("a.plink.image");
                sneaker.link = SITEURL + linkHTML.GetAttribute("href");

                //prices             
                var salePriceHTML = item.QuerySelector("span.sale");
                string priceString = String.Empty;
                //если товар идет по сейлу
                if (salePriceHTML != null)
                {
                    //price
                    priceString = salePriceHTML.InnerHtml;
                    priceString = priceString.Replace("$", "").Replace("EUR", "").Replace("Now: ", "");
                    priceString = priceString.Replace(".", ",").Trim();
                    sneaker.price = double.Parse(priceString);

                    //old price
                    priceString = item.QuerySelector("del.sale").InnerHtml;
                    priceString = priceString.Replace("(", "").Replace(")", "").Replace("$", "").Replace("EUR", "").Replace(".", ",").Trim();
                    sneaker.oldPrice = double.Parse(priceString);
                }
                else
                {
                    priceString = item.QuerySelector("span.price").InnerHtml;
                    priceString = priceString.Replace("$", "");
                    priceString = priceString.Replace("EUR", "");
                    priceString = priceString.Replace(".", ",").Trim();
                    //если пустая строка, значит солдаут
                    if (!String.IsNullOrWhiteSpace(priceString))
                        sneaker.price = double.Parse(priceString);
                }

                //title
                //fullCatalogSneaker.title = String.Empty;
                //if (fullCatalogSneaker.brand != "Jordan") fullCatalogSneaker.title = fullCatalogSneaker.brand + " "; //у найк и найк сб нет бренда в тайтле, а у джордан уже есть
                //fullCatalogSneaker.title += item.QuerySelector("span.ftpFolder").InnerHtml;
                //fullCatalogSneaker.title = fullCatalogSneaker.title.ToUpper();
                //fullCatalogSneaker.ParseTitle();

                //add to catalog
                if (!String.IsNullOrWhiteSpace(priceString))
                {
                    if (!catalog.isExistSneakerInCatalog(sneaker))
                        catalog.sneakers.Add(sneaker);
                }

            }

            //next page
            var nextPage = document.QuerySelector("a.control.next");
            if (nextPage != null)
            {
                string nextPageLink = SITEURL + nextPage.GetAttribute("href");
                //System.Threading.Thread.Sleep(5000);
                ParseSneakersFromPage(catalog, brand, nextPageLink);
            }
        }

        public void ParseAllSneakers(Catalog @catalog, int startItem = 0)
        {
            var newCatalog = new Catalog();
            int count = catalog.sneakers.Count;
            for (int i = startItem; i < count; i++)
            {
                //_logger.Info(i + " " + catalog.sneakers[i].link);
                var sneaker = ParseOneSneaker(catalog.sneakers[i]);
                if (sneaker != null) newCatalog.AddUniqueSneaker(sneaker);
                else _logger.Info("item not add");
                _logger.Info(i + ". sku: " + catalog.sneakers[i].sku + " title: " + catalog.sneakers[i].title);
                //System.Threading.Thread.Sleep(3000);
                //sneakerParser.DownloadSneakerImages(i);
            }
            catalog.sneakers = newCatalog.sneakers;
        }

        public Sneaker ParseOneSneaker(string url)
        {
            var sneaker = new Sneaker();
            sneaker.link = url;
            return ParseOneSneaker(sneaker);
        }

        public Sneaker ParseOneSneaker(Sneaker @sneaker)
        {
            //var document = GetHtmlPage(sneaker.link);
            var document = GetHtmlPage(sneaker.link);
            //var document = GetHtmlPageCrawlera5Try(sneaker.link, ERegion.US);
            if (document == null)
                return null;
            _logger.Info("parsing... link:" + sneaker.link);

            if (document.QuerySelector("div.product-buy-container") != null) //если null значит товар еще не поступил в продажу
            {
                //sku
                var skuContainer = document.All.Where(m => m.LocalName == "span" && m.Id == "product-artno");
                if (skuContainer.Count() == 0)
                {
                    _logger.Warn("sku is empty");
                    return null;                    
                }
                    
                var skuHTML = skuContainer.First().InnerHtml.Replace("Article number:&nbsp;", "");
                sneaker.sku = skuHTML.ToUpper();
                if (string.IsNullOrWhiteSpace(sneaker.sku.Trim()))
                {
                    _logger.Warn("sku is empty");
                    return null;
                }

                if (sneaker.sku == "BA9760")
                {
                    bool test = true;
                }

                //color 
                var colorHTML = document.All.Where(m => m.LocalName == "span" && m.Id == "product-color").First().InnerHtml;
                sneaker.color = colorHTML;

                //добавляем цвет к тайтлу
                var titleHTML = document.QuerySelector("h1").InnerHtml;
                titleHTML = titleHTML.Replace("<br>", "").Replace("  ", " ").Trim();
                sneaker.title = titleHTML.ToUpper() + " " + sneaker.color.ToUpper();
                sneaker.title = sneaker.title.Replace("JORDAN BRAND", "").Replace("BRAND", "").Trim();
                if (string.IsNullOrWhiteSpace(sneaker.title.Trim()))
                {
                    _logger.Warn("title is empty");
                    return null;
                }

                //images
                //var imageContainer = document.All.Where(m => m.LocalName == "div" && m.Id == "thumbnail-wrapper").First();
                var images = document.QuerySelectorAll("a.c-1");
                //List<String> listImage = new List<String>();
                foreach (var image in images)
                {
                    var imageHTML = image.QuerySelector("img");
                    string imageString = SITEURL + imageHTML.GetAttribute("data-large-image");
                    if (imageString.Contains(".jpg"))
                    {
                        sneaker.images.Add(imageString);
                    }
                    else
                    {
                        _logger.Warn("wrong image " + sneaker.sku);
                    }
                }
                if (sneaker.images.Count == 0) _logger.Warn("no images " + sneaker.sku);

                //sizes
                bool iswomen = false;
                var dd = document.QuerySelector("div.size-button.property.available").InnerHtml;
                if (dd.Contains("US Wm") || dd.Contains("W")) //женские
                {
                    iswomen = true;
                }
                else if (dd.Contains("US")) //мужские
                {

                }
                else
                {
                    _logger.Warn("category didn't detect");
                    return null;
                }; //детские значит

                var sizes = document.QuerySelectorAll("div.size-button.property.available");
                for (int i = 0; i < sizes.Count(); i++)
                {
                    var sizeUS = sizes[i].QuerySelector("span").InnerHtml;
                    sizeUS = sizeUS.Replace("US Wm", "").Replace("US", "").Trim();
                    //string sizeUS = sizes[i].InnerHtml.Trim();
                    SneakerSize size = new SneakerSize(sizeUS);
                    size.quantity = 1;
                    sneaker.sizes.Add(size);
                } //sizes

                //categorySneakerFullCatalog and sex
                if (sneaker.sizes.Count > 0)
                {
                    if (sneaker.sizes[0].sizeUS.Contains("C") || sneaker.sizes[0].sizeUS.Contains("Y") || sneaker.sizes[0].sizeUS.Contains("K"))
                    {
                        sneaker.category = Settings.CATEGORY_KIDS;
                    }
                    else
                    {
                        if (iswomen)
                        {
                            sneaker.category = Settings.CATEGORY_WOMEN;
                            sneaker.sex = Settings.GENDER_WOMAN;
                        }
                        else
                        {
                            sneaker.category = Settings.CATEGORY_MEN;
                            sneaker.sex = Settings.GENDER_MAN;
                        }
                    }
                    return sneaker;
                }
                else
                {
                    _logger.Info("sizes count = 0");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
