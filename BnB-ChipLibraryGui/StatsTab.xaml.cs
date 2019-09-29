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
            StatNames stat;
            switch (naviSkill)
            {
                case Chip.ChipSkills.Sense:
                case Chip.ChipSkills.Info:
                case Chip.ChipSkills.Coding:
                    stat = StatNames.Mind;
                    break;

                case Chip.ChipSkills.Strength:
                case Chip.ChipSkills.Speed:
                case Chip.ChipSkills.Stamina:
                    stat = StatNames.Body;
                    break;

                case Chip.ChipSkills.Charm:
                case Chip.ChipSkills.Bravery:
                case Chip.ChipSkills.Affinity:
                    stat = StatNames.Spirit;
                    break;

                default:
                    MainWindow.ErrorWindow();
                    return;
            }

            if (!PlayerStats.Instance.NaviCanIncreaseSkill(naviSkill, stat))
            {
                MessageBox.Show("Cannot do that, your " + stat.ToString() + " is too low");
                return;
            }
            PlayerStats.Instance.NaviIncreaseSkill(naviSkill);
        }

        private void CmdDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null || !(sender is Button downButton)) return;
            string skill = downButton.Name.Substring(0, downButton.Name.Length - 4);
            Chip.ChipSkills naviSkill = (Chip.ChipSkills)Enum.Parse(typeof(Chip.ChipSkills), skill);
            if (PlayerStats.Instance.NaviDecreaseSkill(naviSkill))
            {
            }
        }

        private void OpCmdUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null || !(sender is Button upButton)) return;
            string skill = upButton.Name.Substring(2, upButton.Name.Length - 4);
            Chip.ChipSkills opSkill = (Chip.ChipSkills)Enum.Parse(typeof(Chip.ChipSkills), skill);
            StatNames stat;
            switch (opSkill)
            {
                case Chip.ChipSkills.Sense:
                case Chip.ChipSkills.Info:
                case Chip.ChipSkills.Coding:
                    stat = StatNames.Mind;
                    break;

                case Chip.ChipSkills.Strength:
                case Chip.ChipSkills.Speed:
                case Chip.ChipSkills.Stamina:
                    stat = StatNames.Body;
                    break;

                case Chip.ChipSkills.Charm:
                case Chip.ChipSkills.Bravery:
                case Chip.ChipSkills.Affinity:
                    stat = StatNames.Spirit;
                    break;

                default:
                    MainWindow.ErrorWindow();
                    return;
            }

            if (!PlayerStats.Instance.OpCanIncreaseSkill(opSkill, stat))
            {
                MessageBox.Show("Cannot do that, your " + stat.ToString() + " is too low");
                return;
            }
            PlayerStats.Instance.OpIncreaseSkill(opSkill);
        }

        private void OpCmdDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null || !(sender is Button downButton)) return;
            string skill = downButton.Name.Substring(2, downButton.Name.Length - 6);
        }

        private void StatUp_Click(object sender, RoutedEventArgs e)
        {
        }

        private void StatDown_Click(object sender, RoutedEventArgs e)
        {
        }

        private void OpStatUp_Click(object sender, RoutedEventArgs e)
        {
        }

        private void OpStatDown_Click(object sender, RoutedEventArgs e)
        {
        }

        private void NaviSkillDecrement(Chip.ChipSkills skill)
        {
        }

        private void NaviSkillIncrement(Chip.ChipSkills skill)
        {
        }

        private void OperatorSkillDecrement(Chip.ChipSkills skill)
        {
        }

        private void OperatorSkillIncrement(Chip.ChipSkills skill)
        {
        }
    }
}