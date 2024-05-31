using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BarbarianLab.Utilities;
using static BoB_LevelData;

namespace BarbarianLab
{
    public partial class SaveLevel : Form
    {
        string levelData = "";
        public SaveLevel(string text)
        {
            InitializeComponent();
            levelData = text;
        }
        private void SaveLevel_Load(object sender, EventArgs e)
        {
            levelText.Text = levelData;
        }
        private void Save_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(levelText.Text);
            }
            catch { MessageBox.Show("Failed to copy to clipboard, please try again."); }
        }
    }
}
