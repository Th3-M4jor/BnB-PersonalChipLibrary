using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        private int _numValue = 10;

        public int NumValue
        {
            get { return _numValue; }
            set
            {
                _numValue = value;
                txtNum.Text = value.ToString();
            }
        }

        private void CmdUp_Click(object sender, RoutedEventArgs e)
        {
            NumValue++;
        }

        private void CmdDown_Click(object sender, RoutedEventArgs e)
        {
            NumValue--;
        }

        private void TxtNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNum == null)
            {
                return;
            }

            if (!int.TryParse(txtNum.Text, out _numValue))
                txtNum.Text = _numValue.ToString();
        }

        //private List<HandChip> ChipsInHand;
        private readonly ObservableCollection<HandChip> ChipsInHand;

        public Hand(IEnumerable<HandChip> hand)
        {
            InitializeComponent();
            txtNum.Text = 10 + "";
            this.ChipsInHand = new ObservableCollection<HandChip>(hand);
            this.PlayerHand.ItemsSource = this.ChipsInHand;
            this.ChipsInHand.CollectionChanged += HandCollectionChanged;
        }

        public void AddChip(Chip newChip)
        {
            if(NumValue == ChipsInHand.Count)
            {
                MessageBox.Show("Cannot add another copy of " + newChip.Name + " to your hand\nYour hand is full", "AddToHand", MessageBoxButton.OK);
                return;
            }
            newChip.NumInHand++;
            ChipsInHand.Add(newChip.MakeHandChip());
        }

        public int ClearHand()
        {
            int toReturn = this.ChipsInHand.Count;
            foreach(HandChip chip in ChipsInHand)
            {
                ChipLibrary.Instance.GetChip(chip.Name).NumInHand--;
            }
            this.ChipsInHand.Clear();
            return toReturn;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != null && PlayerHand.SelectedItems != null && PlayerHand.SelectedItems.Count == 1)
            {
                DataGridRow dgr = PlayerHand.ItemContainerGenerator.ContainerFromItem(PlayerHand.SelectedItem) as DataGridRow;
                if (!dgr.IsMouseOver)
                {
                    (dgr as DataGridRow).IsSelected = false;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;
            if (!(PlayerHand.SelectedItem is HandChip selected)) return;
            foreach(HandChip chip in ChipsInHand)
            {
                if(chip.Name == selected.Name)
                {
                    ChipsInHand.Remove(chip);
                    ChipLibrary.Instance.GetChip(selected.Name).NumInHand--;
                    break;
                }
            }

        }

        private void HandCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(ChipsInHand.Count > 0 && this.Visibility == Visibility.Hidden)
            {
                this.Show();
            }
            else if(ChipsInHand.Count == 0 && this.Visibility != Visibility.Hidden)
            {
                this.Hide();
            }
        }
    }
}
