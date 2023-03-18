using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Common.Utilities;
using DisCatSharp.Lavalink.EventArgs;

namespace YukiMusicCSharp;
public class MyCommand : ApplicationCommandsModule
{

    [SlashCommand("join", "Connect to VC.")]
    public async Task commandJoin(InteractionContext context)
    {
        Console.WriteLine("join hit.");
        var channel = context.Member.VoiceState.Channel;
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        await node.ConnectAsync(channel);
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Connected."));
    }

    [SlashCommand("play", "音楽を再生する。")]
    public async Task play(InteractionContext ctx, [Option("url", "URLまたはタイトル")] string url)
    {
        // var q = queue.GuildQueueDictionary[ctx.Guild.Id.ToString()];
        
        // if (queue.GuildQueueDictionary.ContainsKey(ctx.Guild.Id.ToString()))
        // {
            // q = queue.GuildQueueDictionary[ctx.Guild.Id.ToString()];
        // }
        // else
        // {
            // q = new MusicQueue<string>();
            // queue.GuildQueueDictionary[ctx.Guild.Id.ToString()] = q;
        // }
        Console.WriteLine("queue count: " + MusicQueue<string>.Instance.Size(ctx.Guild.Id));
        Console.WriteLine("queue list:");
        foreach (var x in MusicQueue<string>.Instance.ToDictionary())
        {
            Console.WriteLine(x);
        }
        var channel = ctx.Member.VoiceState.Channel;
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        await node.ConnectAsync(channel);

        var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

        var loadResult = await node.Rest.GetTracksAsync(url);
        if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed ||
            loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
        {
            await ctx.Channel.SendMessageAsync("該当なし.");
            return;
        }

        var track = loadResult.Tracks.First();
        // q.Enqueue(track.Uri.ToString());
        MusicQueue<string>.Instance.Enqueue(ctx.Guild.Id,track.Uri.ToString());
        await ctx.Channel.SendMessageAsync($"キューに追加されました: {track.Title}");
        await conn.SetVolumeAsync(3);
        if (MusicQueue<string>.Instance.Size(ctx.Guild.Id) == 1)
        {
            await conn.PlayAsync(track);
        }
        conn.PlaybackFinished += ConnOnPlaybackFinished;
        // PlayQueue(conn).GetAwaiter();
        // if (!main._isPlaying)
        // {
        // main._isPlaying = true;
        // await PlayQueue(conn);
        // }
    }

    private async Task ConnOnPlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
    {
        // var q = queue.GuildQueueDictionary[sender.Guild.Id.ToString()];
        if (MusicQueue<string>.Instance.Size(sender.Guild.Id) != 0)
        {
            var url = MusicQueue<string>.Instance.Dequeue(sender.Guild.Id);
            var loadresult = await sender.GetTracksAsync(url);
            await sender.PlayAsync(loadresult.Tracks.First());
        }
    }

    [SlashCommand("leave", "BOTを退出させる。")]
    public async Task leave(InteractionContext ctx)
    {
        // var q = queue.GuildQueueDictionary[ctx.Guild.Id.ToString()];
        try
        {
            MusicQueue<string>.Instance.Clear(ctx.Guild.Id);
            // queue.GuildQueueDictionary[ctx.Guild.Id.ToString()] = q;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
        
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
        await conn.DisconnectAsync();
        // await ctx.Member.VoiceState.Channel.ConnectAsync()
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Disconnecting..."));
        
    }

    [SlashCommand("stop", "再生を止める")]
    public async Task stop(InteractionContext ctx)
    {
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
        await conn.StopAsync();
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("停止しています..."));
    }

    // private async Task PlayQueue(LavalinkGuildConnection conn)
    // {
    //     while (queue.TrackQueue.Count > 0)
    //     {
    //         var uri = queue.TrackQueue.Dequeue();
    //         var loadResult = await conn.Node.Rest.GetTracksAsync(uri);
    //         if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed ||
    //             loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
    //         {
    //             await conn.Channel.SendMessageAsync($"トラックの読み込みに失敗しました: {uri}");
    //             continue;
    //         }
    //
    //         var track = loadResult.Tracks.First();
    //         await conn.PlayAsync(track);
    //         await conn.Channel.SendMessageAsync($"再生中: {track.Title}");
    //
    //         // If the queue is empty, stop playing
    //         // if (main._trackQueue.Count == 0)
    //         // {
    //             // main._isPlaying = false;
    //             // break;
    //         // }
    //     }
    // }
    

}
