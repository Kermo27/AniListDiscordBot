namespace AniListDiscordBot.Models;

public class AnimeInfo
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public DateTime NextEpisodeAiringAt { get; set; }
    public string PosterUrl { get; set; }
    public string Season { get; set; }
    public int? SeasonYear { get; set; }
}