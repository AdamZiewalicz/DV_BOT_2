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
using OpenAI_API.Models;
using OpenAI_API.Audio;
using Lavalink4NET.Clients;
using OpenAI_API.Images;
using DSharpPlus.Interactivity.Extensions;
using System.Reflection.Emit;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using Lavalink4NET.Tracks;
using DV_BOT_2.guildInfo;
using DSharpPlus;
using DSharpPlus.AsyncEvents;
using System.Net;


namespace DV_BOT_2.commands
{

    public class SlashCommands : ApplicationCommandModule
    {
        private readonly IAudioService _audioService;
        private readonly Guilds _guilds;
        private readonly FakeYouNet.Client? _fakeYouClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordClient _discordClient;
        public SlashCommands(IAudioService audioService, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(audioService);
            ArgumentNullException.ThrowIfNull(serviceProvider);
            _serviceProvider = serviceProvider;
            
            var client = (DiscordClient?)_serviceProvider.GetService(typeof(DiscordClient));
           

            ArgumentNullException.ThrowIfNull(client);

            _discordClient = client;

            var guilds = _discordClient.Guilds;

            _audioService = audioService;
            if (ApplicationHost._guilds ==null )
            {
                throw new NullReferenceException("Guilds null when running SlashCommands constructor");
            }
            _guilds = ApplicationHost._guilds;
            _fakeYouClient = ApplicationHost._fakeYouClient;
        }

