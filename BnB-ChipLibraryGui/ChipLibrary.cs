﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public const string ChipUrl = "https://docs.google.com/feeds/download/documents/export/Export?id=1lvAKkymOplIJj6jS-N5__9aLIDXI6bETIMz01MK9MfY&exportFormat=txt";

        public const string regexVal = @"(.+?)\s-\s(.+?)\s\|\s(.+?)\s\|\s(.+?)\s\|\s(\d+d\d+|--)\s?(?:damage)?\s?\|?\s?(Mega|Giga)?\s\|\s(\d+|\d+-\d+|--)\s?(?:hits?)?\.?";

        private static readonly Lazy<ChipLibrary> lazy = new Lazy<ChipLibrary>(() => new ChipLibrary());

        private readonly Dictionary<string, Chip> Library;

        public static ChipLibrary Instance
        {
            get => lazy.Value;
        }

        private ChipLibrary()
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            try
            {
                string json = wc.DownloadString("http://spartan364.hopto.org/chips.json");
                json = json.Replace("â€™", "'"); //replace unicode apostraphe with ascii one
                var result = JsonConvert.DeserializeObject<List<Chip>>(json);
                Library = new Dictionary<string, Chip>(result.Count);

                foreach (Chip chip in result)
                {
                    Library.Add(chip.Name.ToLower(), chip);
                }
                SaveBackup(json);
            }
            catch (Exception e) when (e is System.Net.WebException)
            {
                string json = GetLocalFile();
                if (json == string.Empty)
                {
                    System.Windows.MessageBox.Show("No data backup and cannot contact server");
                    throw new NullReferenceException("Cannot access server and no backup");
                }
                var result = JsonConvert.DeserializeObject<List<Chip>>(json);
                this.Library = new Dictionary<string, Chip>(result.Count);
                result.ForEach(delegate (Chip aChip)
                {
                    this.Library.Add(aChip.Name.ToLower(), aChip);
                });
            }
            finally
            {
                wc.Dispose();
            }
        }

        public Chip GetChip(string name)
        {
            bool exists = this.Library.TryGetValue(name.ToLower(), out Chip toReturn);
            if (exists) return toReturn;
            else return null;
        }

        public List<Chip> GetList(ChipListOptions AllOrOwned, LibrarySortOptions sortOptions, Chip.ChipRanges rangeOption, bool invert)
        {
            List<Chip> toReturn;

            if (AllOrOwned == ChipListOptions.DisplayAll)
            {
                toReturn = (from kvp in this.Library
                            where rangeOption == Chip.ChipRanges.All ||
                            kvp.Value.ChipRange == rangeOption
                            select kvp.Value).ToList();
            }
            else
            {
                toReturn = (from kvp in this.Library
                            where (rangeOption == Chip.ChipRanges.All ||
                            kvp.Value.ChipRange == rangeOption) && kvp.Value.ChipCount != 0
                            select kvp.Value).ToList();
            }
            switch (sortOptions)
            {
                case LibrarySortOptions.AvgDamage:
                    if (invert) return toReturn.OrderBy(a => a.AverageDamage).ThenBy(a => a.Name).ToList();
                    return toReturn.OrderByDescending(a => a.AverageDamage).ThenBy(a => a.Name).ToList();

                case LibrarySortOptions.Owned:
                    if (invert) return toReturn.OrderBy(a => a.ChipType).ThenBy(a => a.ChipCount).ThenBy(a => a.Name).ToList();
                    return toReturn.OrderBy(a => a.ChipType).ThenByDescending(a => a.ChipCount).ThenBy(a => a.Name).ToList();

                case LibrarySortOptions.Element:
                    if (invert) return toReturn.OrderByDescending(a => a.ChipElement[0]).ThenBy(a => a.Name).ToList();
                    return toReturn.OrderBy(a => a.ChipElement[0]).ThenBy(a => a.Name).ToList();

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

        public List<Chip> Search(string name)
        {
            name = name.ToLower();
            /*List<Chip> toReturn = new List<Chip>();
            foreach (var item in this.Library)
            {
                if (item.Key.Contains(name))
                {
                    toReturn.Add(item.Value);
                }
            }
            return toReturn;*/
            return (from kvp in this.Library where kvp.Key.Contains(name) select kvp.Value).ToList();
        }

        public string GenerateExport()
        {
            StringBuilder build = new StringBuilder();
            var toSave = this.GetList(ChipListOptions.DisplayOwned,
                LibrarySortOptions.Name, Chip.ChipRanges.All, false);
            uint numWritten = 0;
            foreach (Chip chip in toSave)
            {
                if (chip.ChipCount <= 0)
                {
                    continue;
                }

                build.Append(chip.Name);
                numWritten++;
                if (chip.ChipCount > 1)
                {
                    build.Append(" x");
                    build.Append(chip.ChipCount);
                }

                if (chip.UsedInBattle > 0)
                {
                    build.Append(" (");
                    build.Append(chip.UsedInBattle);
                    build.Append(" Used)");
                }

                build.Append(", ");
            }
            if (numWritten == 0)
            {
                throw new Exception("Pack is empty");
            }
            build.Remove(build.Length - 2, 2);
            return build.ToString();
        }

        private void SaveBackup(string backup)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                if (isoStore.FileExists("chips.json"))
                {
                    StreamReader reader = new StreamReader(isoStore.OpenFile("chips.json", FileMode.Open));
                    string json = reader.ReadToEnd();
                    reader.Close();
                    if (backup.Equals(json)) return;
                }
                StreamWriter writer = new StreamWriter(isoStore.OpenFile("chips.json", FileMode.Create));
                writer.Write(backup);
                writer.Close();
            }
        }

        private string GetLocalFile()
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                if (isoStore.FileExists("chips.json"))
                {
                    var stream = isoStore.OpenFile("chips.json", FileMode.Open);
                    StreamReader reader = new StreamReader(stream);
                    string json = reader.ReadToEnd();
                    reader.Close();
                    stream.Close();
                    return json;
                }
                else return string.Empty;
            }
        }
    }
}