using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.AsyncEvents;
using DSharpPlus.EventArgs;
using DSharpPlus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DV_BOT_2.propositions
{
    public class Propositions
    {
        private List<Proposition> propositions = [];
        private string propositionsFilePath;
        public List<Proposition> PropositionsAsProperty { get => propositions; set => propositions = value; }
        public int Count { get => propositions.Count; }
        public void AddProposition(Proposition proposition)
        {
            propositions.Add(proposition);
            BackUpData();
        }
        public void DeleteProposition(Proposition proposition)
        {
            propositions.Remove(proposition);
            BackUpData();
        }
        public Propositions(string propositionsFilePath)
        {

            this.propositionsFilePath = propositionsFilePath;

            StreamReader sr = new(propositionsFilePath);

            string propositionsFileContents = sr.ReadToEnd().ToString();

            Console.WriteLine(propositionsFileContents);

            sr.Close();
        }
        public void BackUpData()
        {

            Console.WriteLine("Backing up propositions...");

            JArray PropositionsAsJArray = [];

            foreach (Proposition proposition in propositions)
            {
                PropositionsAsJArray.Add((JObject)proposition);
            }

            StreamWriter sw = new(propositionsFilePath);

            sw.Write(PropositionsAsJArray.ToString());

            sw.Close();

            Console.WriteLine("Propositions backed up.");
        }
        public void SortByApproval()
        {
            propositions.Sort(new Proposition.PropositionComparerApproval());
        }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine("Propositions: ");

            foreach(Proposition proposition in propositions)
            {
                if(proposition!=null)
                {
                    sb.AppendLine(proposition.ToString());
                }
            }
            return sb.ToString();
        }
    }
}
