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
            OperatorName.Text = PlayerStats.Instance.OperatorName;
            ElementBox.SelectedIndex = (int)PlayerStats.Instance.NaviElement;
            foreach (var stat in Enum.GetValues(typeof(StatNames)))
            {
                if (this.FindName(stat.ToString()) is TextBox box)
                {
                    box.Text = PlayerStats.Instance.GetNaviStat((StatNames)stat).ToString();
                }
                else
                {
                    MainWindow.ErrorWindow();
                }

                if (this.FindName("Op" + stat.ToString()) is TextBox opBox)
                {
                    opBox.Text = PlayerStats.Instance.GetOpStat((StatNames)stat).ToString();
                }
                else
                {
                    MainWindow.ErrorWindow();
                }
            }

            for (int i = 0; i < 9; i++)
            {
                Chip.ChipSkills skill = (Chip.ChipSkills)i;
                if (this.FindName(skill.ToString()) is TextBox skillBox)
                {
                    skillBox.Text = PlayerStats.Instance.GetNaviSkill(skill).ToString();
                }
                else
                {
                    MainWindow.ErrorWindow();
                }

                if (this.FindName("Op" + skill.ToString()) is TextBox opSkillBox)
                {
                    opSkillBox.Text = PlayerStats.Instance.GetOpSkill(skill).ToString();
                }
                else
                {
                    MainWindow.ErrorWindow();
                }
            }

            /*this.Mind.Text = PlayerStats.Instance.GetNaviStat(StatNames.Mind).ToString();
            this.Body.Text = PlayerStats.Instance.GetNaviStat(StatNames.Body).ToString();
            this.Spirit.Text = PlayerStats.Instance.GetNaviStat(StatNames.Spirit).ToString();*/
        }

        private void ElementBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsInitialized) return;
            PlayerStats.Instance.ElementChanged(ElementBox.SelectedIndex);
        }

        private void NaviName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NaviName == null) return;
            PlayerStats.Instance.NaviName = NaviName.Text;
        }

        private void OpNameChanged(object sender, TextChangedEventArgs e)
        {
            if (OperatorName == null) return;
            PlayerStats.Instance.OperatorName = OperatorName.Text;
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

        private void CustomUp_Click(object sender, RoutedEventArgs e)
        {
            CustomCt.Text = PlayerStats.Instance.IncCustomPlus().ToString();
        }

        private void CustomDown_Click(object sender, RoutedEventArgs e)
        {
            CustomCt.Text = PlayerStats.Instance.DecCustomPlus().ToString();
        }

        private void HPPlusUp_Click(object sender, RoutedEventArgs e)
        {
            HPPCt.Text = PlayerStats.Instance.IncHPPlus().ToString();
            NaviHP.Text = PlayerStats.Instance.NaviHPMax.ToString();
        }

        private void HPPlusDown_Click(object sender, RoutedEventArgs e)
        {
            HPPCt.Text = PlayerStats.Instance.DecHPPlus().ToString();
            NaviHP.Text = PlayerStats.Instance.NaviHPMax.ToString();
        }

        private void HPDiceDown_Click(object sender, RoutedEventArgs e)
        {
            HPDiceCt.Text = PlayerStats.Instance.DecHPFromDice().ToString();
            NaviHP.Text = PlayerStats.Instance.NaviHPMax.ToString();
        }

        private void HPDiceUp_Click(object sender, RoutedEventArgs e)
        {
            HPDiceCt.Text = PlayerStats.Instance.IncHPFromDice().ToString();
            NaviHP.Text = PlayerStats.Instance.NaviHPMax.ToString();
        }

        private void HPMemUp_Click(object sender, RoutedEventArgs e)
        {
            HPPCt.Text = PlayerStats.Instance.IncHPMem().ToString();
            NaviHP.Text = PlayerStats.Instance.NaviHPMax.ToString();
        }

        private void HPMemDown_Click(object sender, RoutedEventArgs e)
        {
            HPPCt.Text = PlayerStats.Instance.DecHPMem().ToString();
            NaviHP.Text = PlayerStats.Instance.NaviHPMax.ToString();
        }
    }
}