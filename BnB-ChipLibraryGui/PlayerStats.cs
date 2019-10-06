using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MessageBox = System.Windows.MessageBox;

namespace BnB_ChipLibraryGui
{
    public enum StatNames
    {
        Mind, Body, Spirit
    }

    public class HandSizeChangedEventArgs : EventArgs
    {
        public uint NewHandSize;

        public HandSizeChangedEventArgs() : base() => NewHandSize = 0;

        public HandSizeChangedEventArgs(uint NewHandSize) : base() => this.NewHandSize = NewHandSize;
    }

    //[Serializable]
    public sealed class PlayerStats
    {
        public string NaviName { get => statData.NaviName; set => statData.NaviName = value; }
        public string OperatorName { get => statData.OperatorName; set => statData.OperatorName = value; }
        public Chip.ChipElements NaviElement { get => statData.NaviElement; set => statData.NaviElement = value; }
        public byte CustomPlusInst { get => statData.CustomPlusInst; set => statData.CustomPlusInst = value; }
        public byte ATKPlusInst { get => statData.ATKPlusInst; set => statData.ATKPlusInst = value; }
        public byte HPPlusInst { get => statData.HPPlusInst; set => statData.HPPlusInst = value; }
        public byte WPNLvLPlusInst { get => statData.WPNLvLPlusInst; set => statData.WPNLvLPlusInst = value; }

        [Serializable]
        private class StatData
        {
            public string NaviName { get; set; }
            public Chip.ChipElements NaviElement { get; set; }
            public byte[] NaviStats;
            public byte[] NaviSkills;
            public string OperatorName { get; set; }
            public byte[] OperatorStats;
            public byte[] OperatorSkills;
            public byte ATKPlusInst { get; set; }
            public byte HPPlusInst { get; set; }
            public byte WPNLvLPlusInst { get; set; }
            public byte CustomPlusInst { get; set; }
        }

        private readonly StatData statData;

        public event EventHandler<HandSizeChangedEventArgs> HandSizeChanged;

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
            statData = new StatData();
            if (!File.Exists("./stats.dat")) //save file doesn't exist
            {
                statData.NaviSkills = new byte[9];
                statData.NaviStats = new byte[3] { 1, 1, 1 }; //initialize stats to 1
                statData.OperatorSkills = new byte[9];
                statData.OperatorStats = new byte[3] { 1, 1, 1 };
                statData.NaviElement = Chip.ChipElements.Null;
                statData.NaviName = "";
                statData.OperatorName = "";
                statData.ATKPlusInst = 0;
                statData.HPPlusInst = 0;
                statData.WPNLvLPlusInst = 0;
                statData.CustomPlusInst = 0;
                return;
            }
            try
            {
                var currStats = LoadUP();
                statData.NaviElement = currStats.NaviElement;
                statData.NaviName = currStats.NaviName;
                statData.NaviSkills = currStats.NaviSkills;
                statData.NaviStats = currStats.NaviStats;
                statData.OperatorName = currStats.OperatorName;
                statData.OperatorSkills = currStats.OperatorSkills;
                statData.OperatorStats = currStats.OperatorStats;
                statData.ATKPlusInst = currStats.ATKPlusInst;
                statData.HPPlusInst = currStats.HPPlusInst;
                statData.WPNLvLPlusInst = currStats.WPNLvLPlusInst;
            }
            catch (Exception e) when (e is SerializationException)
            {
                MessageBox.Show("Unable to load Player Stats, resetting to zero");
                File.Delete("./stats.dat");
                statData.NaviSkills = new byte[9];
                statData.NaviStats = new byte[3] { 1, 1, 1 }; //initialize stats to 1
                statData.OperatorSkills = new byte[9];
                statData.OperatorStats = new byte[3] { 1, 1, 1 };
                statData.NaviElement = Chip.ChipElements.Null;
                statData.NaviName = "";
                statData.OperatorName = "";
                statData.ATKPlusInst = 0;
                statData.HPPlusInst = 0;
                statData.WPNLvLPlusInst = 0;
                statData.CustomPlusInst = 0;
            }
        }

