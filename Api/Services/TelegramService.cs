using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Api.Services.Interfaces;

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

            if (update.Message.Text.Equals("/help"))
            {
                var message = $"- `/ShowAllDogs` to see overview on all dogs\n" + 
                    // $"- `/GetDogById` to see detail about dog with the specified id\n" +
                    // $"For example: `/GetDogById 9`\n" +
                    $"- `/Search` to search dogs by your request\n"+
                    $"For example `/Search cute dog`\n";

                await _telegramBotClient.SendTextMessageAsync(chatId, message, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, cancellationToken: cancellationToken);
                return;
            }

            if (update.Message.Text.Contains("/echo"))
            {
                if (update.Message.Text.Split(" ").Length < 2)
                {
                    await _telegramBotClient.SendTextMessageAsync(chatId, "...", cancellationToken: cancellationToken);
                    return;                    
                }
                
                await _telegramBotClient.SendTextMessageAsync(chatId, update.Message.Text.Split(" ", 2)[1], cancellationToken: cancellationToken);
                return;
            }

            if (update.Message.Text.Equals("/ShowAllDogs"))
            {
                var query = new GetAllDogsQuery { };
                var response = await _mediator.Send(query, cancellationToken);
                var result = response.Dogs;

                foreach (var dog in result)
                {
                    var message = $"*Name:* {dog.Name}\n" +
                        $"*Breed:* {dog.Breed}\n" +
                        $"*Size:* {dog.Size}\n" +
                        $"*Age:* {GetAge(dog.BirthDate)}\n" +
                        $"*About:* {dog.About}\n" +
                        $"*Row:* {dog.Row}\n" +
                        $"*Enclosure:* {dog.Enclosure}\n" +
                        $"*Last update:* {dog.LastUpdate}";                        

                    await _telegramBotClient.SendPhotoAsync(chatId,
                        photo: $"https://storage.googleapis.com/sirius_dogs_test/{dog.Id}/{dog.TitlePhoto}",
                        caption: message,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken);  
                }

                return;
            }

            if (update.Message.Text.Contains("/GetDogById"))
            {
                if (update.Message.Text.Split(" ").Length < 2)
                {
                    await _telegramBotClient.SendTextMessageAsync(chatId, "Wrong request", cancellationToken: cancellationToken);
                    return;
                }

                var dogId = int.Parse(update.Message.Text.Split(" ", 2)[1]);
                var query = new GetDogByIdQuery { DogId = dogId };
                var response = await _mediator.Send(query, cancellationToken);
                var dog = response.Dog;

                if (dog == null)
                {
                    await _telegramBotClient.SendTextMessageAsync(chatId, "Dog not found", cancellationToken: cancellationToken);
                    return;
                }

                var message = $"*Name:* {dog.Name}\n" +
                        $"*Breed:* {dog.Breed}\n" +
                        $"*Size:* {dog.Size}\n" +
                        $"*Age:* {GetAge(dog.BirthDate)}\n" +
                        $"*About:* {dog.About}\n" +
                        $"*Row:* {dog.Row}\n" +
                        $"*Enclosure:* {dog.Enclosure}\n" +
                        $"*Last update:* {dog.LastUpdate}";                    

                if (dog.Photos.Count != 0)
                {
                    List<InputMediaPhoto> media = new()
                    {
                        new InputMediaPhoto(new InputMedia($"https://storage.googleapis.com/sirius_dogs_test/{dog.Id}/{dog.TitlePhoto}"))
                    };

                    foreach (var photo in dog.Photos)
                    {
                        media.Add(new InputMediaPhoto(new InputMedia($"https://storage.googleapis.com/sirius_dogs_test/{dog.Id}/{photo.PhotoPath}")));
                    }

                    media[0].Caption = message;

                    var mediaGroupResponse = await _telegramBotClient.SendMediaGroupAsync(chatId,
                        media: media,
                        cancellationToken: cancellationToken);

                    await _telegramBotClient.EditMessageCaptionAsync(chatId,
                        messageId: mediaGroupResponse[0].MessageId,
                        caption: mediaGroupResponse[0].Caption,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                    
                    return;
                }

                await _telegramBotClient.SendPhotoAsync(chatId,
                    photo: $"https://storage.googleapis.com/sirius_dogs_test/{dog.Id}/{dog.TitlePhoto}",
                    caption: message,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: cancellationToken); 

                return;
            }

            if (update.Message.Text.Contains("/Search"))
            {
                if (update.Message.Text.Split(" ").Length < 2)
                {
                    await _telegramBotClient.SendTextMessageAsync(chatId, "Wrong request", cancellationToken: cancellationToken);
                    return;
                }

                var searchRequest = update.Message.Text.Split(" ", 2)[1];
                var query = new SearchDogQuery
                {
                    SearchRequest = searchRequest,
                    MaxAge = 1000,
                    Row = 0,
                    Enclosure = 0,
                    WentHome = false
                };
                var response = await _mediator.Send(query, cancellationToken);
                var dogs = response.Dogs;

                if (dogs.Count == 0)
                {
                    await _telegramBotClient.SendTextMessageAsync(chatId, "Dogs not found", cancellationToken: cancellationToken);
                    return;
                }
                
                foreach (var dog in dogs)
                {
                    var message = $"*Name:* {dog.Name}\n" +
                        $"*Breed:* {dog.Breed}\n" +
                        $"*Size:* {dog.Size}\n" +
                        $"*Age:* {GetAge(dog.BirthDate)}\n" +
                        $"*About:* {dog.About}\n" +
                        $"*Row:* {dog.Row}\n" +
                        $"*Enclosure:* {dog.Enclosure}\n" +
                        $"*Last update:* {dog.LastUpdate}";                    

                    if (dog.Photos.Count != 0)
                    {
                        List<InputMediaPhoto> media = new()
                        {
                            new InputMediaPhoto(new InputMedia($"https://storage.googleapis.com/sirius_dogs_test/{dog.Id}/{dog.TitlePhoto}"))
                        };

                        foreach (var photo in dog.Photos)
                        {
                            media.Add(new InputMediaPhoto(new InputMedia($"https://storage.googleapis.com/sirius_dogs_test/{dog.Id}/{photo.PhotoPath}")));
                        }

                        media[0].Caption = message;

                        var mediaGroupResponse = await _telegramBotClient.SendMediaGroupAsync(chatId,
                            media: media,
                            cancellationToken: cancellationToken);

                        await _telegramBotClient.EditMessageCaptionAsync(chatId,
                            messageId: mediaGroupResponse[0].MessageId,
                            caption: mediaGroupResponse[0].Caption,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                            cancellationToken: cancellationToken); 
                        
                        continue;
                    }

                    await _telegramBotClient.SendPhotoAsync(chatId,
                        photo: $"https://storage.googleapis.com/sirius_dogs_test/{dog.Id}/{dog.TitlePhoto}",
                        caption: message,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken);                
                }

                return;
            }
        }

    }

    private static string GetAge(DateTime birthDate)
    {
        birthDate = birthDate.ToLocalTime();
        if ((DateTime.Now - birthDate).Days > 365)
        {
            int years;
            if (DateTime.Now.DayOfYear > birthDate.DayOfYear)
            {
                years = DateTime.Now.Year - birthDate.Year;
                return IsPlural(years) ? $"{years} years" : $"{years} year";
            }
            years = DateTime.Now.Year - birthDate.Year - 1;
            return IsPlural(years) ? $"{years} years" : $"{years} year";
        }

        int months = ((DateTime.Now.Year - birthDate.Year) * 12) + DateTime.Now.Month - birthDate.Month;
        return IsPlural(months) ? $"{months} months" : $"{months} month";
    }

    private static bool IsPlural(int number)
    {
        return number % 100 == 11 || number % 10 != 1;
    }
}