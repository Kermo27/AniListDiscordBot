﻿namespace AniListDiscordBot
{
    public class Program
    {
        public static async Task Main(string[] args) 
            => await new AnimeBot().RunAsync();
    }
}