using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.VisualBasic;
using OpenAI_API.Chat;

namespace DV_BOT_2
{
    public class Members
    {
        public List<Member> _Members = new List<Member>();
        public Members()
        {
            GetMembers();
        }
        public Conversation TryAddConversation(DiscordMember member, Conversation conversation)
        {
            foreach(var _member in _Members)
            {
                if(_member!=null)
                {
                    if(_member.userAsDMember==member)
                    {
                        if (_member.conversation == null)
                        { _member.conversation = conversation; }
                        return conversation;
                    }
                }
            }
            throw new ArgumentException("Member not found!");
        }
        public Conversation GetConversation(DiscordMember member)
        {
            foreach(var _member in _Members)
            {
                if (_member != null)
                {
                    if (_member.userAsDMember == member)
                    {
                        if (_member.conversation != null)
                        { return _member.conversation; }
                        else { return null; }
                    }
                }
            }
            throw new ArgumentException("Member not found!");
        }
        public bool IsConversation(DiscordMember member)
        {
            foreach (var _member in _Members)
            {
                if (_member != null)
                {
                    if (_member.userAsDMember != null)
                    {
                        if (_member.userAsDMember == member)
                        {
                            return _member.conversation != null;
                        }
                    }
                }
            }
            throw new ArgumentException("Member not found!");
        }
        public async Task GetMembers()
        {
            var guild = await globalVariables.discordClientGlobal.GetGuildAsync(globalVariables.GuildID);
            var members = await guild.GetAllMembersAsync();
            foreach(var member in members)
            {
                _Members.Add(new Member(member));
            }
        }

        public override string ToString()
        {
            string toReturn = "";

            foreach(var member in _Members)
            {
                toReturn += member.userAsDMember.Username + "\"";
            }
            return toReturn;
        }

    }
}
