using AngleSharp.Parser.Html;
using CsvHelper;
using Newtonsoft.Json;
using SneakerIcon.Classes.Catalogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

//еще один сайт https://www.barcodelookup.com/
//еще сайт https://www.upcindex.com/ без апи правда

namespace SneakerIcon.Classes.UPCDB
{
    public class UPCDB
    {
        private FullCatalog fullCatalog { get; set; }
        private MyUPCDBRootJSONObject myUPCDB { get; set; }
        public const string myJsonFileName = "upcdb.json";
        public const string UpcDbCsvFileName = "upcdb.csv";
        private int _limitRequst { get; set; }
        private DateTime _resetTimeLimits { get; set; }
        private int _doRequest { get; set; }
        private int _timeDelay { get; set; }
        private string _stopListFileName = "stopList.json";
        private StopList MyStopList { get; set; }
        public UPCDB()
        {
            var appSettings = ConfigurationManager.AppSettings;
            var timeDelay = appSettings["upcdbTimeDelaySec"];
            _timeDelay = Int32.Parse(timeDelay);
            _doRequest = 0;
            _limitRequst = 1;
            //fullCatalog = new FullCatalog();
            string fullCatalogFileName = "FullCatalog.csv";
            GetFileFromFtp(fullCatalogFileName, fullCatalogFileName);
            fullCatalog = new FullCatalog(fullCatalogFileName);
            GetFileFromFtp(myJsonFileName, myJsonFileName);
            GetFileFromFtp(_stopListFileName, _stopListFileName);
        }

        public void run()
        {
            readMyStopListJson(_stopListFileName);           
            readMyJson(myJsonFileName);
            
            obhodFullCatalog();
            
            //markets
            saveMyJson(myJsonFileName);
            GoToFTP(myJsonFileName, myJsonFileName);
            
            //csv
            ExportToCSV(UpcDbCsvFileName);
            GoToFTP(UpcDbCsvFileName, UpcDbCsvFileName);
            
            //stopList
            saveMyStopListJson(_stopListFileName);
            GoToFTP(_stopListFileName, _stopListFileName);
        }

        public void run2()
        {
            
            while (true != false)
            {
                GetMyIp();
                run();
                RebootModem();
            }
        }

        public void run3(int count = 100)
        {
            readMyStopListJson(_stopListFileName);
            readMyJson(myJsonFileName);

            obhodFullCatalog2(count);

            //markets
            saveMyJson(myJsonFileName);
            GoToFTP(myJsonFileName, myJsonFileName);

            //csv
            ExportToCSV(UpcDbCsvFileName);
            GoToFTP(UpcDbCsvFileName, UpcDbCsvFileName);

            //stopList
            saveMyStopListJson(_stopListFileName);
            GoToFTP(_stopListFileName, _stopListFileName);
        }

        private void RebootModem()
        {          
            try
            {
                System.IO.File.WriteAllText("MP709.local.set", "RELE1=OFF");
                System.Threading.Thread.Sleep(1000 * 10);
                System.IO.File.WriteAllText("MP709.local.set", "RELE1=ON");
                System.Threading.Thread.Sleep(1000 * 90);
            }
            catch (Exception e) {
                Program.Logger.Error(e.StackTrace);
                System.Threading.Thread.Sleep(1000 * 10);
                RebootModem();
            }
        }

        private void GetMyIp() {
                        
            Uri uri = new Uri("https://2ip.ru/");
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;

            string source = webClient.DownloadString(uri);
            webClient.Dispose();
            var parser = new HtmlParser();
            var document = parser.Parse(source);

            var linkHTML = document.QuerySelector("big").InnerHtml;

            Program.Logger.Info("My IP: " + linkHTML);
        }

        public void readMyJson(string filename)
        {
            if (File.Exists(filename))
            {
                var textJson = System.IO.File.ReadAllText(filename);
                myUPCDB = JsonConvert.DeserializeObject<MyUPCDBRootJSONObject>(textJson);
            }
            else
            {
                //throw new Exception("file not exist");
                myUPCDB = new MyUPCDBRootJSONObject();
                myUPCDB.sneakers = new List<SneakerJson>();
            }
        }

