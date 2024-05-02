using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using DSharpPlus.VoiceNext;
using System.Diagnostics;
using NAudio.Wave;
using NAudio.Utils;
using System.Reflection.PortableExecutable;
using System.Speech.AudioFormat;
using Concentus;
using Concentus.Enums;
using System.Threading;
using System.Globalization;


namespace DV_BOT_2.commands
{
    public class SlashCommands : ApplicationCommandModule
    {
        private readonly IAudioService _audioService;
        
        public SlashCommands(IAudioService audioService)
        {
            ArgumentNullException.ThrowIfNull(audioService);

            _audioService = audioService;
        }

        [SlashCommand("play", description: "Plays music")]
        public async Task Play(InteractionContext interactionContext, [Option("query", "Track to play")] string query)
        {
            await interactionContext.DeferAsync().ConfigureAwait(false);

            var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: true).ConfigureAwait(false);

            if (player is null)
            {
                return;
            }

            var track = await _audioService.Tracks
                .LoadTrackAsync(query, TrackSearchMode.YouTube)
                .ConfigureAwait(false);

            if (track is null)
            {
                var errorResponse = new DiscordFollowupMessageBuilder()
                    .WithContent("😖 No results.")
                    .AsEphemeral();

                await interactionContext
                    .FollowUpAsync(errorResponse)
                    .ConfigureAwait(false);

                return;
            }

            var position = await player
                .PlayAsync(track)
                .ConfigureAwait(false);

            if (position is 0)
            {
                await interactionContext
                    .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"🔈 Playing: {track.Uri}"))
                    .ConfigureAwait(false);
            }
            else
            {
                await interactionContext
                    .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"🔈 Added to queue: {track.Uri}"))
                    .ConfigureAwait(false);
            }
        }

        [SlashCommand("stop", description: "Stops current track")]
        public async Task Stop(InteractionContext ctx)
        {
            await ctx.DeferAsync().ConfigureAwait(false);

            var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

            await player.StopAsync().ConfigureAwait(false);

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Stopping track..."));

            await player.DisconnectAsync();
        }

        [SlashCommand("StopTTS",description: "Stop TTS message playback")]
        public async Task StopTTS(InteractionContext ctx)
        {
            await ctx.DeferAsync().ConfigureAwait(false);
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Stopping TTS"));
            try
            {
                var vnext = ctx.Client.GetVoiceNext();
                var connection = vnext.GetConnection(ctx.Guild);

                connection.Disconnect();
            }catch(Exception) { }
        }

        [SlashCommand("PlayTTS", description: "Play TTS message")]
        public async Task PlayTTS(InteractionContext ctx, [Option("Message", "Message to play")] string message)
        {
            await ctx.DeferAsync().ConfigureAwait(false);

            try
            {
                var player = await GetPlayerAsync(ctx, connectToVoiceChannel: false).ConfigureAwait(false);
                if (player != null) { await player.DisconnectAsync(); }
                
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var connection = await ctx.Member.VoiceState.Channel.ConnectAsync();


            try
            {
                if (ctx.Member.VoiceState == null)
                {
                    await ctx.Channel.SendMessageAsync("User not in voice");
                    return;
                }
                var transmit = connection.GetTransmitSink();

                SpeechSynthesizer TTS = new SpeechSynthesizer();

                TTS.SelectVoice("Microsoft Zira Desktop");

                Console.WriteLine(TTS.Voice.Name);
                Console.WriteLine(TTS.Voice.Culture);

                string filename = DateTime.Now.Ticks.ToString();

                TTS.SetOutputToWaveFile(filename);

                PromptBuilder pb = new PromptBuilder();

                pb.Culture = CultureInfo.GetCultureInfo("en-US");

                pb.AppendText(message);

                TTS.Speak(pb);

                TTS.SetOutputToNull();

                Stream stream = ConvertAudioToPcm(filename);

                await stream.CopyToAsync(transmit);
                await stream.DisposeAsync();

                TTS.SetOutputToNull();

                File.Delete(filename);
            }
            catch(Exception e) { Console.WriteLine(e.Message+"\n"+e.StackTrace); }

            

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Playing "+message));
            connection.Disconnect();
            return;

        }
        private Stream ConvertAudioToPcm(string filePath)
        {
            var ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = @"C:\ffmpeg\bin\ffmpeg",
                Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            return ffmpeg.StandardOutput.BaseStream;
        }
        public async ValueTask<QueuedLavalinkPlayer?> GetPlayerAsync(InteractionContext interactionContext, bool connectToVoiceChannel = true)
        {
            ArgumentNullException.ThrowIfNull(interactionContext);

            var retrieveOptions = new PlayerRetrieveOptions(
                ChannelBehavior: connectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);

            var playerOptions = new QueuedLavalinkPlayerOptions { HistoryCapacity = 10000 };

            if (interactionContext.Member.VoiceState == null)
            {
                var errorMessage = "You are not connected to a voice channel.";

                var errorResponse = new DiscordFollowupMessageBuilder()
                    .WithContent(errorMessage)
                    .AsEphemeral();

                await interactionContext
                    .FollowUpAsync(errorResponse)
                    .ConfigureAwait(false);

                return null;
            }

            var result = await _audioService.Players
                .RetrieveAsync(interactionContext.Guild.Id, interactionContext.Member?.VoiceState.Channel.Id, playerFactory: PlayerFactory.Queued, Options.Create(playerOptions), retrieveOptions)
                .ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                var errorMessage = result.Status switch
                {
                    PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
                    PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
                    _ => "Unknown error.",
                };

                var errorResponse = new DiscordFollowupMessageBuilder()
                    .WithContent(errorMessage)
                    .AsEphemeral();

                if (connectToVoiceChannel == true)
                {
                    await interactionContext
                    .FollowUpAsync(errorResponse)
                    .ConfigureAwait(false);
                }
                return null;
            }

            return result.Player;
        }
    }
}
