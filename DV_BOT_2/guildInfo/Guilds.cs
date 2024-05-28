using DSharpPlus.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DV_BOT_2.guildInfo
{
    public class Guilds : IEnumerable<Guild>
    {
        public List<Guild> GuildsList = [];

        public Guild this[ulong guildID]
        {
            get
            {
                foreach (Guild guild in GuildsList)
                {
                    if (guild != null)
                    {
                        if (guild.GuildID == guildID)
                        {
                            return guild;
                        }
                    }
                }
                Guild toAdd = new Guild(guildID);
                GuildsList.Add(toAdd);
                return toAdd;
            }
            set
            {
                foreach (Guild guild in GuildsList)
                {
                    if (guild != null)
                    {
                        if (guild.GuildID == guildID)
                        {
                            return;
                        }
                    }
                }
                if(value!=null)
                {
                    GuildsList.Add(value);
                }
            }
        }

        public Guilds() { }
        public Guilds(IReadOnlyDictionary<ulong,DiscordGuild> guilds)
        { 
            foreach(var guild in guilds)
            {
                GuildsList.Add(new Guild(guild.Key));
            }
        }

        public IEnumerator<Guild> GetEnumerator()
        {
            return GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new GuildsEnumerator();
        }
        public class GuildsEnumerator() :Guilds, IEnumerator<Guild>
        {
            private int index = 0;
            public Guild Current => GuildsList[index];

            object IEnumerator.Current => (object)GuildsList[index];

            public void Dispose()
            {
                GuildsList.Remove(Current);
            }

            public bool MoveNext()
            {
                index++;
                if (index < GuildsList.Count)
                {
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                index = 0;
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine("Guilds: ");
            foreach(var guild in GuildsList)
            {
                sb.AppendLine(guild.ToString());
            }
            return sb.ToString();
        }
    }
}
