using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.VoiceNext;
using dotenv.net;
using DisCatSharp.Net;
using DisCatSharp.Lavalink;

namespace YukiMusicCSharp;

public class main
{
    public static void Main(String[] args)
    {
        MainAsync().GetAwaiter().GetResult();
    }

    public static async Task MainAsync()
    {
        DotEnv.Load();
        
        var discord = new DiscordClient(new DiscordConfiguration()
        {
            Token = DotEnv.Read()["DISCORD_TOKEN"],
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All
        });
        discord.UseVoiceNext();
        var lavaEndpoint = new ConnectionEndpoint
        {
            Hostname = "bot-2.risaton.net",
            Port = 2333,
        };
        var lavaConfig = new LavalinkConfiguration
        {
            Password = "youshallnotpass",
            RestEndpoint = lavaEndpoint,
            SocketEndpoint = lavaEndpoint
        };
        var lavalink = discord.UseLavalink();
        var appCommands = discord.UseApplicationCommands();
        appCommands.RegisterGuildCommands<MyCommand>(977138017095520256);
        discord.Ready += ReadyHandler;
        discord.MessageCreated += MessageCreatedHandler;
        discord.VoiceStateUpdated += VoiceStateUpdatedHandler;
        
        await discord.ConnectAsync();
        await lavalink.ConnectAsync(lavaConfig);
        await Task.Delay(-1);
    }

    private static async Task VoiceStateUpdatedHandler(DiscordClient client, VoiceStateUpdateEventArgs @event)
    {
        if (@event.Channel.Users.Count == 1)
        {
            try
            {
                MusicQueue<string>.Instance.Clear(@event.Guild.Id);
                // queue.GuildQueueDictionary[ctx.Guild.Id.ToString()] = q;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        
            var lava = client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(@event.Guild);
            await conn.DisconnectAsync();
            var vcnext = client.GetVoiceNext();
            var connection = vcnext.GetConnection(@event.Guild);
            connection.Disconnect();
        }
    }

    private static Task MessageCreatedHandler(DiscordClient c, MessageCreateEventArgs e)
    {
        Console.WriteLine("MessageCreatedEvent: " + e.Message.Content);
        return default;
    }

    private static async Task ReadyHandler(DiscordClient sender, ReadyEventArgs e)
    {
        Console.WriteLine("Logged in as: " + sender.CurrentUser.Username);
        foreach (var x in sender.Guilds)
        {
            // Console.WriteLine(x.Value.Name);
            var guild = await sender.TryGetGuildAsync(x.Value.Id);
            Console.WriteLine(guild.Name);
        }
        Console.WriteLine(sender.Guilds.Count);
        // throw new NotImplementedException();
    }
}