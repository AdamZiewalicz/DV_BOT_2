using DSharpPlus.AsyncEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DV_BOT_2.customEvents
{
    public class PropositionEventArgs : EventArgs
    {
        public bool wasApproved;
        public string text;
        public ulong userId;
        public PropositionEventArgs(bool wasApproved, string text,ulong userId)
        {
            this.wasApproved = wasApproved;
            this.text = text;
            this.userId = userId;
        }
        public override string ToString()
        {
            return wasApproved + " " + text + " " + userId;
        }
    }
}
