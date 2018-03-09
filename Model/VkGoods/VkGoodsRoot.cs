using System;
using System.Collections.Generic;

namespace SneakerIcon.Model.VkGoods
{
    public class VkGoodsRoot
    {
        public DateTime UpdateTime { get; set; }

        public List<VkGoodsItem> Records { get; set; }

        public VkGoodsRoot()
        {
            Records = new List<VkGoodsItem>();
        }
    }
}
