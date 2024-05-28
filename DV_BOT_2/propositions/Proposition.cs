using DSharpPlus.AsyncEvents;
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lavalink4NET.Events;
using DV_BOT.messageHandlers;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Numerics;


namespace DV_BOT_2.propositions
{
    public class Proposition : IComparable<Proposition>
    {
        public string PropositionId { get; set; }
        public ulong UserProposingId { get; set; }
        public bool PropositionSeen { get; set; }
        public bool? PropositionApproved { get; set; }
        public ulong GuildId {  get; set; }

        public string UserProposing { get; set; }
        public string PropositionText { get; set; }

        public static explicit operator JObject(Proposition proposition)
        {
                JObject toReturn = new()
                {
                    { "propositionId", proposition.PropositionId },
                    { "userProposing", proposition.UserProposing },
                    { "userProposingId", proposition.UserProposingId },
                    { "propositionText", proposition.PropositionText },
                    { "propositionSeen", proposition.PropositionSeen },
                    { "propositionApproved", proposition.PropositionApproved },
                    { "guildId", proposition.GuildId }
                };
                return toReturn;
        }
        public Proposition(string propositionId, string userProposing, ulong  userProposingId, string propositionText,ulong guildId) 
        {
            Console.WriteLine("Creating proposition object");
            this.PropositionId = propositionId;
            this.UserProposing= userProposing;
            this.UserProposingId = userProposingId;
            this.PropositionText = propositionText;
            this.PropositionSeen = false;
            this.PropositionApproved = null;
            this.GuildId = guildId;
        }
        public int CompareTo(Proposition? other)
        {
            throw new NotImplementedException();
        }

        public class PropositionComparerApproval : IComparer<Proposition>
        {
            public int Compare(Proposition? x, Proposition? y)
            {
                if(x==null || y == null) { return 0; }
                if (x.PropositionApproved == true && y.PropositionApproved==true) { return 0; }
                if (x.PropositionApproved == true && y.PropositionApproved == false) { return 1; }
                return -1;
            }
        }

        public override string ToString()
        {
            return this.UserProposing + " proposed :\n" + this.PropositionText;
        }
    }
}
    