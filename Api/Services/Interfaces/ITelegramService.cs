using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot.Types;

namespace Api.Services.Interfaces
{
    public interface ITelegramService
    {
        Task HandleMessage(Update update, CancellationToken cancellationToken = default);
    }
}