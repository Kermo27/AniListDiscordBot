using AniListDiscordBot.Models;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

namespace AniListDiscordBot.Services;

public class AniListService
{
    private readonly GraphQLHttpClient _client;

    public AniListService()
    {
        _client = new GraphQLHttpClient("https://graphql.anilist.co", new NewtonsoftJsonSerializer());
    }

    public async Task<AnimeInfo> GetAnimeInfoAsync(string animeName)
    {
        var query = new GraphQLRequest
        {
            Query = @"
                query ($search: String) {
                    Media (search: $search, type: ANIME) {
                        id
                        title {
                            romaji
                        }
                        status
                        nextAiringEpisode {
                            airingAt
                            episode
                        }
                        coverImage {
                            large
                        }
                        season
                        seasonYear
                    }
                }",
            Variables = new { search = animeName }
        };

        var response = await _client.SendQueryAsync<AniListResponse>(query);

        if (response.Errors != null && response.Errors.Length != 0)
        {
            throw new Exception($"Error from AniList API: {string.Join(", ", response.Errors.Select(e => e.Message))}");
        }

        var media = response.Data.Media;

        var polandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

        DateTime nextEpisodeAiringAt;
        if (media.NextAiringEpisode?.AiringAt.HasValue == true)
        {
            nextEpisodeAiringAt =
                DateTimeOffset.FromUnixTimeSeconds(media.NextAiringEpisode.AiringAt.Value).UtcDateTime;
            nextEpisodeAiringAt = DateTime.SpecifyKind(nextEpisodeAiringAt, DateTimeKind.Utc);
            nextEpisodeAiringAt = TimeZoneInfo.ConvertTime(nextEpisodeAiringAt, polandTimeZone);
        }
        else
        {
            nextEpisodeAiringAt = DateTime.MaxValue;
        }

        return new AnimeInfo
        {
            Id = media.Id,
            Title = media.Title.Romaji,
            Status = media.Status,
            NextEpisodeAiringAt = nextEpisodeAiringAt,
            Episode = media.NextAiringEpisode.Episode,
            PosterUrl = media.CoverImage.Large,
            Season = media.Season,
            SeasonYear = media.SeasonYear
        };
    }
}