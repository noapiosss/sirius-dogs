using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Api.Services.Interfaces;

using Contracts.Database;

using Domain.Queries;

using MediatR;

using Telegram.Bot;
using Telegram.Bot.Types;

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
        if (update.Message == null)
        {
            return;
        }

        var chatId = update.Message.Chat.Id;

        if (update.Message.Text != null)
        {
            if (update.Message.Text.Equals("/start") || update.Message.Text.Equals("/help"))
            {
                var message = $"- `/search` to search dogs by your request\n"+
                    $"For example `/search cute dog`";

                await _telegramBotClient.SendTextMessageAsync(chatId,
                    message,
                    cancellationToken: cancellationToken);

                return;
            }

            if (update.Message.Text.ToLower().Contains("/echo"))
            {                
                await _telegramBotClient.SendTextMessageAsync(chatId,
                    update.Message.Text.Split(" ").Length < 2 ? "..." : update.Message.Text.Split(" ", 2)[1],
                    cancellationToken: cancellationToken);

                return;
            }

            if (update.Message.Text.ToLower().Equals("/showalldogs"))
            {
                await HandleMessageShowAllDogsAsync(chatId, cancellationToken);
                return;
            }

            if (update.Message.Text.ToLower().Contains("/getdogbyid"))
            {
                if (!int.TryParse(update.Message.Text.Split(" ", 2)[1], out int dogId))
                {
                    await _telegramBotClient.SendTextMessageAsync(chatId, "Wrong request", cancellationToken: cancellationToken);
                    return;
                }

                var query = new GetDogByIdQuery { DogId = dogId };
                var response = await _mediator.Send(query, cancellationToken);
                var dog = response.Dog;

                if (dog == null)
                {
                    await _telegramBotClient.SendTextMessageAsync(chatId, "Dog not found", cancellationToken: cancellationToken);
                    return;
                }

                var message = GetDogInfo(dog); 
                await SendDogMediaAsync(chatId, dog, message, cancellationToken);

                return;
            }


            if (update.Message.Text.ToLower().Equals("/search"))
            {
                var message = "To search dogs send message in this format:\n\n" +
                    "/search {your search request}";

                await _telegramBotClient.SendTextMessageAsync(chatId,
                    message,
                    cancellationToken: cancellationToken);

                return;
            }
            if (update.Message.Text.ToLower().Contains("/search"))
            {
                if (update.Message.Text.Split(" ").Length < 2)
                {
                    await _telegramBotClient.SendTextMessageAsync(chatId, "Wrong request", cancellationToken: cancellationToken);
                    return;
                }

                var searchRequest = update.Message.Text.Split(" ", 2)[1];
                var query = new TelegramSearchDogQuery
                {
                    SearchRequest = searchRequest
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
                    var message = GetDogInfo(dog);
                    await SendDogMediaAsync(chatId, dog, message, cancellationToken);
                }

                return;
            }
        }

    }

    

    private async Task HandleMessageShowAllDogsAsync(long chatId, CancellationToken cancellationToken)
    {
        var query = new GetDogsByWentHomeQuery()
        {
            WentHome = true,
            Page = 1
        };
        var response = await _mediator.Send(query, cancellationToken);
        var result = response.Dogs;

        foreach (var dog in result)
        {
            var message = GetDogInfo(dog);                        

            await _telegramBotClient.SendPhotoAsync(chatId,
                photo: $"https://storage.googleapis.com/sirius_dogs_test/{dog.Id}/{dog.TitlePhoto}",
                caption: message,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                cancellationToken: cancellationToken);  
        }
    }

    private async Task SendDogMediaAsync(long chatId, Dog dog, string message, CancellationToken cancellationToken)
    {
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

    private static string GetDogInfo(Dog dog)
    {
        return $"*Name:* {dog.Name}\n" +
            $"*Breed:* {dog.Breed}\n" +
            $"*Breed:* {dog.Gender}\n" +
            $"*Size:* {dog.Size}\n" +
            $"*Age:* {GetAge(dog.BirthDate)}\n" +
            $"*About:* {dog.About}\n" +
            $"*Row:* {dog.Row}\n" +
            $"*Enclosure:* {dog.Enclosure}\n" +
            $"*Last update:* {dog.LastUpdate}"; 
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