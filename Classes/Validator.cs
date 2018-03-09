using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Classes.SizeConverters.Model;
using SneakerIcon.Controller.ValitatorController;
using SneakerIcon.Model.ShopCatalogModel;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;
using SneakerIcon.Model.AllBrands.DB;
using SneakerIcon.Model.FullCatalog;

namespace SneakerIcon.Classes
{
    public class Validator
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public string log { get; set; }
        public static string br = "\n";

        public Validator()
        {
            log = String.Empty;
        }

        public static void ValidateAllShops()
        {
            ShopValidator.ValidateAllShops();
        }

        public static bool ValidateTitle(string title, string brand, out string log)
        {
            var titleLow = title.ToLower();
            var brandLow = brand.ToLower();

            log = String.Empty;

            //тайтл пустой?
            if (String.IsNullOrWhiteSpace(title))
            {
                log = "WARN: Title is Empty";
                return false;
            }

            //содержит бренд?
            if (!titleLow.Contains(brandLow))
            {
                log = "WARN: Title didn't contains brand. title: " + title;
                return false;
            }

            //перед проверкой на 2 бренда можно 

            //может быть содержит бренд 2 раза?
            var sep = new string[] { brandLow };
            var countBrandContaing = titleLow.Split(sep,StringSplitOptions.None);
            if (countBrandContaing.Length > 2)
            {
                log = "WARN: Title contains brand 2 or more tnan once. title: " + title;
                return false;
            }

            return true;
        }

        internal static string CorrectTitle(string title, string brand)
        {
            //меняем два бренда в названии
            title = title.ToUpper();
            var upBrand = brand.ToUpper();
            title = title.Replace(upBrand + " " + upBrand, upBrand);

            if (brand.ToLower() == "jordan")
            {
                title = title.Replace("JORDAN AIR JORDAN", "JORDAN");
            }

            return title;
        }

        public static bool ValidateSku(string sku, string brand)
        {
            if (sku == "RM17S9810-BY2992-1141")
            {
                bool test = true;
            }

            if (brand.ToLower() == "nike" || brand.ToLower() == "jordan")
            {
                var sPattern = @"^(A|\d){6}\-\d{3}$";
                if (System.Text.RegularExpressions.Regex.IsMatch(sku, sPattern))
                {
                    return true; //всё ок работает, проверил, можно переделать на этот паттерн
                }
                return false;
            }
            else if (brand.ToLower() == "adidas" || brand.ToLower() == "reebok")
            {
                var sPattern = @"^[A-Z]{1}[0-9A-Z]{1}\d{4}$";
                if (System.Text.RegularExpressions.Regex.IsMatch(sku, sPattern))
                {
                    return true; //всё ок работает, проверил, можно переделать на этот паттерн
                }
                return false;
            }
            else if (brand.ToLower() == "new balance")
            {
                //var sPattern = @"[A-Z]{1,3}\d{3,4}[0-9A-Z]{1,3}";
                var sPattern = @"^[A-Z]{1,3}[0-9A-Z]{3,4}[0-9A-Z]{1,3}$";
                if (System.Text.RegularExpressions.Regex.IsMatch(sku, sPattern))
                {
                    return true; //всё ок работает, проверил, можно переделать на этот паттерн
                }
                return false;
            }
            else if (brand.ToLower() == "puma")
            {
                var sPattern = @"^\d{6}-\d{2}$";
                if (System.Text.RegularExpressions.Regex.IsMatch(sku, sPattern))
                {
                    return true; //всё ок работает, проверил, можно переделать на этот паттерн
                }
                return false;
            }
            else if (brand.ToLower() == "converse")
            {
                var sPattern = @"^(A|\d){6}C\-\d{3}$";
                if (System.Text.RegularExpressions.Regex.IsMatch(sku, sPattern))
                {
                    return true; //всё ок работает, проверил, можно переделать на этот паттерн
                }
                return false;
            }
            else if (brand.ToLower() == "vans")
            {
                var sPattern = @"^V[0-9A-Z]{5,8}$";
                if (System.Text.RegularExpressions.Regex.IsMatch(sku, sPattern))
                {
                    return true; //всё ок работает, проверил, можно переделать на этот паттерн
                }
                return false;
            }
            else
            {
                Program.Logger.Warn("Validate SKU Error. Wrong brand");
                return false;
            }
        }

