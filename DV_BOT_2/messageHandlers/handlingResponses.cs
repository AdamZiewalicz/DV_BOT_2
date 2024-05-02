using DSharpPlus.EventArgs;
using DV_BOT_2;
using DV_BOT_2.commands;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Lavalink4NET;
using BOT1.commands;
using DV_BOT_2.customEvents;

namespace DV_BOT.messageHandlers
{
    public sealed class handlingResponses
    {   
        public static bool DoesItFeelLikeAWednesday(string s)
        {
            s = s.ToLower();

            string[] keywordsDoesnt = { "not", "nt", "n't"};

            foreach(string keyword in keywordsDoesnt) 
            {
                if(s.Contains(keyword))
                {
                    return false;
                }
            }
            return true;

        }
        public static async void HandleWednesday(DiscordClient sender, MessageCreateEventArgs args)
        {
            string lowerCaseMessage = args.Message.Content.ToLower();

            Console.WriteLine(lowerCaseMessage);

            if (args.Message.ToString().ToLower().Contains("wednesday"))
            {
                if (lowerCaseMessage.Contains("wednesday") && lowerCaseMessage.Contains("feel") && lowerCaseMessage.Contains("like"))
                {
                    string messageToSend = "";
                    if (DoesItFeelLikeAWednesday(args.Message.Content))//it feels like a wednesday
                    {
                        if (Convert.ToInt32(DateTime.Now.DayOfWeek) == 3)//but it is. so it doesnt!
                        { messageToSend += $"{args.Author.Username} you shall be struck down when you least expect it"; }
                        else//and its not wednesday. youre an idiot
                        { 
                            messageToSend += $"It does NOT feel like a wednesday. You're an idiot {args.Author.Username}";

                            IAudioService audioService = (IAudioService)globalVariables.serviceProviderGlobal.GetService(typeof(IAudioService));

                            MiscMethods playWednesday = new MiscMethods(audioService,sender);

                            await playWednesday.PlayWednesday(args);
                        }

                        await args.Channel.SendMessageAsync(messageToSend);
                        return;
                    }
                    else
                    {
                        messageToSend = $"{args.Author.Username} It really does not feel like a wednesday";

                        if (Convert.ToInt32(DateTime.Now.DayOfWeek) == 3)
                        { messageToSend += " even though it is."; }
                        else
                        { messageToSend += ". Maybe because it isn't."; }

                        await args.Channel.SendMessageAsync(messageToSend);
                        return;
                    }
                }
            }
        }   
        public static async void HandlePropositionDecision(Proposition sender, PropositionEventArgs args)
        {
            var guild = await globalVariables.discordClientGlobal.GetGuildAsync(globalVariables.GuildID);
            var user = await guild.GetMemberAsync(args.userId);

            string messageToSend = "Hello! Your proposition: \"" + args.text + "\" ";

            if(args.wasApproved)
            {
                messageToSend += " has been approved! Pog!";
            }
            else { messageToSend += " has been denied :( yikes!"; }

            await user.SendMessageAsync(messageToSend);
            
            return;
        }
    }
}
