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
using DSharpPlus.Entities;

namespace DV_BOT.messageHandlers
{
    public sealed class HandlingResponses
    {   

        /*public static async void HandlePropositionDecision(Proposition sender, PropositionEventArgs args, ulong guildID)
        {
            var guild = await globalVariables.discordClientGlobal.GetGuildAsync(guildID);
            var user = await guild.GetMemberAsync(args.userId);

            string messageToSend = "Hello! Your proposition: \"" + args.text + "\" ";

            if(args.wasApproved)
            {
                messageToSend += " has been approved! Pog!";
            }
            else { messageToSend += " has been denied :( yikes!"; }

            await user.SendMessageAsync(messageToSend);
            
            return;
        }*/
    }
}
