using System.Collections;
using System.Diagnostics;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.VoiceNext;

namespace ConsoleApp5;

public class MyCommand : ApplicationCommandsModule
{
    private Queue<LavalinkTrack> q = new Queue<LavalinkTrack>();
    private LavalinkGuildConnection _currentConnection = null;
    [SlashCommand("join", "Connect to VC.")]
    public async Task commandJoin(InteractionContext context)
    {
        Console.WriteLine("join hit.");
        // var vc = context.User

        var channel = context.Member.VoiceState.Channel;
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        await node.ConnectAsync(channel);
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Connected."));
        // await channel.ConnectAsync();
    }

    [SlashCommand("play", "音楽を再生する。")]
    public async Task Play(InteractionContext ctx, [Option("url", "URLまたはタイトル")] string url)
    {
        var channel = ctx.Member.VoiceState.Channel;
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        await node.ConnectAsync(channel);

        var loadResult = await node.Rest.GetTracksAsync(url);
        if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed ||
            loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
        {
            await ctx.Channel.SendMessageAsync("該当なし.");
            return;
        }

        var track = loadResult.Tracks.First();
        q.Enqueue(track);

        if (_currentConnection == null || !_currentConnection.IsConnected)
        {
            _currentConnection = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            await PlayNextTrack();
        }

        await ctx.Channel.SendMessageAsync($"キューに追加された曲: {track.Title}");
    }

    [SlashCommand("leave", "BOTを退出させる。")]
    public async Task leave(InteractionContext ctx)
    {
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(ctx.Guild);
        await conn.DisconnectAsync();
        // await ctx.Member.VoiceState.Channel
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Disconnecting..."));
    }
    private async Task PlayNextTrack()
    {
        if (q.Count == 0) return;

        var track = q.Dequeue();
        await _currentConnection.PlayAsync(track);
        await _currentConnection.WaitForPlaybackFinishAsync();
        

        await PlayNextTrack();
    }

}