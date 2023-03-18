using ConsoleApp5;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Common.Utilities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.VoiceNext;
using dotenv.net;
using DisCatSharp.Net;
using DisCatSharp.Lavalink;

namespace ConsoleApp5;

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
        await discord.ConnectAsync();
        await lavalink.ConnectAsync(lavaConfig);
        await Task.Delay(-1);
    }

    private static Task MessageCreatedHandler(DiscordClient c, MessageCreateEventArgs e)
    {
        Console.WriteLine("MessageCreatedEvent: " + e.Message.Content);
        return default;
    }

    private static Task ReadyHandler(DiscordClient sender, ReadyEventArgs e)
    {
        Console.WriteLine("Logged in as: " + sender.CurrentUser.Username);
        // throw new NotImplementedException();
        return default;
    }
}