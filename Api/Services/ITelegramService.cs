using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

public interface ITelegramService
{
    Task HandleMessage(Update update, CancellationToken cancellationToken = default);
}