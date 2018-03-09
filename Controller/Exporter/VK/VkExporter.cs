using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using SneakerIcon.Classes.Exporters;
using SneakerIcon.Classes.SizeConverters.Model;
using SneakerIcon.Classes.Utils;
using SneakerIcon.Model.AllStock;
using SneakerIcon.Model.FullCatalog;
using SneakerIcon.Model.VkGoods;
using SneakerIcon.Sys;

namespace SneakerIcon.Controller.Exporter.VK
{
    public class VkExporter
    {
        public const int Margin = 1500;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private VkPosting Posting { get; set; }
        private AllStockRoot Allstock { get; set; }
        private FullCatalogRoot FullCatalog { get; set; }

        public VkExporter()
        {
            Allstock = AllStockExporter2.LoadLocalFile();
            FullCatalog = FullCatalog2.LoadLocalFile();
            Posting = new VkPosting();
        }

        public void Run()
        {
            //_posting.DeleteAllGoods();

            
            var oldVkGoods = LoadVkGoodsRootFromYd() ?? new VkGoodsRoot();
            LoadAllItems(Allstock, FullCatalog, oldVkGoods);
        }

        private void LoadAllItems(AllStockRoot allstock, FullCatalogRoot fullCatalog, VkGoodsRoot oldVkGoodsRoot)
        {
            var vkGoods = new VkGoodsRoot();
            //var i = 0;
            //const int lim = 10;
            foreach (var artikul in allstock.sneakers)
            {
                try
                {
                    var vkItem = PostItemToVk(fullCatalog, artikul, oldVkGoodsRoot);
                    if (vkItem == null) continue;

                    vkGoods.Records.Add(vkItem);
                    //i++;
                    //if (i != lim) continue;
                    //
                    //i = 0;
                    vkGoods.UpdateTime = DateTime.Now;
                    SaveVkGoodsRootToYd(vkGoods);
                }
                catch (Exception e)
                {
                    _logger.Error(e, " Ошибка при добавлении товара ВК");
                    _logger.Error(e.Message);
                    _logger.Error(e.StackTrace);
                }
            }
        }

        private VkGoodsItem PostItemToVk(FullCatalogRoot fullCatalog, AllStockSneaker artikul, VkGoodsRoot oldVkGoodsRoot)
        {
            try
            {
                var task = new VkTask();

                var fcSneaker = fullCatalog.records.Find(x => x.sku == artikul.sku);
                if (fcSneaker.images == null)
                    return null;
                if (fcSneaker.images.Count == 0)
                    return null;
                if (string.IsNullOrWhiteSpace(artikul.category))
                    return null;
                if (artikul.sizes == null)
                    return null;
                if (artikul.sizes.Count == 0)
                    return null;

                task.Header = artikul.title.ToUpper() + " " + fcSneaker.sku;

                var sizeBlock = "";

                foreach (var size in artikul.sizes)
                {
                    var scSize = SizeChart.GetSizeStatic(new Size(artikul.brand, artikul.category, size.us, null, null, null, null));
                    var priceSize = size.offers[0].price_usd_with_delivery_to_usa_and_minus_vat;
                    var priceSizeRub = (int)CurrencyRate.ConvertCurrency("USD", "RUB", priceSize) + Margin;
                    if (scSize != null)
                        sizeBlock += scSize.GetAllSizeString() + ". Цена:" + priceSizeRub + Environment.NewLine;
                }

                var start = fcSneaker.type.ToLower() + " " + artikul.title.ToUpper() + Environment.NewLine;
                task.Description = start.Substring(0, 1).ToUpper() + start.Remove(0, 1);
                task.Description += "Артикул: " + artikul.sku + Environment.NewLine;
                task.Description += "В оригинальной упаковке Nike." + Environment.NewLine;
                task.Description += "БЕСПЛАТНАЯ доставка по Москве" + Environment.NewLine;
                task.Description += "Доставка по России 3-5 дней компанией СДЭК." + Environment.NewLine;
                task.Description += "Стоимость доставки по РФ - 300 рублей." + Environment.NewLine;
                task.Description += "Доставка в регионы только по 100% предоплате, наложенным платежом не отправляем" + Environment.NewLine;
                task.Description += "Работаем с 2012 года, более 4000 моделей в ассортименте. Можете перейти в группу Вконтакте, чтобы убедиться в том, что мы продаем только оригинальную обувь. Никаких подделок и реплик!" + Environment.NewLine;
                task.Description += "В нашей группе Вконтакте более 70 реальных отзывов от клиентов!" + Environment.NewLine;
                task.Description += "Размеры в наличии:" + Environment.NewLine;
                task.Description += sizeBlock;
                task.Description += "Уточняйте наличие размеров." + Environment.NewLine;
                task.Description += "Если размера нет в наличии, можем его привезти под заказ. Срок поставки товара под заказ: 7-10 дней." + Environment.NewLine;
                task.Description += "Sneaker Icon - это большой выбор оригинальной брендовой обуви Nike и Jordan. Кроссовки, кеды, бутсы, футзалки, шиповки, сланцы, ботинки и другие типы обуви" + Environment.NewLine;


                var priceUsd = artikul.GetMinPrice();
                var priceRub = (int)CurrencyRate.ConvertCurrency("USD", "RUB", priceUsd) + Margin;
                task.Price = priceRub;
                task.ImageList = fcSneaker.images;

                task.CollectionList = GetCollectionsName(fcSneaker.sku);
                //todo пока так. Нормально удаление сделаю, как попросят.
                task.IsDeleted = false;
                task.SKU = fcSneaker.sku;

                var oldItem = oldVkGoodsRoot.Records.Where(x => x.SKU != null).FirstOrDefault(x => x.SKU.Equals(fcSneaker.sku));

                var result = Posting.AddOrEditVkGoods(task, oldItem);
                if (result != null)
                {
                    //todo подумать над информативностью. Тут не только именно отправленные данные.
                    _logger.Debug($"В товары ВК отправлен SKU {fcSneaker.sku} ID {result.VkId}");
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Ошибка при добавлении товара в ВК");
                _logger.Error(e.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Метод возвращает список коллекций для ВК
        /// Метод для Романа
        /// </summary>
        /// <param name="scu"></param>
        /// <returns></returns>
        private List<string> GetCollectionsName(string scu)
        {
            _logger.Debug("sku = " + scu);

            var sku = scu;
            var tags = new List<string>();



            return new List<string>();
        }

        private VkGoodsRoot LoadVkGoodsRootFromYd()
        {
            Program.Logger.Info("Load VkGoodsRoot from YD...");

            try
            {
                string textJson;
                using (var sr = new StreamReader(Config.GetConfig().YdVkGoodsFile))
                {
                    textJson = sr.ReadToEnd();
                }

                var fullCatalogJson = JsonConvert.DeserializeObject<VkGoodsRoot>(textJson);
                Program.Logger.Info("VkGoodsRoot downloaded. ");
                return fullCatalogJson;
            }
            catch (Exception e)
            {
                _logger.Error(e, "ошибка при чтении файла");
                return null;
            }

            
        }

        private void SaveVkGoodsRootToYd(VkGoodsRoot vkGoodsRoot)
        {
            Program.Logger.Info("Upload VkGoodsRoot To YD...");

            using (var sw = new StreamWriter(Config.GetConfig().YdVkGoodsFile, false))
            {
                sw.WriteLine(JsonConvert.SerializeObject(vkGoodsRoot));
            }

            Program.Logger.Info("Uploaded VkGoodsRoot Complete");
        }
    }
}