        public void readMyStopListJson(string filename)
        {
            if (File.Exists(filename))
            {
                var textJson = System.IO.File.ReadAllText(filename);
                MyStopList = JsonConvert.DeserializeObject<StopList>(textJson);
            }
            else
            {
                //throw new Exception("file not exist");
                MyStopList = new StopList();
                MyStopList.skuList = new List<String>();
            }
        }

        public void saveMyJson(string filename)
        {
            var textJson = JsonConvert.SerializeObject(myUPCDB);
            System.IO.File.WriteAllText(filename, textJson);
        }

        private void saveMyStopListJson(string filename)
        {
            var textJson = JsonConvert.SerializeObject(MyStopList);
            System.IO.File.WriteAllText(filename, textJson);
        }

        public void obhodFullCatalog()
        {
            int i = 0;
            int j = 0;
            foreach (var sneaker in fullCatalog.sneakers)
            {
                var jsonSneaker = myUPCDB.sneakers.Find(x => x.sku == sneaker.sku);
                var stopListSku = MyStopList.skuList.Find(x => x == sneaker.sku);
                if (jsonSneaker == null && stopListSku == null)
                {

                    bool isLimits = false;
                    JsonRootObject json = GetAllSizes(sneaker.brand, sneaker.sku,out isLimits);
                    //if (_limitRequst == 0) isLimits = true;
                    if (isLimits)
                    {
                        Program.Logger.Info("Добавлено артикулов: " + i);
                        Program.Logger.Info("Добавлено размеров: " + j);
                        Program.Logger.Info("Выполнено запросов: " + _doRequest);
                        Program.Logger.Info("Остаток запросов: " + _limitRequst);
                        Program.Logger.Info("Время перезагрузки лимитов: " + _resetTimeLimits);
                        break;
                    }

                    if (json != null)
                    {
                        jsonSneaker = new SneakerJson();
                        jsonSneaker.sku = sneaker.sku;
                        jsonSneaker.title = sneaker.title;
                        jsonSneaker.brand = sneaker.brand;
                        jsonSneaker.sizes = json.items;
                        jsonSneaker.category = sneaker.category;
                        jsonSneaker.sex = sneaker.sex;

                        i += 1;
                        j += jsonSneaker.sizes.Count;
                        myUPCDB.sneakers.Add(jsonSneaker);                        

                        //todo убрать, сейчас по 1 кроссовку добавляется
                        //break;
                        Program.Logger.Info("Добавили артикул: " + sneaker.sku + ". Размеров: " + jsonSneaker.sizes.Count + ". Осталось запросов: " + _limitRequst);
                    }
                    else
                    {
                        bool test = true;
                    }
                }
            }
            //Program.logger.Info("Обход фуллкаталога завершен!");
        }

        public void obhodFullCatalog2(int count = 100)
        {
            int i = 0;
            int j = 0;
            int k = 0;
            foreach (var sneaker in fullCatalog.sneakers)
            {

                var jsonSneaker = myUPCDB.sneakers.Find(x => x.sku == sneaker.sku);
                var stopListSku = MyStopList.skuList.Find(x => x == sneaker.sku);
                if (jsonSneaker == null && stopListSku == null)
                {
                    k++;
                    if (k > count)
                    {
                        Program.Logger.Info("Добавлено артикулов: " + i);
                        Program.Logger.Info("Добавлено размеров: " + j);
                        Program.Logger.Info("Выполнено запросов: " + _doRequest);
                        Program.Logger.Info("Артикулов в UPCDB.json: " + myUPCDB.sneakers.Count + ". В фулкаталоге: " + fullCatalog.sneakers.Count);
                        //Program.logger.Info("Остаток запросов: " + _limitRequst);
                        //Program.logger.Info("Время перезагрузки лимитов: " + _resetTimeLimits);
                        break;
                    }
                    bool isLimits = false;
                    JsonRootObject json = GetAllSizes2(sneaker.brand, sneaker.sku, out isLimits);
                    //if (_limitRequst == 0) isLimits = true;
                    if (isLimits)
                    {
                        Program.Logger.Info("Добавлено артикулов: " + i);
                        Program.Logger.Info("Добавлено размеров: " + j);
                        Program.Logger.Info("Выполнено запросов: " + _doRequest);
                        //Program.logger.Info("Остаток запросов: " + _limitRequst);
                        //Program.logger.Info("Время перезагрузки лимитов: " + _resetTimeLimits);
                        break;
                    }

                    if (json != null)
                    {
                        jsonSneaker = new SneakerJson();
                        jsonSneaker.sku = sneaker.sku;
                        jsonSneaker.title = sneaker.title;
                        jsonSneaker.brand = sneaker.brand;
                        jsonSneaker.sizes = json.items;
                        jsonSneaker.category = sneaker.category;
                        jsonSneaker.sex = sneaker.sex;
                        jsonSneaker.add_time = DateTime.Now;

                        i += 1;
                        j += jsonSneaker.sizes.Count;
                        myUPCDB.sneakers.Add(jsonSneaker);

                        //todo убрать, сейчас по 1 кроссовку добавляется
                        //break;
                        Program.Logger.Info(k + ":Добавили артикул:" + sneaker.sku + ". Размеров:" + jsonSneaker.sizes.Count + ". Осталось запросов:" + _limitRequst);
                    }
                    else
                    {
                        bool test = true;
                    }
                }
            }
            //Program.logger.Info("Обход фуллкаталога завершен!");
        }

