using System;
using System.Windows.Media.Imaging;

namespace BnB_ChipLibraryGui
{
    public class HandChip
    {

        /*
         * <DataGridTextColumn Header="Num" Binding="{Binding Num}" IsReadOnly="True"/>
         */
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

        public string Skill
        {
            get
            {
                return this.self.Skill;
            }
        }

        public string Range
        {
            get
            {
                return this.self.Range;
            }
        }

        public string Damage
        {
            get => this.self.Damage;
        }

        public BitmapImage ElementImage
        {
            get => ChipImages.Instance[this.self.ChipElement];
        }

        public char ChipClass
        {
            get => this.self.ChipClass;
        }

        public string Description
        {
            get => self.Description;
        }

        public HandChip(string chipName)
        {
            self = ChipLibrary.Instance.GetChip(chipName) ?? throw new ArgumentException("NonExistentChip");
            _name = chipName;
            Used = false;
        }

        public bool Used { get; set; }

        private string _name;
        private Chip self;

    }
}
