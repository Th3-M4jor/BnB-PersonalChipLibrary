using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BnB_ChipLibraryGui
{
    public enum StatNames
    {
        Mind, Body, Spirit
    }

    [Serializable]
    public sealed class PlayerStats
    {
        public string NaviName { get; set; }
        public Chip.ChipElements NaviElement { get; private set; }
        private readonly byte[] NaviStats;
        private readonly byte[] NaviSkills;
        public string OperatorName { get; set; }
        private readonly byte[] OperatorStats;
        private readonly byte[] OperatorSkills;
        public byte ATKPlusInst { get; private set; }
        public byte HPPlusInst { get; private set; }
        public byte WPNLvLPlusInst { get; private set; }
        public byte CustomPlusInst { get; private set; }

        public event EventHandler<uint> HandSizeChanged;

        private static readonly Lazy<PlayerStats> lazy = new Lazy<PlayerStats>(() => new PlayerStats());

        public static PlayerStats Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        private PlayerStats()
        {
            if (!File.Exists("./stats.dat")) //save file doesn't exist
            {
                NaviSkills = new byte[9];
                NaviStats = new byte[3] { 1, 1, 1 }; //initialize stats to 1
                OperatorSkills = new byte[9];
                OperatorStats = new byte[3] { 1, 1, 1 };
                NaviElement = Chip.ChipElements.Null;
                NaviName = "";
                OperatorName = "";
                ATKPlusInst = 0;
                HPPlusInst = 0;
                WPNLvLPlusInst = 0;
                CustomPlusInst = 0;
                return;
            }

            var currStats = LoadUP();
            this.NaviElement = currStats.NaviElement;
            this.NaviName = currStats.NaviName;
            this.NaviSkills = currStats.NaviSkills;
            this.NaviStats = currStats.NaviStats;
            this.OperatorName = currStats.OperatorName;
            this.OperatorSkills = currStats.OperatorSkills;
            this.OperatorStats = currStats.OperatorStats;
            this.ATKPlusInst = currStats.ATKPlusInst;
            this.HPPlusInst = currStats.HPPlusInst;
            this.WPNLvLPlusInst = currStats.WPNLvLPlusInst;
        }

        public void ElementChanged(int element)
        {
            this.NaviElement = (Chip.ChipElements)element;
        }

        public void Save()
        {
            SaveData();
        }

        public string GetBonus(Chip.ChipSkills skill)
        {
            if (skill == Chip.ChipSkills.None)
            {
                return "N/A";
            }
            else if (skill == Chip.ChipSkills.Varies)
            {
                return "Special";
            }
            else
            {
                return NaviSkills[(int)skill].ToString("+#;-#;0");
            }
        }

        public byte GetOpSkill(Chip.ChipSkills skill) => this.OperatorSkills[(int)skill];

        public byte GetOpStat(StatNames stat) => this.OperatorStats[(int)stat];

        public byte GetNaviSkill(Chip.ChipSkills skill) => this.NaviSkills[(int)skill];

        public byte GetNaviStat(StatNames stat) => this.NaviStats[(int)stat];

        public bool NaviCanIncreaseSkill(Chip.ChipSkills skill, StatNames stat)
        {
            if (NaviSkills[(int)skill] < (NaviStats[(int)stat] * 4))
            {
                return true;
            }
            else return false;
        }

        public void NaviIncreaseSkill(Chip.ChipSkills skill)
        {
            NaviSkills[(int)skill]++;
        }

        public bool NaviDecreaseSkill(Chip.ChipSkills skill)
        {
            if (NaviSkills[(int)skill] != 0)
            {
                NaviSkills[(int)skill]--;
                return true;
            }
            return false;
        }

        public bool OpCanIncreaseSkill(Chip.ChipSkills skill, StatNames stat)
        {
            if (OperatorSkills[(int)skill] < (OperatorStats[(int)stat] * 4))
            {
                return true;
            }
            else return false;
        }

        public void OpIncreaseSkill(Chip.ChipSkills skill)
        {
            OperatorSkills[(int)skill]++;
        }

        public void OpDecreaseSkill(Chip.ChipSkills skill)
        {
            if (OperatorSkills[(int)skill] != 0)
            {
                OperatorSkills[(int)skill]--;
            }
        }

        public bool OpIncreaseStat(StatNames stat)
        {
            if (OperatorStats[(int)stat] < 5)
            {
                OperatorStats[(int)stat]++;
                return true;
            }
            return false;
        }

        public bool NaviIncreaseStat(StatNames stat)
        {
            if (NaviStats[(int)stat] < 5)
            {
                NaviStats[(int)stat]++;
                if (stat == StatNames.Mind) HandSizeChanged.Invoke(this, CustomPlusInst + (uint)NaviStats[(int)StatNames.Mind]);
                return true;
            }
            return false;
        }

        public bool NaviDecreaseStat(StatNames stat)
        {
            if (NaviStats[(int)stat] != 1)
            {
                NaviStats[(int)stat]--;
                if (stat == StatNames.Mind) HandSizeChanged.Invoke(this, CustomPlusInst + (uint)NaviStats[(int)StatNames.Mind]);
                return true;
            }
            return false;
        }

        public bool OpDecreaseStat(StatNames stat)
        {
            if (OperatorStats[(int)stat] != 1)
            {
                OperatorStats[(int)stat]--;
                return true;
            }
            return false;
        }

        private static void SaveData()
        {
            IFormatter formatter = new BinaryFormatter();
            var stream = new FileStream("./stats.dat", FileMode.Create);
            formatter.Serialize(stream, lazy.Value);
            stream.Dispose();
        }

        private static PlayerStats LoadUP()
        {
            IFormatter formatter = new BinaryFormatter();
            var stream = new FileStream("./stats.dat", FileMode.Open, FileAccess.Read);
            PlayerStats stats = (PlayerStats)formatter.Deserialize(stream);
            stream.Dispose();
            return stats;
        }

        public byte IncCustomPlus()
        {
            CustomPlusInst++;
            HandSizeChanged.Invoke(this, CustomPlusInst + (uint)NaviStats[(int)StatNames.Mind]);
            return CustomPlusInst;
        }

        public byte DecCustomPlus()
        {
            if (CustomPlusInst == 0) return CustomPlusInst;
            else CustomPlusInst--;
            HandSizeChanged.Invoke(this, CustomPlusInst);
            return CustomPlusInst;
        }
    }
}