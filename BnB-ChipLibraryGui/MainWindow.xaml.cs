using System.ComponentModel;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Controls;

namespace BnB_ChipLibraryGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChipLibrary.LibrarySortOptions sortOption;
        private bool invert = false;
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
                        Chip toModify = ChipLibrary.Instance.GetChip(input[0]);
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

        private void ExitClicked(object sender, CancelEventArgs e)
        {
            var toSave = ChipLibrary.Instance.getList(ChipLibrary.ChipListOptions.DisplayOwned, ChipLibrary.LibrarySortOptions.Name, false);
            using (var chipFile = new StreamWriter(new FileStream("./userChips.dat", System.IO.FileMode.Create)))
            {
                foreach (Chip chip in toSave)
                {
                    chipFile.WriteLine("{0}:{1}:{2}", chip.Name, chip.ChipCount, chip.UsedInBattle);
                }
            }
        }

        private void LoadChips()
        {
            ChipLibrary.ChipListOptions listAll = ChipLibrary.ChipListOptions.DisplayAll;
            if (ShowNotOwned == null) return;
            if (ShowNotOwned.IsChecked.HasValue && ShowNotOwned.IsChecked == true)
            {
                listAll = ChipLibrary.ChipListOptions.DisplayOwned;
            }
            UserChips.ItemsSource = ChipLibrary.Instance.getList(listAll, this.sortOption, this.invert);
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
                case "SortByAvgDamage":
                    this.sortOption = ChipLibrary.LibrarySortOptions.AvgDamage;
                    break;
                case "SortByMaxDamage":
                    this.sortOption = ChipLibrary.LibrarySortOptions.MaxDamage;
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

        private void InvertSort(object sender, RoutedEventArgs e)
        {
            invert = !invert;
            LoadChips();
        }

        private void AddChip()
        {
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
        }
        private void RemoveChip()
        {
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
        }
    }
}
