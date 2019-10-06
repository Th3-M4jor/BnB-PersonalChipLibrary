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
using System.Threading;

namespace BnB_ChipLibraryGui
{
    /// <summary>
    /// Interaction logic for HandTab.xaml
    /// </summary>
    public partial class HandTab : UserControl, IDisposable
    {
        public HandTab()
        {
            InitializeComponent();
            this.ChipsInHand = new ObservableCollection<HandChip>();
            this.PlayerHand.ItemsSource = this.ChipsInHand;
            this.ChipsInHand.CollectionChanged += HandCollectionChanged;
            NumValue = PlayerStats.Instance.GetNaviStat(StatNames.Mind) + (uint)PlayerStats.Instance.CustomPlusInst;
            Players.Visibility = Visibility.Hidden;
            GroupRefreshButton.Visibility = Visibility.Hidden;
            GroupLeaveButton.Visibility = Visibility.Hidden;
            netLock = new Semaphore(1, 1);
            PlayerStats.Instance.HandSizeChanged += HandSizeChanged;
        }

        public void SetHand(IEnumerable<HandChip> hand)
        {
            foreach (var chip in hand)
            {
                ChipsInHand.Add(chip);
            }
        }

        public event EventHandler ChipHandUpdated;

        private uint _numValue;

        public uint NumValue
        {
            get { return _numValue; }
            set
            {
                _numValue = value;
                txtNum.Text = value.ToString();
            }
        }

        private void HandSizeChanged(object sender, HandSizeChangedEventArgs size)
        {
            this.NumValue = size.NewHandSize;
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

        /*private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var (numRemoved, numUsed) = this.ClearHand();
            string message = "Hand cleared, " + numRemoved + " chips removed\nof which "
                + numUsed + " were used.";
            //(this.Owner as MainWindow).SetMessage(message, Brushes.Red);
            //(this.Owner as MainWindow).LoadChips();
            e.Cancel = true;

            //this.Hide();
        }*/

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
            if (!SessionClosed && change == Visibility.Hidden) return;
            if ((Window.GetWindow(this) as MainWindow).TabHand.Visibility == change) return;
            if (change == Visibility.Visible)
            {
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    //(Window.GetWindow(this) as MainWindow).TabHand.IsSelected = true;
                    (Window.GetWindow(this) as MainWindow).TabHand.Visibility = Visibility.Visible;
                }));
            }
            else if (change == Visibility.Hidden)
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
            //(Window.GetWindow(this) as MainWindow).HandUpdated();
            ChipHandUpdated?.Invoke(this, new EventArgs());
            HandUpdate();
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

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.netLock.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~HandTab()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}