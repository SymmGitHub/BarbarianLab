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
        public BoB_LevelData.Level level;
        bool hiddenInitialText = false;
        public OpenLevel()
        {
            InitializeComponent();
            this.Focus();
        }

        private void Open_Click(object sender, EventArgs e)
        {
            level = BoB_LevelData.Level.ParseLevel(levelText.Text);
            if (!level.parse_succeeded) return;
            DialogResult = DialogResult.OK;
        }

        private void levelText_Enter(object sender, EventArgs e)
        {
            if (!hiddenInitialText)
            {
                levelText.Text = "";
                hiddenInitialText = true;
            }
        }
    }
}
