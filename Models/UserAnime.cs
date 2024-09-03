namespace AniListDiscordBot.Models;

public class UserAnime
{
    public int Id { get; set; }
    public ulong UserId { get; set; }
    public int AnimeId { get; set; }
    public string Title { get; set; }
    public DateTime NextEpisodeAiringAt { get; set; }
    public string Status { get; set; }
    public string Season { get; set; }
    public int? SeasonYear { get; set; }
}