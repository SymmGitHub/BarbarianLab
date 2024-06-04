namespace BarbarianLab
{
    partial class SaveLevel
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
            Save = new Button();
            levelText = new TextBox();
            SuspendLayout();
            // 
            // Save
            // 
            Save.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Save.BackColor = Color.FromArgb(15, 10, 15);
            Save.ForeColor = SystemColors.Control;
            Save.Location = new Point(605, 405);
            Save.Name = "Save";
            Save.Size = new Size(115, 36);
            Save.TabIndex = 44;
            Save.Text = "Copy Text";
            Save.UseVisualStyleBackColor = false;
            Save.Click += Save_Click;
            // 
            // levelText
            // 
            levelText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            levelText.BackColor = Color.FromArgb(200, 180, 200);
            levelText.ForeColor = Color.Black;
            levelText.Location = new Point(12, 12);
            levelText.Margin = new Padding(3, 3, 0, 3);
            levelText.MaxLength = 999999999;
            levelText.Multiline = true;
            levelText.Name = "levelText";
            levelText.ScrollBars = ScrollBars.Both;
            levelText.Size = new Size(708, 387);
            levelText.TabIndex = 43;
            levelText.TabStop = false;
            levelText.WordWrap = false;
            // 
            // SaveLevel
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 20, 30);
            ClientSize = new Size(732, 453);
            Controls.Add(Save);
            Controls.Add(levelText);
            Name = "SaveLevel";
            ShowIcon = false;
            Text = "Save Level";
            Load += SaveLevel_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Save;
        private TextBox levelText;
    }
}