using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using NLog;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Controller.Parser;
using SneakerIcon.Controller.TelegramController;

namespace SneakerIcon.Controller.AllBrands.Parser.ShopsParsers
{
    public class SnkrsComParserAllBrands : ParserAllBrands
    {
        public const string SITEURL = "https://www.snkrs.com";
        public const string NAME = "snkrs.com";
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
            market_info.photo_parameters.background_color = "grey";
            market_info.photo_parameters.first_photo_sneaker_direction = "right";
            market_info.currently_language = "en";
            var json = CreateJson(market_info);
            SaveJson(json, JSON_FILENAME, FOLDER, NAME);
            var nikeFolder = DIRECTORY_PATH + @"\" + NAME + @"\";
            SaveNikeJson(json, JSON_FILENAME, nikeFolder, NAME);
        }

        public Catalog ParseSneakersFromAllPages()
        {
            Catalog catalog = new Catalog();

            var startPage = "https://www.snkrs.com/en/2-sneakers";
            _logger.Info("start page: " + startPage);
            ParseSneakersFromPage(catalog, "https://www.snkrs.com/en/2-sneakers");

            return catalog;
        }

        public void ParseSneakersFromPage(Catalog catalog, string link)
        {
            _logger.Info("Парсим " + link);
            //Uri uri = new Uri(link);
            //WebClient webClient = new WebClient();
            //webClient.Encoding = Encoding.UTF8;

            //string source = webClient.DownloadString(uri);
            //webClient.Dispose();
            //var parser = new HtmlParser();
            //var document = parser.Parse(source);
            var document = GetHtmlPage(link);

            //items
            var items = document.QuerySelectorAll("li.ajax_block_product");
            _logger.Info("items count: " + items.Length);

            foreach (var item in items)
            {
                var sneaker = new Sneaker();

                //fullCatalogSneaker.sex = sex;
                //fullCatalogSneaker.categorySneakerFullCatalog = categorySneakerFullCatalog;

                //link
                sneaker.link = item.QuerySelector("a.brand_name").GetAttribute("href");
                _logger.Info("sneaker.link: " + sneaker.link);

                //brand
                sneaker.brand = item.QuerySelector("a.brand_name").InnerHtml;
                sneaker.brand = sneaker.brand.Replace("Nike SB", "Nike");
                _logger.Info("sneaker.brand = " + sneaker.brand);

                //sneaker.link = SITEURL + linkHTML.GetAttribute("href");

                //prices             
                var salePriceHTML = item.QuerySelector("span.content_price.reduced");
                string priceString = String.Empty;
                //если товар идет по сейлу
                if (salePriceHTML != null)
                {
                    //price
                    priceString = item.QuerySelector("span.price.reduced").InnerHtml;
                    priceString = priceString.Replace("€", "").Trim();
                    sneaker.price = double.Parse(priceString);
                    _logger.Info("sneaker.price = " + sneaker.price);

                    //old price
                    priceString = item.QuerySelector("span.old_price").InnerHtml;
                    priceString = priceString.Replace("€", "").Trim();
                    sneaker.oldPrice = double.Parse(priceString);
                    _logger.Info("sneaker.oldPrice = " + sneaker.oldPrice);
                }
                else
                {
                    priceString = item.QuerySelector("span.price").InnerHtml.Replace("€", "").Trim();
                    sneaker.price = double.Parse(priceString);
                    _logger.Info("sneaker.price = " + sneaker.price);
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
                    {
                        catalog.sneakers.Add(sneaker);
                        _logger.Info("item add to catalog");
                    }
                    else
                    {
                        _logger.Info("item didn't add to catalog. Reason: already exist in catalog");
                    }

                }

            }

            //next page
            //var nextPage = document.QuerySelector("a.control.next");
            //if (nextPage != null)
            //{
            //    string nextPageLink = SITEURL + nextPage.GetAttribute("href");
            //    //System.Threading.Thread.Sleep(5000);
            //    ParseSneakersFromPage(catalog, nextPageLink);
            //}
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

        public Sneaker ParseOneSneaker(Sneaker @sneaker)
        {
            var document = GetHtmlPage(sneaker.link);
            _logger.Info("parsing... link:" + sneaker.link);

            //sku
            sneaker.sku = document.QuerySelector("span.editable").InnerHtml;
            _logger.Info("sneaker.sku = " + sneaker.sku);

            if (string.IsNullOrWhiteSpace(sneaker.sku.Trim()))
            {
                _logger.Warn("sku is empty");
                return null;
            }

            //color 
            //var colorHTML = document.All.Where(m => m.LocalName == "span" && m.Id == "product-color").First().InnerHtml;
            //sneaker.color = colorHTML;
            
            //title
            //добавляем цвет к тайтлу
            var title = document.QuerySelector("div.page_title").InnerHtml.ToUpper().Trim();
            sneaker.title = title.Replace(" - ", " ");
            _logger.Info("sneaker.title = " + sneaker.title);

            if (string.IsNullOrWhiteSpace(sneaker.title.Trim()))
            {
                _logger.Warn("title is empty");
                return null;
            }

            //images
            //var imageContainer = document.All.Where(m => m.LocalName == "div" && m.Id == "thumbnail-wrapper").First();
            var images = document.QuerySelector("#thumbs_list_frame").QuerySelectorAll("li");
            _logger.Info("images count = " + images.Length);
            //List<String> listImage = new List<String>();
            foreach (var image in images)
            {
                var imageString = image.QuerySelector("a").GetAttribute("href");
                //string imageString = SITEURL + imageHTML.GetAttribute("data-large-image");
                if (imageString.Contains(".jpg"))
                {
                    sneaker.images.Add(imageString);
                    _logger.Info("imageString = " + imageString);
                }
                else
                {
                    _logger.Warn("wrong image " + sneaker.sku);
                }
            }
            if (sneaker.images.Count == 0) _logger.Warn("no images " + sneaker.sku);

            //sizes
            var sizes = document.QuerySelectorAll("select.attribute_select option");
            _logger.Info("sizes.count = " + sizes.Length);
            for (int i = 0; i < sizes.Count(); i++)
            {
                var sep = new char[] {'-'};
                var sizeUsEu = sizes[i].InnerHtml.Split(sep);
                var sizeUS = sizeUsEu[0].Replace("US","").Trim();
                var sizeEU = sizeUsEu[1].Trim();
                _logger.Info("size: " + sizeUS + " US " + sizeEU + " EU");
                //string sizeUS = sizes[i].InnerHtml.Trim();
                SneakerSize size = new SneakerSize(sizeUS);
                size.sizeEU = sizeEU;
                size.quantity = 1;
                sneaker.sizes.Add(size);
            } //sizes

            //chech wait release 
            var wait = document.QuerySelector("#axproductlaunch");
            if (wait != null)
            {
                var price = document.QuerySelector("#our_price_display").InnerHtml.Trim();
                var br = "\n";              
                var m = sneaker.title + br;
                m += "date: " + wait.InnerHtml.Replace("\n", "").Trim() + br;
                m += "price: " + price + br;             
                m += "<a href=\"" + sneaker.images[0] + "\">image</a>" + br;
                m += "<a href=\"" + sneaker.link + "\">" + NAME + "</a>" + br;
                _logger.Info("This item is wait release");
                MyTelegram.PostMessageWaitRelease(m);
                return null;
            }

            //categorySneakerFullCatalog and sex
            if (sneaker.sizes.Count > 0)
            {
                return sneaker;
            }
            else
            {
                _logger.Info("sizes count = 0");
                return null;
            }
        }
    }
}
