using System.Timers;
using Discord;
using Discord.WebSocket;
using Timer = System.Timers.Timer;

namespace AniListDiscordBot.Services;

public class NotificationService
{
    private readonly DiscordSocketClient _client;
    private readonly DatabaseService _databaseService;
    private readonly AniListService _aniListService;
    private readonly Timer _timer;
    private DateTime _lastCheckTime;
    private readonly ulong _channelId;
    
    public NotificationService(DiscordSocketClient client, DatabaseService databaseService, AniListService aniListService, ulong channelId)
    {
        _client = client;
        _databaseService = databaseService;
        _aniListService = aniListService;
        _channelId = channelId;
        _timer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds); // Check every minute
        _timer.Elapsed += CheckAndNotifyNewEpisodes;
        _lastCheckTime = DateTime.UtcNow;
        _timer.Start();
    }

    private async void CheckAndNotifyNewEpisodes(object sender, ElapsedEventArgs e)
    {
        var polandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        var nowInPoland = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        nowInPoland = TimeZoneInfo.ConvertTime(nowInPoland, polandTimeZone);
    
        var lastCheckTimeInPoland = DateTime.SpecifyKind(_lastCheckTime, DateTimeKind.Utc);
        lastCheckTimeInPoland = TimeZoneInfo.ConvertTime(lastCheckTimeInPoland, polandTimeZone);
        
        foreach (var guild in _client.Guilds)
        {
            var channel = guild.GetTextChannel(_channelId);
            if (channel == null) continue;
            
            foreach (var user in guild.Users)
            {
                var watchlist = await _databaseService.GetUserWatchlistAsync(user.Id);
                foreach (var anime in watchlist)
                {
                    var updatedAnimeInfo = await _aniListService.GetAnimeInfoAsync(anime.Title);
                    
                    if (anime.NextEpisodeAiringAt > lastCheckTimeInPoland && anime.NextEpisodeAiringAt <= nowInPoland)
                    {
                        var embed = new EmbedBuilder()
                            .WithTitle("Nowy odcinek dostępny!")
                            .WithDescription($"Nowy odcinek anime '{anime.Title}' jest już dostępny!")
                            .WithColor(Color.Green)
                            .WithCurrentTimestamp()
                            .WithImageUrl(anime.PosterUrl)
                            .Build();

                        await channel.SendMessageAsync(embed: embed);
                    }
                    
                    await _databaseService.UpdateNextEpisodeAiringAtAsync(anime.Id,
                        updatedAnimeInfo.NextEpisodeAiringAt);
                }
            }
        }

        _lastCheckTime = DateTime.UtcNow;
    }

    public void SetLastCheckTime(DateTime lastCheckTime)
    {
        _lastCheckTime = lastCheckTime;
    }
}