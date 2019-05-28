using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System;

namespace BnB_ChipLibraryGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool SortDesc { get; private set; }
        public Chip.ChipRanges RangeOption { get; private set; }
        public ChipLibrary.LibrarySortOptions SortOption { get; private set; }

        private Hand handWindow;

        public MainWindow()
        {
            this.SortDesc = false;
            this.SortOption = ChipLibrary.LibrarySortOptions.Name;
            this.RangeOption = Chip.ChipRanges.All;
            InitializeComponent();
            bool chipsOwned = false;
            List<HandChip> playerHand = new List<HandChip>();
            if (System.IO.File.Exists("./userChips.dat"))
            {
                using (var chipFile = System.IO.File.OpenText("./userChips.dat"))
                {

                    while (!chipFile.EndOfStream)
                    {
                        var line = chipFile.ReadLine();
                        line.Trim();
                        var input = line.Split(':');
                        sbyte count = sbyte.Parse(input[1]);
                        byte used = byte.Parse(input[2]);
                        Chip toModify = ChipLibrary.Instance.GetChip(input[0]);
                        if (toModify == null)
                        {
                            MessageBox.Show("The chip " + input[0] + " doesn't exist, ignoring", "ChipLibrary", MessageBoxButton.OK);
                            continue;
                        }
                        toModify.ChipCount = count;
                        toModify.UsedInBattle = used;
                        if (input.Length == 4)
                        {
                            byte numInHand = byte.Parse(input[3]);
                            toModify.NumInHand = numInHand;
                            for (int i = 0; i < numInHand; i++)
                            {
                                playerHand.Add(toModify.MakeHandChip());
                            }
                        }
                        chipsOwned = true;
                    }
                }
            }

            if (chipsOwned)
            {
                ShowNotOwned.IsChecked = true;
            }
            LoadChips();
            this.SourceInitialized += (s, a) =>
            {
                this.handWindow = new Hand(playerHand)
                {
                    Owner = this
                };

                if(playerHand.Count > 0)
                {
                    handWindow.Show();
                } 
            };
        }

        private void AddChip()
        {
            FoundChips.Foreground = Brushes.Black;
            if (ChipNameEntered.Text != string.Empty)
            {
                var chip = ChipLibrary.Instance.GetChip(ChipNameEntered.Text);
                if (chip != null)
                {
                    ChipNameEntered.Text = string.Empty;
                    FoundChips.Text = string.Empty;
                    chip++;
                }
                else
                {
                    var chips = ChipLibrary.Instance.Search(ChipNameEntered.Text);
                    if (chips.Count == 0)
                    {
                        FoundChips.Text = "No chips were returned";
                        return;
                    }
                    StringBuilder possibleChips = new StringBuilder();
                    foreach (var possibleChip in chips)
                    {
                        possibleChips.Append(possibleChip);
                        possibleChips.Append('\n');
                    }
                    FoundChips.Text = possibleChips.ToString();
                }
            }
            else
            {
                if(UserChips.SelectedItem != null )
                {
                    Chip selected = UserChips.SelectedItem as Chip;
                    Chip toModify = ChipLibrary.Instance.GetChip(selected.Name);
                    toModify.ChipCount += 1;
                    LoadChips();
                }
            }
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            switch ((e.Source as FrameworkElement).Name)
            {
                case "Add":
                    AddChip();
                    break;

                case "Remove":
                    RemoveChip();
                    break;

                case "JackedOut":
                    JackOut();
                    break;

                case "Search":
                    SearchChip();
                    break;
            }
            LoadChips();
        }

        private void ChipNameEntered_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && ChipNameEntered.Text != string.Empty)
            {
                AddChip();
            }
            LoadChips();
        }

        private void ExitClicked(object sender, CancelEventArgs e)
        {
            var toSave = ChipLibrary.Instance.GetList(ChipLibrary.ChipListOptions.DisplayOwned,
                ChipLibrary.LibrarySortOptions.Name, Chip.ChipRanges.All, false);
            using (var chipFile = new StreamWriter(new FileStream("./userChips.dat", System.IO.FileMode.Create)))
            {
                foreach (Chip chip in toSave)
                {
                    chipFile.WriteLine("{0}:{1}:{2}:{3}", chip.Name, chip.ChipCount, chip.UsedInBattle, chip.NumInHand);
                }
            }
            handWindow.Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (sender != null && UserChips.SelectedItems != null && UserChips.SelectedItems.Count == 1)
            {
                DataGridRow dgr = UserChips.ItemContainerGenerator.ContainerFromItem(UserChips.SelectedItem) as DataGridRow;
                if (!dgr.IsMouseOver)
                {
                    (dgr as DataGridRow).IsSelected = false;
                }
            }
        }

        private void SortReverse(object sender, RoutedEventArgs e)
        {
            this.SortDesc = !this.SortDesc;
            LoadChips();
        }

        private void JackOut()
        {
            FoundChips.Foreground = Brushes.Red;
            int handSize = this.handWindow.ClearHand().numRemoved;
            uint count = ChipLibrary.Instance.JackOut();
            FoundChips.Text = count + " chip(s) refreshed\n" + handSize + " chip(s) cleared from hand";
        }

        private void SearchChip()
        {

        }

        public void SetMessage(string message, SolidColorBrush colorBrush)
        {
            if(colorBrush == null)
            {
                colorBrush = Brushes.Black;
            }
            FoundChips.Foreground = colorBrush;
            FoundChips.Text = message;
        }

        public void LoadChips()
        {
            ChipLibrary.ChipListOptions listAll = ChipLibrary.ChipListOptions.DisplayAll;
            if (ShowNotOwned == null) return;
            if (ShowNotOwned.IsChecked.HasValue && ShowNotOwned.IsChecked == true)
            {
                listAll = ChipLibrary.ChipListOptions.DisplayOwned;
            }
            UserChips.ItemsSource = ChipLibrary.Instance.GetList(listAll, this.SortOption, this.RangeOption, this.SortDesc);
        }

        private void RangeClick(object sender, RoutedEventArgs e)
        {
            switch ((e.Source as FrameworkElement).Name)
            {
                case "AllRanges":
                    this.RangeOption = Chip.ChipRanges.All;
                    break;

                case "FarRange":
                    this.RangeOption = Chip.ChipRanges.Far;
                    break;

                case "NearRange":
                    this.RangeOption = Chip.ChipRanges.Near;
                    break;

                case "CloseRange":
                    this.RangeOption = Chip.ChipRanges.Close;
                    break;

                case "SelfRange":
                    this.RangeOption = Chip.ChipRanges.Self;
                    break;
            }
            LoadChips();
        }

        private void RemoveChip()
        {
            FoundChips.Foreground = Brushes.Black;
            if (ChipNameEntered.Text != string.Empty)
            {
                var chip = ChipLibrary.Instance.GetChip(ChipNameEntered.Text);
                if (chip != null)
                {
                    ChipNameEntered.Text = string.Empty;
                    FoundChips.Text = string.Empty;
                    chip--;
                }
                else
                {
                    var chips = ChipLibrary.Instance.Search(ChipNameEntered.Text);
                    if (chips.Count == 0)
                    {
                        FoundChips.Text = "No chips were returned";
                        return;
                    }
                    StringBuilder possibleChips = new StringBuilder();
                    foreach (var possibleChip in chips)
                    {
                        possibleChips.Append(possibleChip);
                        possibleChips.Append('\n');
                    }
                    FoundChips.Text = possibleChips.ToString();
                }
            }
            else
            {
                if (UserChips.SelectedItem != null)
                {
                    Chip selected = UserChips.SelectedItem as Chip;
                    Chip toModify = ChipLibrary.Instance.GetChip(selected.Name);
                    if (toModify.ChipCount > 0)
                    {
                        toModify.ChipCount -= 1;
                        LoadChips();
                    }
                    else
                    {
                        FoundChips.Text = "Cannot remove a chip you don't have";
                    }
                }
            }
        }

        private void ShowOwned(object sender, RoutedEventArgs e)
        {
            LoadChips();
        }

        private void SortClick(object sender, RoutedEventArgs e)
        {
            switch ((e.Source as FrameworkElement).Name)
            {
                case "SortByName":
                    this.SortOption = ChipLibrary.LibrarySortOptions.Name;
                    break;

                case "SortByAvgDamage":
                    this.SortOption = ChipLibrary.LibrarySortOptions.AvgDamage;
                    break;

                case "SortByMaxDamage":
                    this.SortOption = ChipLibrary.LibrarySortOptions.MaxDamage;
                    break;

                case "SortByOwned":
                    this.SortOption = ChipLibrary.LibrarySortOptions.Owned;
                    break;

                case "SortByElement":
                    this.SortOption = ChipLibrary.LibrarySortOptions.Element;
                    break;

                case "SortByRange":
                    this.SortOption = ChipLibrary.LibrarySortOptions.Range;
                    break;

                case "SortBySkill":
                    this.SortOption = ChipLibrary.LibrarySortOptions.Skill;
                    break;
            }
            LoadChips();
        }

        //SelectionChanged="TextBox_TextChanged"
        private void TextBox_TextChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
            DataGridCell chipUsed = dataGrid.Columns[dataGrid.Columns.Count - 1].GetCellContent(row).Parent as DataGridCell;
            DataGridCell chipName = dataGrid.Columns[0].GetCellContent(row).Parent as DataGridCell;
            string numChipsUsed = ((TextBlock)chipUsed.Content).Text;
            string changedChipName = ((TextBlock)chipName.Content).Text;

            Chip toUpdate = ChipLibrary.Instance.GetChip(changedChipName);
            //toUpdate.UsedInBattle = int.Parse(numChipsUsed);
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;
            if (!(UserChips.SelectedItem is Chip selected)) return;
            try
            {

                if (selected.ChipCount <= 0 || selected.NumInHand >= selected.ChipCount || selected.UsedInBattle >= selected.ChipCount)
                {
                    MessageBox.Show("Cannot add another copy of " + selected.Name + " to your hand", "AddToHand", MessageBoxButton.OK);
                    return;
                }

                handWindow.AddChip(selected);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Cannot add another copy of " + selected.Name + " to your hand", "AddToHand", MessageBoxButton.OK);
            }
        }
    }
}