using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DV_BOT.config
{
    internal class JSONReader
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string GPTSecret { get; set; }

        public async Task ReadJSON()
        {
            using (StreamReader sr = new StreamReader("config.json")) 
            { 
                string json = await sr.ReadToEndAsync();
                JSONStructure data = JsonConvert.DeserializeObject<JSONStructure>(json);

                this.Token = data.Token;
                this.Prefix = data.Prefix;
                this.GPTSecret = data.GPTSecret;
            }
        }
    }

    internal sealed class JSONStructure
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string GPTSecret { get; set; }
    }
}
