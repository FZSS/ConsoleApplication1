using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NLog;
using SneakerIcon.Classes;
using SneakerIcon.Classes.Catalogs;
using SneakerIcon.Classes.SizeConverters;
using SneakerIcon.Controller.Parser;
using SneakerIcon.Controller.TelegramController;

namespace SneakerIcon.Controller.AllBrands.Parser.ShopsParsers
{
    public class SivasParserAB : ParserAllBrands
    {
        public const string SITEURL = "https://www.sivasdescalzo.com";
        public const string NAME = "sivasdescalzo.com";
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
            market_info.photo_parameters.first_photo_sneaker_direction = "left";
            market_info.currently_language = "en";
            var json = CreateJson(market_info);
            SaveJson(json, JSON_FILENAME, FOLDER, NAME);
        }

        public Catalog ParseSneakersFromAllPages()
        {
            Catalog catalog = new Catalog();

            var startPage = "https://www.snkrs.com/en/2-sneakers";
            _logger.Info("start page: " + startPage);
            ParseSneakersFromPage(catalog, "Adidas", "https://www.sivasdescalzo.com/en/search/?q=Adidas&fq%5Bsize_us%5D%5B0%5D=4&fq%5Bsize_us%5D%5B1%5D=4.5&fq%5Bsize_us%5D%5B2%5D=5&fq%5Bsize_us%5D%5B3%5D=5.5&fq%5Bsize_us%5D%5B4%5D=6&fq%5Bsize_us%5D%5B5%5D=6.5&fq%5Bsize_us%5D%5B6%5D=7&fq%5Bsize_us%5D%5B7%5D=7.5&fq%5Bsize_us%5D%5B8%5D=8&fq%5Bsize_us%5D%5B9%5D=8.5&fq%5Bsize_us%5D%5B10%5D=9&fq%5Bsize_us%5D%5B11%5D=9.5&fq%5Bsize_us%5D%5B12%5D=10&fq%5Bsize_us%5D%5B13%5D=10.5&fq%5Bsize_us%5D%5B14%5D=11&fq%5Bsize_us%5D%5B15%5D=11.5&fq%5Bsize_us%5D%5B16%5D=12&fq%5Bsize_us%5D%5B17%5D=12.5&fq%5Bsize_us%5D%5B18%5D=13");

            ParseSneakersFromPage(catalog, "Reebok", "https://www.sivasdescalzo.com/en/search/?q=Reebok&fq%5Bsize_us%5D%5B0%5D=4&fq%5Bsize_us%5D%5B1%5D=5&fq%5Bsize_us%5D%5B2%5D=6&fq%5Bsize_us%5D%5B3%5D=4.5&fq%5Bsize_us%5D%5B4%5D=6.5&fq%5Bsize_us%5D%5B5%5D=7&fq%5Bsize_us%5D%5B6%5D=5.5&fq%5Bsize_us%5D%5B7%5D=3.5&fq%5Bsize_us%5D%5B8%5D=7.5&fq%5Bsize_us%5D%5B9%5D=8&fq%5Bsize_us%5D%5B10%5D=8.5&fq%5Bsize_us%5D%5B11%5D=9&fq%5Bsize_us%5D%5B12%5D=9.5&fq%5Bsize_us%5D%5B13%5D=10&fq%5Bsize_us%5D%5B14%5D=10.5&fq%5Bsize_us%5D%5B15%5D=11&fq%5Bsize_us%5D%5B16%5D=11.5&fq%5Bsize_us%5D%5B17%5D=12&fq%5Bsize_us%5D%5B18%5D=12.5&fq%5Bsize_us%5D%5B19%5D=13");

            ParseSneakersFromPage(catalog, "Puma", "https://www.sivasdescalzo.com/en/search/?q=Puma&fq%5Bsize_us%5D%5B0%5D=4.5&fq%5Bsize_us%5D%5B1%5D=5&fq%5Bsize_us%5D%5B2%5D=4&fq%5Bsize_us%5D%5B3%5D=6&fq%5Bsize_us%5D%5B4%5D=6.5&fq%5Bsize_us%5D%5B5%5D=7&fq%5Bsize_us%5D%5B6%5D=7.5&fq%5Bsize_us%5D%5B7%5D=5.5&fq%5Bsize_us%5D%5B8%5D=8&fq%5Bsize_us%5D%5B9%5D=8.5&fq%5Bsize_us%5D%5B10%5D=9&fq%5Bsize_us%5D%5B11%5D=9.5&fq%5Bsize_us%5D%5B12%5D=10&fq%5Bsize_us%5D%5B13%5D=10.5&fq%5Bsize_us%5D%5B14%5D=11&fq%5Bsize_us%5D%5B15%5D=11.5&fq%5Bsize_us%5D%5B16%5D=12&fq%5Bsize_us%5D%5B17%5D=13");

            ParseSneakersFromPage(catalog, "New Balance", "https://www.sivasdescalzo.com/en/search/?q=New+Balance");

            ParseSneakersFromPage(catalog, "Vans", "https://www.sivasdescalzo.com/en/search/?q=Vans&fq%5Bsize_us%5D%5B0%5D=5.5&fq%5Bsize_us%5D%5B1%5D=6&fq%5Bsize_us%5D%5B2%5D=6.5&fq%5Bsize_us%5D%5B3%5D=4.5&fq%5Bsize_us%5D%5B4%5D=5&fq%5Bsize_us%5D%5B5%5D=7&fq%5Bsize_us%5D%5B6%5D=3.5&fq%5Bsize_us%5D%5B7%5D=7.5&fq%5Bsize_us%5D%5B8%5D=8&fq%5Bsize_us%5D%5B9%5D=8.5&fq%5Bsize_us%5D%5B10%5D=9&fq%5Bsize_us%5D%5B11%5D=9.5&fq%5Bsize_us%5D%5B12%5D=10&fq%5Bsize_us%5D%5B13%5D=10.5&fq%5Bsize_us%5D%5B14%5D=11&fq%5Bsize_us%5D%5B15%5D=11.5&fq%5Bsize_us%5D%5B16%5D=12");

            return catalog;
        }