        #region GPTCommands
        /*
        [SlashCommand("GPTPhoto2", description: "GPTPhoto but better")]
        public async Task GPTPhoto2(InteractionContext ctx, [Option("query", "Query for photo")] string text)
        { 
            try {
                await ctx.DeferAsync().ConfigureAwait(false);
                var m1 = new DiscordFollowupMessageBuilder().WithContent("Creating picture from prompt \"" + text + "\"");
                await ctx.FollowUpAsync(m1);

                var chatGPT4 = globalVariables.Api.Chat.CreateConversation();
                chatGPT4.Model = Model.GPT4_Turbo;
                chatGPT4.AppendUserInput("Create a DALLE3 prompt that's as detailed as possible. Make sure the message is just and only" +
                    "the prompt with no additional input from your side and no quotation marks. make the prompt based on the following: ");
                chatGPT4.AppendUserInput(text);
                var result = await chatGPT4.GetResponseFromChatbotAsync();

                string response = result.ToString();

                Console.WriteLine(response);

                var request = new ImageGenerationRequest()
                {
                    Prompt = response,
                    Model = Model.DALLE3,
                    Size = ImageSize._1024,
                    ResponseFormat = ImageResponseFormat.Url
                };

                var picture = await globalVariables.Api.ImageGenerations.CreateImageAsync(request);

                if (picture.Created == null)
                {
                    Console.WriteLine("Didnt create?");
                }
                var message = new DiscordEmbedBuilder();
                message.WithTitle("Your picture: ");
                message.WithImageUrl(picture.ToString());

                var interactivity = ctx.Client.GetInteractivity();

                await ctx.Channel.SendMessageAsync(embed: message);
                bool makeMore = true;
                string newPrompt;
                while (makeMore)
                {
                    await ctx.Channel.SendMessageAsync("If youre not satisfied, say " +
                    "what you'd like to change in the previous prompt. If satisfied, say \"enough\"");

                    var nextMessage = await interactivity.WaitForMessageAsync(message => message.Author.Username == ctx.Member.Username, TimeSpan.FromMinutes(3));
                    if(nextMessage.TimedOut)
                    {
                        await ctx.Channel.SendMessageAsync("Request timed out.");
                        break;
                    }
                    if (nextMessage.Result.Content.Contains("enough"))
                    {
                        await ctx.Channel.SendMessageAsync("Enough is enough then :)");
                        break;
                    }else
                    {
                        await ctx.Channel.SendMessageAsync("Creating new...");
                        newPrompt = "Give me the same as before, but this time also " + nextMessage.Result.Content.Substring(7);
                        chatGPT4.AppendUserInput(newPrompt);
                        string nextResult="";
                        try
                        {
                            nextResult = await chatGPT4.GetResponseFromChatbotAsync();
                        }catch (Exception ex) { await ctx.Channel.SendMessageAsync(ex.Message); return;}
                        Console.WriteLine("\nNew Prompt ["+DateTime.Now.ToString()+"]\n\n"+nextResult+"\n\n");
                        var nextRequest = new ImageGenerationRequest()
                        {
                            Prompt = nextResult.ToString(),
                            Model = Model.DALLE3,
                            Size = ImageSize._1024,
                            ResponseFormat = ImageResponseFormat.Url
                        };
                        ImageResult newPicture;
                        try
                        { newPicture = await globalVariables.Api.ImageGenerations.CreateImageAsync(nextRequest); }
                        catch(Exception e)
                        {
                            newPicture = null;
                            await ctx.Channel.SendMessageAsync("Failed to create message. Add some info or say enough: "+e.Message); 
                        }
                        var newMessage = new DiscordEmbedBuilder();
                        newMessage.WithTitle("Your picture: ");
                        if (newPicture != null)
                        { newMessage.WithImageUrl(newPicture.ToString()); }
                        await ctx.Channel.SendMessageAsync(embed: newMessage);
                    }

                }


            } catch (Exception ex) { Console.WriteLine("error in gptphoto2 " + ex.Message);
                await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent("Failed to create"));
            } }


        [SlashCommand("GPTphoto",description:"create photo from prompt")]
        public async Task GPTphoto(InteractionContext ctx ,[Option("text","Text to create photo with")]string text)
        {
            try{
                await ctx.DeferAsync().ConfigureAwait(false);

                var request = new ImageGenerationRequest()
                {
                    Prompt = text,
                    Model = Model.DALLE3,
                    Size = ImageSize._1024,
                    ResponseFormat = ImageResponseFormat.Url
                };

                var picture = await globalVariables.Api.ImageGenerations.CreateImageAsync(request);

                if (picture.Created == null)
                {
                    Console.WriteLine("Didnt create?");
                }
                var message = new DiscordEmbedBuilder();
                message.WithTitle("Your picture: ");
                message.WithImageUrl(picture.ToString());

                var m1 = new DiscordFollowupMessageBuilder().WithContent("Creating picture from prompt \"" + text + "\"");

                await ctx.FollowUpAsync(m1);
                await ctx.Channel.SendMessageAsync(embed: message);
            }catch(Exception ex) { Console.WriteLine(ex.Message); }
        }

        /*[SlashCommand("GPTTTS",description:"play TTS with GPT")]
        public async Task GPTTTS(InteractionContext ctx, [Option("text","Text to play")]string text)
        {
            await ctx.DeferAsync().ConfigureAwait(false);

            try
            {
                var player = await GetPlayerAsync(ctx, connectToVoiceChannel: false).ConfigureAwait(false);
                if (player != null) { await player.DisconnectAsync(); }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var connection = await ctx.Member.VoiceState.Channel.ConnectAsync();
            //got connection

            try
            {
                if (ctx.Member.VoiceState == null)
                {
                    await ctx.Channel.SendMessageAsync("User not in voice");
                    return;
                }
                var transmit = connection.GetTransmitSink();

                var request = new TextToSpeechRequest()
                {
                    Input = text,
                    Model = Model.TTS_HD,
                    ResponseFormat = TextToSpeechRequest.ResponseFormats.MP3,
                    Speed = 1,
                    Voice = TextToSpeechRequest.Voices.Echo
                };
                string filename = DateTime.Now.Ticks.ToString()+".mp3";

                await globalVariables.Api.TextToSpeech.SaveSpeechToFileAsync(request, filename);

                Stream stream = ConvertAudioToPcm(filename);

                transmit.VolumeModifier = 1;

                await stream.CopyToAsync(transmit);

                File.Delete(filename);
            }
            catch (Exception e) { Console.WriteLine(e.Message + "\n" + e.StackTrace); }



            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Playing " + text));
            connection.Disconnect();
            return;
        }*/
        #endregion

        #region LavaLinkCommands
        [SlashCommand("LoopPlaylist", "Loops all the songs that are in the queue ahead including the currently playing one or stops a loop")]
        public async Task LoopPlaylist(InteractionContext interactionContext)
        {
            await interactionContext.DeferAsync().ConfigureAwait(false);

            var player = await GetPlayerAsync(interactionContext, false);

            if (_guilds[interactionContext.Guild.Id].PlaylistLoopOn)
            {
                _guilds[interactionContext.Guild.Id].PlaylistLoopOn = false;
                await interactionContext.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Stopping loop..."));
                return;
            }

            if (player is null)               
            {                   
                await interactionContext.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("No track is playing."));                    
                return;  
            }
                               
