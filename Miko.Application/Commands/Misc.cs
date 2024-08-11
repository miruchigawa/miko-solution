namespace Miko.Application.Commands;

using Telegram.Bot;
using Telegram.Bot.Types;

using Miko.Application.Core;

public class MiscCommands 
{
    private readonly ITelegramBotClient _client;

    public MiscCommands(ITelegramBotClient client) => _client = client;

    [Command("/start")]
    [Description("Start conversation")]
    public async Task StartAsync(Message message)
    {
        await _client.SendTextMessageAsync(message.Chat.Id, "Welcome to @YunaYunaEverydayBot. Type /help to show all commands.");
    }

    [Command("/ping")]
    [Description("Replied pong.")]
    public async Task PingAsync(Message message)
    {
        await _client.SendTextMessageAsync(message.Chat.Id, "Pong.");
    }
}
