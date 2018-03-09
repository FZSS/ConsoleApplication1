using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.SizeConverters.Model
{
    public class SizeChart
    {
        public List<Size> sizes { get; set; }

        public SizeChart()
        {
            sizes = new List<Size>();
        }

        public void Load()
        {
            this.sizes = SizeChart.ReadSizeChartJson().sizes;
        }

        public Size GetSize(Size size) //надо проверить работу метода
        {
            var log = String.Empty;
            var resultSize = GetSize(size, out log);
            if (resultSize == null)
                Program.Logger.Warn(log);
            return resultSize;

        }

        /// <summary>
        /// Получает размер из таблицы размеров по бренду, категорию и хотя бы одному размеру
        /// </summary>
        /// <param name="size"></param>
        /// <returns>Возвращает размер из таблицы размеров со всеми заполнеными размерами us,eu,uk,cm,ru</returns>
        public Size GetSize(Size size, out string log)
        {
            //check brand 
            log = String.Empty;
            if (!Validator.ValidateBrand(size.brand))
            {
                
                log = "SizeChart.GetSize() Failed. Brand is NULL. brand:" + size.brand;
                return null;
            }
            var brand = size.brand;
            if (brand.ToLower() == "jordan") brand = "nike";

            //check category
            if (!Validator.ValidateCategory(size.category)) 
            {
                log = "SizeChart.GetSize() Failed. Invalid Category. category:" + size.category;
                return null;
            }

            //get sizes for this brand and category
            var brandCategorySizes = sizes.FindAll(x => x.brand.ToLower() == brand.ToLower() && x.category == size.category);
            
            bool test2 = true;
            if (brandCategorySizes == null)
            {
                log = "SizeChart.GetSize() Failed. Sizes for this brand and category is not Exist. " +
                    " brand:" + size.brand + " category:" + size.category;
                return null;
            }
            //подготовка завершена, бренд и категория норм. получили для данных бренда и категории их размеры.

            //первым пытаемся найти размер по юс
            if (!string.IsNullOrWhiteSpace(size.us))
            {
                var resultSize = brandCategorySizes.Find(x => x.us == size.us);
                if (resultSize == null)
                {
                    log = "SizeChart.GetSize() Failed. размера US для данного бренда и категории не существует. brand:" + size.brand + " category:" + size.category + " sizeUS:" + size.us;
                    return null;
                }
                else
                {
                    //проверяем, чтобы остальные размеры юк, евро и см совпадали, иначе нул
                    if (!string.IsNullOrWhiteSpace(size.uk))
                    {
                        if (resultSize.uk != size.uk)
                        {
                            log = "SizeChart.GetSize() Failed. размер UK не совпадает для размера US. brand:" + size.brand + " category:" + size.category + " sizeUS:" + size.us + " sizeUK:" + size.uk;
                            return null;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(size.eu))
                    {
                        if (resultSize.eu != size.eu)
                        {
                            log = "SizeChart.GetSize() Failed. размер EU не совпадает для размера US. brand:" + size.brand + " category:" + size.category + " sizeUS:" + size.us + " sizeEU:" + size.eu;
                            return null;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(size.cm))
                    {
                        if (resultSize.cm != size.cm)
                        {
                            log = "SizeChart.GetSize() Failed. размер CM не совпадает для размера US. brand:" + size.brand + " category:" + size.category + " sizeUS:" + size.us + " sizeEU:" + size.cm;
                            return null;
                        }
                    }
                    return resultSize;
                }
            }
            else if (!string.IsNullOrWhiteSpace(size.us)) //если us пуст, ищем по eu
            {
                var resultSize = brandCategorySizes.Find(x => x.eu == size.eu);
                if (resultSize == null)
                {
                    log = "SizeChart.GetSize() Failed. размера EU для данного бренда и категории не существует. brand:" + size.brand + " category:" + size.category + " sizeEU:" + size.eu;
                    return null;
                }
                else
                {
                    //проверяем, чтобы остальные размеры совпадали, иначе нул
                    if (!string.IsNullOrWhiteSpace(size.uk))
                    {
                        if (resultSize.uk != size.uk)
                        {
                            log = "SizeChart.GetSize() Failed. размер UK не совпадает для размера EU. brand:" + size.brand + " category:" + size.category + " sizeEU:" + size.eu + " sizeUK:" + size.uk;
                            return null;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(size.cm))
                    {
                        if (resultSize.cm != size.cm)
                        {
                            log = "SizeChart.GetSize() Failed. размер CM не совпадает для размера EU. brand:" + size.brand + " category:" + size.category + " sizeEU:" + size.eu + " sizeCM:" + size.cm;
                            return null;
                        }
                    }
                    return resultSize;
                }
            }
            else if (!string.IsNullOrWhiteSpace(size.uk)) //если us и eu пусты, ищем по uk
            {
                var resultSize = brandCategorySizes.Find(x => x.uk == size.uk);
                if (resultSize == null)
                {
                    log = "SizeChart.GetSize() Failed. размера UK для данного бренда и категории не существует. brand:" + size.brand + " category:" + size.category + " sizeUK:" + size.uk;
                    return null;
                }
                else
                {
                    //проверяем, чтобы остальные размеры совпадали, иначе нул
                    if (!string.IsNullOrWhiteSpace(size.cm))
                    {
                        if (resultSize.cm != size.cm)
                        {
                            log = "SizeChart.GetSize() Failed. размер CM не совпадает для размера EU. brand:" + size.brand + " category:" + size.category + " sizeEU:" + size.eu + " sizeCM:" + size.cm;
                            return null;
                        }
                    }
                    return resultSize;
                }
            }
            else if (!string.IsNullOrWhiteSpace(size.cm)) //если us, eu, uk пусты, ищем по cm
            {
                var resultSize = brandCategorySizes.Find(x => x.cm == size.cm);
                if (resultSize == null)
                {
                    log = "SizeChart.GetSize() Failed. размера CM для данного бренда и категории не существует. brand:" + size.brand + " category:" + size.category + " sizeCM:" + size.cm;
                    return null;
                }
                else
                {
                    return resultSize;
                }
            }
            else
            {
                log = "SizeChart.GetSize() Failed. Все размеры (US,EU,UK,CM) пусты";
                return null;
            }

        }

        public static Size GetSizeStatic(Size size) {
            var sizeChart = SizeChart.ReadSizeChartJson();
            return sizeChart.GetSize(size);
        }

        public static SizeChart ReadSizeChartJson()
        {
            var json = System.IO.File.ReadAllText("SizeChart.json");
            return JsonConvert.DeserializeObject<SizeChart>(json);
        }

        public static void GenerateSizeChart()
        {
            var chart = new SizeChart();

            //NIKE

            chart.sizes.Add(SizeConverter.GetSize("0C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("1C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("2C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("2.5C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("3C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("3.5C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("4C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("4.5C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("5C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("5.5C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("6C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("6.5C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("7C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("7.5C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("8C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("8.5C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("9C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("9.5C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("10C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("10.5C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("11C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("11.5C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("12C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("12.5C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("13C", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("13.5C", "kids"));

            //gradeschool
            chart.sizes.Add(SizeConverter.GetSize("1Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("1.5Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("2Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("2.5Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("3Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("3.5Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("4Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("4.5Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("5Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("5.5Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("6Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("6.5Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("7Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("7.5Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("8Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("8.5Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("9Y", "kids"));
            chart.sizes.Add(SizeConverter.GetSize("9.5Y", "kids"));

            //men
            chart.sizes.Add(SizeConverter.GetSize("4", "men"));
            chart.sizes.Add(SizeConverter.GetSize("4.5", "men"));
            chart.sizes.Add(SizeConverter.GetSize("5", "men"));
            chart.sizes.Add(SizeConverter.GetSize("5.5", "men"));
            chart.sizes.Add(SizeConverter.GetSize("6", "men"));
            chart.sizes.Add(SizeConverter.GetSize("6.5", "men"));
            chart.sizes.Add(SizeConverter.GetSize("7", "men"));
            chart.sizes.Add(SizeConverter.GetSize("7.5", "men"));
            chart.sizes.Add(SizeConverter.GetSize("8", "men"));
            chart.sizes.Add(SizeConverter.GetSize("8.5", "men"));
            chart.sizes.Add(SizeConverter.GetSize("9", "men"));
            chart.sizes.Add(SizeConverter.GetSize("9.5", "men"));
            chart.sizes.Add(SizeConverter.GetSize("10", "men"));
            chart.sizes.Add(SizeConverter.GetSize("10.5", "men"));
            chart.sizes.Add(SizeConverter.GetSize("11", "men"));
            chart.sizes.Add(SizeConverter.GetSize("11.5", "men"));
            chart.sizes.Add(SizeConverter.GetSize("12", "men"));
            chart.sizes.Add(SizeConverter.GetSize("12.5", "men"));
            chart.sizes.Add(SizeConverter.GetSize("13", "men"));
            chart.sizes.Add(SizeConverter.GetSize("13.5", "men"));
            chart.sizes.Add(SizeConverter.GetSize("14", "men"));
            chart.sizes.Add(SizeConverter.GetSize("15", "men"));
            chart.sizes.Add(SizeConverter.GetSize("16", "men"));
            chart.sizes.Add(SizeConverter.GetSize("17", "men"));
            chart.sizes.Add(SizeConverter.GetSize("18", "men"));

            //women
            chart.sizes.Add(SizeConverter.GetSize("5", "women"));
            chart.sizes.Add(SizeConverter.GetSize("5.5", "women"));
            chart.sizes.Add(SizeConverter.GetSize("6", "women"));
            chart.sizes.Add(SizeConverter.GetSize("6.5", "women"));
            chart.sizes.Add(SizeConverter.GetSize("7", "women"));
            chart.sizes.Add(SizeConverter.GetSize("7.5", "women"));
            chart.sizes.Add(SizeConverter.GetSize("8", "women"));
            chart.sizes.Add(SizeConverter.GetSize("8.5", "women"));
            chart.sizes.Add(SizeConverter.GetSize("9", "women"));
            chart.sizes.Add(SizeConverter.GetSize("9.5", "women"));
            chart.sizes.Add(SizeConverter.GetSize("10", "women"));
            chart.sizes.Add(SizeConverter.GetSize("10.5", "women"));
            chart.sizes.Add(SizeConverter.GetSize("11", "women"));
            chart.sizes.Add(SizeConverter.GetSize("11.5", "women"));
            chart.sizes.Add(SizeConverter.GetSize("12", "women"));

            //ADIDAS MEN
            chart.sizes.Add(new Size("Adidas", "men", "5", "37 1/3", "4.5", "22.9", "36.5"));
            chart.sizes.Add(new Size("Adidas", "men", "5.5", "38", "5", "23.3", "37"));
            chart.sizes.Add(new Size("Adidas", "men", "6", "38 2/3", "5.5", "23.8", "37.5"));
            chart.sizes.Add(new Size("Adidas", "men", "6.5", "39 1/3", "6", "24.2", "38"));
            chart.sizes.Add(new Size("Adidas", "men", "7", "40", "6.5", "24.6", "38.5"));
            chart.sizes.Add(new Size("Adidas", "men", "7.5", "40 2/3", "7", "25", "39"));
            chart.sizes.Add(new Size("Adidas", "men", "8", "41 1/3", "7.5", "25.5", "40"));
            chart.sizes.Add(new Size("Adidas", "men", "8.5", "42", "8", "26", "40.5"));
            chart.sizes.Add(new Size("Adidas", "men", "9", "42 2/3", "8.5", "26.3", "41"));
            chart.sizes.Add(new Size("Adidas", "men", "9.5", "43 1/3", "9", "26.7", "42"));
            chart.sizes.Add(new Size("Adidas", "men", "10", "44", "9.2", "27.1", "42.5"));
            chart.sizes.Add(new Size("Adidas", "men", "10.5", "44 2/3", "10", "27.6", "43"));
            chart.sizes.Add(new Size("Adidas", "men", "11", "45 1/3", "10.5", "28", "44"));
            chart.sizes.Add(new Size("Adidas", "men", "11.5", "46", "11", "28.4", "44.5"));
            chart.sizes.Add(new Size("Adidas", "men", "12", "46 2/3", "11.5", "28.8", "45"));
            chart.sizes.Add(new Size("Adidas", "men", "12.5", "47 1/3", "12", "29.3", "46"));
            chart.sizes.Add(new Size("Adidas", "men", "13", "48", "12.5", "29.7", "46.5"));
            chart.sizes.Add(new Size("Adidas", "men", "13.5", "48 2/3", "13", "30.1", "47"));
            chart.sizes.Add(new Size("Adidas", "men", "14", "49 1/3", "13.5", "30.5", "48"));
            chart.sizes.Add(new Size("Adidas", "men", "14.5", "50", "14", "31", "49"));
            chart.sizes.Add(new Size("Adidas", "men", "15", "50 2/3", "14.5", "31.4", "49.5"));
            chart.sizes.Add(new Size("Adidas", "men", "15.5", "51 1/3", "15", "32.2", "50"));
            chart.sizes.Add(new Size("Adidas", "men", "16", "52", "15.5", "33.1", "51"));
            chart.sizes.Add(new Size("Adidas", "men", "16.5", "52 2/3", "16", "33.9", "52"));
            chart.sizes.Add(new Size("Adidas", "men", "17", "53 1/3", "17", "34.8", "53"));
            chart.sizes.Add(new Size("Adidas", "men", "17.5", "54 2/3", "18", "35.6", "54"));


            var textJson = JsonConvert.SerializeObject(chart);
            System.IO.File.WriteAllText("SizeChart.json", textJson);
        }
    }
}
