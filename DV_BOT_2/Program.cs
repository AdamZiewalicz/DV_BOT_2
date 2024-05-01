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
using DV_BOT;
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


// DSharpPlus
builder.Services.AddHostedService<ApplicationHost>();
builder.Services.AddSingleton<DiscordClient>();
builder.Services.AddSingleton(discordConfig); 

builder.Services.AddLavalink();

builder.Services.ConfigureLavalink(config => 
{
    config.BaseAddress = new Uri("https://lava-v4.ajieblogs.eu.org:443");
    config.Passphrase = "https://dsc.gg/ajidevserver";
});

// Logging
builder.Services.AddLogging(s => s.AddConsole().SetMinimumLevel(LogLevel.Debug));

builder.Build().Run();

public static class globalVariables
{
    private static Propositions propositions;
    public static Propositions Propositions { get => propositions; set=> propositions=value; }
    static globalVariables()
    {
        Debug.WriteLine("global variables constructor called");
        propositions = new Propositions("propositions.json");
    }
}

file sealed class ApplicationHost : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private DiscordClient _discordClient;
    public ApplicationHost(IServiceProvider serviceProvider, DiscordClient discordClient)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(discordClient);

        _serviceProvider = serviceProvider;
        _discordClient = discordClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jsonReader = new JSONReader();
        await jsonReader.ReadJSON();
        var commandsConfig = new CommandsNextConfiguration()
        {
            StringPrefixes = new string[] { jsonReader.Prefix },
            EnableMentionPrefix = true,
            EnableDms = true,
            EnableDefaultHelp = false
        };

        _discordClient.UseInteractivity();

        _discordClient
            .UseSlashCommands(new SlashCommandsConfiguration { Services = _serviceProvider })
            .RegisterCommands<MusicCommands>(1194760267075178657); // Add guild id here
        var Commands = _discordClient.UseCommandsNext(commandsConfig);

        Commands.RegisterCommands<AdminCommands>();
        Commands.RegisterCommands<InfoCommands>();
        Commands.RegisterCommands<ResponseCommands>();

        // connect to discord gateway and initialize node connection
        await _discordClient
            .ConnectAsync()
            .ConfigureAwait(false);

        var readyTaskCompletionSource = new TaskCompletionSource();

        

        _discordClient.Ready += SetResult;
        await readyTaskCompletionSource.Task.ConfigureAwait(false);
        _discordClient.Ready -= SetResult;

        _discordClient.MessageCreated += MessageCreatedHandler;

        await Task
            .Delay(Timeout.InfiniteTimeSpan, stoppingToken)
            .ConfigureAwait(false);
        Task SetResult(DiscordClient client, ReadyEventArgs eventArgs)
        {
            readyTaskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        }
        Task MessageCreatedHandler(DiscordClient sender, MessageCreateEventArgs args)
        {
            if (args.Author != _discordClient.CurrentUser)
            {
                sillyResponses.HandleWednesday(args);
            }
            return Task.CompletedTask;
        }
    }
}