using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BnB_ChipLibraryGui
{
    public class Chip
    {
        public readonly string name;
        public readonly string range;
        public readonly string skill;
        public readonly string damage;
        public readonly string element;
        public readonly string type;
        public readonly string description;
        public readonly string all;
        public uint chipCount;
        public uint usedInBattle;

        [JsonConstructor]
        public Chip(string name, string range, string skill, string damage, string element, string type , string description, string All)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.range = range ?? throw new ArgumentNullException(nameof(range));
            this.skill = skill ?? "N/A";
            this.damage = damage ?? "N/A";
            this.element = element ?? throw new ArgumentNullException(nameof(element));
            this.type = type ?? "Standard";
            this.description = description ?? throw new ArgumentNullException(nameof(description));
            this.all = All ?? throw new ArgumentNullException(nameof(All));
            this.chipCount = 0;
            this.usedInBattle = 0;
        }

        public Chip(string name, string range, string skill, string damage, string element, string type, string description, string All, uint ChipCount, uint usedInBattle = 0)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.range = range ?? throw new ArgumentNullException(nameof(range));
            this.skill = skill ?? "N/A";
            this.damage = damage ?? "N/A";
            this.element = element ?? throw new ArgumentNullException(nameof(element));
            this.description = description ?? throw new ArgumentNullException(nameof(description));
            this.all = All ?? throw new ArgumentNullException(nameof(All));
            this.chipCount = ChipCount;
            this.usedInBattle = usedInBattle;
        }

        /// <summary>
        /// Increases the number of copies of the held chip by 1
        /// </summary>
        /// <param name="chip">The chip to have it's count increased by 1</param>
        /// <returns></returns>
        public static Chip operator++(Chip chip)
        {
            chip.chipCount++;
            return chip;
        }

        /// <summary>
        /// Reduces the number of held copies of the said chip by 1, cannot go below zero
        /// </summary>
        /// <param name="chip">The chip to have it's count reduced by 1</param>
        /// <returns>The chip</returns>
        public static Chip operator--(Chip chip)
        {
            if(chip.chipCount > 0)
            {
                chip.chipCount--;
            }
            return chip;
        }


        /// <summary>
        /// Converts it to a string that should look exactly like the documentation
        /// </summary>
        /// <returns>The chip as a string like the documentation has it</returns>
        public override string ToString()
        {
            return this.all;
        }


    }
}
