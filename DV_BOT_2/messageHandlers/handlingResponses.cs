﻿using DSharpPlus.EventArgs;
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
using DSharpPlus.Entities;

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
        public static async Task<Task> HandleMessage(DiscordClient sender, MessageCreateEventArgs args)
        {
            string lowerCaseMessage = args.Message.Content.ToLower();

            Console.WriteLine("[User "+args.Message.Author.Username+" at "+args.Message.CreationTimestamp.ToString()+"]\n\r"+args.Message.Content.ToLower()+"\n");

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
                            messageToSend += $"It does NOT feel like a wednesday. You're an idiot {((DiscordMember)args.Author).Nickname}";

                            IAudioService audioService = (IAudioService)globalVariables.serviceProviderGlobal.GetService(typeof(IAudioService));

                            MiscMethods playWednesday = new MiscMethods(audioService,sender);

                            await playWednesday.PlayWednesday(args);
                        }

                        await args.Channel.SendMessageAsync(messageToSend);
                        return Task.CompletedTask;
                    }
                    else
                    {
                        messageToSend = $"{args.Author.Username} It really does not feel like a wednesday";

                        if (Convert.ToInt32(DateTime.Now.DayOfWeek) == 3)
                        { messageToSend += " even though it is."; }
                        else
                        { messageToSend += ". Maybe because it isn't."; }

                        await args.Channel.SendMessageAsync(messageToSend);
                        return Task.CompletedTask;
                    }
                }
            }
            return Task.CompletedTask;
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