            _guilds[interactionContext.Guild.Id].TrackLoopOn = false;                    
                  
            _guilds[interactionContext.Guild.Id].PlaylistLoopOn = true;
            
            await interactionContext.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Looping playlist..."));
            return;


        }


        [SlashCommand("looptrack",description:"Loops chosen track. Using \"skip\" or \"play\" will stop looping")]
        public async Task Loop(InteractionContext interactionContext, [Option("query","Track to loop")] string query, [Choice("YouTube", "YouTube")][Choice("Spotify", "Spotify")][Option("Source", "Choose source to play from")] string strSearchMode = "YouTube")
        {
            try
            {
                _guilds[interactionContext.Guild.Id].TrackLoopOn = true;
                _guilds[interactionContext.Guild.Id].PlaylistLoopOn = false;

                TrackSearchMode current = TrackSearchMode.YouTube;

                if (strSearchMode == "Spotify")
                {
                    current = TrackSearchMode.Spotify;
                }
                await interactionContext.DeferAsync().ConfigureAwait(false);

                var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: true).ConfigureAwait(false);

                if (player is null)
                {
                    await interactionContext.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Failed to get player."));
                    return;
                }

                var track = await _audioService.Tracks
                    .LoadTrackAsync(query, current)
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

                _guilds[interactionContext.Guild.Id].LoopingTrack = track;

                var position = await player
                    .PlayAsync(track)
                    .ConfigureAwait(false);

                if (position is 0)
                {                                                        
                    await interactionContext
                        .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"🔈 Looping: {track.Uri}"))
                        .ConfigureAwait(false);
                }
                else
                {

                    await interactionContext
                        .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"🔈 Loop added to queue: {track.Uri}"))
                        .ConfigureAwait(false);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }


        [SlashCommand("Play", description: "Plays music from chosen source")]
        public async Task Play(InteractionContext interactionContext, [Option("query", "Track to play")] string query, [Choice("YouTube", "YouTube")] [Choice("Spotify", "Spotify")]
        [Option("Source", "Choose source to play from")] string strSearchMode="YouTube")
        {
            try
            {
                _guilds[interactionContext.Guild.Id].TrackLoopOn = false;

                TrackSearchMode current = TrackSearchMode.YouTube;

                if (strSearchMode == "Spotify")
                {
                    current = TrackSearchMode.Spotify;
                }
                await interactionContext.DeferAsync().ConfigureAwait(false);

                var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: true).ConfigureAwait(false);

                if (player is null)
                {
                    await interactionContext.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Failed to get player."));
                    return;
                }

                var track = await _audioService.Tracks
                    .LoadTrackAsync(query, current)
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
            catch(Exception ex) 
            {
                await interactionContext
                        .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Error playing track"))
                        .ConfigureAwait(false);
                Console.WriteLine(ex.Message);
            }
        }


        [SlashCommand("RemoveFromPlaylist","Removes current song from playlist if playlist loop is on")]
        public async Task RemoveNextFromPlaylist(InteractionContext ctx)
        {
           

            if (_guilds[ctx.Guild.Id].PlaylistLoopOn==false)
            {
                await ctx.DeferAsync().ConfigureAwait(false);
                await ctx
                       .FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"No playlist looping"))
                       .ConfigureAwait(false);
                return;
            }
            _guilds[ctx.Guild.Id].RemoveFromPlaylist = true;
            await Skip(ctx);
            _audioService.Players.TryGetPlayer(ctx.Guild.Id, out LavalinkPlayer? player);
            if (player != null)
            {
                if(player.State == PlayerState.NotPlaying)
                {
                    _guilds[ctx.Guild.Id].PlaylistLoopOn = false;
                    await player.DisconnectAsync();
                }
            }
        }


        [SlashCommand("skip",description:"Skips the current track")]
        public async Task Skip(InteractionContext ctx)
        {
            await ctx.DeferAsync().ConfigureAwait(false);
            var player = await GetPlayerAsync(ctx, connectToVoiceChannel: false).ConfigureAwait(false);

            if (player is null) 
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("No track is playing."));
                return; 
            }
            if(player.State ==PlayerState.NotPlaying)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("No track is playing."));
                await player.DisconnectAsync();
                return;
            }
            _guilds[ctx.Guild.Id].TrackLoopOn = false;

            await player.SkipAsync(1).ConfigureAwait(false);

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Skipping track..."));

            
        }


        [SlashCommand("Pause", description: "Pauses/unpauses the current track")]
        public async Task Pause(InteractionContext ctx)
        {
            await ctx.DeferAsync().ConfigureAwait(false);
            var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);

            if(player==null || player.State == PlayerState.NotPlaying)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("No track is playing/paused"));
                return;
            }
            if(player.IsPaused)
            {
                await player.ResumeAsync();
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Track resumed."));
                return;
            }
            await player.PauseAsync();
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Track paused."));
        }


        [SlashCommand("stop", description: "Stops current track")]
        public async Task Stop(InteractionContext ctx)
        {
            await ctx.DeferAsync().ConfigureAwait(false);

            var player = await GetPlayerAsync(ctx, connectToVoiceChannel: false).ConfigureAwait(false);

            if (player is null)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("No track is playing."));
                return;
            }

            await player.StopAsync().ConfigureAwait(false);

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Stopping track..."));
            
            _guilds[ctx.Guild.Id].TrackLoopOn = false;
            _guilds[ctx.Guild.Id].PlaylistLoopOn = false;
            
            await player.DisconnectAsync();

            
        }

        [SlashCommand("SetVolume","Set volume of currently playing track")]
        public async Task SetVolume(InteractionContext ctx, [Option("Volume","Volume to set (%)")] double volumeToSet)
        {
            await ctx.DeferAsync().ConfigureAwait(false);

            if(volumeToSet<0 || volumeToSet >100)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Volume is outside of bounds (0 - 100)"));
                return;
            }

            var player = await GetPlayerAsync(ctx,false).ConfigureAwait(false);

            if (player != null)
            {
                await player.SetVolumeAsync((float)(volumeToSet / 100));
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Volume set to " + volumeToSet));
                _guilds[ctx.Guild.Id].Volume = (float)volumeToSet / 100f;
                return;
            }

            var connection = _discordClient
                .GetVoiceNext()
                .GetConnection(_discordClient.Guilds[ctx.Guild.Id]);

            if(connection != null)
            {   
                connection.GetTransmitSink().VolumeModifier = volumeToSet / 100;
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Volume set to " + volumeToSet));
                return;
            }

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("No player found in channel"));

        }

        [SlashCommand("NIGHTMARENIGHTMARENIGHTMARE","incoming")]
        public async Task NightmareNightmareNightmare(InteractionContext ctx, [Option("user", "User to traumatize")] DiscordUser user)
        {
            await ctx.DeferAsync().ConfigureAwait(false);

            var members = await ctx.Guild.GetAllMembersAsync();
            
            DiscordMember memberToMove = (DiscordMember)user;
            
            if(memberToMove.VoiceState == null)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("User not connected"));
                return;
            }

            var channelToMoveBackTo = memberToMove.VoiceState.Channel;

            var nightmareChannel = await ctx.Guild.CreateChannelAsync("NIGHTMARENIGHTMARENIGHTMARE", ChannelType.Voice);

            var retrieveOptions = new PlayerRetrieveOptions(ChannelBehavior: PlayerChannelBehavior.Join);

            var playerOptions = new QueuedLavalinkPlayerOptions { HistoryCapacity = 10000 };

            var player = (await _audioService.Players
                .RetrieveAsync(ctx.Guild.Id, nightmareChannel.Id, playerFactory: PlayerFactory.Queued, Options.Create(playerOptions), retrieveOptions)
                .ConfigureAwait(false)).Player;

            var track = await _audioService.Tracks
                   .LoadTrackAsync("https://www.youtube.com/watch?v=0QSAHS0sVhg", TrackSearchMode.YouTube)
                   .ConfigureAwait(false);

            if(track == null)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Track not found"));
                return;
            }
            if (player == null)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Player didn't connect"));
                return;
            }

            await player.PlayAsync(track,enqueue:false);

            await player.SetVolumeAsync(1);

            await player.SeekAsync(TimeSpan.FromSeconds(5));

            await memberToMove.PlaceInAsync(nightmareChannel);

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Connected :)"));

            while (player.State == PlayerState.Playing) 
            {
                Thread.Sleep(1000);
            }

            if(memberToMove.VoiceState != null)
            {
                await memberToMove.PlaceInAsync(channelToMoveBackTo);
            }
            await nightmareChannel.DeleteAsync();

        }

        [SlashCommand("JumpToTimeInTrack","Jumps to timestamp in currently playing track")]
        public async Task JumpToTimeInTrack(InteractionContext ctx, [Option("Minutes","Minutes")] double minutes = 0, [Option("Seconds","Seconds")] double seconds = 0)
        {
            await ctx.DeferAsync().ConfigureAwait(false);
            var player = await GetPlayerAsync(ctx, connectToVoiceChannel: false).ConfigureAwait(false);

            TimeSpan timeToSeek = TimeSpan.FromSeconds(seconds + minutes * 60);

            Console.WriteLine("Seconds to seek: "+timeToSeek.TotalSeconds);

            if (player is null)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("No track is playing."));
                return;
            }
            if (player.State == PlayerState.NotPlaying || player.CurrentTrack == null)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("No track is playing."));
                return;
            }
            Console.WriteLine("Duration is " + player.CurrentTrack.Duration.TotalSeconds);
            if(player.CurrentTrack.Duration.TotalSeconds<timeToSeek.TotalSeconds || minutes<0 || seconds<0)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Timestamp outside of song duration."));
                return;
            }
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Jumping to "+timeToSeek.Minutes+" minutes "+timeToSeek.Seconds+" seconds."));
            await player.SeekAsync(timeToSeek);
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

        #endregion

        #region FakeYouCommands
        [SlashCommand("FakeYouTTS", description: "Play text to speech message in your channel (Mario voice)")]
        public async Task FakeYouTTS(InteractionContext ctx, [Option("Message", "Message to play")] string message, [Option("Voice","Voice to find")] string voiceQuery = "Mario (Charles Martinet, 1994-2023) (New!)")
        {
            await ctx.DeferAsync().ConfigureAwait(false);

            if (_fakeYouClient == null)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("No connection to FakeYou API."));
                return;
            }

            try
            {
                if (ctx.Member.VoiceState == null)
                {
                    await ctx.Channel.SendMessageAsync("User not in voice");
                    return;
                }


                var voice = await _fakeYouClient.FindVoiceByTitle(voiceQuery);


                string filename = DateTime.Now.Ticks.ToString() + ".wav";

                Console.WriteLine("Getting FakeYou tts...");


                

                await _fakeYouClient.DownloadMakeTTS(voice, message, filename);
                
                Console.WriteLine("Got tts.");

                var startTime = DateTime.Now.Ticks;
                while (!File.Exists(filename))
                {
                    Thread.Sleep(100);
                    if(DateTime.Now.Ticks - startTime > 10000000)
                    {
                        throw new Exception("File not found");
                    }
                }
                Stream stream = ConvertAudioToPcm(filename);

                _guilds[ctx.Guild.Id].TrackLoopOn = false;
                _guilds[ctx.Guild.Id].PlaylistLoopOn = false;

                var player = await GetPlayerAsync(ctx, connectToVoiceChannel: false).ConfigureAwait(false);
                if (player != null) { await player.DisconnectAsync(); }
                var connection = await ctx.Member.VoiceState.Channel.ConnectAsync();
                var transmit = connection.GetTransmitSink();
                transmit.VolumeModifier = 1;

                await stream.CopyToAsync(transmit);
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Playing " + message));
                await stream.DisposeAsync();

                connection.Disconnect();
                File.Delete(filename);
            }
            catch (Exception e) { Console.WriteLine(e.Message + "\n" + e.StackTrace); }

            return;

        }

        /*[SlashCommand("FakeYouVoices", description: "Gets list of voice titles")]
        public async Task FakeYouVoices(InteractionContext ctx)
        {
            await ctx.DeferAsync().ConfigureAwait(false);
            try
            {
                StreamReader sr = new StreamReader("fakeYouVoices.json");
                sr.ReadToEnd();
                Console.WriteLine(sr.ToString().Substring(0, 20));
                JObject voicesAsJObject = JsonConvert.DeserializeObject(sr.ToString()) as JObject;
                sr.Close();
                Console.WriteLine("Read voices");
                JArray Models = voicesAsJObject["models"] as JArray;
                Console.WriteLine("Got models");
                await ctx.Channel.SendMessageAsync("Models: ");
                foreach (var model in Models)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent(model["title"].ToString()));
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("^"));
        }*/
        #endregion

        #region AudioConvertCommands
        private Stream ConvertAudioToPcm(string filePath)
        {
            var ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = @"C:\Users\adzie\Desktop\ffmpeg-7.0-full_build\bin\ffmpeg",
                Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            }) ?? throw new Exception("ffmpeg is missing or not working correctly");

            return ffmpeg.StandardOutput.BaseStream;
        }
 
        #endregion

    }
}