        public void ElementChanged(int element)
        {
            statData.NaviElement = (Chip.ChipElements)element;
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
                return statData.NaviSkills[(int)skill].ToString("+#;-#;0");
            }
        }

        public byte GetOpSkill(Chip.ChipSkills skill) => statData.OperatorSkills[(int)skill];

        public byte GetOpStat(StatNames stat) => statData.OperatorStats[(int)stat];

        public byte GetNaviSkill(Chip.ChipSkills skill) => statData.NaviSkills[(int)skill];

        public byte GetNaviStat(StatNames stat) => statData.NaviStats[(int)stat];

        public bool NaviCanIncreaseSkill(Chip.ChipSkills skill, StatNames stat)
        {
            if (statData.NaviSkills[(int)skill] < (statData.NaviStats[(int)stat] * 4))
            {
                return true;
            }
            else return false;
        }

        public void NaviIncreaseSkill(Chip.ChipSkills skill)
        {
            statData.NaviSkills[(int)skill]++;
        }

        public bool NaviDecreaseSkill(Chip.ChipSkills skill)
        {
            if (statData.NaviSkills[(int)skill] != 0)
            {
                statData.NaviSkills[(int)skill]--;
                return true;
            }
            return false;
        }

        public bool OpCanIncreaseSkill(Chip.ChipSkills skill, StatNames stat)
        {
            if (statData.OperatorSkills[(int)skill] < (statData.OperatorStats[(int)stat] * 4))
            {
                return true;
            }
            else return false;
        }

        public void OpIncreaseSkill(Chip.ChipSkills skill)
        {
            statData.OperatorSkills[(int)skill]++;
        }

        public void OpDecreaseSkill(Chip.ChipSkills skill)
        {
            if (statData.OperatorSkills[(int)skill] != 0)
            {
                statData.OperatorSkills[(int)skill]--;
            }
        }

        public bool OpIncreaseStat(StatNames stat)
        {
            if (statData.OperatorStats[(int)stat] < 5)
            {
                statData.OperatorStats[(int)stat]++;
                return true;
            }
            return false;
        }

        public bool NaviIncreaseStat(StatNames stat)
        {
            if (statData.NaviStats[(int)stat] < 5)
            {
                statData.NaviStats[(int)stat]++;
                if (stat == StatNames.Mind) HandSizeChanged?.Invoke(this, new HandSizeChangedEventArgs(CustomPlusInst + (uint)statData.NaviStats[(int)StatNames.Mind]));
                return true;
            }
            return false;
        }

        public bool NaviDecreaseStat(StatNames stat)
        {
            if (statData.NaviStats[(int)stat] != 1)
            {
                statData.NaviStats[(int)stat]--;
                if (stat == StatNames.Mind) HandSizeChanged?.Invoke(this, new HandSizeChangedEventArgs(CustomPlusInst + (uint)statData.NaviStats[(int)StatNames.Mind]));
                return true;
            }
            return false;
        }

        public bool OpDecreaseStat(StatNames stat)
        {
            if (statData.OperatorStats[(int)stat] != 1)
            {
                statData.OperatorStats[(int)stat]--;
                return true;
            }
            return false;
        }

        private static void SaveData()
        {
            IFormatter formatter = new BinaryFormatter();
            var stream = new FileStream("./stats.dat", FileMode.Create);
            formatter.Serialize(stream, lazy.Value.statData);
            stream.Dispose();
        }

        private static StatData LoadUP()
        {
            FileStream stream = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                stream = new FileStream("./stats.dat", FileMode.Open, FileAccess.Read);
                StatData stats = (StatData)formatter.Deserialize(stream);
                //stream.Dispose();
                return stats;
            }
            finally
            {
                stream?.Dispose();
            }
        }

        public byte IncCustomPlus()
        {
            statData.CustomPlusInst++;
            HandSizeChanged?.Invoke(this, new HandSizeChangedEventArgs(CustomPlusInst + (uint)statData.NaviStats[(int)StatNames.Mind]));
            return statData.CustomPlusInst;
        }

        public byte DecCustomPlus()
        {
            if (statData.CustomPlusInst == 0) return statData.CustomPlusInst;
            else statData.CustomPlusInst--;
            HandSizeChanged?.Invoke(this, new HandSizeChangedEventArgs(CustomPlusInst + (uint)statData.NaviStats[(int)StatNames.Mind]));
            return statData.CustomPlusInst;
        }
    }
}