        public static bool ValidateBrand(string brand)
        {
            var brandList = new List<string>() {"Nike", "Jordan", "Adidas", "New Balance", "Puma", "Reebok", "Converse", "Vans"};
            var brand2 = brandList.Find(x => x.ToLower() == brand.ToLower());

            if (brand2 != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="category">Eng Category. Values: men women kids</param>
        /// <returns></returns>
        public static bool ValidateCategory(string category)
        {
            if (category == "men" || category == "women" || category == "kids")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static string CorrectCategory(string category)
        {
            category = category.ToLower().Trim();

            //если вдруг русская категория, то преобразуем в англ.
            var engCategory = Helper.ConvertCategoryRusToEng(category);
            if (engCategory != null)
                category = engCategory;

            return category;
        }



        /* Если категория уже есть, то всё ок. (на соответствие категории и размеров проверяем позже)
         * Если категории нет, то
         *  Смотрим, есть ли размеры в разных размерных сетках
         *      Если нет, 
         *          то оставляем листинг без категории, бросаем варнинг что категория не определена
         *          и проверку на размеры проходить не надо, 
         *          или проходить но всё равно добавлять эти листинги в валид json, иначе они не попадут в фулкаталог. 
         *          Или же в фулкаталог добавлять товары 
         *      Если да, 
         *          то пробуем определить категорию по размерным сеткам разным. 
         *          При чем нужно проверять не один, а все размеры. 
         *          И присваивать категорию только в том случае если все размеры (или больше половиные) подтверждают эту категорию. 
         *          Чтобы исключить ошибку, что первый размер например детский, а второй и следующий женский. 
         *          Вряд ли конечно такое будет, но всё же.
         */
        public static string ValidateCategory(Listing listing, out string log)
        {
            //string resultCategory;
            log = String.Empty;

            if (!String.IsNullOrWhiteSpace(listing.category))
            {

            }
            else
            {
                //если категория null, пытаемся определить её по другим размерам

            }

            return null;
        }


        public static string ValidateSize(string brand, string category, string sizeUS, string sizeEU, string sizeUK, string sizeCM, SizeChart sizeChart = null)
        {
            var log = String.Empty;
            if (ValidateBrand(brand))
            {
                if (ValidateCategory(category))
                {
                    //проверяю есть ли такой размер в таблице размеров (по разным размерным сеткам)
                    if (sizeChart == null)
                        sizeChart = SizeChart.ReadSizeChartJson();

                    var size = new Size(brand, category, sizeUS, sizeEU, sizeUK, sizeCM, null);
                    var resultSize = sizeChart.GetSize(size,out log);
                    if (resultSize == null)
                    {                     
                        return log;
                    }
                    else {
                        return null; //если null значит всё норм
                    }
                }
                else
                {
                    log = "ValidateSize failed. Reason: wrong category. Class Validator. Category value: " + category;
                    return log;
                }
            }
            else
            {
                log = "ValidateSize failed. Reason: wrong brand. Class Validator. Brand value: " + brand;
                return log;
            }
        }

        public static string DetectCategoryFromTitle(FullCatalogRecord record)
        {
            return DetectCategoryFromTitle(record.title, record.brand);
        }

        public static string DetectCategoryFromTitle(string title, string brand)
        {
            if (brand.ToLower() == "nike" || brand.ToLower() == "jordan")
            {
                //женская
                if (title.ToUpper().Contains("WMNS"))
                    return "women";

                if (title.ToUpper().Contains("WMNA"))
                    return "women";

                if (title.ToUpper().Contains("WMS"))
                    return "women";

                if (title.ToUpper().Contains("WMN"))
                    return "women";

                if (title.ToUpper().Contains("W NIKE"))
                    return "women";

                if (title.ToUpper().Contains("NIKE W "))
                    return "women";

                if (title.ToUpper().Contains("NIKELAB W "))
                    return "women";

                //kids

                if (Regex.IsMatch(title.ToUpper(), @"( |\()GS($| |\))"))
                    return "kids";

                if (Regex.IsMatch(title.ToUpper(), @"( |\()GG($| |\))"))
                    return "kids";

                if (Regex.IsMatch(title.ToUpper(), @"( |\()PS($| |\))"))
                    return "kids";

                if (Regex.IsMatch(title.ToUpper(), @"( |\()PSV($| |\))"))
                    return "kids";

                if (Regex.IsMatch(title.ToUpper(), @"( |\()TD($| |\))"))
                    return "kids";

                if (Regex.IsMatch(title.ToUpper(), @"( |\()BG($| |\))"))
                    return "kids";

                if (Regex.IsMatch(title.ToUpper(), @"( |\()JR($| |\))"))
                    return "kids";

                if (Regex.IsMatch(title.ToUpper(), @"( |\()CB($| |\))"))
                    return "kids";

                if (Regex.IsMatch(title.ToUpper(), @"( |\()TDV($| |\))"))
                    return "kids";

                if (Regex.IsMatch(title.ToUpper(), @"( |\()BP($| |\))"))
                    return "kids";

                if (Regex.IsMatch(title.ToUpper(), @"( |\()PC($| |\))"))
                    return "kids";

                if (Regex.IsMatch(title.ToUpper(), @"( |\()PCV($| |\))"))
                    return "kids";

            }
            else
            {
                bool test = true;
                return null;
                //wrong brand
            }
            return null;
        }

        public static void Validate(DbRoot db)
        {
            _logger.Info("Validate Db");

            //images
            var sneakersWithoutImages = db.Sneakers.FindAll(x => x.ImageCollectionList.Count == 0);
            foreach (var sneaker in sneakersWithoutImages)
            {
                _logger.Warn("sneaker without images. sku: " + sneaker.Sku);
            }
            db.Sneakers.RemoveAll(x => x.ImageCollectionList.Count == 0);

            _logger.Info("db validate is finished");
        }
    }
}
