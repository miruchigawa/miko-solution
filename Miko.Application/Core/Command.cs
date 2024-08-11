namespace Miko.Application.Core;

using System.Reflection;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

/// <summary>
/// Setup method as command 
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
    /// <summary> Name of command </summary>
    public string Name { get; }
    /// <summary> Initialize command </summary>
    /// <param name="name">Name of command </param>
    public CommandAttribute(string name) => Name = name;
}

/// <summary>
/// Set descriptiom of command 
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class DescriptionAttribute : Attribute 
{
    /// <summary>Description text</summary>
    public string Description { get; }
    /// <summary>Initialize description</summary>
    /// <param name="description">Description value</param>
    public DescriptionAttribute(string description) => Description = description;
}

public struct CommandProperty
{
    public string Name { get; }
    public string Description { get; }
    public Func<Message, Task> Run { get; }

    public CommandProperty(string name, string description, Func<Message, Task> run)
    {
        Name = name;
        Description = description;
        Run = run;
    }
}

public class CommandDispatcher
{
    private readonly ITelegramBotClient _client;
    private readonly Dictionary<string, CommandProperty> _commands = new();

    public CommandDispatcher(ITelegramBotClient client) => _client = client; 

    /// <summary>
    /// Register command from assembly 
    /// </summary>
    public async Task InitAsync()
    {
        await Task.Run(() => 
        {
            var methods = Assembly.GetExecutingAssembly().GetTypes().SelectMany(t => t.GetMethods()).Where(m => m.GetCustomAttribute<CommandAttribute>() != null);

            foreach (var method in methods)
            {
                var commandAttribute = method.GetCustomAttribute<CommandAttribute>()!;
                var descriptionAttribute = method.GetCustomAttribute<DescriptionAttribute>();
                var instance = Activator.CreateInstance(method.DeclaringType!, _client);
                var runFunc = (Func<Message, Task>)Delegate.CreateDelegate(typeof(Func<Message, Task>), instance, method);
                
                _commands.Add(commandAttribute.Name, new(commandAttribute.Name, descriptionAttribute?.Description ?? "No Info", runFunc));
            }
        });
    }

    /// <summary>
    /// Execute command on mentioned string
    /// </summary>
    /// <param name="message">Message content (Make sure text or caption is not empty)</param>
    public async Task ExecuteCommand(Message message)
    {
        // TODO: This may will be not working on slashreply or on groups, so add parser for parse string ex: /start@YunaBot 
        if (message.Text is null && message.Caption is null) return;

        var name = (message.Text?.Split(' ') ?? message.Caption?.Split(' ') ?? [])[0];

        if (name == "/help") 
        {
            var caption = "*💬 List All Command*\n";

            foreach (var command in _commands)
            {
                caption += $"\n*{command.Key}* => {command.Value.Description}";
            }

            await _client.SendTextMessageAsync(message.Chat.Id, caption, parseMode: ParseMode.Markdown);
        }
        else if (_commands.TryGetValue(name, out var command))
        {
            await command.Run(message);
        }
        else
        {
            await _client.SendTextMessageAsync(message.Chat.Id, $"Command {name} not found.");
        }
    }
}
