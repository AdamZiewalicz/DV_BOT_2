using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BOT1.commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BOT1.propositions
{
    public class Propositions
    {
        private List<Proposition> propositions = new List<Proposition>();
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

            StreamReader sr = new StreamReader(propositionsFilePath);

            string propositionsFileContents = sr.ReadToEnd().ToString();

            Console.WriteLine(propositionsFileContents);
       
            sr.Close();

            JArray propositionsAsJArray;

            if (propositionsFileContents != "" && propositionsFileContents!=null)
            {
                
                try
                { 
                    propositionsAsJArray = JsonConvert.DeserializeObject(propositionsFileContents) as JArray;
                    foreach (JObject proposition in propositionsAsJArray)
                    {
                        propositions.Add(new Proposition(proposition));
                    }
                }
                catch (Exception e) 
                {
                    Console.WriteLine("Error initializing propositions file");
                    Console.WriteLine(e.ToString());
                    propositionsAsJArray = new JArray(); 
                }

            }
            else { propositionsAsJArray = new JArray(); }

        }
        public void BackUpData()
        {

            Console.WriteLine("Backing up propositions...");

            JArray PropositionsAsJArray = new JArray();

            string PropositionsAsJArrayAsString = "";

            foreach (Proposition proposition in propositions)
            {
                PropositionsAsJArray.Add(proposition.PropositionAsJObject);
            }

            PropositionsAsJArrayAsString = PropositionsAsJArray.ToString();

            StreamWriter sw = new StreamWriter(propositionsFilePath);

            sw.Write(PropositionsAsJArrayAsString);

            sw.Close();

            Console.WriteLine("Propositions backed up.");
        }
        public void SortByApproval()
        {
            propositions.Sort(new Proposition.PropositionComparerApproval());
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
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
