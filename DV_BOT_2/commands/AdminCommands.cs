using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOT1.commands
{
    public class AdminCommands : BaseCommandModule
    {
        [Command("SeePropositions")]
        public async Task SeePropositions(CommandContext ctx) 
        {
            Console.WriteLine(globalVariables.Propositions.ToString());
            if (ctx.User.Username.ToString() == "neononagsxr")
            {
                if (globalVariables.Propositions.Count == 0)
                {
                    await ctx.Channel.SendMessageAsync("No propositions as of now");
                    return;
                }
                await ctx.Channel.SendMessageAsync("Up to 3 first propositions: ");

                var interactivity = ctx.Client.GetInteractivity();

                DiscordEmoji[] emojiOptions =
                {
                    DiscordEmoji.FromName(ctx.Client, ":ballot_box_with_check:"),
                    DiscordEmoji.FromName(ctx.Client, ":x:")
                };
                string optionsDescription = $"\n{emojiOptions[0]} | Approve\n" +
                                            $"{emojiOptions[1]} | Deny";
                globalVariables.Propositions.SortByApproval();
                
                string resultsDescription = "";

                for (int i = 0; i < 3 && i < globalVariables.Propositions.Count; i++)
                {
                    Proposition currentProposition = globalVariables.Propositions.PropositionsAsProperty[i];

                    string descriptionText = "";

                    if (currentProposition.PropositionSeen)
                    {
                        descriptionText = "\nSeen\n";
                    }
                    else { descriptionText = "\nNot seen yet\n"; }

                    if (currentProposition.PropositionApproved == true)
                    {
                        descriptionText += "Proposition has been approved\n";
                    }
                    if (currentProposition.PropositionApproved == false)
                    {
                        descriptionText += "Proposition has been denied\n";
                    }
                    if (currentProposition.PropositionApproved == null)
                    {
                        descriptionText += "Proposition has not been acted upon\n";
                    }

                    var message = new DiscordEmbedBuilder()
                    {
                        Title = currentProposition.UserProposing + " proposed:",
                        Description = currentProposition.PropositionText + descriptionText + optionsDescription,
                        Color = DiscordColor.Blue
                    };
                    var sentProposition = await ctx.Channel.SendMessageAsync(embed: message);

                    foreach (var emoji in emojiOptions)
                    {
                        await sentProposition.CreateReactionAsync(emoji);
                    }

                    //new
                   // var postpone = await interactivity.WaitForMessageAsync(message=>) // finish up here

                    var reaction = await interactivity.WaitForReactionAsync(reaction => (emojiOptions[0] == reaction.Emoji || emojiOptions[1] == reaction.Emoji) && reaction.Message == sentProposition && reaction.User == ctx.User, TimeSpan.FromSeconds(5));

                    int count1 = 0;
                    int count2 = 0;

                    if (!reaction.TimedOut)
                    {
                        count1 = Convert.ToInt32(reaction.Result.Emoji == emojiOptions[0]);
                        count2 = Convert.ToInt32(reaction.Result.Emoji == emojiOptions[1]);
                    }

                    int indexOfProposition = i;

                    Console.WriteLine(count1 + " " + count2);

                    resultsDescription += globalVariables.Propositions.PropositionsAsProperty[indexOfProposition].UserProposing + "'s proposition has";

                    if (count1 > 0)
                    {
                        globalVariables.Propositions.PropositionsAsProperty[indexOfProposition].PropositionApproved = true;
                        resultsDescription += " been approved\n";
                    }
                    else
                    if (count2 > 0)
                    {
                        globalVariables.Propositions.PropositionsAsProperty[indexOfProposition].PropositionApproved = false;
                        resultsDescription += " been denied\n";
                    }
                    else
                    {
                        globalVariables.Propositions.PropositionsAsProperty[indexOfProposition].PropositionApproved = null;
                        resultsDescription += " not been acted upon\n";
                    }
                    if (globalVariables.Propositions.PropositionsAsProperty[indexOfProposition] != null)
                    { globalVariables.Propositions.PropositionsAsProperty[indexOfProposition].PropositionSeen = true; }

                    resultsDescription += "\n";
                }

                var resultEmbed = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Green,
                    Title = "Session results",
                    Description = resultsDescription
                };

                await ctx.Channel.SendMessageAsync(embed: resultEmbed);
            }
            else
            {
                await ctx.Channel.SendMessageAsync("Non admin users are not allowed to use this command");
            }
        }

    }
}
