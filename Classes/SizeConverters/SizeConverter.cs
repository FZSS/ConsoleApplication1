using Newtonsoft.Json;
using SneakerIcon.Classes.SizeConverters.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.SizeConverters
{
    public class SizeConverter
    {
        public String sizeUS { get; set; }
        public String sizeUK { get; set; }
        public String sizeEUR { get; set; }
        public String sizeCM { get; set; }
        public String sizeRUS { get; set; }
        /// <summary>
        /// man woman or kids
        /// </summary>
        public String category { get; set; }
        public String allSize { get; set; }
        public string sku { get; set; }
        public SizeChart sizeChart { get; set; }



        public SizeConverter()
        {
            sizeChart = SizeConverter.ReadSizeChartJson();
        }

        public SizeConverter(string sizeUS, string category, string sku)
        {
            this.sku = sku;
            Run(sizeUS, category);

        }

        public void Run(string sizeUS, string category)
        {
            this.category = category;
            if (category == "Мужская") this.category = "men";
            if (category == "Женская") this.category = "women";
            if (category == "Детская") this.category = "kids";

            this.sizeUS = sizeUS.Trim();
            ConvertSize();
            if (!String.IsNullOrEmpty(sizeEUR) && !String.IsNullOrEmpty(sizeUK))
            {
                allSize = sizeEUR + " EUR / " + sizeUS + " US / " + sizeUK + " UK / " + sizeCM + " CM";
            }
            else
            {
                //throw new Exception("Size is empty");
                //allSize = sizeUS + " US";
            }
        }

        public SizeConverter(string sizeUS, string category)
        {
            Run(sizeUS, category);
        }

        public bool ConvertSize() {
            if (category == "men")
            {
                switch (sizeUS)
                {
                    case "4":
                        sizeUK = "3.5";
                        sizeEUR = "36";
                        sizeCM = "23";
                        sizeRUS = "36";
                        break;
                    case "4.5":
                        sizeUK = "4";
                        sizeEUR = "36.5";
                        sizeCM = "23.5";
                        sizeRUS = "36.5";
                        break;
                    case "5":
                        sizeUK = "4.5";
                        sizeEUR = "37.5";
                        sizeCM = "23.5";
                        sizeRUS = "36.5";
                        break;
                    case "5.5":
                        sizeUK = "5";
                        sizeEUR = "38";
                        sizeCM = "24";
                        sizeRUS = "37";
                        break;
                    case "6":
                        sizeUK = "5.5";
                        sizeEUR = "38.5";
                        sizeCM = "24";
                        sizeRUS = "37.5";              
                        break;
                    case "6.5":
                        sizeEUR = "39";
                        sizeUK = "6";
                        sizeCM = "24.5";
                        sizeRUS = "38";
                        break;
                    case "7":
                        sizeEUR = "40";
                        sizeUK = "6";
                        sizeCM = "25";
                        sizeRUS = "39";
                        break;
                    case "7.5":
                        sizeEUR = "40.5";
                        sizeUK = "6.5";
                        sizeCM = "25.5";
                        sizeRUS = "40";
                        break;
                    case "8":
                        sizeEUR = "41";
                        sizeUK = "7";
                        sizeCM = "26";
                        sizeRUS = "40.5";
                        break;
                    case "8.5":
                        sizeEUR = "42";
                        sizeUK = "7.5";
                        sizeCM = "26.5";
                        sizeRUS = "41";
                        break;
                    case "9":
                        sizeEUR = "42.5";
                        sizeUK = "8";
                        sizeCM = "27";
                        sizeRUS = "42";
                        break;
                    case "9.5":
                        sizeEUR = "43";
                        sizeUK = "8.5";
                        sizeCM = "27.5";
                        sizeRUS = "43";
                        break;
                    case "10":
                        sizeEUR = "44";
                        sizeUK = "9";
                        sizeCM = "28";
                        sizeRUS = "43.5";
                        break;
                    case "10.5":
                        sizeEUR = "44.5";
                        sizeUK = "9.5";
                        sizeCM = "28.5";
                        sizeRUS = "44";
                        break;
                    case "11":
                        sizeEUR = "45";
                        sizeUK = "10";
                        sizeCM = "29";
                        sizeRUS = "45";
                        break;
                    case "11.5":
                        sizeEUR = "45.5";
                        sizeUK = "10.5";
                        sizeCM = "29.5";
                        sizeRUS = "46";
                        break;
                    case "12":
                        sizeEUR = "46";
                        sizeUK = "11";
                        sizeCM = "30";
                        sizeRUS = "46.5";
                        break;
                    case "12.5":
                        sizeEUR = "47";
                        sizeUK = "11.5";
                        sizeCM = "30.5";
                        sizeRUS = "47";
                        break;
                    case "13":
                        sizeEUR = "47.5";
                        sizeUK = "12";
                        sizeCM = "31";
                        sizeRUS = "47.5";
                        break;
                    case "13.5":
                        sizeEUR = "48";
                        sizeUK = "12.5";
                        sizeCM = "31.5";
                        sizeRUS = "48";
                        break;
                    case "14":
                        sizeEUR = "48.5";
                        sizeUK = "13";
                        sizeCM = "32";
                        sizeRUS = "48.5";
                        break;
                    case "15":
                        sizeEUR = "49.5";
                        sizeUK = "14";
                        sizeCM = "33";
                        sizeRUS = "49.5";
                        break;
                    case "16":
                        sizeEUR = "50.5";
                        sizeUK = "15";
                        sizeCM = "34";
                        sizeRUS = "50.5";
                        break;
                    case "17":
                        sizeEUR = "51.5";
                        sizeUK = "16";
                        sizeCM = "35";
                        sizeRUS = "52";
                        break;
                    case "18":
                        sizeEUR = "52.5";
                        sizeUK = "17";
                        sizeCM = "36";
                        sizeRUS = "52.5";
                        break;
                    case "XXXS":
                        sizeUS = "XXXS(6-7US)";
                        sizeEUR = "39";
                        sizeUK = "5.5";
                        sizeCM = "24";
                        sizeRUS = "38.5";
                        break;
                    case "XXS":
                        sizeUS = "XXS(7-8US)";
                        sizeEUR = "40.5";
                        sizeUK = "6.5";
                        sizeCM = "25.5";
                        sizeRUS = "40";
                        break;
                    case "XS":
                        sizeUS = "XS(8-9US)";
                        sizeEUR = "42";
                        sizeUK = "7.5";
                        sizeCM = "26.5";
                        sizeRUS = "41";
                        break;
                    case "S":
                        sizeUS = "S(9-10US)";
                        sizeEUR = "43";
                        sizeUK = "8.5";
                        sizeCM = "27.5";
                        sizeRUS = "43";
                        break;
                    case "M":
                        sizeUS = "M(10-11US)";
                        sizeEUR = "44.5";
                        sizeUK = "9.5";
                        sizeCM = "28.5";
                        sizeRUS = "44";
                        break;
                    case "L":
                        sizeUS = "L(11-12US)";
                        sizeEUR = "45.5";
                        sizeUK = "10.5";
                        sizeCM = "29.5";
                        sizeRUS = "46";
                        break;
                    case "XL":
                        sizeUS = "XL(12-13US)";
                        sizeEUR = "47";
                        sizeUK = "11.5";
                        sizeCM = "30.5";
                        sizeRUS = "47";
                        break;
                    case "XXL":
                        sizeUS = "XXL(13-14US)";
                        sizeEUR = "48";
                        sizeUK = "12.5";
                        sizeCM = "31.5";
                        sizeRUS = "48";
                        break;
                    case "2XL":
                        sizeUS = "XXL(13-14US)";
                        sizeEUR = "48";
                        sizeUK = "12.5";
                        sizeCM = "31.5";
                        sizeRUS = "48";
                        break;
                    case "3XL":
                        sizeUS = "XXXL(14-15US)";
                        sizeEUR = "49";
                        sizeUK = "13.5";
                        sizeCM = "32.5";
                        sizeRUS = "49";
                        break;
                    case "XXXL":
                        sizeUS = "XXXL(14-15US)";
                        sizeEUR = "49";
                        sizeUK = "13.5";
                        sizeCM = "32.5";
                        sizeRUS = "49";
                        break;
                    default:
                        throw new Exception("wrong size: " + sizeUS);
                        break;
                }
            } //men

            if (category == "women")
            {
                switch (sizeUS)
                {
                    case "5":
                        sizeEUR = "35.5";
                        sizeUK = "2.5";
                        sizeCM = "22";
                        sizeRUS = "34.5";
                        break;
                    case "5.5":
                        sizeEUR = "36";
                        sizeUK = "3";
                        sizeCM = "22.5";
                        sizeRUS = "35";
                        break;
                    case "6":
                        sizeEUR = "36.5";
                        sizeUK = "3.5";
                        sizeCM = "23";
                        sizeRUS = "36";
                        break;
                    case "6.5":
                        sizeEUR = "37.5";
                        sizeUK = "4";
                        sizeCM = "23.5";
                        sizeRUS = "37";
                        break;
                    case "7":
                        sizeEUR = "38";
                        sizeUK = "4.5";
                        sizeCM = "24";
                        sizeRUS = "37.5";
                        break;
                    case "7.5":
                        sizeEUR = "38.5";
                        sizeUK = "5";
                        sizeCM = "24.5";
                        sizeRUS = "38";
                        break;
                    case "8":
                        sizeEUR = "39";
                        sizeUK = "5.5";
                        sizeCM = "25";
                        sizeRUS = "39";
                        break;
                    case "8.5":
                        sizeEUR = "40";
                        sizeUK = "6";
                        sizeCM = "25.5";
                        sizeRUS = "40";
                        break;
                    case "9":
                        sizeEUR = "40.5";
                        sizeUK = "6.5";
                        sizeCM = "26";
                        sizeRUS = "40.5";
                        break;
                    case "9.5":
                        sizeEUR = "41";
                        sizeUK = "7";
                        sizeCM = "26.5";
                        sizeRUS = "41";
                        break;
                    case "10":
                        sizeEUR = "42";
                        sizeUK = "7.5";
                        sizeCM = "27";
                        sizeRUS = "42";
                        break;
                    case "10.5":
                        sizeEUR = "42.5";
                        sizeUK = "8";
                        sizeCM = "27.5";
                        sizeRUS = "43";
                        break;
                    case "11":
                        sizeEUR = "43";
                        sizeUK = "8.5";
                        sizeCM = "28";
                        sizeRUS = "44";
                        break;
                    case "11.5":
                        sizeEUR = "44";
                        sizeUK = "9";
                        sizeCM = "28.5";
                        sizeRUS = "44.5";
                        break;
                    case "12":
                        sizeEUR = "44.5";
                        sizeUK = "9.5";
                        sizeCM = "29";
                        sizeRUS = "44.5";
                        break;
                    default:
                        throw new Exception("wrong size: " + sizeUS);
                        //Program.logger.Warn("wrong sizeUS: " + sizeUS + " categorySneakerFullCatalog:" + categorySneakerFullCatalog + "sku: " + sku);
                        //return false;
                        break;
                }
            } //woman

            if (category == "kids")
            {
                switch (sizeUS)
                {
                    case "0C":
                        sizeEUR = "15";
                        sizeUK = "0";
                        sizeCM = "6";
                        sizeRUS = "-";
                        break;
                    case "1C":
                        sizeEUR = "16";
                        sizeUK = "0.5";
                        sizeCM = "7";
                        sizeRUS = "-";
                        break;
                    case "2C":
                        sizeEUR = "17";
                        sizeUK = "1.5";
                        sizeCM = "8";
                        sizeRUS = "-";
                        break;
                    case "2.5C":
                        sizeEUR = "18";
                        sizeUK = "2";
                        sizeCM = "8.5";
                        sizeRUS = "-";
                        break;
                    case "3C":
                        sizeEUR = "18.5";
                        sizeUK = "2.5";
                        sizeCM = "9";
                        sizeRUS = "-";
                        break;
                    case "3.5C":
                        sizeEUR = "19";
                        sizeUK = "3";
                        sizeCM = "9.5";
                        sizeRUS = "16";
                        break;
                    case "4C":
                        sizeEUR = "19.5";
                        sizeUK = "3.5";
                        sizeCM = "10";
                        sizeRUS = "16.5";
                        break;
                    case "4.5C":
                        sizeEUR = "20";
                        sizeUK = "4";
                        sizeCM = "10.5";
                        sizeRUS = "17";
                        break;
                    case "5C":
                        sizeEUR = "21";
                        sizeUK = "4.5";
                        sizeCM = "11";
                        sizeRUS = "18";
                        break;
                    case "5.5C":
                        sizeEUR = "21.5";
                        sizeUK = "5";
                        sizeCM = "11.5";
                        sizeRUS = "19";
                        break;
                    case "6C":
                        sizeEUR = "22";
                        sizeUK = "5.5";
                        sizeCM = "12";
                        sizeRUS = "19.5";
                        break;
                    case "6.5C":
                        sizeEUR = "22.5";
                        sizeUK = "6";
                        sizeCM = "12.5";
                        sizeRUS = "20";
                        break;
                    case "7C":
                        sizeEUR = "23.5";
                        sizeUK = "6.5";
                        sizeCM = "13";
                        sizeRUS = "21";
                        break;
                    case "7.5C":
                        sizeEUR = "24";
                        sizeUK = "7";
                        sizeCM = "13.5";
                        sizeRUS = "22";
                        break;
                    case "8C":
                        sizeEUR = "25";
                        sizeUK = "7.5";
                        sizeCM = "14";
                        sizeRUS = "22.5";
                        break;
                    case "8.5C":
                        sizeEUR = "25.5";
                        sizeUK = "8";
                        sizeCM = "14.5";
                        sizeRUS = "23";
                        break;
                    case "9C":
                        sizeEUR = "26";
                        sizeUK = "8.5";
                        sizeCM = "15";
                        sizeRUS = "24";
                        break;
                    case "9.5C":
                        sizeEUR = "26.5";
                        sizeUK = "9";
                        sizeCM = "15.5";
                        sizeRUS = "25";
                        break;
                    case "10C":
                        sizeEUR = "27";
                        sizeUK = "9.5";
                        sizeCM = "16";
                        sizeRUS = "25.5";
                        break;
                    case "10.5C":
                        sizeEUR = "27.5";
                        sizeUK = "10";
                        sizeCM = "16.5";
                        sizeRUS = "26";
                        break;
                    case "11C":
                        sizeEUR = "28";
                        sizeUK = "10.5";
                        sizeCM = "17";
                        sizeRUS = "27";
                        break;
                    case "11.5C":
                        sizeEUR = "28.5";
                        sizeUK = "11";
                        sizeCM = "17.5";
                        sizeRUS = "28";
                        break;
                    case "12C":
                        sizeEUR = "29.5";
                        sizeUK = "11.5";
                        sizeCM = "18";
                        sizeRUS = "28.5";
                        break;
                    case "12.5C":
                        sizeEUR = "30";
                        sizeUK = "12";
                        sizeCM = "18.5";
                        sizeRUS = "29";
                        break;
                    case "13C":
                        sizeEUR = "31";
                        sizeUK = "12.5";
                        sizeCM = "19";
                        sizeRUS = "30";
                        break;
                    case "13.5C":
                        sizeEUR = "31.5";
                        sizeUK = "13";
                        sizeCM = "19.5";
                        sizeRUS = "31";
                        break;
                    case "1Y":
                        sizeEUR = "32";
                        sizeUK = "13.5"; //это еще детский размер в размерной сетке UK
                        sizeCM = "20";
                        sizeRUS = "31.5";
                        break;
                    case "1.5Y":
                        sizeEUR = "33";
                        sizeUK = "1";
                        sizeCM = "20.5";
                        sizeRUS = "32";
                        break;
                    case "2Y":
                        sizeEUR = "33.5";
                        sizeUK = "1.5";
                        sizeCM = "21";
                        sizeRUS = "33";
                        break;
                    case "2.5Y":
                        sizeEUR = "34";
                        sizeUK = "2";
                        sizeCM = "21.5";
                        sizeRUS = "34";
                        break;
                    case "3Y":
                        sizeEUR = "35";
                        sizeUK = "2.5";
                        sizeCM = "22";
                        sizeRUS = "34.5";
                        break;
                    case "3.5Y":
                        sizeEUR = "35.5";
                        sizeUK = "3";
                        sizeCM = "22.5";
                        sizeRUS = "35";
                        break;
                    case "4Y":
                        sizeEUR = "36";
                        sizeUK = "3.5";
                        sizeCM = "23";
                        sizeRUS = "36";
                        break;
                    case "4.5Y":
                        sizeEUR = "36.5";
                        sizeUK = "4";
                        sizeCM = "23.5";
                        sizeRUS = "37";
                        break;
                    case "5Y":
                        sizeEUR = "37.5";
                        sizeUK = "4.5";
                        sizeCM = "23.5";
                        sizeRUS = "37";
                        break;
                    case "5.5Y":
                        sizeEUR = "38";
                        sizeUK = "5";
                        sizeCM = "24";
                        sizeRUS = "37.5";
                        break;
                    case "6Y":
                        sizeEUR = "38.5";
                        sizeUK = "5.5";
                        sizeCM = "24";
                        sizeRUS = "37.5";
                        break;
                    case "6.5Y":
                        sizeEUR = "39";
                        sizeUK = "6";
                        sizeCM = "24.5";
                        sizeRUS = "38";
                        break;
                    case "7Y":
                        sizeEUR = "40";
                        sizeUK = "6.5";
                        sizeCM = "25";
                        sizeRUS = "39";
                        break;
                    case "7.5Y":
                        sizeEUR = "40.5";
                        sizeUK = "6.5";
                        sizeCM = "25.5";
                        sizeRUS = "39.5";
                        break;
                    case "8Y":
                        sizeEUR = "41";
                        sizeUK = "7";
                        sizeCM = "26";
                        sizeRUS = "40";
                        break;
                    case "8.5Y":
                        sizeEUR = "42";
                        sizeUK = "7.5";
                        sizeCM = "26.5";
                        sizeRUS = "41";
                        break;
                    case "9Y":
                        sizeEUR = "42.5";
                        sizeUK = "8";
                        sizeCM = "27";
                        sizeRUS = "41.5";
                        break;
                    case "9.5Y":
                        sizeEUR = "43";
                        sizeUK = "8.5";
                        sizeCM = "27.5";
                        sizeRUS = "42";
                        break;
                    default:
                        throw new Exception("wrong size: " + sizeUS);
                        break;
                }
            }
            return true;
        }

        public static string GetCategory(string us, string eu, string uk, string cm, string brand = "Nike")
        {
            if (brand.ToLower() == "jordan") brand = "Nike";
            if (Validator.ValidateBrand(brand))
            {
                //string categorySneakerFullCatalog;
                var sizeChart = SizeConverter.ReadSizeChartJson();

                var brandSizes = sizeChart.sizes.FindAll(x => x.brand.ToLower() == brand.ToLower());
                if (brandSizes != null)
                {
                    if (!String.IsNullOrWhiteSpace(us))
                    {
                        var goodSizes = brandSizes.FindAll(x => x.us == us);
                        if (!String.IsNullOrWhiteSpace(eu))
                        {
                            var resultSize = goodSizes.Find(x => x.eu == eu);
                            if (resultSize != null)
                            {
                                var resultCategory = Helper.ConvertEngToRusCategory(resultSize.category);
                                return resultCategory;
                            }
                        }
                        else if (!String.IsNullOrWhiteSpace(uk))
                        {
                            var resultSize = goodSizes.Find(x => x.uk == uk);
                            if (resultSize != null)
                            {
                                var resultCategory = Helper.ConvertEngToRusCategory(resultSize.category);
                                return resultCategory;
                            }
                        }
                        else if (!String.IsNullOrWhiteSpace(cm))
                        {
                            var resultSize = goodSizes.Find(x => x.cm == cm);
                            if (resultSize != null)
                            {
                                var resultCategory = Helper.ConvertEngToRusCategory(resultSize.category);
                                return resultCategory;
                            }
                        }
                    }
                    else if (!String.IsNullOrWhiteSpace(eu))
                    {
                        var goodSizes = brandSizes.FindAll(x => x.eu == eu);
                        if (!String.IsNullOrWhiteSpace(uk))
                        {
                            var resultSize = goodSizes.Find(x => x.uk == uk);
                            if (resultSize != null)
                            {
                                var resultCategory = Helper.ConvertEngToRusCategory(resultSize.category);
                                return resultCategory;
                            }
                        }
                        else if (!String.IsNullOrWhiteSpace(cm))
                        {
                            var resultSize = goodSizes.Find(x => x.cm == cm);
                            if (resultSize != null)
                            {
                                var resultCategory = Helper.ConvertEngToRusCategory(resultSize.category);
                                return resultCategory;
                            }
                        }
                    }
                    else if (!String.IsNullOrWhiteSpace(uk))
                    {
                        var goodSizes = brandSizes.FindAll(x => x.uk == uk);
                        if (!String.IsNullOrWhiteSpace(cm))
                        {
                            var resultSize = goodSizes.Find(x => x.cm == cm);
                            if (resultSize != null)
                            {
                                var resultCategory = Helper.ConvertEngToRusCategory(resultSize.category);
                                return resultCategory;
                            }
                        }
                    }
                }


            }
            return null;
        }



        public static Size GetSize(string sizeUS, string category, string brand = "Nike")
        {
            var converter = new SizeConverter();
            //kids
            //var categorySneakerFullCatalog = "kids";
            //var sizeUS = "0C";
            converter.Run(sizeUS, category);
            var size = new Size(brand, category, sizeUS, converter.sizeEUR, converter.sizeUK, converter.sizeCM, converter.sizeRUS);

            return size;
        }

        public static SizeChart ReadSizeChartJson()
        {
            var json = System.IO.File.ReadAllText("SizeChart.json");
            return JsonConvert.DeserializeObject<SizeChart>(json);
        }

        /// <summary>
        /// Определяет размер в юс по одной из размерных сеток (с учетом бренда и категории)
        /// </summary>
        /// <param name="brand"></param>
        /// <param name="EngCategory"></param>
        /// <param name="sizeEU"></param>
        /// <param name="sizeUK"></param>
        /// <param name="sizeCM"></param>
        /// <returns></returns>
        public static string GetSizeUs(string brand, string EngCategory, string sizeEU, string sizeUK, string sizeCM)
        {
            string sizeUS = String.Empty;
            brand = brand.ToUpper();
            if (brand == "JORDAN") brand = "NIKE";
            var sizeChart = ReadSizeChartJson();
            var sizes = sizeChart.sizes.FindAll(x => x.brand.ToUpper() == "NIKE");
            sizes = sizes.FindAll(x => x.category == EngCategory);
            if (sizes == null)
            {
                throw new Exception("brand not found in sizechart");
            }
            if (!String.IsNullOrWhiteSpace(sizeEU))
            {
                var size = sizes.Find(x => x.eu == sizeEU);
                if (size == null)
                {
                    Program.Logger.Warn("wrong size. size not found in sizechart");
                    return null;
                    //throw new Exception("size not found in sizechart");
                }
                else
                {
                    return size.us;
                }
            }
            if (!String.IsNullOrWhiteSpace(sizeUK))
            {
                var size = sizes.Find(x => x.uk == sizeUK);
                if (size == null)
                {
                    Program.Logger.Warn("wrong size. size not found in sizechart");
                    return null;
                    //throw new Exception("size not found in sizechart");
                }
                else
                {
                    return size.us;
                }
            }
            if (!String.IsNullOrWhiteSpace(sizeCM))
            {
                var size = sizes.Find(x => x.cm == sizeCM);
                if (size == null)
                {
                    throw new Exception("size not found in sizechart");
                }
                else
                {
                    return size.us;
                }
            }
            throw new Exception("all sizes is empty. size recognize is failed");
            return null;
        }
    }
}
