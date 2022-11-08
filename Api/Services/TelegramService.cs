using System;
using System.Collections.Generic;
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

    public TelegramService(ITelegramBotClient telegramBotClient, IMediator mediator)
    {
        _telegramBotClient = telegramBotClient;
        _mediator = mediator;
    }

    public async Task HandleMessage(Update update, CancellationToken cancellationToken = default)
    {
        if (update.Message == null) return;
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
                        $"Age: {dog.BirthDate.Year}\n" +
                        $"About: {dog.About}\n" +
                        $"Row: {dog.Row}\n" +
                        $"Enclosure: {dog.Enclosure}\n" +
                        $"Last update: {dog.LastUpdate}";

                    using (Stream stream = System.IO.File.OpenRead($"./wwwroot{dog.TitlePhoto}"))
                    {                    
                        await _telegramBotClient.SendPhotoAsync(chatId,
                            photo: new InputOnlineFile(content: stream, fileName: "Title photo"),
                            caption: message);
                    }
                }
                return;
            }

            if (update.Message.Text.Contains("/GetDogById"))
            {
                var dogId = Int32.Parse(update.Message.Text.Split(" ", 2)[1]);
                var query = new GetDogByIdQuery{DogId = dogId};
                var response = await _mediator.Send(query, cancellationToken);
                var dog = response.Dog;
                
                var message = $"Name: {dog.Name}\n" + 
                    $"Breed: {dog.Breed}\n" +
                    $"Size: {dog.Size}\n" +
                    $"Age: {dog.BirthDate}\n" +
                    $"About: {dog.About}\n" +
                    $"Row: {dog.Row}\n" +
                    $"Enclosure: {dog.Enclosure}\n" +
                    $"Last update: {dog.LastUpdate}";


                List<Stream> streamList = new List<Stream>();
                streamList.Add(System.IO.File.OpenRead($"./wwwroot{dog.TitlePhoto}"));
                foreach (var photo in dog.Photos)
                {
                    streamList.Add(System.IO.File.OpenRead($"./wwwroot{photo.PhotoPath}"));
                }

                List<InputMediaPhoto> media = new List<InputMediaPhoto>();

                foreach (var stream in streamList)
                {
                    media.Add(new InputMediaPhoto(new InputMedia(stream, stream.GetHashCode().ToString())));
                }

                media[0].Caption = message;
                
                await _telegramBotClient.SendMediaGroupAsync(chatId,
                    media: media,
                    cancellationToken: cancellationToken);

                foreach(var stream in streamList)
                {
                    stream.Close();
                }

                return;
            }

            if (update.Message.Text.Contains("/Search"))
            {
                var searchRequest = update.Message.Text.Split(" ", 2)[1];
                var query = new SearchGodQuery{SearchRequest = searchRequest};
                var response = await _mediator.Send(query, cancellationToken);
                var dogs = response.Dogs;
                if (dogs.Count == 0)
                {
                    await _telegramBotClient.SendTextMessageAsync(chatId, "Dogs not found", cancellationToken: cancellationToken);
                    return;
                }
                
                foreach (var dog in dogs)
                {
                    var message = $"Name: {dog.Name}\n" + 
                        $"Breed: {dog.Breed}\n" +
                        $"Size: {dog.Size}\n" +
                        $"Age: {dog.BirthDate}\n" +
                        $"About: {dog.About}\n" +
                        $"Row: {dog.Row}\n" +
                        $"Enclosure: {dog.Enclosure}\n" +
                        $"Last update: {dog.LastUpdate}";


                    List<Stream> streamList = new List<Stream>();
                    streamList.Add(System.IO.File.OpenRead($"./wwwroot{dog.TitlePhoto}"));
                    foreach (var photo in dog.Photos)
                    {
                        streamList.Add(System.IO.File.OpenRead($"./wwwroot{photo.PhotoPath}"));
                    }

                    List<InputMediaPhoto> media = new List<InputMediaPhoto>();

                    foreach (var stream in streamList)
                    {
                        media.Add(new InputMediaPhoto(new InputMedia(stream, stream.GetHashCode().ToString())));
                    }

                    media[0].Caption = message;
                    
                    await _telegramBotClient.SendMediaGroupAsync(chatId,
                        media: media,
                        cancellationToken: cancellationToken);

                    foreach(var stream in streamList)
                    {
                        stream.Close();
                    }
                }

                return;
            }
        }
        
    }
}