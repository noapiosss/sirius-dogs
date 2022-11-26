using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Telegram.Bot.Types;

namespace Api.Controllers
{
    public class BotController : ControllerBase
    {
        private readonly ITelegramService _telegramService;

        public BotController(ITelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        [HttpPost("/api/bot")]
        public async Task<IActionResult> Post([FromBody] Update update, CancellationToken cancellationToken = default)
        {
            await _telegramService.HandleMessage(update, cancellationToken);
            return Ok();
        }
    }
}