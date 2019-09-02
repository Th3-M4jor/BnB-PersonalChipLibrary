using System;
using System.Windows.Media.Imaging;

namespace BnB_ChipLibraryGui
{
    public class HandChip
    {
        /*
         * <DataGridTextColumn Header="Num" Binding="{Binding Num}" IsReadOnly="True"/>
         */

        private string _name;

        private Chip self;

        public char ChipClass
        {
            get => this.self.ChipClass;
        }

        public string Damage
        {
            get => this.self.Damage;
        }

        public string Description
        {
            get => self.Description;
        }

        public BitmapImage ElementImage
        {
            get => ChipImages.Instance[this.self.ChipElement];
        }

        public string Name
        {
            get => _name;
            set
            {
                Chip newSelf = ChipLibrary.Instance.GetChip(value);
                this.self = newSelf ?? throw new ArgumentException("NonExistentChip");
                _name = value;
            }
        }

        public string Range
        {
            get
            {
                return this.self.Range;
            }
        }

        public Chip.ChipSkills Skill
        {
            get
            {
                return this.self.ChipSkill;
            }
        }

        public string Hits
        {
            get
            {
                return this.self.Hits;
            }
        }

        public bool Used { get; set; }

        public HandChip(string chipName)
        {
            self = ChipLibrary.Instance.GetChip(chipName) ?? throw new ArgumentException("NonExistentChip");
            _name = chipName;
            Used = false;
        }
    }
}