using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarbarianLab
{
    public partial class OpenLevel : Form
    {
        public BoB_LevelData.Level[] levels;
        public OpenLevel()
        {
            InitializeComponent();
            this.Focus();
        }
        private void OpenLevel_Load(object sender, EventArgs e)
        {
            levelText.Text =
                $"// Copy-Paste any amount of levels below to import them into the tool.{Environment.NewLine}" +
                $"// You can even put entire f_Levels_InitLevels functions here!{Environment.NewLine}{Environment.NewLine}" +
                $"// _locX_[Y] = new Array(Big list of numbers);{Environment.NewLine}" +
                $"// _locX_[Y] = f_ConcatArray(_locX_[Y],new Array(Additional data for larger levels);{Environment.NewLine}";
        }
        private void Open_Click(object sender, EventArgs e)
        {
            string inputText = "";
            for (int i = 0; i < levelText.Lines.Length; i++)
            {
                string line = levelText.Lines[i].Split("//", StringSplitOptions.None)[0];
                if (!String.IsNullOrEmpty(line))
                {
                    inputText += line + Environment.NewLine;
                }
            }
            bool deleteFirstDefault = false;
            if (inputText.Contains(".a_newLevel = new Array")) deleteFirstDefault = true;
            levels = BoB_LevelData.Level.ParseLevels(inputText, deleteFirstDefault);
            DialogResult = DialogResult.OK;
        }
    }
}
