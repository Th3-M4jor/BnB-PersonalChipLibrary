using Newtonsoft.Json;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BnB_ChipLibraryGui
{
    public class Chip
    {
        public enum ChipElements
        {
            Fire, Aqua, Elec, Wood, Wind, Sword, Break, Cursor, Recovery, Invis, Object, Null
        }

        public enum ChipRanges
        {
            Far, Near, Close, Self, All
        }

        public enum ChipSkills
        {
            Sense, Info, Coding, Strength, Speed, Stamina, Charm, Bravery, Affinity, Varies, Mind, Body, Spirit, None
        }

        public enum ChipTypes
        {
            Standard, Mega, Giga
        }

        public static readonly SolidColorBrush MegaChipColor;
        public static readonly SolidColorBrush GigaChipColor;

        static Chip()
        {
            MegaChipColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#90f8f8");
            MegaChipColor.Freeze();
            GigaChipColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#f8c8d8");
            GigaChipColor.Freeze();
        }

        [JsonProperty("All")]
        public string All { get; set; }

        public decimal AverageDamage { get; private set; }

        public char ChipClass
        {
            get => ChipType.ToString()[0];
        }

        public byte NumInHand
        {
            get => _numInHand;
            set
            {
                if (this.ChipCount < 0)
                {
                    _numInHand = 0;
                    return;
                }

                if (value <= this.ChipCount)
                {
                    _numInHand = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Can't add chips you don't have");
                }
            }
        }

        public sbyte ChipCount { get; set; }

        public SolidColorBrush ChipColor
        {
            get
            {
                if (UsedInBattle >= ChipCount && ChipCount != 0)
                {
                    return Brushes.Gray;
                }

                switch (this.ChipType)
                {
                    case ChipTypes.Mega:
                        return MegaChipColor;

                    case ChipTypes.Giga:
                        return GigaChipColor;

                    default:
                        return Brushes.White;
                }
            }
        }// private set; }

        public ChipElements[] ChipElement { get; private set; }
        public ChipRanges ChipRange { get; private set; }
        public ChipSkills ChipSkill { get; private set; }

        [JsonProperty("Hits")]
        public string Hits { get; set; }

        public ChipTypes ChipType { get; private set; }

        [JsonProperty("Damage")]
        public string Damage
        {
            get => _damage;
            set
            {
                if (value == null || value == "N/A" || value == string.Empty || value == "--")
                {
                    this.AverageDamage = 0;
                    this.MaxDamage = 0;
                }
                else
                {
                    var avg = value.Split(damageDelims);
                    uint numDice = uint.Parse(avg[0]);
                    uint dieSize = uint.Parse(avg[1]);
                    this.AverageDamage = ((dieSize / 2m) + 0.5m) * numDice;
                    this.MaxDamage = dieSize * numDice;
                }
                this._damage = value;
            }
        }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Element")]
        public string[] Element
        {
            get => this._element;
            set
            {
                this.ChipElement = new ChipElements[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    this.ChipElement[i] = (ChipElements)Enum.Parse(typeof(ChipElements), value[i]);
                }

                this._element = value;
            }
        }

        public BitmapImage ElementImage
        {
            get => ChipImages.Instance[ChipElement];
        }

        public uint MaxDamage { get; private set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Range")]
        public string Range
        {
            get => ChipRange.ToString();
            set
            {
                this.ChipRange = (ChipRanges)Enum.Parse(typeof(ChipRanges), value);
            }
        }

        [JsonProperty("Skills")]
        public string[] Skill
        {
            set
            {
                if (value.Length > 1)
                {
                    this.ChipSkill = ChipSkills.Varies;
                }
                else if (value[0] == "N/A" || value[0] == null || value[0] == "--" || value[0] == string.Empty)
                {
                    this.ChipSkill = ChipSkills.None;
                }
                else
                {
                    this.ChipSkill = (ChipSkills)Enum.Parse(typeof(ChipSkills), value[0]);
                }
            }
        }

        [JsonProperty("Type")]
        public string Type
        {
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

                /*switch (this.ChipType)
                {
                    case ChipTypes.Standard:
                        this.ChipColor = Brushes.White;
                        break;

                    case ChipTypes.Mega:
                        this.ChipColor = MegaChipColor;
                        break;

                    case ChipTypes.Giga:
                        this.ChipColor = GigaChipColor;
                        break;
                }*/
            }
        }

        public byte UsedInBattle { get; set; }

        public Chip()
        {
            AverageDamage = 0;
            ChipCount = 0;
            UsedInBattle = 0;
            NumInHand = 0;
        }

        public Chip(string name, string range, string[] skill, string damage, string hits, string[] element, string type, string description, string All)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Range = range;
            this.Skill = skill;
            this.Damage = damage ?? "N/A";
            this.Element = element;
            this.Type = type;
            this.Description = description ?? throw new ArgumentNullException(nameof(description));
            this.All = All ?? throw new ArgumentNullException(nameof(All));
            this.ChipCount = 0;
            this.UsedInBattle = 0;
            this.NumInHand = 0;
            this.Hits = hits;
        }

        public HandChip MakeHandChip()
        {
            return new HandChip(this.Name);
        }

        /// <summary>
        /// Reduces the number of held copies of the said chip by 1, cannot go below zero
        /// </summary>
        /// <param name="chip">The chip to have it's count reduced by 1</param>
        /// <returns>The chip</returns>
        public static Chip operator --(Chip chip)
        {
            chip.ChipCount--;
            return chip;
        }

        /// <summary>
        /// Increases the number of copies of the held chip by 1
        /// </summary>
        /// <param name="chip">The chip to have it's count increased by 1</param>
        /// <returns></returns>
        public static Chip operator ++(Chip chip)
        {
            chip.ChipCount++;
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

        public override bool Equals(object obj)
        {
            if (obj is Chip)
            {
                return this.Name == ((Chip)obj).Name;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        private static readonly char[] damageDelims = { 'd', ' ' };

        private string _damage;

        private byte _numInHand;

        private string[] _element;
    }
}