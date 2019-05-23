using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BnB_ChipLibraryGui
{
    /// <summary>
    /// Interaction logic for Hand.xaml
    /// </summary>
    public partial class Hand : Window
    {
        private List<Chip> ChipsInHand;

        public Hand(List<Chip> hand)
        {
            InitializeComponent();
            this.ChipsInHand = hand;
            this.PlayerHand.ItemsSource = this.ChipsInHand;
        }

        public void AddChip(Chip newChip)
        {
            ChipsInHand.Add(newChip);
            this.PlayerHand.ItemsSource = ChipsInHand;
        }
    }
}
