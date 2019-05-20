using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BnB_ChipLibraryGui
{
    public sealed class ChipLibrary
    {
        public static readonly ChipLibrary instance = new ChipLibrary();
        private readonly Dictionary<string, Chip> Library;
        private ChipLibrary()
        {
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                var json = wc.DownloadString("http://spartan364.hopto.org/chips.json");
                var result = JsonConvert.DeserializeObject<List<Chip>>(json);
                this.Library = new Dictionary<string,Chip>(result.Count);
                result.ForEach(delegate (Chip aChip)
                {
                    this.Library.Add(aChip.name, aChip);
                });
            }
        }

        public Chip GetChip(string name)
        {
            bool exists = this.Library.TryGetValue(name, out Chip toReturn);
            if (exists) return toReturn;
            else return null;
        }

    }
}
