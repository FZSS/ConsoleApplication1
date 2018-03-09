using SneakerIcon.Classes.SizeConverters;
using SneakerIcon.Classes.SizeConverters.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Exporters
{
    public class BonanzaRecord
    {
        public string id { get; set; }
        public string description { get; set; }
        public int quantity { get; set; }
        public string category { get; set; }
        public string title { get; set; }
        public string price { get; set; }
        public string shipping_type { get; set; }
        public int shipping_price { get; set; }
        public string shipping_carrier { get; set; }
        public string shipping_service { get; set; }
        public string shipping_package { get; set; }
        //public int worldwide_shipping_price { get; set; }
        //public string worldwide_shipping_type { get; set; }
        public string image1 { get; set; }
        public string image2 { get; set; }
        public string image3 { get; set; }
        public string image4 { get; set; }
        public string brand { get; set; }
        public string size { get; set; }
        //public string us_size { get; set; }
        public string condition { get; set; }
        public string upc { get; set; }
        public string force_update { get; set; }
        public string MPN { get; set; }
        public string width { get; set; }
        public string traits { get; set; }


        public BonanzaRecord()
        {

        }

        //public BonanzaRecord(string brand, string title, string sku, string sizeUS, string upc, string sex, string categorySneakerFullCatalog, double price)
        //{
        //    this.run(brand, title, sku, sizeUS, upc, sex, categorySneakerFullCatalog, price);
        //}

        public static string CreateSizeString(string size, string category)
        {
            var converter = new SizeConverter(size, category);
            var sizeString = size + "US / " + converter.sizeUK + "UK / " + converter.sizeEUR + "EU / " + converter.sizeCM + "CM";
            return sizeString;
        }

        public bool run(string brand, string title, string sku, string size, string upc, string sex, string category, double price, List<string> images)
        {
            //int marzha = 1500;
            //int dostavka = 1600;
            //int dollar = 61;

            //id
            //this.id = sku + "-" + sizeUS;

            //description
            string sizeString = String.Empty; 
            try
            {
                var converter = new SizeConverter(size, category);
                sizeString = size + "US / " + converter.sizeUK + "UK / " + converter.sizeEUR + "EU / " + converter.sizeCM + "CM";
            }
            catch (Exception e)
            {
                string message = "Wrong size. sku:" + sku + " size:" + size + " category: " + category;
                Program.Logger.Warn(message);
                //Console.WriteLine("Размер и категория не совпадают. sku:" + sku + " sizeUS:" + sizeUS);
                return false;
            }
            //var converterMan = new SizeConverter(sizeUS, categorySneakerFullCatalog);
            this.description = SetDescription(title,sku,sizeString);

            //quantity
            this.quantity = 2;

            //categorySneakerFullCatalog
            switch (category)
            {
                case "Мужская" :
                    this.category = "93427"; //Full ftpFolder: Fashion >> Men's Shoes
                    this.width = "Medium (D, M)";
                    break;
                case "Женская":
                    this.category = "3034"; //Full ftpFolder: Fashion >> Women's Shoes
                    this.width = "Medium (B, M)";
                    break;
                case "Детская":
                    this.width = "Medium";
                    if (sex == "Мужской")
                    {
                        this.category = "57929"; //Fashion >> Kids' Clothing, Shoes & Accs >> Boys' Shoes
                    }
                    else if (sex == "Женский")
                    {
                        this.category = "57974"; //Fashion >> Kids' Clothing, Shoes & Accs >> Girls' Shoes
                    }
                    else //детская унисекс
                    {
                        this.category = "155202"; //Full ftpFolder: Fashion >> Kids' Clothing, Shoes & Accs >> Unisex Shoes
                    }
                    break;
                default:
                    this.category = "93427"; //по умолчанию тоже мужская обувь
                    break;
            }

            //title
            string hvost = " SZ " + size + "US " + sku;
            if (brand.Count() + title.Count() + hvost.Count() < 80)
            {
                this.title = title.ToUpper() + hvost;
            }
            else
            {
                int indexSubstringTitle = 79 - hvost.Count() - brand.Count() - 1;
                this.title = title.ToUpper().Substring(0, indexSubstringTitle) + hvost;
                int titlecount = this.title.Count();
            }
            

            //price
            //double floatprice = ((price + dostavka) * 1.18 / dollar);
            double floatprice = (price * 1.18) - 29;
            this.price = floatprice.ToString("F", CultureInfo.CreateSpecificCulture("en-US"));

            //shipping
            this.shipping_price = 29;
            this.shipping_type = "flat";
            this.shipping_carrier = "usps";
            this.shipping_service = "EconomyShipping";
            this.shipping_package = "normal";
            //this.worldwide_shipping_type = "flat";          
            //this.worldwide_shipping_price = 29;

            //others
            if (images == null) {
                return false;
                //this.image1 = "http://photo.sneakersfamily.ru/" + sku + "-1.jpg";
                //this.image2 = "http://photo.sneakersfamily.ru/" + sku + "-2.jpg";
                //this.image3 = "http://photo.sneakersfamily.ru/" + sku + "-3.jpg";
                //this.image4 = "http://photo.sneakersfamily.ru/" + sku + "-4.jpg";
            }
            else
            {
                if (images.Count == 1 && String.IsNullOrWhiteSpace(images[0]))
                {
                    return false;
                    //this.image1 = "http://photo.sneakersfamily.ru/" + sku + "-1.jpg";
                    //this.image2 = "http://photo.sneakersfamily.ru/" + sku + "-2.jpg";
                    //this.image3 = "http://photo.sneakersfamily.ru/" + sku + "-3.jpg";
                    //this.image4 = "http://photo.sneakersfamily.ru/" + sku + "-4.jpg";
                }
                else
                {
                    if (images.Count > 0)
                        this.image1 = images[0];
                    if (images.Count > 1)
                        this.image2 = images[1];
                    if (images.Count > 2)
                        this.image3 = images[2];
                    if (images.Count > 3)
                        this.image4 = images[3];
                }
            }

            this.brand = brand;
            this.size = size;
            //this.us_size = size;
            this.traits = "[[US Size:"+size+"]] ";
            this.condition = "New with box";
            this.upc = upc;
            this.MPN = sku;
            this.traits += "[[MPN:" + sku + "]] ";
            this.force_update = "true";
            return true;
        }

        private string SetDescription(string title, string sku, string sizeString)
        {
            string description =
                "<h3 style=\"text-align: center;\"><strong>" +
                title.ToUpper() +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "STYLE: " + sku +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "SIZE: " + sizeString +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "100% AUTHENTIC" +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "WORLDWIDE SHIPPING FOR 5-10 DAYS IN DOUBLE BOX WITH TRACKING NUMBER.<br>" +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "SHIP IN 2 BUSINESS DAY." +
                "</strong></h3>";

            //throw new NotImplementedException();
            return description;
        }

        public static string CreateDescription2Small(string title, string sku, string sizeString, List<string> sizeList)
        {
            var boothUrl = System.Configuration.ConfigurationManager.AppSettings["bonanzaBoothUrl"];

            string description =
                "<h3 style=\"text-align: center;\"><strong>" +
                title.ToUpper() +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "STYLE: " + sku +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "SIZE: " + sizeString +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "100% AUTHENTIC" +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "WORLDWIDE SHIPPING FOR 5-10 DAYS IN DOUBLE BOX WITH TRACKING NUMBER.<br>" +
                "</strong></h3>" +
                "<h3 style=\"text-align: center;\"><strong>" +
                "SHIP IN 2 BUSINESS DAY." +
                "</strong></h3>";

            //description += "<h3 style=\"text-align: center;\"><strong>All sizes of this model available in stock</strong></h3>";

            //if (sizeList != null)
            //{
            //    if (sizeList.Count > 0)
            //    {
            //        foreach (var size in sizeList)
            //        {
            //            //725076-301+6.5US
            //            description += "<h4 style=\"text-align: center;\"><strong><a href=\"" + boothUrl + "?utf8=%E2%9C%93&amp;item_sort_options%5Bfilter_string%5D=" + sku + "+" + size + "US&amp;item_sort_options%5Bfilter_category_id%5D=&amp;item_sort_options%5Bcustom_category_id%5D=&amp;commit=Go\" rel=\"nofollow\" target=\"_blank\">Size " + size + " US</a></strong></h4>";
            //        }
            //    }
            //}

            description += "<h3 style=\"text-align: center;\"><strong>" +
                "Other sizes of this model available in out stock: " +
                "<a href=\"" + boothUrl + "?utf8=%E2%9C%93&amp;item_sort_options%5Bfilter_string%5D=" + sku + "&amp;item_sort_options%5Bfilter_category_id%5D=&amp;item_sort_options%5Bcustom_category_id%5D=&amp;commit=Go\" rel=\"nofollow\" target=\"_blank\">Link</a>"+
                "</strong></h3>";

            description += "<h3 style=\"text-align: center;\"><strong>" + "Please feel free to ask any questions and see <a href=\"" + boothUrl + "\" rel=\"nofollow\" target=\"_blank\">all our listings</a> for more great deals." + "</strong></h3>";

            //throw new NotImplementedException();
            return description;
        }

        public static string SetDescription2(string title, string sku, string sizeString, string brand, string condition, List<string> sizeList)
        {
            var boothUrl = System.Configuration.ConfigurationManager.AppSettings["bonanzaBoothUrl"];

            string desc = "<p><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-480a-1c82-6d0c-646e48139b74\"><span style=\"font-size: 19pt; color: rgb(106, 168, 79); font-weight: 700; vertical-align: baseline; white-space: pre-wrap;\">Product Description</span></span></span></p>";
            desc += "<p dir=\"ltr\" style=\"line-height: 1.295; margin-top: 2pt; margin-bottom: 0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><strong>" + title.ToUpper() + "</strong></span></p>";
            desc += "<ul style=\"margin-top:0pt;margin-bottom:0pt;\"><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:14pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">Model:В </span></span>" + sku + "<span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">В </span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">Size: </span></span>"+sizeString+"</span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">Brand: "+brand+"</span></span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">Condition: "+condition+"</span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:14pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">Guaranteed 100%</span><span style=\"font-size: 14pt; font-weight: 700; vertical-align: baseline; white-space: pre-wrap;\"> </span><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">authentic </span></span></span></p></li></ul>";

            if (sizeList != null)
            {
                if (sizeList.Count > 0)
                {
                    desc += "<p dir=\"ltr\" style=\"line-height:1.295;margin-top:0pt;margin-bottom:0pt;\"><span style=\"color: rgb(106, 168, 79); font-family: arial, helvetica, sans-serif; font-size: 25.3333px; font-weight: bold; white-space: pre-wrap;\">All sizes of this model available in stock</span></p><p dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;margin-top: 0pt; margin-bottom: 0pt; line-height: 1.2;\">В </p><ul style=\"margin-top: 0pt; margin-bottom: 0pt;\">";
                    foreach (var size in sizeList)
                    {
                        desc += "	<li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"margin-top: 0pt; margin-bottom: 0pt; line-height: 1.2;\"><a href=\""+boothUrl+"?utf8=%E2%9C%93&amp;item_sort_options%5Bfilter_string%5D="+sku+"-"+size+"&amp;item_sort_options%5Bfilter_category_id%5D=&amp;item_sort_options%5Bcustom_category_id%5D=&amp;commit=Go\" rel=\"nofollow\" target=\"_blank\"><span style=\"font-family: arial, helvetica, sans-serif;\"><span style=\"font-size: 14.6667px; white-space: pre-wrap;\">Size "+size+" US</span></span></a></p></li>";
                    }
                    desc += "</ul><p style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\">В </p>";
                }
            }

            desc += "<p dir=\"ltr\" style=\"line-height:1.295;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 19pt; color: rgb(106, 168, 79); font-weight: 700; vertical-align: baseline; white-space: pre-wrap;\">Shipping Information</span></span></span></p><p dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;line-height:1.2;margin-top:0pt;margin-bottom:0pt;\">В </p><ul style=\"margin-top:0pt;margin-bottom:0pt;\"><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span style=\"color: rgb(0, 0, 0); font-size: 14.6667px; white-space: pre-wrap;\">All orders will be dispatched within 24 hours of payment on business days. Order placed on Saturday will be shipped on Monday.</span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span style=\"color: rgb(0, 0, 0); font-size: 14.6667px; white-space: pre-wrap;\">Worldwide shipping takes 5-10 days</span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">All shipping methods come with tracking number</span></span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">Please make sure your address is correct before purchase.</span></span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">Please make sure to check with conversion charts for sizing. Keep in mind that not all brands have same sizing conversions.</span></span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">Please keep in mind that import duties, taxes and charges are not included in the item price or shipping charges. These charges are the buyer's responsibility. Taxes and duties vary for each country. Please check with your country's customs office to determine what these additional costs will be prior to bidding/buying.</span></span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">Expect delayed shipments during customs, holidays or inclement weather conditions.</span></span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:14pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">Please note that once a package is picked up by post service we have no control over the transit time while in route to the final destination.</span></span></span></p></li></ul>";

            desc += "<p dir=\"ltr\" style=\"line-height:1.295;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 19pt; color: rgb(106, 168, 79); font-weight: 700; vertical-align: baseline; white-space: pre-wrap;\">Return / Exchanges</span></span></span></p><ul style=\"margin-top:0pt;margin-bottom:0pt;\"><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:14pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">We accept returns within 14 days from the time you received the pair.</span></span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">All returned merchandise must be BRAND NEW and UNWORN. </span></span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">For return instructions, please write us a message </span></span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:14pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">If the product is not in resalable conditions (brand new, unworn and in original packaging), return for that item will not be accepted and shipping charges will not be refunded. </span></span></span></p></li></ul><p dir=\"ltr\" style=\"line-height:1.295;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 19pt; color: rgb(106, 168, 79); font-weight: 700; vertical-align: baseline; white-space: pre-wrap;\">Terms &amp; Conditions</span></span></span></p><ul style=\"margin-top:0pt;margin-bottom:0pt;\"><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:14pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">EVERYTHING we sell is 100% Authentic.В We are a sneaker collector as well and know everything there is to know regarding the real thing compared to variants/B-grade/counterfeit sneakers and clothing. So please refrain from asking this question, just check my feedbackВ </span></span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:0pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">If you feel that we have made a mistake or that we do not deserve a positive rating, please contact us through messages so we can address any issues. </span></span></span></p></li><li dir=\"ltr\" style=\"list-style-type: disc; font-size: 10pt; font-family: Arial; color: rgb(0, 0, 0); vertical-align: baseline;\"><p dir=\"ltr\" style=\"line-height:1.2;margin-top:0pt;margin-bottom:14pt;\"><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 11pt; vertical-align: baseline; white-space: pre-wrap;\">We are committed to making sure that you leave this transaction satisfied. That means having access to real people that get your questions and concerns answered quickly. Give us a shot and we will make sure that you will look to us again! </span></span></span></p></li></ul><p><span style=\"font-family:arial,helvetica,sans-serif;\"><span id=\"docs-internal-guid-ba942b0a-4801-b904-97fc-ff7beae2823b\"><span style=\"font-size: 10pt; color: rgb(0, 0, 0); font-weight: 700; vertical-align: baseline; white-space: pre-wrap;\">Please feel free to ask any questions and see </span><a href=\""+boothUrl+"\" rel=\"nofollow\" target=\"_blank\"><span style=\"font-size: 10pt; color: rgb(17, 85, 204); font-weight: 700; text-decoration: underline; vertical-align: baseline; white-space: pre-wrap;\">our other listings</span></a><span style=\"font-size: 10pt; color: rgb(0, 0, 0); font-weight: 700; vertical-align: baseline; white-space: pre-wrap;\"> for more great deals.</span></span></span></p>"; 

            return desc;
        }
    }
}
