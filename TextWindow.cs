using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace BarbarianLab
{
    public partial class TextWindow : Form
    {
        public BoB_LevelData.Level[]? levels;
        public Form1.Theme t;
        public string windowType = "Open_Level";
        public string windowText = "";
        public TextWindow(Form1.Theme theme, string type, string windowText)
        {
            InitializeComponent();
            this.Focus();
            t = theme;
            this.windowText = windowText;
            windowType = type;
        }
        private void OpenLevel_Load(object sender, EventArgs e)
        {
            Text = windowType.Replace("_", " ");
            BackColor = t.bg;
            levelText.BackColor = t.ui;
            levelText.ForeColor = t.ui_text;
            Button.BackColor = t.btn;
            Button.ForeColor = t.btn_text;
            switch (windowType)
            {
                case "Open_Level":
                    Button.Text = "Open Level(s)";
                    break;
                case "Export_Level":
                    Button.Text = "Copy Level Text";
                    break;
                case "Export_Playlist":
                    Button.Text = "Copy Playlist Text";
                    break;
            }
            levelText.Text = windowText;
        }
        private void Button_Click(object sender, EventArgs e)
        {
            switch (windowType)
            {
                case "Open_Level":
                    OpenLevels();
                    break;
                case "Export_Level":
                    CopyText();
                    break;
                case "Export_Playlist":
                    CopyText();
                    break;
            }
        }
        private void CopyText()
        {
            string newText = "";
            for (int i = 0; i < levelText.Lines.Length; i++)
            {
                string line = levelText.Lines[i].Split("//", StringSplitOptions.None)[0];
                if (!String.IsNullOrEmpty(line))
                {
                    newText += line + Environment.NewLine;
                }
            }
            try
            {
                Clipboard.SetText(newText.TrimEnd());
            }
            catch { MessageBox.Show("Failed to copy to clipboard, please try again."); }
        }
        private void OpenLevels()
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
