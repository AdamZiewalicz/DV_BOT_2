﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DV_BOT;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using DV_BOT_2.propositions;

namespace DV_BOT_2.commands
{
    public class ResponseCommands : BaseCommandModule
    {
       
        [Command("propose")]
        public async Task Propose(CommandContext ctx)
        {
           
            await ctx.Channel.SendMessageAsync("Please present your proposition");          

            var interactivity = ctx.Client.GetInteractivity();

            var messageToRetrieve = await interactivity.WaitForMessageAsync(message=>(message.Author==ctx.User && message.Channel==ctx.Channel));
            
            string propositionText = messageToRetrieve.Result.Content;
            Console.WriteLine("proposition \""+propositionText+"\" by user " + ctx.User);

            Proposition proposition = new(Guid.NewGuid().ToString(), ctx.User.Username,ctx.User.Id ,propositionText,ctx.Guild.Id);

            Debug.WriteLine("Proposition object created");

            // globalVariables.Propositions.AddProposition(proposition);

            await ctx.Channel.SendMessageAsync("Proposition noted");
        }
    }
}
