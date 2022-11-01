using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Http;
using Domain.Queries;

using MediatR;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Api.Services;
public class TelegramService : ITelegramService
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IMediator _mediator;
    private readonly string _baseURL = "https://1cf1-176-119-235-147.eu.ngrok.io";

    public TelegramService(ITelegramBotClient telegramBotClient, IMediator mediator)
    {
        _telegramBotClient = telegramBotClient;
        _mediator = mediator;
    }

    public async Task HandleMessage(Update update, CancellationToken cancellationToken = default)
    {
        var chatId = update.Message.Chat.Id;

        if (update.Message.Text != null)
        {
            if (update.Message.Text.Equals("/start"))
            {
                await _telegramBotClient.SendTextMessageAsync(chatId, "Welcome to our bot!", cancellationToken: cancellationToken);
                return;
            }

            if (update.Message.Text.Contains("/echo"))
            {
                await _telegramBotClient.SendTextMessageAsync(chatId, update.Message.Text.Split(" ", 2)[1], cancellationToken: cancellationToken);
                return;
            }

            if (update.Message.Text.Equals("/ShowAllDogs"))
            {
                var query = new GetAllDogsQuery{};
                var response = await _mediator.Send(query, cancellationToken);
                var result = response.Dogs;

                foreach (var dog in result)
                {
                    var message = $"Name: {dog.Name}\n" + 
                        $"Breed: {dog.Breed}\n" +
                        $"Size: {dog.Size}\n" +
                        $"Age: {dog.Age}\n" +
                        $"About: {dog.About}\n" +
                        $"Row: {dog.Row}\n" +
                        $"Enclosure: {dog.Enclosure}\n" +
                        $"Last update: {dog.LastUpdate}";

                    await using Stream stream = System.IO.File.OpenRead($"./wwwroot{dog.TitlePhoto}"); 
                    await _telegramBotClient.SendPhotoAsync(chatId,
                        photo: new InputOnlineFile(content: stream, fileName: "Title photo"),
                        caption: message);
                }
                return;
            }
        }
        
    }
}