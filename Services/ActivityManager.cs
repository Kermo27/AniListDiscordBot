using Discord;
using Discord.WebSocket;
using Timer = System.Timers.Timer;

namespace AniListDiscordBot.Services;

public class ActivityManager
{
    private readonly DiscordSocketClient _client;
    private readonly List<IActivity> _activities;
    private Timer _activityTimer;
    private readonly Random _random = new Random();

    public ActivityManager(DiscordSocketClient client, List<IActivity> activities)
    {
        _client = client;
        _activities = activities;
    }

    public async Task StartActivityCycleAsync()
    {
        _activityTimer = new Timer(300000); // 5 minutes
        _activityTimer.Elapsed += async (sender, e) => await ChangeActivityAsync();
        _activityTimer.Start();

        await ChangeActivityAsync();
    }

    private async Task ChangeActivityAsync()
    {
        if (_activities.Count == 0)
            return;

        var activity = _activities[_random.Next(_activities.Count)];

        await _client.SetActivityAsync(activity);
    }
}