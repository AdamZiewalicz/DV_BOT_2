using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DV_BOT.config;
using Microsoft.Extensions.Configuration;
using DSharpPlus.Net;
using DSharpPlus.CommandsNext;
using DV_BOT.messageHandlers;
using System.Diagnostics;
using DSharpPlus.Interactivity.Extensions;
using DV_BOT_2.commands;
using DSharpPlus.VoiceNext;
using Lavalink4NET;
using Lavalink4NET.Events.Players;
using DV_BOT_2;
using DSharpPlus.Entities;
using OpenAI_API;
using System.Dynamic;
using FakeYouNet;
using System.Text;
using Lavalink4NET.Rest;
using DV_BOT_2.guildInfo;

try
{
    var builder = new HostApplicationBuilder(args);

    var jsonReader = new JSONReader();
    await jsonReader.ReadJSON();

    ArgumentNullException.ThrowIfNull(jsonReader.Token);

    var discordConfig = new DiscordConfiguration()
    {
        Intents = DiscordIntents.All,
        Token = jsonReader.Token,
        TokenType = TokenType.Bot,
        AutoReconnect = true,
    };

    // DSharpPlus
    builder.Services.AddHostedService<ApplicationHost>();
    builder.Services.AddSingleton<DiscordClient>();
    builder.Services.AddSingleton(discordConfig);

    //configure lavalink - connection with YouTube
    builder.Services.AddLavalink();
    builder.Services.ConfigureLavalink(config =>
    {
        config.BaseAddress = new Uri("https://lavalink4.alfari.id:443");
        config.Passphrase = "catfein";
        config.ResumptionOptions = new LavalinkSessionResumptionOptions(TimeSpan.FromSeconds(10));
    });

    builder.Logging.SetMinimumLevel(LogLevel.Information);

    builder.Build().Run();
}
catch (Exception e)
{
    Console.WriteLine(e.Message); 
}
 Console.ReadKey();