        public void ParseSneakersFromPage(Catalog catalog, string brand, string link)
        {
            _logger.Info("Парсим " + link);
            var document = GetHtmlPageCrawlera5Try(link);
            if (document == null)
                return;

            //items
            var items = document.QuerySelector("ul.products-list").QuerySelectorAll("li");

            foreach (var item in items)
            {
                var sneaker = new Sneaker();

                //fullCatalogSneaker.sex = sex;
                //fullCatalogSneaker.categorySneakerFullCatalog = categorySneakerFullCatalog;

                //brand
                sneaker.brand = brand;

                //link
                var links = item.QuerySelectorAll("a");
                sneaker.link = links[2].GetAttribute("href");

                //prices
                var priceHTML = item.QuerySelector("div.price");
                var salePriceHTML = item.QuerySelector("div.sale-price");
                string priceString = String.Empty;
                //если товар идет по сейлу
                if (salePriceHTML != null)
                {
                    //price
                    priceString = salePriceHTML.QuerySelector("b").InnerHtml;
                    priceString = priceString.Replace(".", ",").Trim();
                    sneaker.price = double.Parse(priceString);

                    //old price
                    priceString = priceHTML.QuerySelector("b").InnerHtml;
                    priceString = priceString.Replace(".", ",").Trim();
                    sneaker.oldPrice = double.Parse(priceString);
                }
                else
                {
                    priceString = priceHTML.QuerySelector("b").InnerHtml;
                    priceString = priceString.Replace(".", ",").Trim();
                    sneaker.price = double.Parse(priceString);
                }

                //title
                sneaker.title = item.QuerySelector("span.model").InnerHtml;
                if (!sneaker.title.Contains(brand.ToUpper()))
                    sneaker.title = brand.ToUpper() + " " + sneaker.title.ToUpper();
                sneaker.title = WebUtility.HtmlDecode(sneaker.title);
                sneaker.ParseTitle();

                if (!catalog.isExistSneakerInCatalog(sneaker))
                    catalog.sneakers.Add(sneaker);
            }

            //next page
            var nextPages = document.QuerySelector("ol.left").QuerySelectorAll("a");
            foreach (var nextPage in nextPages)
            {
                if (nextPage.GetAttribute("title") == "Next page")
                {
                    string nextPageLink = nextPage.GetAttribute("href");
                    ParseSneakersFromPage(catalog, brand, nextPageLink);
                }
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

        public Sneaker ParseOneSneaker(Sneaker @sneaker)
        {
            var document = GetHtmlPageCrawlera5Try(sneaker.link);
            if (document == null)
                return null;
            _logger.Info("parsing... link:" + sneaker.link);

            //sku
            sneaker.sku = document.QuerySelector("p.ref.hide-for-small").InnerHtml;

            //images
            var images = document.QuerySelector("div.medium-5.columns.column-carousel").QuerySelectorAll("meta");
            //List<String> listImage = new List<String>();
            foreach (var image in images)
            {
                string imageString = image.GetAttribute("content");
                if (imageString.Contains(".jpg"))
                    sneaker.images.Add(imageString);
            }
            sneaker.images.RemoveAt(sneaker.images.Count - 1);

            //sizes
            var HTMLSizes = document.QuerySelector("div.content.size-options.size_us-options");
            if (HTMLSizes == null) //если null значит выход модели только ожидается
            {
                //var price = sneaker.price;
                //var br = "\n";
                //var m = sneaker.title + br;
                ////todo добавить парсинг даты релиза
                ////m += "date: " + wait.InnerHtml.Replace("\n", "").Trim() + br;
                //m += "price: " + price + br;
                //m += "<a href=\"" + sneaker.images[0] + "\">image</a>" + br;
                //m += "<a href=\"" + sneaker.link + "\">" + NAME + "</a>" + br;
                //_logger.Info("This item is wait release");
                //MyTelegram.PostMessageWaitRelease(m);
                return null;
            }
            else
            {
                var sizesUs = HTMLSizes.QuerySelectorAll("a.size-button.available");
                var sizesEu = document.QuerySelector("div.content.size-options.size_eu-options")
                    .QuerySelectorAll("a.size-button.available");
                var sizesUk = document.QuerySelector("div.content.size-options.size_uk-options")
                    .QuerySelectorAll("a.size-button.available");
                var sizesCm = document.QuerySelector("div.content.size-options.size_cm-options")
                    .QuerySelectorAll("a.size-button.available");



                for (int i = 0; i < sizesUs.Length; i++)
                {
                    var sizeUs = sizesUs[i].InnerHtml.Trim();
                    var sizeEu = sizesEu[i].InnerHtml.Trim();
                    var sizeUk = sizesUk[i].InnerHtml.Trim();
                    var sizeCm = sizesCm[i].InnerHtml.Trim();
                    SneakerSize size = new SneakerSize(sizeUs);
                    size.sizeEU = sizeEu;
                    size.sizeUK = sizeUk;
                    size.sizeCM = sizeCm;
                    size.quantity = 1;
                    sneaker.sizes.Add(size);
                } //sizes

                if (sneaker.sizes.Count == 0)
                {
                    //отсекаем солдаут позиции
                    _logger.Warn("sneaker.sizes = 0. link: " + sneaker.link);
                    return null;
                }

                ////categorySneakerFullCatalog and sex
                //if (sneaker.sizes.Count > 0)
                //{
                //    if (sneaker.sizes[0].sizeUS.Contains("C") || sneaker.sizes[0].sizeUS.Contains("Y"))
                //    {
                //        sneaker.category = Settings.CATEGORY_KIDS;
                //    }
                //    else
                //    {
                //        string sizeEU = document.QuerySelector("div.content.size-options.size_eu-options").QuerySelectorAll("a.size-button.available")[0].InnerHtml;
                //        SizeConverter converterMan = new SizeConverter(sneaker.sizes[0].sizeUS, Settings.CATEGORY_MEN);
                //        if (converterMan.sizeEUR == sizeEU)
                //        {
                //            sneaker.category = Settings.CATEGORY_MEN;
                //            sneaker.sex = Settings.GENDER_MAN;
                //        }
                //        else
                //        {
                //            SizeConverter converterWoman = new SizeConverter(sneaker.sizes[0].sizeUS, Settings.CATEGORY_WOMEN);
                //            if (converterWoman.sizeEUR == sizeEU)
                //            {
                //                sneaker.category = Settings.CATEGORY_WOMEN;
                //                sneaker.sex = Settings.GENDER_WOMAN;
                //            }
                //            else
                //            {
                //                Program.Logger.Warn("wrong category sku:" + sneaker.sku + " link: " + sneaker.link);
                //            }
                //        }
                //    }
                //}

                //color
                sneaker.color = document.QuerySelector("p.variant.hide-for-small").InnerHtml;
                sneaker.title += " " + sneaker.color.ToUpper();

                return sneaker;
            }
        }
    }
}
