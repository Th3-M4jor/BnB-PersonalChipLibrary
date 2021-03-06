﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BnB_ChipLibraryGui
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        public SearchWindow()
        {
            InitializeComponent();
        }

        private void AddChipToPack()
        {
            if (SearchResultGrid.SelectedItem is Chip selected)
            {
                ChipLibrary.Instance.GetChip(selected.Name).ChipCount++;
                (this.Owner as MainWindow).LoadChips();
                MessageBox.Show("A copy of " + selected.Name + "\nhas been added to your pack!");
            }
            else
            {
                MessageBox.Show("No chip is currently selected!");
            }
        }

        private void AddToPack_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            AddChipToPack();
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;
            AddChipToPack();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != null && SearchResultGrid.SelectedItems != null && SearchResultGrid.SelectedItems.Count == 1)
            {
                DataGridRow dgr = SearchResultGrid.ItemContainerGenerator.ContainerFromItem(SearchResultGrid.SelectedItem) as DataGridRow;
                if (!dgr.IsMouseOver)
                {
                    (dgr as DataGridRow).IsSelected = false;
                }
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (this.SearchText.Text == string.Empty)
            {
                MessageBox.Show("You must enter a search query");
                return;
            }
            var chips = ChipLibrary.Instance.Search(this.SearchText.Text);
            if (chips.Count == 0)
            {
                MessageBox.Show("No chips were returned");
                return;
            }
            SearchResultGrid.ItemsSource = chips;
        }

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (this.SearchText.Text == string.Empty)
                {
                    MessageBox.Show("You must enter a search query");
                    return;
                }
                var chips = ChipLibrary.Instance.Search(this.SearchText.Text);
                if (chips.Count == 0)
                {
                    MessageBox.Show("No chips were returned");
                    return;
                }
                SearchResultGrid.ItemsSource = chips;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}