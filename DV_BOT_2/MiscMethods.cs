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
        public async Task GPTPhoto2(DiscordClient client, DiscordUser user, string text, DiscordChannel channel)
        {
            try
            {
                var m1 = new DiscordMessageBuilder().WithContent("Tworzenie zdjęcia z tekstu: \"" + text + "\"");
                await channel.SendMessageAsync(m1);

                var chatGPT4 = globalVariables.api.Chat.CreateConversation();
                chatGPT4.Model = Model.GPT4_Turbo;
                chatGPT4.AppendUserInput("" +
                    "stwórz " +
                    "prompt dla DALLE3 z dużą ilością detali. Upewnij się, że wiadomość zwrotna zawiera tylko i wyłącznie" +
                    "prompt bez żadnych dodatkowych informacji i bez cudzysłowów. stwórz prompt na podstawie następującego tekstu : ");
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
                message.WithTitle("Twoje zdjęcie: ");
                message.WithImageUrl(picture.ToString());

                var interactivity = client.GetInteractivity();

                await channel.SendMessageAsync(embed: message);
                bool makeMore = true;
                string newPrompt;
                while (makeMore)
                {
                    await channel.SendMessageAsync("Jeśli chcesz coś zmienić, napisz wiadomość. " +
                    "Jeśli nie, napisz \"koniec\"");

                    var nextMessage = await interactivity.WaitForMessageAsync(message => message.Author.Username == "neononagsxr", TimeSpan.FromMinutes(3));
                    if (nextMessage.TimedOut)
                    {
                        await channel.SendMessageAsync("Czas na odpowiedź minął.");
                        break;
                    }
                    if (nextMessage.Result.Content.Contains("koniec"))
                    {
                        await channel.SendMessageAsync("Koniec akcji :)");
                        break;
                    }
                    else
                    {
                        await channel.SendMessageAsync("Tworzę nowe...");
                        newPrompt = "Daj mi to samo co wcześniej, ale tym razem też" +
                            " " + nextMessage.Result.Content;
                        chatGPT4.AppendUserInput(newPrompt);
                        var nextResult = await chatGPT4.GetResponseFromChatbotAsync();
                        Console.WriteLine("\nNew Prompt [" + DateTime.Now.ToString() + "]\n\n" + nextResult.ToString() + "\n\n");
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
                        catch (Exception e)
                        {
                            newPicture = null;
                            await channel.SendMessageAsync("Problem z tworzeniem zdjęcia. Dodaj inne" +
                                "informacje lub napisz koniec: ");
                        }
                        var newMessage = new DiscordEmbedBuilder();
                        newMessage.WithTitle("Twoje zdjęcie: ");
                        if (newPicture != null)
                        { newMessage.WithImageUrl(newPicture.ToString()); }
                        await channel.SendMessageAsync(embed: newMessage);
                    }

                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("error in gptphoto2 " + ex.Message);
                await channel.SendMessageAsync(new DiscordMessageBuilder().WithContent("Failed to create"));
            }
        }
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
