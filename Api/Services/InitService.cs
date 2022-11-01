using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Api.Services;

public class InitService : IHostedService
{
    private ITelegramBotClient _telegramBotClient;
    private readonly IOptionsMonitor<BotConfiguration> _botConfiguration;
    public InitService (ITelegramBotClient telegramBotClient, IOptionsMonitor<BotConfiguration> botConfiguration)
    {
        _telegramBotClient = telegramBotClient;
        _botConfiguration = botConfiguration;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var domain = _botConfiguration.CurrentValue.Domain;
        var token = _botConfiguration.CurrentValue.BotAccessToken;

        // return _telegramBotClient.SetWebhookAsync($"{domain}/bot/{token}",
        return _telegramBotClient.SetWebhookAsync($"{domain}/api/bot",
            allowedUpdates: Enumerable.Empty<UpdateType>(),
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _telegramBotClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
    }
}