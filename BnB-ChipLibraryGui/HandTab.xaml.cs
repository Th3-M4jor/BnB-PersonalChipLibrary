using Newtonsoft.Json;
using System;
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
    /// Interaction logic for HandTab.xaml
    /// </summary>
    public partial class HandTab : UserControl
    {
        public HandTab()
        {
            InitializeComponent();
            txtNum.Text = 2 + "";
            this.ChipsInHand = new ObservableCollection<HandChip>();
            this.PlayerHand.ItemsSource = this.ChipsInHand;
            this.ChipsInHand.CollectionChanged += HandCollectionChanged;
            _numValue = PlayerStats.Instance.GetNaviStat(StatNames.Mind) + PlayerStats.Instance.CustomPlusInst;
        }

        public void SetHand(IEnumerable<HandChip> hand)
        {
            foreach (var chip in hand)
            {
                ChipsInHand.Add(chip);
            }
        }

        private int _numValue;

        private bool inGroup = false;

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
                    HandToReturn[i] = chip.Name + '*'; //asterisk to indicate it is used
                else
                    HandToReturn[i] = chip.Name;
                i++;
            }
            return JsonConvert.SerializeObject(HandToReturn);
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

        public void JoinedGroup()
        {
            this.inGroup = true;
            SetTabVisibility(Visibility.Visible);
        }

        public void LeftGroup()
        {
            this.inGroup = false;
            if(ChipsInHand.Count == 0)
            {
                SetTabVisibility(Visibility.Hidden);
            }
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

        public void SetGroupHand(IEnumerable<GroupHands.GroupedHand> group)
        {
            if (inGroup == false) throw new Exception("not in a group");
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
            //(this.Owner as MainWindow).SetMessage(message, Brushes.Red);
            //(this.Owner as MainWindow).LoadChips();
            e.Cancel = true;

            //this.Hide();
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

        private void SetTabVisibility(Visibility change)
        {
            if (inGroup && change == Visibility.Hidden) return;
            if ((Window.GetWindow(this) as MainWindow).TabHand.Visibility == change) return;
            if(change == Visibility.Visible)
            {
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    (Window.GetWindow(this) as MainWindow).TabHand.IsSelected = true;
                    (Window.GetWindow(this) as MainWindow).TabHand.Visibility = Visibility.Visible;
                }));
            }
            else if(change == Visibility.Hidden)
            {
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    (Window.GetWindow(this) as MainWindow).Pack.IsSelected = true;
                    (Window.GetWindow(this) as MainWindow).TabHand.Visibility = Visibility.Hidden;
                }));
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private void HandCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender == null) return;
            if (ChipsInHand.Count > 0)
            {
                SetTabVisibility(Visibility.Visible);
            }
            else
            {
                SetTabVisibility(Visibility.Hidden);
            }
            (Window.GetWindow(this) as MainWindow).HandUpdated();
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
                            //(this.Owner as MainWindow).LoadChips();
                            (Window.GetWindow(this) as MainWindow).LoadChips();
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