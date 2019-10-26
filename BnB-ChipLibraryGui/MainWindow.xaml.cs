﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using System.Threading.Tasks;
using System.Text;
using System.Runtime.CompilerServices;

namespace BnB_ChipLibraryGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

        //private Hand handWindow;
        private SearchWindow searchWindow;

        public Chip.ChipRanges RangeOption { get; private set; }
        public bool SortDesc { get; private set; }
        public bool HideUsedChips { get; private set; }
        public ChipLibrary.LibrarySortOptions SortOption { get; private set; }
        public bool InGroup { get; private set; }

        public MainWindow()
        {
            this.SortDesc = false;
            this.HideUsedChips = false;
            this.SortOption = ChipLibrary.LibrarySortOptions.Name;
            this.RangeOption = Chip.ChipRanges.All;
            InitializeComponent();

            var (playerHand, chipsOwned) = this.LoadChipFile();

            if (chipsOwned)
            {
                ShowNotOwned.IsChecked = true;
            }

            this.SourceInitialized += (s, a) =>
            {
                LoadChips(chipsOwned);
                /*this.handWindow = new Hand(playerHand)
                {
                    Owner = this
                };*/
                this.HandWindowObject.SetHand(playerHand);
                /*if (playerHand.Count > 0)
                {
                    handWindow.Show();
                }*/
                if (playerHand.Count > 0)
                {
                    this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        this.TabHand.Visibility = Visibility.Visible;
                    }));
                }
                this.HandWindowObject.ChipHandUpdated += this.HandUpdated;
                searchWindow = new SearchWindow
                {
                    Owner = this
                };
                searchWindow.Hide();
            };
        }

        public async void LoadChips(bool forceDisplayOwned = false)
        {
            ChipLibrary.ChipListOptions listAll = ChipLibrary.ChipListOptions.DisplayAll;
            if (ShowNotOwned == null) return;
            if ((ShowNotOwned.IsChecked.HasValue && ShowNotOwned.IsChecked == true) || forceDisplayOwned == true)
            {
                listAll = ChipLibrary.ChipListOptions.DisplayOwned;
            }
            var res = await ChipLibrary.Instance.GetList(listAll, this.SortOption, this.RangeOption, this.SortDesc, this.HideUsedChips);
            UserChips.ItemsSource = res;
        }

        public string GetHand()
        {
            return this.HandWindowObject.GetHand();
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

        private void HandUpdated(object sender, EventArgs e)
        {
            LoadChips();
        }

        private void AddChip()
        {
            if (UserChips.SelectedItem is Chip selected)
            {
                try
                {
                    if (selected.ChipCount <= 0 || selected.NumInHand >= selected.ChipCount || selected.UsedInBattle >= selected.ChipCount)
                    {
                        MessageBox.Show("Cannot add another copy of " + selected.Name + " to your hand", "AddToHand", MessageBoxButton.OK);
                        return;
                    }

                    this.HandWindowObject.AddChip(selected);
                    //LoadChips();
                }
                catch (ArgumentOutOfRangeException)
                {
                    MessageBox.Show("Cannot add another copy of " + selected.Name + " to your hand", "AddToHand", MessageBoxButton.OK);
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

                case "FolderExport":
                    ExportChips();
                    break;
            }
            LoadChips();
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (UserChips.IsMouseOver)
            {
                AddChip();
            }
        }

        private void ExitClicked(object sender, CancelEventArgs e)
        {
            //e.Cancel = true;

            Task.Run(async () =>
            {
                var toSave = await ChipLibrary.Instance.GetList(ChipLibrary.ChipListOptions.DisplayOwned,
                ChipLibrary.LibrarySortOptions.Name, Chip.ChipRanges.All, false);
                await this.HandWindowObject.LeaveGroup();
                using (var chipFile = File.CreateText("./userChips.dat"))
                {
                    StringBuilder fullText = new StringBuilder();
                    foreach (Chip chip in toSave)
                    {
                        //string toWrite = string.Format("{0}:{1}:{2}:{3}\n", chip.Name, chip.ChipCount, chip.UsedInBattle, chip.NumInHand);
                        //await chipFile.WriteLineAsync(toWrite);
                        //fullText.Append(toWrite);
                        fullText.AppendFormat("{0}:{1}:{2}:{3}\n", chip.Name, chip.ChipCount, chip.UsedInBattle, chip.NumInHand);
                    }
                    chipFile.Write(fullText.ToString());
                }

                PlayerStats.Instance.Save();
            }).Wait(); //call an async task and wait for it to finish before exiting

            //handWindow.Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != null && UserChips.SelectedItems != null
                && UserChips.SelectedItems.Count == 1
                && UserChips.ItemContainerGenerator.ContainerFromItem(UserChips.SelectedItem) is DataGridRow dgr
                && !dgr.IsMouseOver)
            {
                dgr.IsSelected = false;
            }
        }

        private async void JackOut()
        {
            FoundChips.Foreground = Brushes.Red;
            int handSize = this.HandWindowObject.ClearHand().numRemoved;
            uint count = await ChipLibrary.Instance.JackOut();
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

        private async void ExportChips()
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
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fName = saveFileDialog.FileName;
                    string toWrite = await ChipLibrary.Instance.GenerateExport();
                    //File.WriteAllText(fName, toWrite);
                    StreamWriter writer = File.CreateText(fName);
                    await writer.WriteAsync(toWrite);
                    await writer.FlushAsync();
                    writer.Dispose();
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

        private void HideUsedBattleChips(object sender, RoutedEventArgs e)
        {
            this.HideUsedChips = !this.HideUsedChips;
            LoadChips();
        }

        private async void JoinClick(object sender, RoutedEventArgs e)
        {
            if (HandWindowObject.SessionClosed == false)
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
            if (!await HandTab.CheckSessionExists(DMName))
            {
                MessageBox.Show("That session does not yet exist, check the group name and try again");
                return;
            }

            //grouphands = new GroupHands(this, DMName, NaviName, false);
            try
            {
                await this.HandWindowObject.JoinGroup(DMName, NaviName, false);
            }
            catch (Exception except)
            {
                if (except.Message == "Name Taken")
                {
                    MessageBox.Show("That name is already taken, try a different one");
                }
                else
                {
                    //MessageBox.Show("An error has occurred, inform Major");
                    ErrorWindow();
                }
                //grouphands = null;
                return;
            }
            //grouphands.Show();

            //MessageBox.Show("Group Joined");
        }

        private async void CreateClick(object sender, RoutedEventArgs e)
        {
            if (HandWindowObject.SessionClosed == false)
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
            if (await HandTab.CheckSessionExists(DMName))
            {
                MessageBox.Show("That session already exists, try a different name");
                return;
            }

            try
            {
                await this.HandWindowObject.JoinGroup(DMName, DMName, true);
            }
            catch (Exception except)
            {
                MessageBox.Show("An error has occurred, inform Major");
                MessageBox.Show(except.Message);
                return;
            }
        }

        private void CmbSortOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (CmbSortOption.SelectedIndex)
            {
                case 0:
                    this.SortOption = ChipLibrary.LibrarySortOptions.Name;
                    break;

                case 1:
                    this.SortOption = ChipLibrary.LibrarySortOptions.AvgDamage;
                    break;

                case 2:
                    this.SortOption = ChipLibrary.LibrarySortOptions.MaxDamage;
                    break;

                case 3:
                    this.SortOption = ChipLibrary.LibrarySortOptions.Owned;
                    break;

                case 4:
                    this.SortOption = ChipLibrary.LibrarySortOptions.Element;
                    break;

                case 5:
                    this.SortOption = ChipLibrary.LibrarySortOptions.Range;
                    break;

                case 6:
                    this.SortOption = ChipLibrary.LibrarySortOptions.Skill;
                    break;

                case 7:
                    this.SortOption = ChipLibrary.LibrarySortOptions.Used;
                    break;

                default:
                    return;
            }
            LoadChips();
        }

        private (List<HandChip> playerHand, bool ownsChips) LoadChipFile()
        {
            bool chipsOwned = false;
            List<HandChip> playerHand = new List<HandChip>();
            if (!File.Exists("./userChips.dat"))
            {
                return (playerHand, chipsOwned);
            }
            using (var chipFile = File.OpenText("./userChips.dat"))
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
            return (playerHand, chipsOwned);
        }

        public static void ErrorWindow(
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null
            ) => MessageBox.Show("An error has occurred at line " + lineNumber.ToString() + " in " + caller);
    }
}