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
        [SlashCommand("GPTPhoto2", description: "GPTPhoto but better")]
        public async Task GPTPhoto2(InteractionContext ctx, [Option("query", "Query for photo")] string text)
        { try {
                await ctx.DeferAsync().ConfigureAwait(false);
                var m1 = new DiscordFollowupMessageBuilder().WithContent("Creating picture from prompt \"" + text + "\"");
                await ctx.FollowUpAsync(m1);

                var chatGPT4 = globalVariables.api.Chat.CreateConversation();
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

                var picture = await globalVariables.api.ImageGenerations.CreateImageAsync(request);

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
                        var nextResult = await chatGPT4.GetResponseFromChatbotAsync();
                        Console.WriteLine("\nNew Prompt ["+DateTime.Now.ToString()+"]\n\n"+nextResult.ToString()+"\n\n");
                        var nextRequest = new ImageGenerationRequest()
                        {
                            Prompt = nextResult.ToString(),
                            Model = Model.DALLE3,
                            Size = ImageSize._1024,
                            ResponseFormat = ImageResponseFormat.Url
                        };
                        ImageResult newPicture;
                        try
                        { newPicture = await globalVariables.api.ImageGenerations.CreateImageAsync(nextRequest); }
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

                var picture = await globalVariables.api.ImageGenerations.CreateImageAsync(request);

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

        [SlashCommand("GPTTTS",description:"play TTS with GPT")]
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

                await globalVariables.api.TextToSpeech.SaveSpeechToFileAsync(request, filename);

                Stream stream = ConvertAudioToPcm(filename);

                transmit.VolumeModifier = 1;

                await stream.CopyToAsync(transmit);

                File.Delete(filename);
            }
            catch (Exception e) { Console.WriteLine(e.Message + "\n" + e.StackTrace); }



            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Playing " + text));
            connection.Disconnect();
            return;
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

        [SlashCommand("skip",description:"Skips the current track")]
        public async Task Skip(InteractionContext ctx)
        {
            await ctx.DeferAsync().ConfigureAwait(false);
            var player = await GetPlayerAsync(ctx, connectToVoiceChannel: true).ConfigureAwait(false);
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
        [SlashCommand("FakeYouTTS", description: "Play TTS message")]
         public async Task FakeYouTTS(InteractionContext ctx, [Option("Message", "Message to play")] string message)
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
             //connected 

             try
             {
                 if (ctx.Member.VoiceState == null)
                 {
                     await ctx.Channel.SendMessageAsync("User not in voice");
                     return;
                 }
                 var transmit = connection.GetTransmitSink();

                 //got transmission sink

                 var voice = await globalVariables.FakeYouClient.FindVoiceByTitle("Mario (Charles Martinet, 1994-2023) (New!)");
                 //var voice = await globalVariables.FakeYouClient.FindVoiceByToken("weight_tjwh5xfmyk1c7p3kh3fapmcjt");

                 string filename = DateTime.Now.Ticks.ToString() + ".wav";

                Console.WriteLine("Getting FakeYou tts...");
                await globalVariables.FakeYouClient.DownloadMakeTTS(voice, message, filename);
                Console.WriteLine("Got tts.");
                 while (!File.Exists(filename))
                 {
                     Thread.Sleep(100);
                 }
                 Stream stream = ConvertAudioToPcm(filename);

                 await stream.CopyToAsync(transmit);
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Playing " + message));
                await stream.DisposeAsync();

                 connection.Disconnect();
                // File.Delete(filename);
             }
             catch (Exception e) { Console.WriteLine(e.Message + "\n" + e.StackTrace); }



             

             return;

         }
        /*
        [SlashCommand("FakeYouVoices",description:"Gets list of voice titles")]
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
            }catch(Exception ex) { Console.WriteLine(ex.Message); }
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("^"));
        }
        [SlashCommand("FakeYouTTS", description: "Choose voice for tts")]
        public async Task FakeYouTTS(InteractionContext ctx, [Option("Message", "Message to play")] string message, [Option("voice", "voice to choose")] string voiceToPlay)
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
            //connected 

            try
            {
                if (ctx.Member.VoiceState == null)
                {
                    await ctx.Channel.SendMessageAsync("User not in voice");
                    return;
                }
                var transmit = connection.GetTransmitSink();

                //got transmission sink

                var voice = await globalVariables.FakeYouClient.FindVoiceByTitle("voiceToPlay");
                //var voice = await globalVariables.FakeYouClient.FindVoiceByToken("weight_tjwh5xfmyk1c7p3kh3fapmcjt");

                string filename = DateTime.Now.Ticks.ToString() + ".wav";

                await globalVariables.FakeYouClient.DownloadMakeTTS(voice, message, filename);

                while (!File.Exists(filename))
                {
                    Thread.Sleep(100);
                }
                Stream stream = ConvertAudioToPcm(filename);

                await stream.CopyToAsync(transmit);
                Thread.Sleep(1000);
                await stream.DisposeAsync();

                connection.Disconnect();
                File.Delete(filename);
            }
            catch (Exception e) { Console.WriteLine(e.Message + "\n" + e.StackTrace); }



            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Playing " + message));

            return;

        }*/
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
