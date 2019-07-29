using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace BnB_ChipLibraryGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

        private Hand handWindow;
        private SearchWindow searchWindow;
        private GroupHands grouphands;
        public Chip.ChipRanges RangeOption { get; private set; }
        public bool SortDesc { get; private set; }
        public ChipLibrary.LibrarySortOptions SortOption { get; private set; }
        public bool InGroup { get; private set; }

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

        public string GetHand()
        {
            return handWindow.GetHand();
        }

        public void HandUpdated()
        {
            if (grouphands != null)
            {
                grouphands.HandUpdate(true);
            }
        }

        public void GroupClosed()
        {
            this.Dispatcher.Invoke(() =>
            {//this refer to form in WPF application
                grouphands = null;
            });
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

                case "FolderExport":
                    ExportChips();
                    break;
            }
            LoadChips();
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AddChip();
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

        private void JackOut()
        {
            FoundChips.Foreground = Brushes.Red;
            int handSize = this.handWindow.ClearHand().numRemoved;
            uint count = ChipLibrary.Instance.JackOut();
            FoundChips.Text = count + " chip(s) refreshed\n" + handSize + " chip(s) cleared from hand";
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

        private void SearchChip()
        {
            searchWindow.Show();
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

        private void ExportChips()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1,
                DefaultExt = ".txt",
                FileName = "Pack"
            };
            try
            {
                string toWrite = ChipLibrary.Instance.GenerateExport();

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, toWrite);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                saveFileDialog.Dispose();
            }
        }

        private void SortReverse(object sender, RoutedEventArgs e)
        {
            this.SortDesc = !this.SortDesc;
            LoadChips();
        }

        private void JoinClick(object sender, RoutedEventArgs e)
        {
            if (this.grouphands != null && this.grouphands.IsSessionClosed() == false)
            {
                MessageBox.Show("You are already in a group, you must leave it if you wish to join a different one");
                return;
            }
            var res = MessageBox.Show("This will transmit your hand data to a remote server", "ShareHand", MessageBoxButton.OKCancel);
            if (res == MessageBoxResult.Cancel) return;
            QuestionWindow questionWindow = new QuestionWindow("JoinGroup", "Please enter your Navi's name");
            if (questionWindow.ShowDialog() == false)
            {
                MessageBox.Show("Join Canceled");
                return;
            }
            string NaviName = questionWindow.GetAnswer();
            questionWindow = new QuestionWindow("DMName", "Please enter the name of the group your DM created");
            if (questionWindow.ShowDialog() == false)
            {
                MessageBox.Show("Join Canceled");
                return;
            }
            string DMName = questionWindow.GetAnswer();
            if (!GroupHands.CheckSessionExists(DMName))
            {
                MessageBox.Show("That session does not yet exist, check the group name and try again");
                return;
            }

            try
            {
                grouphands = new GroupHands(this, DMName, NaviName, false);
            }
            catch (Exception except)
            {
                if (except.Message == "Name Taken")
                {
                    MessageBox.Show("That name is already taken, try a different one");
                }
                else
                {
                    MessageBox.Show("An error has occurred, inform Major");
                }
                grouphands = null;
                return;
            }
            grouphands.Show();
            //MessageBox.Show("Group Joined");
        }

        private void CreateClick(object sender, RoutedEventArgs e)
        {
            if (this.grouphands != null && this.grouphands.IsSessionClosed() == false)
            {
                MessageBox.Show("You are already in a group, you must leave it if you wish to join a different one");
                return;
            }
            QuestionWindow questionWindow = new QuestionWindow("CreateGroup", "Please enter the group name");
            if (questionWindow.ShowDialog() == false)
            {
                MessageBox.Show("Create Canceled");
                return;
            }
            string DMName = questionWindow.GetAnswer();
            if (GroupHands.CheckSessionExists(DMName))
            {
                MessageBox.Show("That session already exists, try a different name");
                return;
            }
            try
            {
                grouphands = new GroupHands(this, DMName, DMName, true);
            }
            catch (Exception)
            {
                MessageBox.Show("An error has occurred, inform Major");
                grouphands = null;
                return;
            }
            grouphands.Show();
        }
    }
}