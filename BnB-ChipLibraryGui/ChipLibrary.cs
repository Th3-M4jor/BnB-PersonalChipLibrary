using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BnB_ChipLibraryGui
{
    public sealed class ChipLibrary
    {
        public static readonly ChipLibrary instance = new ChipLibrary();
        private SortedDictionary<string, Chip> library;
        private ChipLibrary()
        {
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                var json = wc.DownloadString("spartan364.hopto.org/chips.json");\

            }
        }
    }
}
