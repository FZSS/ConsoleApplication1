using SneakerIcon.Classes.Parsing.Model;
using SneakerIcon.Classes.SizeConverters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using NLog;
using Telegram.Bot;
using System.Threading.Tasks;
using SneakerIcon.Classes.Utils;

namespace SneakerIcon.Classes
{
    public static class Helper
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static string[] GetFileListFromDirectory(string directory, string host, string user, string pass)
        {
            try
            {
                /* Create an FTP Request */
                var ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(user, pass);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                /* Establish Return Communication with the FTP Server */
                var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                var ftpStream = ftpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(ftpStream);
                /* Store the Raw Response */
                string directoryRaw = null;
                /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                /* Resource Cleanup */
                ftpReader.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
                /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return new string[] { "" };
        }

        internal static bool CheckExistFtpFolder(string ftpHost, string ftpUser, string ftpPass, string folder)
        {
            var ftp = new Ftp(ftpHost,ftpUser,ftpPass);
            var dirArr = GetFileListFromDirectory("" ,"ftp://" + ftpHost, ftpUser, ftpPass);
            var dirList = dirArr.ToList();
            if (dirList.Exists(x => x == folder))
                return true;
            else 
                return false;
        }

        public static string[] GetListDirectoryFromFtp(string ftpHost, string ftpUser, string ftpPass)
        {
            var list = new List<string>();

            //todo подумать, что с этим дублированием сделать. Скорее всего внутренний класс замутить
            var request = (FtpWebRequest)WebRequest.Create(ftpHost);
            request.Credentials = new NetworkCredential(ftpUser, ftpPass);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.UseBinary = true;
            request.EnableSsl = false;
            request.UsePassive = true;
            request.KeepAlive = false;

            using (var response = (FtpWebResponse)request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream, true))
                        {
                            while (!reader.EndOfStream)
                            {
                                list.Add(reader.ReadLine());
                            }
                        }
                    }
                    else
                    {
                        throw new NullReferenceException("В ответ на запрос ФТП о списке каталогов на хост " + ftpHost + " пришел нулл");
                    }
                        
                }
            }

            return list.Where(x => !x.EndsWith(".") && !x.EndsWith("..")).ToArray();
        }

        public static T GetJsonObjFromFtp<T>(string ftpFileName, string ftpHost, string ftpUser, string ftpPass)
        {
            var request = (FtpWebRequest)WebRequest.Create(Combine(ftpHost, ftpFileName));
            request.Credentials = new NetworkCredential(ftpUser, ftpPass);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UseBinary = true;
            request.EnableSsl = false;
            request.UsePassive = true;
            request.KeepAlive = false;

            string data;

            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            var reader = new StreamReader(stream);
                            data = reader.ReadToEnd();
                        }
                        else
                        {
                            throw new NullReferenceException("В ответ на запрос ФТП на хост " + ftpHost + " пришел нулл");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn(e, "Ошибка при запросе с ФТП");
                throw;
            }

            T obj;

            try
            {
                obj = JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception e)
            {
                Log.Warn(e, "Ошибка при десериализации объекта");
                throw;
            }

            return obj;
        }

        public static string Combine(string path1, string path2)
        {
            if (path1 == null) throw new ArgumentNullException("path1");
            return Path.Combine(path1, path2).Replace("\\", "/");
        }

        public static void GetFileFromFtp(string filename, string ftpFileName, string ftpHost, string ftpUser, string ftpPass)
        {
            string inputfilepath = filename;
            //string ftphost = "s07.webhost1.ru/";
            string ftpfilepath = ftpFileName;

            string ftpfullpath = "ftp://" + ftpHost + ftpfilepath;

            using (WebClient request = new WebClient())
            {
                request.Credentials = new NetworkCredential(ftpUser, ftpPass);
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

        public static void CreateDirectoryFtp(string newDirectory, string _host, string _user, string _pass)
        {
            try
            {
                /* Create an FTP Request */
                _host = "ftp://" + _host;
                var _ftpRequest = (FtpWebRequest)WebRequest.Create(_host + "/" + newDirectory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                _ftpRequest.Credentials = new NetworkCredential(_user, _pass);
                /* When in doubt, use these options */
                _ftpRequest.UseBinary = true;
                _ftpRequest.UsePassive = true;
                _ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                _ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                /* Establish Return Communication with the FTP Server */
                var _ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse();
                /* Resource Cleanup */
                _ftpResponse.Close();
                _ftpRequest = null;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return;
        }

        public static string GetFileFromFtpIntoString(string filename, string ftpFileName, string ftpHost, string ftpUser, string ftpPass)
        {
            //string inputfilepath = filename;
            //string ftphost = "s07.webhost1.ru/";
            string ftpfilepath = ftpFileName;

            string ftpfullpath = "ftp://" + ftpHost + ftpfilepath;

            using (WebClient request = new WebClient())
            {
                request.Credentials = new NetworkCredential(ftpUser, ftpPass);
                byte[] fileData = request.DownloadData(ftpfullpath);

                Encoding enc8 = Encoding.UTF8;
                var resultStr = enc8.GetString(fileData);
                return resultStr;

                //using (FileStream file = File.Create(inputfilepath))
                //{
                //    file.Write(fileData, 0, fileData.Length);
                //    file.Close();
                //}
                //MessageBox.Show("Download Complete");
            }
            //throw new NotImplementedException();
        }

        public static string GetFileFromFtpIntoString(string path, string ftpHost, string ftpUser, string ftpPass)
        {
            //string inputfilepath = filename;
            //string ftphost = "s07.webhost1.ru/";
            string ftpfilepath = path;

            string ftpfullpath = "ftp://" + ftpHost + ftpfilepath;

            using (WebClient request = new WebClient())
            {
                request.Credentials = new NetworkCredential(ftpUser, ftpPass);
                byte[] fileData = request.DownloadData(ftpfullpath);

                Encoding enc8 = Encoding.UTF8;
                var resultStr = enc8.GetString(fileData);
                return resultStr;

                //using (FileStream file = File.Create(inputfilepath))
                //{
                //    file.Write(fileData, 0, fileData.Length);
                //    file.Close();
                //}
                //MessageBox.Show("Download Complete");
            }
            //throw new NotImplementedException();
        }

        public static void LoadFileToFtp(string filename, string ftpFileName, string ftpHost, string ftpUser, string ftpPass)
        {
            // Создаем объект FtpWebRequest - он указывает на файл, который будет создан
            var url = "ftp://" + ftpHost + ftpFileName;
            url = url.Replace(" ", "");
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
            // устанавливаем метод на загрузку файлов
            request.Method = WebRequestMethods.Ftp.UploadFile;
            //login pass
            request.Credentials = new NetworkCredential(ftpUser, ftpPass);

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

        public static bool isTrueMaskSKUForNike(string SKU)
        {
            string sPattern = "^\\d{6}-\\d{3}$";
            //(A|\d){6}\-\d{3}
            if (System.Text.RegularExpressions.Regex.IsMatch(SKU, sPattern))
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
        /// <param ftpFolder="sizeUS"></param>
        /// <param ftpFolder="sizeEU"></param>
        /// <param ftpFolder="categorySneakerFullCatalog"></param>
        /// <param ftpFolder="sex"></param>
        /// <returns>возвращает true если преобразование было удачным</returns>
 
        public static bool GetCategoryAndSex(string sizeUS, string sizeEU, out string category, out string sex)
        {
            if (sizeUS.Contains("C") || sizeUS.Contains("Y"))
            {
                category = Settings.CATEGORY_KIDS;
                sex = null;
                return true;
            }
            else
            {
                SizeConverter converterMan = new SizeConverter(sizeUS, Settings.CATEGORY_MEN);
                if (converterMan.sizeEUR == sizeEU)
                {
                    category = Settings.CATEGORY_MEN;
                    sex = Settings.GENDER_MAN;
                    return true;
                }
                else
                {
                    SizeConverter converterWoman = new SizeConverter(sizeUS, Settings.CATEGORY_WOMEN);
                    if (converterWoman.sizeEUR == sizeEU)
                    {
                        category = Settings.CATEGORY_WOMEN;
                        sex = Settings.GENDER_WOMAN;
                        return true;
                    }
                    else
                    {
                        category = null;
                        sex = null;
                        return false;
                    }
                }
            }
            bool test = true;
        }

        public static string ConvertCategoryRusToEng(string category) {
            if (category == Settings.CATEGORY_KIDS)
            {
                return "kids";
            }
            else if (category == Settings.CATEGORY_MEN)
            {
                return "men";
            }
            else if (category == Settings.CATEGORY_WOMEN)
            {
                return "women";
            }
            else
            {
                return null;
                //throw new Exception("Wrong Category: " + categorySneakerFullCatalog);
            }
        }

        public static string ConvertEngToRusCategory(string category)
        {
            if (category == "kids" )
            {
                return Settings.CATEGORY_KIDS;
            }
            else if (category == "men")
            {
                return Settings.CATEGORY_MEN;
            }
            else if (category == "women")
            {
                return Settings.CATEGORY_WOMEN ;
            }
            else
            {
                return null;
                //throw new Exception("Wrong Category: " + categorySneakerFullCatalog);
            }
        }

        public static string GetSexFromCategory(string category)
        {
            if (category == Settings.CATEGORY_KIDS)
            {
                return null;
            }
            else if (category == Settings.CATEGORY_MEN)
            {
                return Settings.GENDER_MAN;
            }
            else if (category == Settings.CATEGORY_WOMEN)
            {
                return Settings.GENDER_WOMAN;
            }
            else
            {
                return null;
                //throw new Exception("Wrong Category: " + categorySneakerFullCatalog);
            }
        }

        public static string GetEngSexFromEngCategory(string category)
        {
            if (category == "kids")
            {
                return null;
            }
            else if (category == "men")
            {
                return "men";
            }
            else if (category == "women")
            {
                return "women";
            }
            else
            {
                return null;
                //throw new Exception("Wrong Category: " + categorySneakerFullCatalog);
            }
        }

        public static string ConvertSexRusToEng(string sex)
        {
            if (String.IsNullOrWhiteSpace(sex))
            {
                return null;
            }
            else if (sex == Settings.GENDER_MAN)
            {
                return "men";
            }
            else if (sex == Settings.GENDER_WOMAN)
            {
                return "women";
            }
            else
            {
                throw new Exception("Wrong Sex: " + sex);
            }
        }

        public static List<ListingSize> GetSizeListUs(List<SneakerSize> sizes)
        {
            var result = new List<ListingSize>();
            foreach (var size in sizes)
            {
                var listingSize = new ListingSize();
                listingSize.us = size.sizeUS;
                listingSize.eu = size.sizeEU;
                listingSize.cm = size.sizeCM;
                listingSize.uk = size.sizeUK;
                listingSize.ru = size.sizeRU;
                listingSize.upc = size.upc;
                listingSize.quantity = size.quantity;
                result.Add(listingSize);
            }
            return result;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static async Task<Telegram.Bot.Types.Message> TelegramPost(string message, string chatId)
        {
            var bot = new TelegramBotClient("352425649:AAGvZWkqYAHz7i5-s2f3I_qGnygTqydkEpU");
            //var updates = await bot.GetUpdatesAsync();
            //var t = await bot.SendTextMessageAsync("@sneake_empire_news", message);
            //chat id -1001101919442 - приватный канал для внутреннего пользования
            //chat id @sneaker_icon_hot
            var t = await bot.SendTextMessageAsync(chatId, message);
            
            //var t = await bot.SendTextMessageAsync("@SneakerEmpireNews", "test message");
            return t;
        }

        public static bool DownloadImage(string url, string path = "image.jpg", int numTry = 5)
        {
            int i = 0;
            while (i < numTry)
            {
                try
                {
                    DownloadImage(url, path);
                    return true;
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                    Log.Error(e.StackTrace);
                }
                i++;
            }         
            return false;
        }

        public static bool DownloadImage(string url, string path = "image.jpg")
        {
            string filename = path;
            WebClient client = new WebClient();
            //string url = sneaker.images[i];
            //string url = stockSneaker.images[i].Replace("/616/", "/2000/").Replace("/370/", "/1200/");
            Uri uri = new Uri(url);
            client.DownloadFile(uri, filename);
            System.Threading.Thread.Sleep(100);
            //Console.WriteLine("image downloaded: " + filename);
            return true;
        }

        public static string UpperFirstCharInWord(string str)
        {
            string[] array = str.Split(' ');
            StringBuilder sb = new StringBuilder();
            foreach (string item in array)
            {
                if (!string.IsNullOrWhiteSpace(item))
                    sb.Append(char.ToUpper(item[0]) + item.Substring(1, item.Length - 1) + " ");
            }
            return sb.ToString();
        }
    }
}
