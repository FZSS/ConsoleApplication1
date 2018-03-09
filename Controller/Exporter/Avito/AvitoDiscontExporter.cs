using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SneakerIcon.Classes.Exporters2;

namespace SneakerIcon.Controller.Exporter.Avito
{
    public class AvitoDiscontExporter : AvitoExporter3
    {
        public static DateTime GetDateBegin(DateTime startTime, int count, int position, int DaysPeriod, int EndHour, int StartHour)
        {
            //DateBegin
            /* нужно все объявления размазать на месяц (ну пусть будет на 30 дней). 
             * допустим если у меня есть 3000 объявлений то это значит что нужно постить по 100 в день
             * Постить будем допустим с 7.00 утра 23.59. Получается 17 часов. это 61200 секунд. 
             * Получается объявление нужно постить каждые 612 секунд, начиная с 7 утра
             * 
             * В сутках 24 часа, это 86400 секунд.
             * В 30 днях 2 592 000 секунд
             * То есть если у нас есть 3000 объявлений, значит начиная с текущего момента нужно постить объявление каждые 864 секунды
             */

            //постинг допустим начинается в 9 утра и заканчивается в 9 вечера
            var postPerDay = (int)count / DaysPeriod;
            var addDay = (int)position / postPerDay;
            var workDaySeconds = (EndHour - StartHour) * 60 * 60;
            var addSecondsToPost = (int)workDaySeconds / postPerDay;
            var dayPosition = position - (postPerDay * addDay);
            //dayPosition--; //позиция с 1 начинается, а не с нуля. иначе первое размещение в 9.35 будет
            var addSeconds = dayPosition * addSecondsToPost;

            var dateBegin = new DateTime(startTime.Year, startTime.Month, startTime.Day, StartHour, 0, 0);
            dateBegin = dateBegin.AddDays(addDay);
            dateBegin = dateBegin.AddSeconds(addSeconds);

            //double delay = 2592000 / count * position; //todo сделать распределение на 10 часов а не 24
            //var intDelay = (int)delay;
            //var dateBegin = startTime.AddSeconds(intDelay); //todo datebegin можно определить только после того, как известно точное кол-во выгружаемых позиций (с учетом отфильтрованных невалидных)
            return dateBegin;

        }
    }
}
