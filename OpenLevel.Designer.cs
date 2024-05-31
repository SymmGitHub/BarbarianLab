namespace BarbarianLab
{
    partial class OpenLevel
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
            levelText = new TextBox();
            Open = new Button();
            SuspendLayout();
            // 
            // levelText
            // 
            levelText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            levelText.BackColor = Color.FromArgb(200, 180, 200);
            levelText.ForeColor = Color.Black;
            levelText.Location = new Point(12, 12);
            levelText.Name = "levelText";
            levelText.ScrollBars = ScrollBars.Horizontal;
            levelText.Size = new Size(423, 27);
            levelText.TabIndex = 1;
            levelText.TabStop = false;
            levelText.Text = "new Array( Enter Level Data Here );";
            levelText.Enter += levelText_Enter;
            // 
            // Open
            // 
            Open.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Open.BackColor = Color.FromArgb(15, 10, 15);
            Open.ForeColor = SystemColors.Control;
            Open.Location = new Point(165, 75);
            Open.Name = "Open";
            Open.Size = new Size(115, 36);
            Open.TabIndex = 42;
            Open.Text = "Open Level";
            Open.UseVisualStyleBackColor = false;
            Open.Click += Open_Click;
            // 
            // OpenLevel
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 20, 30);
            ClientSize = new Size(447, 123);
            Controls.Add(Open);
            Controls.Add(levelText);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "OpenLevel";
            StartPosition = FormStartPosition.CenterParent;
            Text = "OpenLevel";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox levelText;
        private Button Open;
    }
}