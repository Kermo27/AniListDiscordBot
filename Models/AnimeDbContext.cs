using Microsoft.EntityFrameworkCore;

namespace AniListDiscordBot.Models;

public class AnimeDbContext : DbContext
{
    public DbSet<UserAnime> UserAnimes { get; set; }
    public DbSet<Configuration> Configurations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("Data Source=animetracker.db");
}