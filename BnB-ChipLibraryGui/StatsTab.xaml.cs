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
            if (sender == null || !(sender is Button upButton)) return;
            string skill = upButton.Name.Substring(0, upButton.Name.Length - 2);
            Chip.ChipSkills naviSkill = (Chip.ChipSkills)Enum.Parse(typeof(Chip.ChipSkills), skill);
            StatNames stat = (StatNames)((int)naviSkill / 3); //because skill num / 3 is relevant stat

            if (!PlayerStats.Instance.NaviCanIncreaseSkill(naviSkill, stat))
            {
                MessageBox.Show("Cannot do that, your " + stat.ToString() + " is too low");
                return;
            }
            PlayerStats.Instance.NaviIncreaseSkill(naviSkill);
            if (this.FindName(skill) is TextBox box)
            {
                box.Text = PlayerStats.Instance.GetNaviSkill(naviSkill).ToString();
            }
            else
            {
                MainWindow.ErrorWindow();
            }
        }

        private void CmdDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null || !(sender is Button downButton)) return;
            string skill = downButton.Name.Substring(0, downButton.Name.Length - 4);
            Chip.ChipSkills naviSkill = (Chip.ChipSkills)Enum.Parse(typeof(Chip.ChipSkills), skill);
            PlayerStats.Instance.NaviDecreaseSkill(naviSkill);
            if (this.FindName(skill) is TextBox box)
            {
                box.Text = PlayerStats.Instance.GetNaviSkill(naviSkill).ToString();
            }
            else
            {
                MainWindow.ErrorWindow();
            }
        }

        private void OpCmdUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null || !(sender is Button upButton)) return;
            string skill = upButton.Name.Substring(2, upButton.Name.Length - 4);
            Chip.ChipSkills opSkill = (Chip.ChipSkills)Enum.Parse(typeof(Chip.ChipSkills), skill);
            StatNames stat = (StatNames)((int)opSkill / 3); //because skill num / 3 is relevant stat

            if (!PlayerStats.Instance.OpCanIncreaseSkill(opSkill, stat))
            {
                MessageBox.Show("Cannot do that, your " + stat.ToString() + " is too low");
                return;
            }
            PlayerStats.Instance.OpIncreaseSkill(opSkill);
            if (this.FindName("Op" + skill) is TextBox box)
            {
                box.Text = PlayerStats.Instance.GetOpSkill(opSkill).ToString();
            }
            else
            {
                MainWindow.ErrorWindow();
            }
        }

        private void OpCmdDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null || !(sender is Button downButton)) return;
            string skill = downButton.Name.Substring(2, downButton.Name.Length - 6);
            Chip.ChipSkills opSkill = (Chip.ChipSkills)Enum.Parse(typeof(Chip.ChipSkills), skill);
            PlayerStats.Instance.OpDecreaseSkill(opSkill);
            if (this.FindName("Op" + skill) is TextBox box)
            {
                box.Text = PlayerStats.Instance.GetOpSkill(opSkill).ToString();
            }
            else
            {
                MainWindow.ErrorWindow();
            }
        }

        private void StatUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null || !(sender is Button upButton)) return;
            string stat = upButton.Name.Substring(0, upButton.Name.Length - 2);
            StatNames NaviStat = (StatNames)Enum.Parse(typeof(StatNames), stat);
            PlayerStats.Instance.NaviIncreaseStat(NaviStat);
            if (this.FindName(stat) is TextBox box)
            {
                box.Text = PlayerStats.Instance.GetNaviStat(NaviStat).ToString();
            }
            else
            {
                MainWindow.ErrorWindow();
            }
        }

        private void StatDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null || !(sender is Button downButton)) return;
            string stat = downButton.Name.Substring(0, downButton.Name.Length - 4);
            StatNames NaviStat = (StatNames)Enum.Parse(typeof(StatNames), stat);
            PlayerStats.Instance.NaviDecreaseStat(NaviStat);
            if (this.FindName(stat) is TextBox box)
            {
                box.Text = PlayerStats.Instance.GetNaviStat(NaviStat).ToString();
            }
            else
            {
                MainWindow.ErrorWindow();
            }
        }

        private void OpStatUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null || !(sender is Button upButton)) return;
            string stat = upButton.Name.Substring(2, upButton.Name.Length - 4);
            StatNames OpStat = (StatNames)Enum.Parse(typeof(StatNames), stat);
            PlayerStats.Instance.OpIncreaseStat(OpStat);
            if (this.FindName("Op" + stat) is TextBox box)
            {
                box.Text = PlayerStats.Instance.GetOpStat(OpStat).ToString();
            }
            else
            {
                MainWindow.ErrorWindow();
            }
        }

        private void OpStatDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null || !(sender is Button downButton)) return;
            string stat = downButton.Name.Substring(2, downButton.Name.Length - 6);
            StatNames OpStat = (StatNames)Enum.Parse(typeof(StatNames), stat);
            PlayerStats.Instance.OpDecreaseStat(OpStat);
            if (this.FindName(stat) is TextBox box)
            {
                box.Text = PlayerStats.Instance.GetOpStat(OpStat).ToString();
            }
            else
            {
                MainWindow.ErrorWindow();
            }
        }
    }
}