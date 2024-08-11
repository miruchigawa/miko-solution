﻿using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;

using Miko.Application.Core;

internal class Program
{
    private static CommandDispatcher? _commands;

    public static async Task Main()
    {
        var client = new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_TOKEN") ?? "");
        _commands = new(client);

        try 
        {
            var me = await client.GetMeAsync();
            Console.WriteLine($"Client connected as {me.Username}");
        }
        catch (Exception except)
        {
            Console.WriteLine($"Failed to run client: {except}");
            Environment.Exit(1);
        }

        await _commands.InitAsync(); 

        client.StartReceiving(HandleUpdateAsync, HandleErrorAsync, new() { AllowedUpdates = {} });

        await Task.Delay(-1);
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancel)
    {
        if (update.Type == UpdateType.Message && (update.Message?.Text != null || update.Message?.Caption != null))
        {
            await _commands!.ExecuteCommand(update.Message!);
        }
    }

    private static Task HandleErrorAsync(ITelegramBotClient client, Exception except, CancellationToken cancel)
    {
        return Task.CompletedTask;
    }
}
