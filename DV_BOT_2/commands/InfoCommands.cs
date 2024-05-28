using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DV_BOT_2.commands
{
    public class InfoCommands : BaseCommandModule
    {
        [Command("help")]
        public async Task Help(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("For command list use !commandlist");
        }
        [Command("commandList")]
        public async Task CommandList(CommandContext ctx) 
        {
            StringBuilder sb = new();
            sb.AppendLine("Use \"/\" to see slash commands");
            sb.AppendLine("!propose\nafter this insert your proposition. It may or may not get reviewed by the admin in uncertain amounts of time.");
            await ctx.Channel.SendMessageAsync(sb.ToString());
        }

    }
}
