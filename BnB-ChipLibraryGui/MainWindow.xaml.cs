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
        private SearchWindow searchWindow;

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

                if (playerHand.Count > 0)
                {
                    handWindow.Show();
                }

                searchWindow = new SearchWindow
                {
                    Owner = this
                };
                searchWindow.Hide();
            };

        }

        private void AddChip()
        {
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
            searchWindow.Show();
        }

        public void SetMessage(string message, SolidColorBrush colorBrush)
        {
            if (colorBrush == null)
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
                    MessageBox.Show("Cannot remove a chip you don't have");
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

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AddChip();
        }
    }
}