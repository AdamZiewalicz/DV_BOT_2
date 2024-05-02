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
using BOT1.propositions;
using BOT1.commands;
using System.Diagnostics;
using DSharpPlus.Interactivity.Extensions;
using DV_BOT_2.commands;
using DSharpPlus.VoiceNext;
using Lavalink4NET;
using Lavalink4NET.Events.Players;
using DV_BOT_2;
using DV_BOT_2.customEvents;
using DSharpPlus.Entities;
using OpenAI_API;
using System.Dynamic;

var builder = new HostApplicationBuilder(args);

var jsonReader = new JSONReader();
await jsonReader.ReadJSON();

var discordConfig = new DiscordConfiguration()
{
    Intents = DiscordIntents.All,
    Token = jsonReader.Token,
    TokenType = TokenType.Bot,
    AutoReconnect = true,
};

globalVariables.api = new OpenAIAPI(jsonReader.GPTSecret);

// DSharpPlus
builder.Services.AddHostedService<ApplicationHost>();
builder.Services.AddSingleton<DiscordClient>();
builder.Services.AddSingleton(discordConfig); 

builder.Services.AddLavalink();

//configure lavalink
builder.Services.ConfigureLavalink(config => 
{
    config.BaseAddress = new Uri("https://lavalink4.alfari.id:443");
    config.Passphrase = "catfein";
    config.ResumptionOptions = new LavalinkSessionResumptionOptions(TimeSpan.FromSeconds(10));
});

builder.Build().Run();

public static class globalVariables
{
    //globally accessible for ease of use. dont care didnt ask + ur bald
    public static OpenAIAPI api;

    public static Members CurrentMembers;

    public static IServiceProvider serviceProviderGlobal;
    public static IAudioService audioServiceGlobal;
    public static DiscordClient discordClientGlobal;

    public static ulong GuildID = 1194760267075178657;

    private static Propositions propositions = new Propositions("propositions.json");
    public static Propositions Propositions { get => propositions; set=> propositions=value; }
}

file sealed class ApplicationHost : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private DiscordClient _discordClient;
    public ApplicationHost(IServiceProvider serviceProvider, DiscordClient discordClient)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(discordClient);

        //assign serviceProvider(containing e.g. audioservice) and discordClient
        _serviceProvider = serviceProvider;
        _discordClient = discordClient;

        //add those to global variables because i couldnt be bothered to fuck with the arguments shits just public idc
        globalVariables.serviceProviderGlobal = _serviceProvider;
        globalVariables.audioServiceGlobal = (IAudioService)_serviceProvider.GetService(typeof(IAudioService));
        globalVariables.discordClientGlobal = _discordClient;
        globalVariables.CurrentMembers = new Members();
        Console.WriteLine(globalVariables.CurrentMembers.ToString());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //get config (prefix+token)
        var jsonReader = new JSONReader();
        await jsonReader.ReadJSON();

        //create config for !commands
        var commandsConfig = new CommandsNextConfiguration()
        {
            StringPrefixes = new string[] { jsonReader.Prefix },
            EnableMentionPrefix = true,
            EnableDms = true,
            EnableDefaultHelp = false
        };

        //enable interactivity, voicenext
        _discordClient.UseInteractivity();
        _discordClient.UseVoiceNext();

        //register /commands
        _discordClient
            .UseSlashCommands(new SlashCommandsConfiguration { Services = _serviceProvider })
            .RegisterCommands<SlashCommands>(globalVariables.GuildID); 

        var prefixCommands = _discordClient.UseCommandsNext(commandsConfig);

        //register all !commands
        prefixCommands.RegisterCommands<AdminCommands>();
        prefixCommands.RegisterCommands<InfoCommands>();
        prefixCommands.RegisterCommands<ResponseCommands>();

        // connect to discord gateway and initialize node connection
        await _discordClient
            .ConnectAsync()
            .ConfigureAwait(false);

        var readyTaskCompletionSource = new TaskCompletionSource();

        //event catching
        _discordClient.Ready += SetResult;
        await readyTaskCompletionSource.Task.ConfigureAwait(false);
        _discordClient.Ready -= SetResult;
        _discordClient.MessageCreated += MessageCreatedHandler;
        globalVariables.audioServiceGlobal.TrackEnded += TrackEndedHandler;   

        //loop app
        await Task
            .Delay(Timeout.InfiniteTimeSpan, stoppingToken)
            .ConfigureAwait(false);

        //event handlers
        Task SetResult(DiscordClient client, ReadyEventArgs eventArgs)
        {
            readyTaskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        }
        Task MessageCreatedHandler(DiscordClient sender, MessageCreateEventArgs args)
        {
            if (args.Author != _discordClient.CurrentUser)
            {
                handlingResponses.HandleWednesday(sender,args);
            }
            return Task.CompletedTask;
        }
        Task TrackEndedHandler(object sender, TrackEndedEventArgs args) //event AsyncEventHandler<TrackEndedEventArgs>? TrackEnded;
        {
            if (args.Player.State == Lavalink4NET.Players.PlayerState.NotPlaying)
            { args.Player.DisconnectAsync(); }
            return Task.CompletedTask;
        }
    }
}