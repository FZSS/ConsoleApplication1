using System.Collections.Generic;

namespace SneakerIcon.Controller.Exporter.VK
{
    public class VkTask
    {
        public string Header { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public List<string> ImageList { get; set; }
        public List<string> CollectionList { get; set; }
        public bool IsDeleted { get; set; }
        // ReSharper disable once InconsistentNaming
        public string SKU { get; set; }

        public VkTask()
        {
            ImageList = new List<string>();
            CollectionList = new List<string>();
        }
    }
}
