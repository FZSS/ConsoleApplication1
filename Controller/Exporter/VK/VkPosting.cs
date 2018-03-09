using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading;
using NLog;
using OpenQA.Selenium.Chrome;
using SneakerIcon.Model.HotOffersModel;
using SneakerIcon.Model.VkGoods;
using SneakerIcon.Sys;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils.AntiCaptcha;

namespace SneakerIcon.Controller.Exporter.VK
{
    public class VkPosting
    {
        public int groupId { get; set; }
        private static int _countAdd;
        private static readonly int MaxAdd = Config.GetConfig().MaxCountVkGoodsPosting;

        private class Capcha : ICaptchaSolver
        {
            public string Solve(string url)
            {
                var webDriver = new ChromeDriver();

                webDriver.Navigate().GoToUrl(url);

                var result = Common.GetCapchaForWebDriver(webDriver);

                webDriver.Quit();

                return result;
            }

            public void CaptchaIsFalse()
            {
                throw new NotImplementedException();
            }
        }


        private readonly VkApi _vk;// = new VkApi(capcha) { RequestsPerSecond = (float)0.3 };
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public VkPosting(int groupId = -1)
        {
            if (groupId == -1)
                this.groupId = Config.GetConfig().IdGroup;
            else 
                this.groupId = groupId;

            var capcha = new Capcha();
            _vk = new VkApi(capcha) { RequestsPerSecond = (float)1 };

            try
            {
                _vk.Authorize(new ApiAuthParams()
                {
                    Login = Config.GetConfig().Login,
                    Password = Config.GetConfig().Pass,
                    ApplicationId = (ulong)Config.GetConfig().AppId,
                    Settings = Settings.All | Settings.Offline
                });
                //_vk.Authorize(new ApiAuthParams()
                //{
                //    AccessToken = Config.GetConfig().UserToken
                //});
            }
            catch (Exception e)
            {
                _logger.Error(e, "Ошибка при авторизации постера ВК");
            }

            if (!_vk.IsAuthorized)
            {
                _logger.Error("Ошибка при авторизации постера ВК. Прямой ошибки нет, но ВК не авторизован");
            }

        }

        public void PostIntoVkWall(HotOffer offer)
        {
            var text = offer.brand + " " + offer.title + " " + offer.sku;
            PostIntoVkWall(text, offer.images);
        }

