using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Model.AvitoModel
{
    public class AvitoAdDelivery
    {
        /*
            Идентификатор склада — целое число.
            <WarehouseKey>34545</WarehouseKey>
         */
        public string WarehouseKey { get; set; }

        /*
            Разрешить предоплату — одно из значений списка:
            «Да»,
            «Нет».
            Примечание: значение по умолчанию — «Нет».

            <IsAllowPrepayment>Да</IsAllowPrepayment>
         * */
        public string IsAllowPrepayment { get; set; }

        /*
            Вес товара в килограммах — десятичное число.
            <Weight>3.2</Weight> 
         */
        public string Weight { get; set; }

        /* 
            Ширина товара, см  — десятичное число.
            Примечание: Поддержка параметра ожидается в 2017 году.
            <Width>10</Width> 
         */
        public string Width { get; set; }

        /* 
            Высота товара, см  — десятичное число.
            Примечание: Поддержка параметра ожидается в 2017 году.
            <Height>10</Height> 
         */
        public string Height { get; set; }

        /* 
            Длина товара, см  — десятичное число.
            Примечание: Поддержка параметра ожидается в 2017 году.
            <Length>20</Length> 
         */
        public string Length { get; set; }

    }
}
