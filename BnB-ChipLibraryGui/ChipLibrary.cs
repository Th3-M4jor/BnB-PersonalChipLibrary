using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BnB_ChipLibraryGui
{
    public sealed class ChipLibrary
    {
        public enum ChipListOptions
        {
            DisplayOwned, DisplayAll
        }

        public enum LibrarySortOptions
        {
            Name, Element, AvgDamage, Owned, MaxDamage, Skill, Range
        }

        public static ChipLibrary Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public Chip GetChip(string name)
        {
            bool exists = this.Library.TryGetValue(name.ToLower(), out Chip toReturn);
            if (exists) return toReturn;
            else return null;
        }

        public List<Chip> getList(ChipListOptions AllOrOwned, LibrarySortOptions sortOptions, Chip.ChipRanges rangeOption, bool invert)
        {
            List<Chip> toReturn = new List<Chip>();
            foreach (var item in this.Library)
            {
                if (item.Value.ChipCount != 0 || AllOrOwned == ChipListOptions.DisplayAll)
                {
                    if (rangeOption == Chip.ChipRanges.All || item.Value.ChipRange == rangeOption)
                    {
                        toReturn.Add(item.Value);
                    }
                }
            }
            //toReturn.OrderBy(a => a.Name).ThenBy(a => a.averageDamage);
            switch (sortOptions)
            {
                case LibrarySortOptions.AvgDamage:
                    if (invert) return toReturn.OrderBy(a => a.AverageDamage).ThenBy(a => a.Name).ToList();
                    return toReturn.OrderByDescending(a => a.AverageDamage).ThenBy(a => a.Name).ToList();

                case LibrarySortOptions.Owned:
                    if (invert) return toReturn.OrderBy(a => a.ChipType).ThenBy(a => a.ChipCount).ThenBy(a => a.Name).ToList();
                    return toReturn.OrderBy(a => a.ChipType).ThenByDescending(a => a.ChipCount).ThenBy(a => a.Name).ToList();

                case LibrarySortOptions.Element:
                    if (invert) toReturn.OrderByDescending(a => a.ChipElement).ThenBy(a => a.ChipType).ThenBy(a => a.Name).ToList();
                    return toReturn.OrderBy(a => a.ChipElement).ThenBy(a => a.Name).ToList();

                case LibrarySortOptions.MaxDamage:
                    if (invert) return toReturn.OrderBy(a => a.MaxDamage).ThenBy(a => a.Name).ToList();
                    return toReturn.OrderByDescending(a => a.MaxDamage).ThenBy(a => a.Name).ToList();

                case LibrarySortOptions.Skill:
                    if (invert) return toReturn.OrderByDescending(a => a.ChipSkill).ThenBy(a => a.Name).ToList();
                    return toReturn.OrderBy(a => a.ChipSkill).ThenBy(a => a.Name).ToList();

                case LibrarySortOptions.Range:
                    if (invert) return toReturn.OrderByDescending(a => a.ChipRange).ThenBy(a => a.Name).ToList();
                    return toReturn.OrderBy(a => a.ChipRange).ThenBy(a => a.Name).ToList();

                case LibrarySortOptions.Name:
                default:
                    if (invert) return toReturn.OrderByDescending(a => a.ChipType).ThenByDescending(a => a.Name).ToList();
                    return toReturn.OrderBy(a => a.ChipType).ThenBy(a => a.Name).ToList();
            }
        }

        public uint JackOut()
        {
            uint countRefreshed = 0;
            foreach (var chip in this.Library)
            {
                countRefreshed += chip.Value.UsedInBattle;
                chip.Value.UsedInBattle = 0;
            }
            return countRefreshed;
        }

        public List<string> Search(string name)
        {
            name = name.ToLower();
            List<string> toReturn = new List<string>();
            foreach (var item in this.Library)
            {
                if (item.Key.Contains(name))
                {
                    toReturn.Add(item.Value.Name);
                }
            }
            return toReturn;
        }

        private static readonly Lazy<ChipLibrary> lazy = new Lazy<ChipLibrary>(() => new ChipLibrary());

        private readonly Dictionary<string, Chip> Library;

        private ChipLibrary()
        {
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                var json = wc.DownloadString("http://spartan364.hopto.org/chips.json");
                json = json.Replace("â€™", "'");
                var result = JsonConvert.DeserializeObject<List<Chip>>(json);
                this.Library = new Dictionary<string, Chip>(result.Count);
                result.ForEach(delegate (Chip aChip)
                {
                    this.Library.Add(aChip.Name.ToLower(), aChip);
                });
            }
        }
    }
}