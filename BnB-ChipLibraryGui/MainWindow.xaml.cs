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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace BnB_ChipLibraryGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChipLibrary.LibrarySortOptions sortOption;
        public MainWindow()
        {
            this.sortOption = ChipLibrary.LibrarySortOptions.Name;
            InitializeComponent();
            if (System.IO.File.Exists("./userChips.dat"))
            {
                using (var chipFile = System.IO.File.OpenText("./userChips.dat"))
                {
                    while (!chipFile.EndOfStream)
                    {
                        var line = chipFile.ReadLine();
                        line.Trim();
                        var input = line.Split(':');
                        int count = int.Parse(input[1]);
                        int used = int.Parse(input[2]);
                        Chip toModify = ChipLibrary.instance.GetChip(input[0]);
                        if (toModify == null)
                        {
                            MessageBox.Show("The chip " + input[0] + " doesn't exist, ignoring", "ChipLibrary", MessageBoxButton.OK);
                        }
                        toModify.UpdateChipCount(count);
                        toModify.UsedInBattle = used;
                    }
                }
            }
            LoadChips();

        }

        private void LoadChips()
        {
            ChipLibrary.ChipListOptions listAll = ChipLibrary.ChipListOptions.DisplayAll;
            if (ShowNotOwned == null) return;
            if(ShowNotOwned.IsChecked.HasValue && ShowNotOwned.IsChecked == true)
            {
                listAll = ChipLibrary.ChipListOptions.DisplayOwned;
            }
            UserChips.ItemsSource = ChipLibrary.instance.getList(listAll, this.sortOption);
        }
       private void ButtonClick(object sender, RoutedEventArgs e)
       {
            FrameworkElement feSource = e.Source as FrameworkElement;
            switch (feSource.Name)
            {
                case "Add":
                    AddChip();
                    break;
                case "Remove":
                    RemoveChip();
                    break;
                case "SortByName":
                    this.sortOption = ChipLibrary.LibrarySortOptions.Name;
                    break;
                case "SortByDamage":
                    this.sortOption = ChipLibrary.LibrarySortOptions.Damage;
                    break;
                case "SortByOwned":
                    this.sortOption = ChipLibrary.LibrarySortOptions.Owned;
                    break;
                case "SortByElement":
                    this.sortOption = ChipLibrary.LibrarySortOptions.Element;
                    break;
            }
            LoadChips();

        }

        private void ShowOwned(object sender, RoutedEventArgs e)
        {
            LoadChips();
        }

        private void AddChip()
        {
            if(ChipNameEntered.Text != string.Empty)
            {
                var chip = ChipLibrary.instance.GetChip(ChipNameEntered.Text.ToLower());
                if(chip != null)
                {
                    FoundChips.Text = string.Empty;
                    chip++;

                }
                else
                {
                    var chips = ChipLibrary.instance.Search(ChipNameEntered.Text);
                    if(chips.Count == 0)
                    {
                        FoundChips.Text = "No chips were returned";
                        return;
                    }
                    StringBuilder possibleChips = new StringBuilder();
                    foreach(var possibleChip in chips)
                    {
                        possibleChips.Append(possibleChip);
                        possibleChips.Append('\n');
                    }
                    FoundChips.Text = possibleChips.ToString();
                }
            }
        }
        private void RemoveChip()
        {
            if (ChipNameEntered.Text != string.Empty)
            {
                var chip = ChipLibrary.instance.GetChip(ChipNameEntered.Text);
                if (chip != null)
                {
                    FoundChips.Text = string.Empty;
                    chip--;
                }
                else
                {
                    var chips = ChipLibrary.instance.Search(ChipNameEntered.Text);
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
        }
    }
}
