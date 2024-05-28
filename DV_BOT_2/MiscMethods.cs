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
using DSharpPlus.Interactivity.Extensions;
using OpenAI_API.Images;
using OpenAI_API.Models;

namespace DV_BOT_2
{
    public class MiscMethods
    {
        private readonly IAudioService _audioService;
        private readonly DiscordClient _discordClient;
        public MiscMethods(IAudioService audioService, DiscordClient discordClient)
        {
            ArgumentNullException.ThrowIfNull(audioService);
            ArgumentNullException.ThrowIfNull(discordClient);

            _audioService = audioService;
            _discordClient = discordClient;
        }

        /*public static async Task SendMessageDM(DiscordClient sender, DiscordUser UserToSend, string UserPlayingName, string UserPlayingGame)
        {
            var guild = await sender.GetGuildAsync(globalVariables.GuildID);
            var user = await guild.GetMemberAsync(UserToSend.Id);

            string messageToSend = "Hi! "+UserPlayingName+" has booted up "+UserPlayingGame+"!";

            await user.SendMessageAsync(messageToSend);
            return;
        }*/
        public async ValueTask<QueuedLavalinkPlayer?> GetPlayerAsync(DiscordMember user, ulong guildID, bool connectToVoiceChannel = true)
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
                .RetrieveAsync(guildID, user.VoiceState.Channel.Id, playerFactory: PlayerFactory.Queued, Options.Create(playerOptions), retrieveOptions)
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
