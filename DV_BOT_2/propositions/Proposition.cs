using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOT1.commands
{
    public class Proposition : IComparable<Proposition>
    {
        private string propositionId;
        private string userProposing;
        private string propositionText;
        private bool propositionSeen;
        private bool? propositionApproved = null;
        
        public string UserProposing { get => userProposing; }
        public string PropositionText { get => propositionText; }

        public JObject PropositionAsJObject
        {
            get
            {
                JObject toReturn = new JObject();
                toReturn.Add("propositionId",propositionId);
                toReturn.Add("userProposing",userProposing);
                toReturn.Add("propositionText", propositionText);
                toReturn.Add("propositionSeen", propositionSeen);
                toReturn.Add("propositionApproved", propositionApproved);
                return toReturn;
            }
        }

        public bool PropositionSeen
        {
            get => propositionSeen;
            set => propositionSeen = value;
        }
        public bool? PropositionApproved
        {
            get=>propositionApproved;
            set
            {
                if (value == false)
                {
                    Console.WriteLine("Proposition denied, deleting");
                    globalVariables.Propositions.DeleteProposition(this);
                }
                else { propositionApproved = value; }
            }
        }

        public Proposition(string propositionId, string userProposing, string propositionText) 
        {
            Console.WriteLine("Creating proposition object");
            this.propositionId = propositionId;
            this.userProposing= userProposing;
            this.propositionText = propositionText;
            this.propositionSeen = false;
            this.propositionApproved = false;
        }

        public Proposition(JObject proposition)
        {
            this.propositionId = proposition["propositionId"].ToString();
            this.userProposing = proposition["userProposing"].ToString();
            this.propositionText = proposition["propositionText"].ToString();
            this.propositionSeen = Convert.ToBoolean(proposition["propositionSeen"].ToString());
            this.propositionApproved = Convert.ToBoolean(proposition["propositionApproved"].ToString());
        }

        public int CompareTo(Proposition other)
        {
            throw new NotImplementedException();
        }

        public class PropositionComparerApproval : IComparer<Proposition>
        {
            public int Compare(Proposition x, Proposition y)
            {
                if (x.propositionApproved == true && y.PropositionApproved==true) { return 0; }
                if (x.PropositionApproved == true && y.PropositionApproved == false) { return 1; }
                return -1;
            }
        }

        public override string ToString()
        {
            return this.userProposing + " proposed :\n" + this.propositionText;
        }

    }
}
