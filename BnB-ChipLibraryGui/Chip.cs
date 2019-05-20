using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BnB_ChipLibraryGui
{
    class Chip
    {
        readonly string name;
        readonly string range;
        readonly string skill;
        readonly string damage;
        readonly string element;
        readonly string description;
        public uint ChipCount { get; private set; }

        public Chip(string name, string range, string skill, string damage, string element, string description, uint chipCount = 0)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.range = range ?? throw new ArgumentNullException(nameof(range));
            this.skill = skill ?? throw new ArgumentNullException(nameof(skill));
            this.damage = damage ?? throw new ArgumentNullException(nameof(damage));
            this.element = element ?? throw new ArgumentNullException(nameof(element));
            this.description = description ?? throw new ArgumentNullException(nameof(description));
            this.ChipCount = chipCount;
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
            if(chip.ChipCount > 0)
            {
                chip.ChipCount--;
            }
            return chip;
        }


        /// <summary>
        /// Converts it to a string that should look exactly like the documentation
        /// </summary>
        /// <returns>The chip as a string like the documentation has it</returns>
        public override string ToString()
        {
            StringBuilder toReturn = new StringBuilder(this.name);
            toReturn.Append(" - ");
            bool commaNeeded = false;
            if(this.skill != "N/A")
            {
                toReturn.Append(this.skill);
                commaNeeded = true;
            }
            
            if(this.range != "N/A")
            {
                if(commaNeeded)
                {
                    toReturn.Append(", ");
                }
                toReturn.Append(this.range);
                commaNeeded = true;
            }

            if(this.damage != "N/A")
            {
                if (commaNeeded)
                {
                    toReturn.Append(", ");
                }
                toReturn.Append(this.damage);
            }

            toReturn.Append(". ");
            toReturn.Append(this.description);
            toReturn.Append(" (");
            toReturn.Append(this.element);
            toReturn.Append(')');

            return toReturn.ToString();
        }


    }
}
