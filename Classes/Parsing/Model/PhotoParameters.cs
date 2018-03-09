using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.Parsing.Model
{
    public class PhotoParameters
    {
        /// <summary>
        /// наличие водяных знаков. true/false
        /// </summary>
        public bool is_watermark_image { get; set; }
        /// <summary>
        /// цвет фона фотографий
        /// white, grey, black, multicolor (мультиколор значит что фотка стайловая)
        /// </summary>
        public string background_color { get; set; } //white, grey, black, multicolor
        /// <summary>
        /// left, right. направление первой фотографии. куда смотрит кроссовок, влево или вправо
        /// </summary>
        public string first_photo_sneaker_direction { get; set; } //left, right
    }
}
