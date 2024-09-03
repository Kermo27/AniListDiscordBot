namespace AniListDiscordBot.Models;

public class AniListMedia
{
    public int Id { get; set; }
    public AniListTitle Title { get; set; }
    public string Status { get; set; }
    public AniListNextAiringEpisode NextAiringEpisode { get; set; }
    public CoverImage CoverImage { get; set; }
    public string Season { get; set; }
    public int? SeasonYear { get; set; }
}