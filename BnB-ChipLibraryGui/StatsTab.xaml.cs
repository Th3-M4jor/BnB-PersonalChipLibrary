using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BnB_ChipLibraryGui
{
    /// <summary>
    /// Interaction logic for StatsTab.xaml
    /// </summary>
    public partial class StatsTab : UserControl
    {
        public StatsTab()
        {
            InitializeComponent();
            NaviName.Text = PlayerStats.Instance.NaviName;
        }

        private void ElementBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayerStats.Instance.ElementChanged(ElementBox.SelectedIndex);
        }

        private void NaviName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NaviName == null) return;
            PlayerStats.Instance.NaviName = NaviName.Text;
        }

        private void CmdUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            if (sender is Button downButton)
            {
                string skill = downButton.Name.Substring(0, downButton.Name.Length - 4);
            }
            //NumValue++;
        }

        private void CmdDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            if (sender is Button upButton)
            {
                string skill = upButton.Name.Substring(0, upButton.Name.Length - 2);
            }
            //NumValue--;
        }
    }
}