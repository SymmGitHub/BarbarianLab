namespace BarbarianLab
{
    partial class TextWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Button = new Button();
            levelText = new TextBox();
            SuspendLayout();
            // 
            // Open
            // 
            Button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Button.BackColor = Color.FromArgb(15, 10, 15);
            Button.ForeColor = SystemColors.Control;
            Button.Location = new Point(559, 405);
            Button.Name = "Open";
            Button.Size = new Size(162, 36);
            Button.TabIndex = 46;
            Button.Text = "Open Level(s)";
            Button.UseVisualStyleBackColor = false;
            Button.Click += Button_Click;
            // 
            // levelText
            // 
            levelText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            levelText.BackColor = Color.FromArgb(200, 180, 200);
            levelText.ForeColor = Color.Black;
            levelText.Location = new Point(12, 12);
            levelText.MaxLength = 999999999;
            levelText.Multiline = true;
            levelText.Name = "levelText";
            levelText.ScrollBars = ScrollBars.Both;
            levelText.Size = new Size(709, 387);
            levelText.TabIndex = 45;
            levelText.TabStop = false;
            levelText.WordWrap = false;
            // 
            // OpenLevel
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 20, 30);
            ClientSize = new Size(732, 453);
            Controls.Add(Button);
            Controls.Add(levelText);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "OpenLevel";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Open Level";
            Load += OpenLevel_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button Button;
        private TextBox levelText;
    }
}