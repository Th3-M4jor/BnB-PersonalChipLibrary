using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BnB_ChipLibraryGui
{

    public class Chip
    {
        private static readonly char[] damageDelims = { 'd', ' ' };
        public enum ChipElements
        {
            Fire, Aqua, Elec, Wood, Wind, Sword, Break, Cursor, Recovery, Invis, Object, Null
        }

        public enum ChipRanges
        {
            Far, Near, Close, Self
        }

        public enum ChipTypes
        {
            Standard, Mega, Giga
        }

        public enum ChipSkills
        {
            Sense, Info, Coding, Strength, Speed, Stamina, Charm, Bravery, Affinity, None
        }

        public string Name { get; set; }
        public ChipRanges ChipRange { get; private set; }
        public ChipSkills ChipSkill { get; private set; }

        public string Damage
        {
            get => Damage;
            set
            {
                if (value == null || value == "N/A" || value == string.Empty)
                {
                    this.AverageDamage = 0;
                }
                else
                {
                    var avg = value.Split(damageDelims);
                    uint numDice = uint.Parse(avg[0]);
                    uint dieSize = uint.Parse(avg[1]);
                    this.AverageDamage = ((dieSize / 2m) + 0.5m) * numDice;
                }
                Damage = value;
            }
        }

        public ChipElements ChipElement { get; private set; }
        public ChipTypes ChipType { get; private set; }
        public decimal AverageDamage { get; private set; }
        public string Description { get; set; }
        public string All { get; set; }
        public int ChipCount { get; private set; }
        public int UsedInBattle {
            get => UsedInBattle;
            set
            {
                if (value > ChipCount) throw new ArgumentOutOfRangeException();
                UsedInBattle = value;
            }
        }

        public string Range {
            get => ChipRange.ToString();
            set
            {
                this.ChipRange = (ChipRanges)Enum.Parse(typeof(ChipRanges), value);
            }
        }

        public string Skill {
            get => ChipSkill.ToString();
            set
            {
                if (value == "N/A" || value == null || value == string.Empty)
                {
                    this.ChipSkill = ChipSkills.None;
                }
                else
                {
                    this.ChipSkill = (ChipSkills)Enum.Parse(typeof(ChipSkills), value);
                }
            }
        }

        public string Element {
            get => ChipElement.ToString();
            set
            {
                this.ChipElement = (ChipElements)Enum.Parse(typeof(ChipElements), value);
            }
        }

        public string Type {
            get => ChipType.ToString();
            set
            {
                if (value == null || value == string.Empty)
                {
                    this.ChipType = ChipTypes.Standard;
                }
                else
                {
                    this.ChipType = (ChipTypes)Enum.Parse(typeof(ChipTypes), value);
                }
            }
        }

        public Chip()
        {
            AverageDamage = 0;
            ChipCount = 0;
            UsedInBattle = 0;
        }

        public Chip(string name, string range, string skill, string damage, string element, string type , string description, string All)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.ChipRange = (ChipRanges) Enum.Parse(typeof(ChipRanges), range);
            if(skill == "N/A" || skill == null || skill == string.Empty)
            {
                this.ChipSkill = ChipSkills.None;
            }
            else
            {
                this.ChipSkill = (ChipSkills)Enum.Parse(typeof(ChipSkills), skill);
            }
            this.Damage = damage ?? "N/A";
            this.ChipElement = (ChipElements)Enum.Parse(typeof(ChipElements), element);
            if (type == null || type == string.Empty)
            {
                this.ChipType = ChipTypes.Standard;
            }
            else
            {
                this.ChipType = (ChipTypes)Enum.Parse(typeof(ChipTypes), type);
            }
            this.Description = description ?? throw new ArgumentNullException(nameof(description));
            this.All = All ?? throw new ArgumentNullException(nameof(All));
            this.ChipCount = 0;
            this.UsedInBattle = 0;
            if (damage == null || damage == "N/A" || damage == string.Empty)
            {
                this.AverageDamage = 0;
            }
            else
            {
                var avg = damage.Split(damageDelims);
                uint numDice = uint.Parse(avg[0]);
                uint dieSize = uint.Parse(avg[1]);
                this.AverageDamage = ((dieSize / 2m) + 0.5m) * numDice;
            }

        }

        /// <summary>
        /// Increases the number of copies of the held chip by 1
        /// </summary>
        /// <param name="chip">The chip to have it's count increased by 1</param>
        /// <returns></returns>
        public static Chip operator++(Chip chip)
        {
            chip.ChipCount++;
            return chip;
        }

        /// <summary>
        /// Reduces the number of held copies of the said chip by 1, cannot go below zero
        /// </summary>
        /// <param name="chip">The chip to have it's count reduced by 1</param>
        /// <returns>The chip</returns>
        public static Chip operator--(Chip chip)
        {
            chip.ChipCount--;
            return chip;
        }


        /// <summary>
        /// Converts it to a string that should look exactly like the documentation
        /// </summary>
        /// <returns>The chip as a string like the documentation has it</returns>
        public override string ToString()
        {
            return this.All;
        }


    }
}
