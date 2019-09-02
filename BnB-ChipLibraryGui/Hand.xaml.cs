using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        public string GetHand()
        {
            if (ChipsInHand.Count == 0)
            {
                string[] EmptyString = new string[1] { "empty" };
                return JsonConvert.SerializeObject(EmptyString);
                //return "[\"empty\"]";
            }

            string[] HandToReturn = new string[ChipsInHand.Count];
            int i = 0;
            foreach (HandChip chip in ChipsInHand)
            {
                if (chip.Used)
                    HandToReturn[i] = chip.Name + '*';
                else
                    HandToReturn[i] = chip.Name;
                i++;
            }
            return JsonConvert.SerializeObject(HandToReturn);
            /*StringBuilder build = new StringBuilder();
            build.Append('[');
            foreach (HandChip chip in ChipsInHand)
            {
                build.Append('\"');
                build.Append(chip.Name);
                build.Append('\"');
                build.Append(", ");
            }
            build.Remove(build.Length - 2, 2);
            build.Append(']');
            return build.ToString();*/
            //return "[" + string.Join(", ", ChipsInHand) + "]";
        }

        public void AddChip(Chip newChip)
        {
            if (NumValue == ChipsInHand.Count)
            {
                MessageBox.Show("Cannot add another copy of " + newChip.Name + " to your hand\nYour hand is full", "AddToHand", MessageBoxButton.OK);
                return;
            }
            newChip.NumInHand++;
            ChipsInHand.Add(newChip.MakeHandChip());
        }

        public (int numRemoved, int numUsed) ClearHand()
        {
            int numRemoved = this.ChipsInHand.Count;
            int numUsed = 0;
            foreach (HandChip chip in ChipsInHand)
            {
                var handchip = ChipLibrary.Instance.GetChip(chip.Name);
                handchip.NumInHand--;
                if (chip.Used == true)
                {
                    handchip.UsedInBattle++;
                    numUsed++;
                }
            }
            this.ChipsInHand.Clear();
            return (numRemoved, numUsed);
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
            var (numRemoved, numUsed) = this.ClearHand();
            string message = "Hand cleared, " + numRemoved + " chips removed\nof which "
                + numUsed + " were used.";
            (this.Owner as MainWindow).SetMessage(message, Brushes.Red);
            (this.Owner as MainWindow).LoadChips();
            e.Cancel = true;
            this.Hide();
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;
            RemoveChipFromHand();
        }

        private void UsedClick(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            if (!(PlayerHand.SelectedItem is HandChip selected)) return;
            selected.Used = !selected.Used;
        }

        private void HandCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender == null) return;
            if (ChipsInHand.Count > 0)
            {
                this.Show();
            }
            else if (ChipsInHand.Count == 0)
            {
                this.Hide();
            }
            (this.Owner as MainWindow).HandUpdated();
        }

        private void RemoveFromHand_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            RemoveChipFromHand();
        }

        private void RemoveChipFromHand()
        {
            if (PlayerHand.SelectedItem is HandChip selected)
            {
                foreach (HandChip chip in ChipsInHand)
                {
                    if (chip.GetHashCode() == selected.GetHashCode())
                    {
                        ChipsInHand.Remove(chip);
                        ChipLibrary.Instance.GetChip(selected.Name).NumInHand--;
                        if (selected.Used == true)
                        {
                            ChipLibrary.Instance.GetChip(selected.Name).UsedInBattle++;
                            (this.Owner as MainWindow).LoadChips();
                        }
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("No chip is currently selected!");
            }
        }
    }
}