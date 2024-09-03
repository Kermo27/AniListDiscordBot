using System.Text.Json;
using AniListDiscordBot.Commands;
using AniListDiscordBot.Models;
using AniListDiscordBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace AniListDiscordBot;

public class AniListDiscordBot
{
    private DiscordSocketClient _client;
    private CommandService _commands;
    private IServiceProvider _services;
    private ActivityManager _activityManager;

    public async Task RunAsync()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Debug,
            MessageCacheSize = 1000,
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.GuildMessages |
                             GatewayIntents.MessageContent
        });

        _commands = new CommandService(new CommandServiceConfig
        {
            LogLevel = LogSeverity.Debug,
            CaseSensitiveCommands = false
        });

        var config = LoadConfig("config.json");
        _activityManager = new ActivityManager(_client, new List<IActivity>
        {
            new Game("Shikanoko Nokonoko Koshitantan", ActivityType.Watching),
            new Game("Tokidoki Bosotto Russiago de Dereru Tonari no Alya-san", ActivityType.Watching),
            new Game("Sword Art Online", ActivityType.Watching),
            new Game("Mid:Zero kara Hajimeru Isekai Seikatsu", ActivityType.Watching),
            new StreamingGame("kermo____", "https://twitch.tv/kermo____"),
        });
        
        _services = ConfigureServices();
        
        _client.Log += LogAsync;
        _commands.Log += LogAsync;
        _client.Ready += async () =>
        {
            await _activityManager.StartActivityCycleAsync();
            Console.WriteLine($"{_client.CurrentUser} is connected!");
        };
        
        await _client.LoginAsync(TokenType.Bot, config.Token);
        await _client.StartAsync();

        await _services.GetRequiredService<CommandHandler>().InstallCommandsAsync();
        
        var databaseService = _services.GetRequiredService<DatabaseService>();
        var notificationService = _services.GetRequiredService<NotificationService>();
        var lastShutdownTime = await databaseService.GetLastShutdownTimeAsync();
        notificationService.SetLastCheckTime(lastShutdownTime);

        AppDomain.CurrentDomain.ProcessExit += async (s, e) =>
        {
            await databaseService.SetLastShutdownTimeAsync(DateTime.UtcNow);
        };

        await Task.Delay(-1);
    }

    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private static BotConfig LoadConfig(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<BotConfig>(json);
    }

    private ServiceProvider ConfigureServices()
    {
        var config = LoadConfig("config.json");
        
        return new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton<CommandHandler>()
            .AddSingleton<HttpClient>()
            .AddSingleton<AniListService>()
            .AddSingleton<DatabaseService>()
            .AddSingleton<NotificationService>(provider => 
                new NotificationService(_client,
                    provider.GetRequiredService<DatabaseService>(),
                    provider.GetRequiredService<AniListService>(),
                    config.ChannelId))
            .BuildServiceProvider();
    }
}