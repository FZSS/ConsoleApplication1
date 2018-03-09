using Newtonsoft.Json;
using SneakerIcon.Classes.UPCDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Classes.Exporters;
using SneakerIcon.Classes.Utils;
using SneakerIcon.Controller;
using SneakerIcon.Controller.Exporter.VK;
using Telegram.Bot;

namespace SneakerIcon.Classes
{
    public class Test
    {
        public Test()
        {
            ////ftp ftpClient = new ftp(@"ftp://s07.webhost1.ru", "amaro_111", "38273827");

            //ftp ftpClient = new ftp(@"ftp://ftp.sneaker-icon.com", "amaro@sneaker-icon.com", "38273827");

            ////ftpClient.download("1.jpg", @"C:\SneakerIcon\1.jpg");
            //ftpClient.download("index.php", @"C:\SneakerIcon\index.php");

            string sku = "123456-001";
            string title = "New Nike Air Super Puper Max";
            string link = "https://sneaker-icon.ru/superpurepnike";
            double price = 199.99;
            string message = "Новый артикул: " + sku;
            string separator = "\n";
            message += separator + title;
            message += separator + link;
            message += separator + "Цена: " + price;

            Telegram(message);
        }

        public static void Crawlera()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var myProxy = new WebProxy("http://proxy.crawlera.com:8010");
            myProxy.Credentials = new NetworkCredential("36f14b90c38c4005a81ccbed16a31f58", "");

            //string url = "https://twitter.com/";
            string url = "https://api.upcitemdb.com/prod/trial/search?s=nike%20859524-005&match_mode=1&type=product";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            var encodedApiKey = Base64Encode("36f14b90c38c4005a81ccbed16a31f58:");
            request.Headers.Add("Proxy-Authorization", "Basic " + encodedApiKey);
            //request.Proxy = proxy;
            //request.PreAuthenticate = true;

            request.Proxy = myProxy;
            request.PreAuthenticate = true;

            request.Method = "GET";
            request.Accept = "application/json";

            WebResponse response = request.GetResponse();
            Console.WriteLine("Response Status: "
                + ((HttpWebResponse)response).StatusDescription);
            Console.WriteLine("\nResponse Headers:\n"
                + ((HttpWebResponse)response).Headers);

            Stream dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Console.WriteLine("Response Body:\n" + responseFromServer);
            reader.Close();

            response.Close();

            JsonRootObject jsonObj = JsonConvert.DeserializeObject<JsonRootObject>(responseFromServer);

        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static void Crawlera2()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;


            string url = "https://api.upcitemdb.com/prod/trial/search?s=nike%20859524-005&match_mode=1&type=product";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            IWebProxy proxy = request.Proxy;
            if (proxy != null)
            {
                Console.WriteLine("Proxy: {0}", proxy.GetProxy(request.RequestUri));
            }
            else
            {
                Console.WriteLine("Proxy is null; no proxy will be used");
            }

            WebProxy myProxy = new WebProxy();
            Uri newUri = new Uri("http://proxy.crawlera.com:8010");
            // Associate the newUri object to 'myProxy' object so that new myProxy settings can be set.
            myProxy.Address = newUri;
            // Create a NetworkCredential object and associate it with the 
            // Proxy property of request object.
            myProxy.Credentials = new NetworkCredential("36f14b90c38c4005a81ccbed16a31f58", "");
            request.Proxy = myProxy;

            request.Method = "GET";
            request.Accept = "application/json";

            WebResponse response = request.GetResponse();
            Console.WriteLine("Response Status: "
                + ((HttpWebResponse)response).StatusDescription);
            Console.WriteLine("\nResponse Headers:\n"
                + ((HttpWebResponse)response).Headers);

            Stream dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Console.WriteLine("Response Body:\n" + responseFromServer);
            reader.Close();

            response.Close();
        }

        public static void Upcdb()
        {
            HttpWebRequest request =
            (HttpWebRequest)WebRequest.Create(
            "https://api.upcitemdb.com/prod/trial/search?s=nike%20859524-005&match_mode=1&type=product");

            request.Method = "GET";
            request.Accept = "application/json";

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                StringBuilder output = new StringBuilder();
                output.Append(reader.ReadToEnd());

                response.Close();
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Status);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
            }
        }

        public static async void Telegram(string message)
        {
            var bot = new TelegramBotClient("352425649:AAGvZWkqYAHz7i5-s2f3I_qGnygTqydkEpU");
            //var updates = await bot.GetUpdatesAsync();
            //var t = await bot.SendTextMessageAsync("@sneake_empire_news", message);
            //chat id -1001101919442
            var t = await bot.SendTextMessageAsync("-1001101919442", message);
            //var t = await bot.SendTextMessageAsync("@SneakerEmpireNews", "test message");
        }

        public static void SortAllStock()
        {
            var allstock = AllStockExporter2.LoadLocalFile();
            //allstock.
            //var sortSneakers = allstock.sneakers.OrderBy(x => x.
        }

        public static void PostVk()
        {
            var vk = new VkPosting();
            var message = "test";
            var images = new List<string>() {"http://img.sneaker-icon.ru/shops/1/images/880845-003-1.jpg","http://img.sneaker-icon.ru/shops/1/images/880845-003-2.jpg","http://img.sneaker-icon.ru/shops/1/images/880845-003-3.jpg","http://img.sneaker-icon.ru/shops/1/images/880845-003-4.jpg"};
            vk.PostIntoVkWall(message,images);
        }

        public static bool TestRegex(string str)
        {
            var sPattern = @"(A|\d){6}\-\d{3}";
            if (System.Text.RegularExpressions.Regex.IsMatch(str, sPattern))
            {
                return true; //всё ок работает, проверил, можно переделать на этот паттерн
            }
            return false;
        }

        public static void LoadVkGoods()
        {
            var margin = 25;
            var priceUsd = 100 + margin;


            var rubPrice = CurrencyRate.ConvertUsdToRub(priceUsd);

        }
    }
}