        public void PostIntoVkWall(string message, List<string> images)
        {
            //var grId = Config.GetConfig().IdGroup;
            var grId = this.groupId;
            var dict = new Dictionary<long, string>();

            foreach (var image in images)
            {
                try
                {
                    var wc = new WebClient();
                    wc.DownloadFileAsync(new Uri(image), "imageForVkWall.jpg");

                    Thread.Sleep(1000);

                    var serverInfo = _vk.Photo.GetWallUploadServer(grId);

                    var nvc = new NameValueCollection();

                    var textAnswer = Common.HttpUploadFile(serverInfo.UploadUrl, "imageForVkWall.jpg",
                        "file", "image/jpeg", nvc);

                    if (_vk.UserId != null)
                    {
                        var loadPhoto = _vk.Photo.SaveWallPhoto(textAnswer, (ulong) _vk.UserId,
                            (ulong)grId);
                        var id = loadPhoto[0].Id;
                        if (id != null) dict.Add(id.Value, image);
                        else
                        {
                            throw new NullReferenceException("Пришел нулл в ответе от сервера ВК при загрузке фото");
                        }
                    }
                    else
                    {
                        throw new NullReferenceException("_vk.UserId = null");
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Ошибка при загрузке файла " + image + " на стену в ВК");
                }
            }

            var att = dict.Select(elem => new Photo()
            {
                Id = elem.Key,
                OwnerId = _vk.UserId,
                BigPhotoSrc = new Uri(elem.Value)
            }).Cast<MediaAttachment>().ToList();

            _vk.Wall.Post(new WallPostParams()
            {
                Message = message,
                Attachments = att,
                OwnerId = grId * (-1)

            });
        }

        public VkGoodsItem AddOrEditVkGoods(VkTask task, VkGoodsItem oldItem)
        {
            const int category = 4;

            using (var imageWorker = new VkImageWorker())
            {
                if (oldItem == null || oldItem.VkId == 0 )
                {
                    if (_countAdd > MaxAdd)
                    {
                        _logger.Warn("Добавлено больше дневного лимита товаров");
                        return null;
                    }

                    long idMainPhoto = 0;

                    var dict = new Dictionary<long, string>();

                    var textAnswer = "";

                    foreach (var image in task.ImageList)
                    {
                        try
                        {
                            var wc = new WebClient();
                            wc.DownloadFile(new Uri(image), imageWorker.Filename);

                            var serverInfo = _vk.Photo.GetMarketUploadServer(Config.GetConfig().IdGroup, idMainPhoto == 0);

                            var nvc = new NameValueCollection();

                            textAnswer = Common.HttpUploadFile(serverInfo.UploadUrl, imageWorker.Filename,
                                "file", "image/jpeg", nvc);

                            if (_vk.UserId != null)
                            {
                                var loadPhoto = _vk.Photo.SaveMarketPhoto(Config.GetConfig().IdGroup, textAnswer);
                                var id = loadPhoto[0].Id;

                                if (id != null)
                                {
                                    if (idMainPhoto == 0)
                                    {
                                        idMainPhoto = id.Value;
                                    }
                                    else
                                    {
                                        dict.Add(id.Value, image);
                                    }
                                }
                                else
                                {
                                    throw new NullReferenceException("Пришел нулл в ответе от сервера ВК при загрузке фото");
                                }
                            }
                            else
                            {
                                throw new NullReferenceException("_vk.UserId = null");
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Ошибка при загрузке файла " + image + " на стену в ВК");
                            _logger.Error(textAnswer);

                            if (textAnswer.Contains("ERR_UPLOAD_BAD_IMAGE_SIZE: market photo min size 400x400"))
                            {
                                _logger.Warn($"Изображение {image} слишком маленькое");
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }

                    if (idMainPhoto != 0)
                    {
                        var id =  _vk.Markets.Add(new MarketProductParams()
                        {
                            CategoryId = category,
                            Deleted = task.IsDeleted,
                            Description = task.Description,
                            MainPhotoId = idMainPhoto,
                            Name = task.Header,
                            OwnerId = Config.GetConfig().IdGroup * -1,
                            Price = task.Price,
                            PhotoIds = dict.Keys
                        });

                        _countAdd++;

                        var result = new VkGoodsItem()
                        {
                            Name = task.Header,
                            Description = task.Description,
                            Price = task.Price,
                            IsDeleted = task.IsDeleted,
                            IdImageList = dict.Keys.ToList(),
                            MainImageId = idMainPhoto,
                            SKU = task.SKU,
                            VkId = id
                        };

                        return result;
                    }

                    const string text = "Ошибка при загрузке товара. Идентификатор основного фото равен 0";

                    _logger.Error(text);
                    throw new Exception(text);
                }

                if (!CheckDiff(task, oldItem)) return oldItem;


                _vk.Markets.Edit(new MarketProductParams()
                {
                    Name = task.Header,
                    Description = task.Description,
                    Price = task.Price,
                    Deleted = task.IsDeleted,
                    ItemId = oldItem.VkId,
                    PhotoIds = oldItem.IdImageList,
                    MainPhotoId = oldItem.MainImageId,
                    OwnerId = Config.GetConfig().IdGroup * -1,
                    CategoryId = category
                });

                oldItem.Description = task.Description;
                oldItem.Name = task.Header;
                oldItem.IsDeleted = task.IsDeleted;
                oldItem.Price = task.Price;

                return oldItem;
            }
        }

        private bool CheckDiff(VkTask task, VkGoodsItem oldItem)
        {
            var result = task.IsDeleted != oldItem.IsDeleted 
                || !task.Description.Equals(oldItem.Description) 
                || !task.Header.Equals(oldItem.Name) 
                || task.Price != oldItem.Price;

            return result;
        }

        public bool DeleteVkGoods(long id)
        {
            _logger.Debug($"Удален товар {id}");
            return _vk.Markets.Delete(Config.GetConfig().IdGroup * -1, id);
        }

        public void DeleteAllGoods()
        {
            var offset = 0;
            var goods = _vk.Markets.Get(Config.GetConfig().IdGroup * -1, null, 200, offset).ToList();
            var goodsTemp = _vk.Markets.Get(Config.GetConfig().IdGroup * -1, null, 200, offset);

            while (goodsTemp.Count == 200)
            {
                offset += 200;
                goodsTemp = _vk.Markets.Get(Config.GetConfig().IdGroup * -1, null, 200, offset);
                goods.AddRange(goodsTemp.ToList());
            }

            foreach (var good in goods)
            {
                DeleteVkGoods(good.Id);
            }

        }
    }
}
