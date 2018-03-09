using System.Collections.Generic;
using SneakerIcon.Classes.SizeConverters.Model;

namespace SneakerIcon.Model.VkGoods
{
    public class VkGoodsItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// цена в рублях с учетом доставки до рф, и вычета налога ват, с маржой
        /// самая низкая цена из всех цен для размеров
        /// </summary>
        public int Price { get; set; }
        public bool IsDeleted { get; set; }
        public long MainImageId { get; set; }
        public List<long> IdImageList { get; set; }
        public List<long> IdCategoryList { get; set; }
        // ReSharper disable once InconsistentNaming
        public string SKU { get; set; }

        public long VkId { get; set; }

        public VkGoodsItem()
        {
            IdImageList = new List<long>();
            IdCategoryList = new List<long>();
            IsDeleted = false;
            VkId = 0;
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}," +
                   $" {nameof(Description)}: {Description}," +
                   $" {nameof(Price)}: {Price}," +
                   $" {nameof(IsDeleted)}: {IsDeleted}," +
                   $" {nameof(MainImageId)}: {MainImageId}," +
                   $" {nameof(IdImageList)}: {IdImageList}," +
                   $" {nameof(IdCategoryList)}: {IdCategoryList}," +
                   $" {nameof(SKU)}: {SKU}," +
                   $" {nameof(VkId)}: {VkId}";
        }
    }
}
