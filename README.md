For the Bot to work as intended: 
  -Debug on x86
  -Download Microsoft Zira Desktop System.Speech voice (in windows settings) 
  -Ensure lavalink server configured in Program.cs is up 
  -insert config.json and propositions.json files into the Debug folder of your repo
  -propositions.json can be empty, config.json NEEDS to contain the authToken for your discord server
  -AdminCommands.cs needs to be updated with your username in line 51 for !SeePropositions to work
  -download ffmpeg and specify the correct path in SlashCommands.cs line 213
