using System;
using System.IO;
using NLog;

namespace SneakerIcon.Controller.Exporter.VK
{
    public class VkImageWorker : IDisposable
    {
        public string  Filename = "imageForVkMarket.jpg";
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void Dispose()
        {
            try
            {
                File.Delete(Filename);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Не удалось удалить файл");
            }
        }
    }
}
