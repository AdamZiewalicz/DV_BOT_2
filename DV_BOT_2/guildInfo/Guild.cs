﻿using Lavalink4NET.Tracks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DV_BOT_2.guildInfo
{
    public class Guild
    {
        public ulong GuildID {  get; set; }
		
		public List<Playlist> Playlists = new List<Playlist>();

        private LavalinkTrack? loopingTrack;
        public LavalinkTrack? LoopingTrack {
            get=> loopingTrack;
            set
            {
                loopingTrack = value;
            }
        }
        public bool RemoveFromPlaylist {  get; set; }

        public float Volume = 1f;

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
			var playlistsJSON = File.ReadAllText("guildInfo/playlists.json");
			var guildsJARRAY = (JArray)JsonConvert.DeserializeObject(playlistsJSON);

			foreach(var JGuild in guildsJARRAY)
			{
				if((ulong)JGuild["GuildID"] == guildID)
				{
					Console.WriteLine("Hi! Found playlists!");
					break;
				}
			}
		}

        public override string ToString()
        {
            return "Guild id: " + GuildID + " Track looping: " + TrackLoopOn;
        }
    }
}