        public JsonRootObject GetAllSizes(string brand, string sku, out bool isLimits)
        {
            //http://www.upcitemdb.com/api/explorer#!/search/get_trial_search

            //string brand = "nike";
            //string sku = "859524-005";
            isLimits = false;
            string sku2 = sku.Replace("-", "%20");
            int offset = 0;
            string data = "s=" + brand + "%20" + sku2 + "&offset=" + offset + "&match_mode=1&type=product";
            HttpStatusCode statusCode;                      
            string textJson = GET("https://api.upcitemdb.com/prod/trial/search", data, out statusCode);
            if (!string.IsNullOrEmpty(textJson))
            {
                JsonRootObject jsonObj = JsonConvert.DeserializeObject<JsonRootObject>(textJson);
                if (jsonObj.code == "OK")
                {
                    if (jsonObj.total <= 5)
                    {
                        return jsonObj;
                    }
                    else
                    {
                        var currentOffset = jsonObj.offset;
                        do
                        {
                            data = "s=" + brand + "%20" + sku2 + "&offset=" + currentOffset + "&match_mode=1&type=product";
                            textJson = GET("https://api.upcitemdb.com/prod/trial/search", data, out statusCode);
                            if (!string.IsNullOrEmpty(textJson))
                            {
                                JsonRootObject currentJsonObj = JsonConvert.DeserializeObject<JsonRootObject>(textJson);

                                foreach (var item in currentJsonObj.items)
                                {
                                    jsonObj.items.Add(item);
                                }

                                currentOffset = currentJsonObj.offset;
                            }
                            else
                            {
                                return null;
                            }
 
                        }
                        while (currentOffset != 0);

                        return jsonObj;
                        //todo дописать чтобы норм работало если больше 5 резульатов
                        //return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                //если ошибка в запросе и вернул пустую строку
                if (statusCode == HttpStatusCode.NotFound)
                {
                    JsonRootObject json = new JsonRootObject();
                    json.items = new List<JsonItem>();
                    return json;
                }
                if ((int)statusCode == 429)
                {
                    MyStopList.skuList.Add(sku);
                    isLimits = true;
                    return null;
                }
                return null;
            }
        }

        public JsonRootObject GetAllSizes2(string brand, string sku, out bool isLimits)
        {
            //http://www.upcitemdb.com/api/explorer#!/search/get_trial_search

            //string brand = "nike";
            //string sku = "859524-005";
            isLimits = false;
            string sku2 = sku.Replace("-", "%20");
            int offset = 0;
            string data = "s=" + brand + "%20" + sku2 + "&offset=" + offset + "&match_mode=1&type=product";
            HttpStatusCode statusCode;
            //string textJson = GET("https://api.upcitemdb.com/prod/trial/search", data, out statusCode);
            string textJson = GetWithCrawlera("https://api.upcitemdb.com/prod/trial/search", data, out statusCode);
            if (!string.IsNullOrEmpty(textJson))
            {
                JsonRootObject jsonObj = JsonConvert.DeserializeObject<JsonRootObject>(textJson);
                if (jsonObj.code == "OK")
                {
                    if (jsonObj.total <= 5)
                    {
                        return jsonObj;
                    }
                    else
                    {
                        var currentOffset = jsonObj.offset;
                        do
                        {
                            data = "s=" + brand + "%20" + sku2 + "&offset=" + currentOffset + "&match_mode=1&type=product";
                            //textJson = GET("https://api.upcitemdb.com/prod/trial/search", data, out statusCode);
                            textJson = GetWithCrawlera("https://api.upcitemdb.com/prod/trial/search", data, out statusCode);
                            if (!string.IsNullOrEmpty(textJson))
                            {
                                JsonRootObject currentJsonObj = JsonConvert.DeserializeObject<JsonRootObject>(textJson);

                                foreach (var item in currentJsonObj.items)
                                {
                                    jsonObj.items.Add(item);
                                }

                                currentOffset = currentJsonObj.offset;
                            }
                            else
                            {
                                return null;
                            }

                        }
                        while (currentOffset != 0);

                        return jsonObj;
                        //todo дописать чтобы норм работало если больше 5 резульатов
                        //return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                //если ошибка в запросе и вернул пустую строку
                if (statusCode == HttpStatusCode.NotFound)
                {
                    JsonRootObject json = new JsonRootObject();
                    json.items = new List<JsonItem>();
                    return json;
                }
                if ((int)statusCode == 429)
                {
                    MyStopList.skuList.Add(sku);
                    isLimits = true;
                    return null;
                }
                return null;
            }
        }

        private string GET(string Url, string Data, out HttpStatusCode statusCode)
        {
            WebRequest req = WebRequest.Create(Url + "?" + Data);
            try
            {
                _doRequest += 1;
                WebResponse resp = req.GetResponse();
                HttpWebResponse httpResponse = (HttpWebResponse)resp;
                statusCode = httpResponse.StatusCode;
                Stream stream = resp.GetResponseStream();
                string limit = resp.Headers["X-RateLimit-Remaining"];
                var resetTime = resp.Headers["X-RateLimit-Reset"];
                _limitRequst = Int32.Parse(limit);
                _resetTimeLimits = UPCDB.UnixTimeStampToDateTime(Double.Parse(resetTime));
                StreamReader sr = new StreamReader(stream);
                string Out = sr.ReadToEnd();
                sr.Close();
                System.Threading.Thread.Sleep(1000 * _timeDelay);
                return Out;
            }
            catch (WebException e)
            {
                HttpWebResponse httpResponse = (HttpWebResponse)e.Response;
                statusCode = httpResponse.StatusCode;
                string limit = httpResponse.Headers["X-RateLimit-Remaining"];
                var resetTime = httpResponse.Headers["X-RateLimit-Reset"];
                _limitRequst = Int32.Parse(limit);
                _resetTimeLimits = UPCDB.UnixTimeStampToDateTime(Double.Parse(resetTime));
                if (statusCode == HttpStatusCode.NotFound)
                {
                    //если артикул не найден, его нужно добавить в файл markets но без размеров, чтобы повторно его не запрашивать на следующий день парсинга
                }
                System.Threading.Thread.Sleep(1000 * _timeDelay);
                return null;
            }
        }

        public string GetWithCrawlera(string Url, string Data, out HttpStatusCode statusCode)
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            var myProxy = new WebProxy("http://proxy.crawlera.com:8010");
            myProxy.Credentials = new NetworkCredential("36f14b90c38c4005a81ccbed16a31f58", "");

            //string url = "https://twitter.com/";
            //string url = "https://api.upcitemdb.com/prod/trial/search?s=nike%20859524-005&match_mode=1&type=product";
            string url = Url + "?" + Data;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            var encodedApiKey = Helper.Base64Encode("36f14b90c38c4005a81ccbed16a31f58:");
            request.Headers.Add("Proxy-Authorization", "Basic " + encodedApiKey);
            //request.Proxy = proxy;
            //request.PreAuthenticate = true;

            request.Proxy = myProxy;
            request.PreAuthenticate = true;

            request.Method = "GET";
            request.Accept = "application/json";

            try
            {
                _doRequest += 1;
                WebResponse response = request.GetResponse();

                HttpWebResponse httpResponse = (HttpWebResponse)response;
                statusCode = httpResponse.StatusCode;

                //Console.WriteLine("Response Status: "
                  //  + ((HttpWebResponse)response).StatusDescription);
                //Console.WriteLine("\nResponse Headers:\n"
                  //  + ((HttpWebResponse)response).Headers);

                Stream dataStream = response.GetResponseStream();

                string limit = response.Headers["X-RateLimit-Remaining"];
                var resetTime = response.Headers["X-RateLimit-Reset"];
                _limitRequst = Int32.Parse(limit);
                _resetTimeLimits = UPCDB.UnixTimeStampToDateTime(Double.Parse(resetTime));

                var reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                //Console.WriteLine("Response Body:\n" + responseFromServer);
                reader.Close();

                response.Close();

                JsonRootObject jsonObj = JsonConvert.DeserializeObject<JsonRootObject>(responseFromServer);
                return responseFromServer;
            }
            catch (WebException e)
            {
                HttpWebResponse httpResponse = (HttpWebResponse)e.Response;
                statusCode = httpResponse.StatusCode;
                if (statusCode == HttpStatusCode.NotFound)
                {
                    //если артикул не найден, его нужно добавить в файл markets но без размеров, чтобы повторно его не запрашивать на следующий день парсинга
                }
                System.Threading.Thread.Sleep(1000 * _timeDelay);
                return null;
            }

        }
           

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private void GoToFTP(string filename, string ftpFileName)
        {            
            // Создаем объект FtpWebRequest - он указывает на файл, который будет создан
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://s07.webhost1.ru/" + ftpFileName);
            // устанавливаем метод на загрузку файлов
            request.Method = WebRequestMethods.Ftp.UploadFile;
            //login pass
            request.Credentials = new NetworkCredential("amaro_upcdb", "38273827");

            // создаем поток для загрузки файла
            FileStream fs = new FileStream(filename, FileMode.Open);
            byte[] fileContents = new byte[fs.Length];
            fs.Read(fileContents, 0, fileContents.Length);
            fs.Close();
            request.ContentLength = fileContents.Length;

            // пишем считанный в массив байтов файл в выходной поток
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            // получаем ответ от сервера в виде объекта FtpWebResponse
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }

        private void GetFileFromFtp(string filename, string ftpFileName)
        {
            string inputfilepath = filename;
            string ftphost = "s07.webhost1.ru/";
            string ftpfilepath = ftpFileName;

            string ftpfullpath = "ftp://" + ftphost + ftpfilepath;

            using (WebClient request = new WebClient())
            {
                request.Credentials = new NetworkCredential("amaro_upcdb", "38273827");
                byte[] fileData = request.DownloadData(ftpfullpath);

                using (FileStream file = File.Create(inputfilepath))
                {
                    file.Write(fileData, 0, fileData.Length);
                    file.Close();
                }
                //MessageBox.Show("Download Complete");
            }
            //throw new NotImplementedException();
        }

        private void ExportToCSV(string filename)
        {
            var csv = new List<UpcDbCsvRecord>();
            foreach (var sneaker in myUPCDB.sneakers)
            {
                if (sneaker.sizes != null)
                {
                    foreach (var size in sneaker.sizes)
                    {
                        var record = new UpcDbCsvRecord();
                        record.brand = sneaker.brand;
                        record.title = sneaker.title;
                        record.sku = sneaker.sku;
                        record.size = size.size;
                        record.upc = size.upc;

                        csv.Add(record);
                    }
                }

            }

            using (var sw = new StreamWriter(filename, false, Encoding.GetEncoding(1251)))
            {
                //sw.Encoding = Encoding.GetEncoding(1251);
                var writer = new CsvWriter(sw);
                writer.Configuration.Delimiter = ";";
                //writer.Configuration.Encoding = Encoding.GetEncoding(1251);
                writer.WriteRecords(csv);
            }

            //throw new NotImplementedException();
        }

        
    }
}
