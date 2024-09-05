using AniListDiscordBot.Models;
using Microsoft.EntityFrameworkCore;

namespace AniListDiscordBot.Services;

public class DatabaseService
{
    private readonly AnimeDbContext _context;

    public DatabaseService()
    {
        _context = new AnimeDbContext();
        _context.Database.EnsureCreated();
    }

    public async Task<bool> AddAnimeToWatchlistAsync(ulong userId, AnimeInfo animeInfo)
    {
        if (animeInfo.Status == "FINISHED")
        {
            return false;
        }
        
        var utcNextEpisode = TimeZoneUtils.ToUtc(animeInfo.NextEpisodeAiringAt);
        
        var userAnime = new UserAnime
        {
            UserId = userId,
            AnimeId = animeInfo.Id,
            Title = animeInfo.Title,
            NextEpisodeAiringAt = utcNextEpisode,
            Episode = animeInfo.Episode,
            PosterUrl = animeInfo.PosterUrl,
            Status = animeInfo.Status,
            Season = animeInfo.Season,
            SeasonYear = animeInfo.SeasonYear
        };

        await _context.UserAnimes.AddAsync(userAnime);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<AnimeInfo>> GetUserWatchlistAsync(ulong userId)
    {
        return await _context.UserAnimes
            .Where(ua => ua.UserId == userId)
            .Select(ua => new AnimeInfo
            {
                Id = ua.AnimeId,
                Title = ua.Title,
                NextEpisodeAiringAt = TimeZoneUtils.ToPolandTime(ua.NextEpisodeAiringAt),
                Status = ua.Status,
                Season = ua.Season,
                SeasonYear = ua.SeasonYear
            })
            .ToListAsync();
    }

    public async Task RemoveFinishedAnimeAsync(ulong userId)
    {
        var finishedAnime = await _context.UserAnimes
            .Where(ua => ua.UserId == userId && ua.NextEpisodeAiringAt <= DateTime.UtcNow)
            .ToListAsync();
        
        _context.UserAnimes.RemoveRange(finishedAnime);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateNextEpisodeAiringAtAsync(int animeId, DateTime nextAiringAt)
    {
        var utcAiringAt = TimeZoneUtils.ToUtc(nextAiringAt);
    
        var animes = await _context.UserAnimes
            .Where(ua => ua.AnimeId == animeId)
            .ToListAsync();

        foreach (var anime in animes)
        {
            anime.NextEpisodeAiringAt = utcAiringAt;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> RemoveAnimeFromWatchlistAsync(ulong userId, string animeName)
    {
        var animeToRemove = await _context.UserAnimes
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.Title.ToLower() == animeName.ToLower());

        if (animeToRemove == null)
        {
            return false;
        }
        
        _context.UserAnimes.Remove(animeToRemove);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SetLastShutdownTimeAsync(DateTime shutdownTime)
    {
        var config = await _context.Configurations.FirstOrDefaultAsync(c => c.Key == "LastShutdownTime");
        if (config == null)
        {
            config = new Configuration { Key = "LastShutdownTime", Value = shutdownTime.ToString("O") };
            _context.Configurations.Add(config);
        }
        else
        {
            config.Value = shutdownTime.ToString("O");
        }

        await _context.SaveChangesAsync();
    }
    
    public async Task<DateTime> GetLastShutdownTimeAsync()
    {
        var config = await _context.Configurations.FirstOrDefaultAsync(c => c.Key == "LastShutdownTime");
        return config == null 
            ? TimeZoneUtils.ToPolandTime(DateTime.UtcNow.AddYears(-1))
            : TimeZoneUtils.ToPolandTime(DateTime.Parse(config.Value));
    }
}