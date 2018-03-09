using NLog;
using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Net;
using Akumu.Antigate;
using OpenQA.Selenium;
using SneakerIcon.Classes.DAO;

namespace SneakerIcon.Sys
{
    public static class Common
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly object LockKapcha = new object();
        private static readonly object LockRead = new object();


        public static string HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            Logger.Debug("Uploading {0} to {1}", file, url);
            var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            var boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            var wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = CredentialCache.DefaultCredentials;

            var rs = wr.GetRequestStream();

            const string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                var formitem = string.Format(formdataTemplate, key, nvc[key]);
                var formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            const string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            lock (LockRead)
            {
                var header = string.Format(headerTemplate, paramName, file, contentType);
                var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                rs.Write(headerbytes, 0, headerbytes.Length);
            
                var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                var buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    rs.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();
            }

            var trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            var value = "";
            try
            {
                wresp = wr.GetResponse();
                var stream2 = wresp.GetResponseStream();
                if (stream2 != null)
                {
                    var reader2 = new StreamReader(stream2);
                    value = reader2.ReadToEnd();
                    Logger.Debug("File uploaded, server response is: {0}", value);
                }
                else
                {
                    throw new NullReferenceException("Ошибка при получении ответа от сервера. Сервер вернул null");
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error uploading file");
                wresp?.Close();
            }
            finally
            {
                // ReSharper disable once RedundantAssignment
                wr = null;
            }

            return value;
        }

        public static string GetCapchaForWebDriver(IWebDriver webDriver)
        {
            const string saveLocation = @"capcha.png";
            string result;

            lock (LockKapcha)
            {
                TakeCaptcha(webDriver, saveLocation);
                result = GetCapcha(saveLocation);
            }
            return result;
        }

        private static void TakeCaptcha(IWebDriver webDriver, string saveLocation)
        {
            var myScreenhot = TakeScreenshot(webDriver, saveLocation);

            var element = webDriver.FindElement(By.TagName("img"));
            var xPosCaptcha = element.Location.X;
            var yPosCaptcha = element.Location.Y;

            // Устанавливаем координаты капчи и ее размер
            var parSection = new Rectangle(xPosCaptcha, yPosCaptcha, 130, 50);
            // Создаем изображение с заданым размером
            var bmpCaptcha = new Bitmap(parSection.Width, parSection.Height);

            // Вырезаем область изображения
            var g = Graphics.FromImage(bmpCaptcha);
            g.DrawImage(myScreenhot, 0, 0, parSection, GraphicsUnit.Pixel);
            g.Dispose();

            bmpCaptcha.Save(saveLocation);
        }

        public static Image TakeScreenshot(IWebDriver webDriver, string saveLocation)
        {
            // Делаем скриншот страницы и сохраняем на диск
            var screenshotDriver = webDriver as ITakesScreenshot;
            if (screenshotDriver != null)
            {
                var screenshot = screenshotDriver.GetScreenshot();
                screenshot.SaveAsFile(saveLocation, ScreenshotImageFormat.Bmp);
            }

            // Считываем изображение из файла в переменную
            var fs = new FileStream(saveLocation, FileMode.Open);
            var myScreenhot = Image.FromStream(fs);
            fs.Close();

            return myScreenhot;
        }

        private static string GetCapcha(string nameFile)
        {
            var answer = "";

            try
            {
                if (string.IsNullOrWhiteSpace(Config.GetConfig().CapchaId))
                {
                    throw new Exception("Не задан ключ для антикапчи");
                }

                var anticap = new AntiCaptcha(Config.GetConfig().CapchaId)
                {
                    CheckDelay = 10000,
                    CheckRetryCount = 20,
                    SlotRetry = 5,
                    SlotRetryDelay = 800
                };
                //anticap.Parameters.Set("is_russian", "1");

                // отправляем файл и ждем ответа
                answer = anticap.GetAnswer(nameFile);

                if (answer == null || "error_captcha_unsolvable".Equals(answer) || "ERROR_BAD_DUPLICATES".Equals(answer))
                    throw new Exception("Ошибка при распознавании капчи");

                return answer;
            }
            //TODO когда появится какая-нибудь реальная ошибка, тогда и будем думать о полном выходе из программы
            catch (AntigateErrorException)
            {
                // Antigate ответил одной из документированных в API ошибкой
            }
            catch (Exception)
            {
                // возникло исключение
            }

            return answer;
        }
    }
}
