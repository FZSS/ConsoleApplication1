using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace SneakerIcon.Controller.TelegramController
{
    public class MyTelegram
    {
        public const string SneakerEmpireBotToken = "352425649:AAGvZWkqYAHz7i5-s2f3I_qGnygTqydkEpU";
        public const string ChatIdWaitRelease = "-1001134642362";
        public static async void PostMessage(string message, string chatId)
        {
            var bot = new TelegramBotClient(SneakerEmpireBotToken);

            //getchatid
            //var t = await bot.SendTextMessageAsync(chatId, message);
            //var chatIdId = t.Chat.Id;
            //Console.WriteLine(chatIdId);

            //main
            var t = await bot.SendTextMessageAsync(chatId, message);
        }

        public static async void PostHtmlMessage(string message, string chatId)
        {
            var bot = new TelegramBotClient(SneakerEmpireBotToken);

            //getchatid
            //var t = await bot.SendTextMessageAsync(chatId, message);
            //var chatIdId = t.Chat.Id;
            //Console.WriteLine(chatIdId);

            //main
            var t = await bot.SendTextMessageAsync(chatId, message, false, false, 0, null, ParseMode.Html);
        }

        public static void PostMessageWaitRelease(string message)
        {
            PostHtmlMessage(message, ChatIdWaitRelease);
            Thread.Sleep(2000);
        }
    }
}
