using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SneakerIcon.Classes.Utils
{
    public class CurrencyRate
    {
        public const string filename = "CurrencyRate.json";
        public DateTime timeParse {get; set;}
        public Dictionary<string, double> CurrencyDict { get; set; }

        public CurrencyRate()
        {
            CurrencyDict = new Dictionary<string, double>();            
            //Run();
        }

        public void Run()
        {
            CurrencyDict.Add("RUB", 1);

            XmlTextReader reader = new XmlTextReader("http://www.cbr.ru/scripts/XML_daily.asp");

            while (reader.Read())
            {
                //Проверяем тип текущего узла
                switch (reader.NodeType)
                {
                    //Если этого элемент Valute, то начинаем анализировать атрибуты
                    case XmlNodeType.Element:

                        if (reader.Name == "Valute")
                        {
                            XmlDocument xml = new XmlDocument();
                            string outerxml = reader.ReadOuterXml();
                            xml.LoadXml(outerxml);
                            XmlNode xmlNode = xml.SelectSingleNode("Valute/CharCode");
                            string CurName = xmlNode.InnerText.Trim();
                            xmlNode = xml.SelectSingleNode("Valute/Value");
                            double CurValue = Convert.ToDouble(xmlNode.InnerText);
                            CurrencyDict.Add(CurName, CurValue);
                        }                       
                        break;
                }               
            }

            this.timeParse = DateTime.Now;
            CurrencyRate.WriteObjectToJsonFile(this);

            Program.Logger.Debug("Курсы валют: USD=" + CurrencyDict["USD"] + " EUR=" + CurrencyDict["EUR"] + ". Дата:" + DateTime.Now + " ");
        }

        public double GetCurrencyRate(string CharCode)
        {
            return CurrencyDict[CharCode];
        }

        public static void WriteObjectToJsonFile(CurrencyRate currencyRate) {
            var textJson = JsonConvert.SerializeObject(currencyRate);
            System.IO.File.WriteAllText(filename, textJson);
        }

        public static CurrencyRate ReadObjectFromJsonFile()
        {
            if (!File.Exists(filename))
            {
                CurrencyRate currate = new CurrencyRate();
                currate.Run();
            }

            var json = System.IO.File.ReadAllText(filename);
            var thisObject = JsonConvert.DeserializeObject<CurrencyRate>(json);
            var nowDate = DateTime.Now;
            var parseDate = thisObject.timeParse;
            var raznica = nowDate - parseDate;
            var day = new TimeSpan(24, 0, 0);
            if (raznica > day)
            {
                CurrencyRate currate = new CurrencyRate();
                currate.Run();
                //throw new Exception("С момента парсинга курсов валют прошло больше суток");
            }
            return thisObject;
        }

        public static double ConvertCurrency(string inputCurrencyName, string outputCurrencyName, double inputCurrencyValue) {
            CurrencyRate currate = CurrencyRate.ReadObjectFromJsonFile();
            double curInputRate = currate.GetCurrencyRate(inputCurrencyName);
            double curOutputRate = currate.GetCurrencyRate(outputCurrencyName);
            var resultPrice = inputCurrencyValue * curInputRate / curOutputRate;
            return Math.Round(resultPrice,2);
        }

        public static int ConvertUsdToRub(double usdValue)
        {
            return (int) ConvertCurrency("USD", "RUB", usdValue);
        }
    }
}
