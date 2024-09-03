using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;

namespace AniListDiscordBot.Commands;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;

    public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services)
    {
        _client = client;
        _commands = commands;
        _services = services;
    }

    public async Task InstallCommandsAsync()
    {
        _client.MessageReceived += HandleCommandAsync;
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        if (!(messageParam is SocketUserMessage message)) return;

        var argPos = 0;
        if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
            message.Author.IsBot)
            return;

        var context = new SocketCommandContext(_client, message);
        var result = await _commands.ExecuteAsync(context, argPos, _services);

        if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
        {
            await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
        }
    }
}