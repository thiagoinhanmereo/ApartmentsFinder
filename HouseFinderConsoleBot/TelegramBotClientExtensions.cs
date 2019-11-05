using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HouseFinderConsoleBot.BotClient.Extensions
{
    public static class TelegramBotClientExtensions
    {
        public static Task<Message> SendApartmentMessages(this ITelegramBotClient botClient, string chatId, ApartmentInfo apartment)
        {
            return botClient.SendTextMessageAsync(
                       chatId: new ChatId(chatId),
                       text: $"*Rua*: {apartment.Rua},\n*Bairro*: {apartment.Bairro},\n*Área*: {apartment.Area} m²,\n*Aluguel*: {apartment.Aluguel.Replace("Aluguel ", "")},\n*Valor Total*: {apartment.Total.Replace("Total ", "")}",
                       parseMode: ParseMode.Markdown,
                       replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl(
                                 "Check this link:",
                                 apartment.Href
                               ))
                     );
        }

        public static Task SendInitialMessage(this ITelegramBotClient botClient, string chatId, List<ApartmentInfo> apartments)
        {
            if (apartments.Count == 1)
            {
                return botClient.SendTextMessageAsync(
                      chatId: new ChatId(chatId),
                      text: $"*Hello! I have a new apartment for you!*",
                      parseMode: ParseMode.Markdown
                    );
            }
            else if (apartments.Any())
            {
                return botClient.SendTextMessageAsync(
                      chatId: new ChatId(chatId),
                      text: $"*Hello! I have new apartments for you!*",
                      parseMode: ParseMode.Markdown
                    );
            }

            return Task.FromResult<object>(null);
        }
    }
}
