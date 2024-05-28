using Lavalink4NET.Tracks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DV_BOT_2.guildInfo
{
    public class Guild
    {
        public ulong GuildID {  get; set; }

        private LavalinkTrack? loopingTrack;
        public LavalinkTrack? LoopingTrack {
            get=> loopingTrack;
            set
            {
                loopingTrack = value;
            }
        }
        public bool RemoveFromPlaylist {  get; set; }

        private bool trackLoopOn;
        public bool TrackLoopOn 
        { 
            get
            {
                return trackLoopOn;
            }
            set
            {
                if(value==false)
                {
                    LoopingTrack = null;
                }
                trackLoopOn = value;
            } 
        }
        public bool PlaylistLoopOn { get; set; }
        public Guild(ulong guildID)
        {
            GuildID = guildID;
            TrackLoopOn = false;
            LoopingTrack = null;
            PlaylistLoopOn = false;
            RemoveFromPlaylist = false;
        }

        public override string ToString()
        {
            return "Guild id: " + GuildID + " Track looping: " + TrackLoopOn;
        }
    }
}
