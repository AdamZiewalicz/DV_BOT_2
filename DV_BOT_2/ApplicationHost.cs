using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using DV_BOT.messageHandlers;
using DV_BOT_2.commands;
using Lavalink4NET.Events.Players;
using Lavalink4NET;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DV_BOT.config;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;
using DV_BOT_2.guildInfo;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Players;
using Lavalink4NET.Events;


namespace DV_BOT_2
{
    sealed class ApplicationHost : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordClient _discordClient;
        private readonly IAudioService _audioService;
        public static Guilds? _guilds;
        public static FakeYouNet.Client? _fakeYouClient;
        public ApplicationHost(IServiceProvider serviceProvider, DiscordClient discordClient, IAudioService audioService)
        {
            _fakeYouClient = new FakeYouNet.Client();
            ArgumentNullException.ThrowIfNull(_fakeYouClient);
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(discordClient);
            ArgumentNullException.ThrowIfNull(audioService);

            //assign serviceProvider(containing e.g. audioservice) and discordClient
            _serviceProvider = serviceProvider;
            _discordClient = discordClient;
            _audioService = audioService;
            _guilds = null;
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //get config (prefix+token)
            var jsonReader = new JSONReader();
            await jsonReader.ReadJSON();

            ArgumentNullException.ThrowIfNull(jsonReader.Prefix);

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
                .RegisterCommands<SlashCommands>(1194760267075178657);

            var prefixCommands = _discordClient.UseCommandsNext(commandsConfig);

            //register all !commands
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
            _discordClient.GuildAvailable += GetGuilds;

            _audioService.TrackEnded += TrackEndedHandler;

            //loop app
            await Task
                .Delay(Timeout.InfiniteTimeSpan, stoppingToken)
                .ConfigureAwait(false);

            Task GetGuilds(DiscordClient client, GuildCreateEventArgs args)
            {
                _guilds = new Guilds(client.Guilds);
                return Task.CompletedTask;
            }
            //event handlers
            Task SetResult(DiscordClient client, ReadyEventArgs eventArgs)
            {
                readyTaskCompletionSource.TrySetResult();
                return Task.CompletedTask;
            }
            async Task TrackEndedHandler(object sender, TrackEndedEventArgs args)
            {
                if (_guilds != null)
                {
                    if (_guilds[args.Player.GuildId].PlaylistLoopOn)
                    {
                        if (_guilds[args.Player.GuildId].RemoveFromPlaylist == false)
                        {
                            await args.Player.PlayAsync(args.Track, cancellationToken: stoppingToken,properties: new TrackPlayProperties(StartPosition: TimeSpan.Zero)).ConfigureAwait(false);
                            await args.Player.SeekAsync(TimeSpan.Zero, stoppingToken);                                       
                        }
                        _guilds[args.Player.GuildId].RemoveFromPlaylist = false;
                        return;
                    }
                    if (_guilds[args.Player.GuildId].TrackLoopOn)
                    {
                        var LoopingTrack = _guilds[args.Player.GuildId].LoopingTrack;
                        if (LoopingTrack != null)
                        {
                            if (args.Track.Uri == LoopingTrack.Uri)
                            {
                                await args.Player.PlayAsync(args.Track, cancellationToken: stoppingToken).ConfigureAwait(false);
                                return;
                            }
                        }
                        
                    }
                }
                if (args.Player.State == PlayerState.NotPlaying)
                {
                    await args.Player.DisconnectAsync(stoppingToken);
                }
                return;
            }
        }
    }
}
