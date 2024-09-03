using System.Text.Json;
using System.Timers;
using AniListDiscordBot.Commands;
using AniListDiscordBot.Models;
using AniListDiscordBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Timer = System.Timers.Timer;

namespace AniListDiscordBot;

public class AniListDiscordBot
{
    private DiscordSocketClient _client;
    private CommandService _commands;
    private IServiceProvider _services;
    private Timer _activityTimer;

    private readonly List<IActivity> _activities = new List<IActivity>
    {
        new Game("Shikanoko Nokonoko Koshitantan", ActivityType.Watching),
        new Game("Tokidoki Bosotto Russiago de Dereru Tonari no Alya-san", ActivityType.Watching),
        new Game("Sword Art Online", ActivityType.Watching),
        new Game("Mid:Zero kara Hajimeru Isekai Seikatsu", ActivityType.Watching),
        new StreamingGame("kermo____", "https://twitch.tv/kermo____"),
    };
    private int _currentActivityIndex = 0;

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

        _services = ConfigureServices();

        _client.Log += LogAsync;
        _commands.Log += LogAsync;
        _client.Ready += ClientReadyASync;

        var config = LoadConfig("config.json");
        
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

    private async Task ClientReadyASync()
    {
        Console.WriteLine($"{_client.CurrentUser} is connected!");

        _activityTimer = new Timer(300000); // 5 minutes 
        _activityTimer.Elapsed += ChangeActivity;
        _activityTimer.Start();

        await ChangeActivityAsync();
    }

    private async void ChangeActivity(object sender, ElapsedEventArgs e)
    {
        await ChangeActivityAsync();
    }

    private async Task ChangeActivityAsync()
    {
        var activity = _activities[_currentActivityIndex];

        await _client.SetActivityAsync(activity);

        _currentActivityIndex = (_currentActivityIndex + 1) % _activities.Count;
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