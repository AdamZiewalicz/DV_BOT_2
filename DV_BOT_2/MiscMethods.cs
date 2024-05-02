using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Players;
using Microsoft.Extensions.Options;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using DSharpPlus;
using Lavalink4NET.Events.Players;

namespace DV_BOT_2
{
    public class MiscMethods
    {
        private readonly IAudioService _audioService;
        private readonly DiscordClient _discordClient;
        public MiscMethods(IAudioService audioService,DiscordClient discordClient)
        {
            ArgumentNullException.ThrowIfNull(audioService);
            ArgumentNullException.ThrowIfNull(discordClient);

            _audioService = audioService;
            _discordClient = discordClient;
        }

        public async Task PlayWednesday(MessageCreateEventArgs args)
        {
            DiscordMember user = (DiscordMember)args.Author;


            var player = await GetPlayerAsync(user, connectToVoiceChannel: true).ConfigureAwait(false);

            if (player is null)
            {
                return;
            }

            var track = await _audioService.Tracks
                .LoadTrackAsync("it doesnt feel like a wednesday", TrackSearchMode.YouTube)
                .ConfigureAwait(false);

            var position = await player
                .PlayAsync(track)
                .ConfigureAwait(false);

        }
        public async ValueTask<QueuedLavalinkPlayer?> GetPlayerAsync(DiscordMember user, bool connectToVoiceChannel = true)
        {
            ArgumentNullException.ThrowIfNull(user);

            var retrieveOptions = new PlayerRetrieveOptions(
                ChannelBehavior: connectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);

            var playerOptions = new QueuedLavalinkPlayerOptions { HistoryCapacity = 10000 };

            if (user.VoiceState == null)
            {
                Console.WriteLine("user not connected for wednesday handling");
                return null;
            }

            var result = await _audioService.Players
                .RetrieveAsync(globalVariables.GuildID, user.VoiceState.Channel.Id, playerFactory: PlayerFactory.Queued, Options.Create(playerOptions), retrieveOptions)
                .ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                var errorMessage = result.Status switch
                {
                    PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
                    PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
                    _ => "Unknown error.",
                };
                Console.WriteLine(errorMessage);
                return null;
            }

            return result.Player;
        }
    }
}
