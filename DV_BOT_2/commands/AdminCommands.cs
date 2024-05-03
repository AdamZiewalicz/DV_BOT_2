﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DV_BOT_2.customEvents;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOT1.commands
{
    public class AdminCommands : BaseCommandModule
    {

        [Command("gpt")]
        public async Task GPT(CommandContext ctx, [RemainingText] string text)
        {
            try
            {
                var member = ctx.Member;
                Conversation chat;

                if(!globalVariables.CurrentMembers.IsConversation(member))
                {
                    var conversation = globalVariables.api.Chat.CreateConversation();
                    chat = globalVariables.CurrentMembers.TryAddConversation(member,conversation);
                }
                else { chat = globalVariables.CurrentMembers.GetConversation(member); }

                //chat.Model = Model.ChatGPTTurbo;
                chat.Model = Model.GPT4_Turbo;
                chat.AppendUserInput("Provide a response to this message in under 2000 characters: ");
                chat.AppendUserInput(text);
                var result = await chat.GetResponseFromChatbotAsync();
                string toSend = result.ToString();
                if (toSend.Length > 2000)
                {
                    string currentMessage = "";
                    int idx = 0;
                    while (currentMessage.Length > 2000)
                    {
                        currentMessage = toSend.Substring(idx, 2000);
                        await ctx.Channel.SendMessageAsync(currentMessage);
                        idx += 2000;
                    }
                }
                else
                { await ctx.Channel.SendMessageAsync(toSend); }
            }
            catch(Exception ex) { await ctx.Channel.SendMessageAsync("We found a problem executing the action. Sowwy!"); Console.WriteLine(ex.Message); }
        }
        [Command("SeePropositions")]
        public async Task SeePropositions(CommandContext ctx)
        {
            try{
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
                    int propositionsDenied = 0;

                    for (int i = 0; i + propositionsDenied < 3 && i < globalVariables.Propositions.Count; i++)
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

                        var reaction = await interactivity.WaitForReactionAsync(reaction => (emojiOptions[0] == reaction.Emoji || emojiOptions[1] == reaction.Emoji) && reaction.Message == sentProposition && reaction.User == ctx.User, TimeSpan.FromSeconds(10));

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
                            i--;
                            propositionsDenied++;
                        }
                        else
                        {
                            globalVariables.Propositions.PropositionsAsProperty[indexOfProposition].PropositionApproved = null;
                            resultsDescription += " not been acted upon\n";
                        }
                        if (count2 == 0)
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
            }catch(Exception e) { Console.WriteLine(e.Message + " PROBLEM WITH SEEPROPOSITIONS"+e.StackTrace+e.Source); }
        }

    }
}
