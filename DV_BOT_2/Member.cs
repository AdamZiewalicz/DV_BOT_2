using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DV_BOT_2
{
    public class Member
    {
        public DiscordMember userAsDMember;

        public OpenAI_API.Chat.Conversation? conversation;

        public Member(DiscordMember userAsDMember)
        {
            this.userAsDMember = userAsDMember;
            this.conversation = null;
        }
    }
}
