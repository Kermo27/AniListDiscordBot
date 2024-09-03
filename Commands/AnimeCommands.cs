using AniListDiscordBot.Services;
using Discord;
using Discord.Commands;

namespace AniListDiscordBot.Commands;

public class AnimeCommands : ModuleBase<SocketCommandContext>
{
    private readonly AniListService _aniListService;
    private readonly DatabaseService _databaseService;

    public AnimeCommands(AniListService aniListService, DatabaseService dataBaseService)
    {
        _aniListService = aniListService;
        _databaseService = dataBaseService;
    }

    [Command("addanime")]
    [Summary("Adds an anime to your watchlist")]
    public async Task AddAnimeAsync([Remainder] string animeName)
    {
        try
        {
            var animeInfo = await _aniListService.GetAnimeInfoAsync(animeName);

            switch (animeInfo.Status)
            {
                case "FINISHED":
                    await ReplyAsync($"Anime '{animeInfo.Title}' jest już zakończone i nie może być dodane do listy obserwowanych.");
                    return;
                case "NOT_YET_RELEASED":
                    await ReplyAsync($"Anime '{animeInfo.Title}' nie jest rozpoczęte i nie może być dodane do listy obserwowanych.");
                    return;
            }

            var added = await _databaseService.AddAnimeToWatchlistAsync(Context.User.Id, animeInfo);
            if (added)
            {
                await ReplyAsync($"Dodano anime '{animeInfo.Title}' do twojej listy obserwowanych.");
            }
            else
            {
                await ReplyAsync($"Nie można dodać anime '{animeInfo.Title}' do listy obserwowanych.");
            }
        }
        catch (Exception ex)
        {
            await ReplyAsync($"Wystąpił błąd podczas dodawania anime {ex.Message}");
        }
    }

    [Command("myanime")]
    [Summary("Lists your watched anime")]
    public async Task ListAnimeAsync()
    {
        await _databaseService.RemoveFinishedAnimeAsync(Context.User.Id);
        
        var watchlist = await _databaseService.GetUserWatchlistAsync(Context.User.Id);
        if (watchlist.Any())
        {
            var embed = new EmbedBuilder()
                .WithTitle("\ud83c\udfac Twoja lista obserwowanych anime")
                .WithColor(Color.Blue)
                .WithFooter($"Łączna liczba anime: {watchlist.Count()}")
                .WithCurrentTimestamp();

            foreach (var anime in watchlist)
            {
                embed.AddField(
                    $"\ud83d\udcfa {anime.Title}",
                    $"Następny odcinek: `{anime.NextEpisodeAiringAt:dd.MM.yyyy HH:mm}`",
                    inline: false
                );
            }
            
            await ReplyAsync(embed: embed.Build());
        }
        else
        {
            await ReplyAsync("Twoja lista obserwowanych anime jest pusta.");
        }
    }

    [Command("removeanime")]
    [Summary("Removes an anime from your watchlist")]
    public async Task RemoveAnimeAsync([Remainder] string animeName)
    {
        try
        {
            var removed = await _databaseService.RemoveAnimeFromWatchlistAsync(Context.User.Id, animeName);
            if (removed)
            {
                await ReplyAsync($"Usunięto anime '{animeName}' z listy obserwowanych.");
            }
            else
            {
                await ReplyAsync($"Nie znaleziono anime '{animeName}' w twojej listy obserwowanych.");
            }
        }
        catch (Exception ex)
        {
            await ReplyAsync($"Wystąpił błąd podczas usuwania anime: {ex.Message}");
            throw;
        }
    }
}