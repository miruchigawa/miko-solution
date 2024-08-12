namespace Miko.Application.Commands;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using Miko.Application.Core;
using Miko.Plugins.Anime;

public class FunCommands 
{
    private readonly ITelegramBotClient _client;
    private readonly AnimeSearchEngine animeService = new();

    public FunCommands(ITelegramBotClient client) => _client = client;

    [Command("/animesauce")]
    [Description("Find anime sauce from screenshot or image.")]
    public async Task AnimeSauceAsync(Message message)
    {
        if (message.Photo != null && message.Photo.Length > 0)
        {
            var photo = message.Photo[message.Photo.Length - 1];
            var file = await _client.GetFileAsync(photo.FileId);
            
            try
            {
                var result = await animeService.Search($"https://api.telegram.org/file/bot{Environment.GetEnvironmentVariable("TELEGRAM_TOKEN")}/{file.FilePath}");
                
                if (result.Result.Count > 0) 
                {
                    var highest = result.Result.OrderByDescending(r => r.Similarity).FirstOrDefault();
                    var caption =   $"📊 *Anime Sauce Result:*\n\n" +
                                    $"📁 *Filename:* {highest.Filename}\n" +
                                    $"📺 *Episode:* {highest.Episode?.ToString() ?? "-"}\n" +
                                    $"⏳ *From:* {highest.From}\n" +
                                    $"⏳ *To:* {highest.To}\n" +
                                    $"⭐ *Similarity:* {highest.Similarity:P2}\n" +
                                    $"🎥 *Video URL:* {highest.Video}\n" +
                                    $"🖼️ *Image URL:* {highest.Image}";

                    await _client.SendVideoAsync(message.Chat.Id, InputFile.FromString(highest.Video), caption: caption, parseMode: ParseMode.Markdown);
                }
                else 
                    await _client.SendTextMessageAsync(message.Chat.Id, "There no frame was found.");
            }
            catch (Exception except)
            {
                await _client.SendTextMessageAsync(chatId: message.Chat.Id, $"Failed to resolve: {except.Message}");
            }
        } 
        else 
            await _client.SendTextMessageAsync(chatId: message.Chat.Id, "Please input a photo on your message.");
    }
}
