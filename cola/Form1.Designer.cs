namespace cola
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            RTB = new RichTextBox();
            BTN = new Button();
            SuspendLayout();
            // 
            // RTB
            // 
            RTB.Location = new Point(33, 496);
            RTB.Name = "RTB";
            RTB.Size = new Size(942, 523);
            RTB.TabIndex = 0;
            RTB.Text = "";
            RTB.TextChanged += RTB_TextChanged;
            // 
            // BTN
            // 
            BTN.Location = new Point(742, 131);
            BTN.Name = "BTN";
            BTN.Size = new Size(220, 93);
            BTN.TabIndex = 1;
            BTN.Text = "button1";
            BTN.UseVisualStyleBackColor = true;
            BTN.Click += BTN_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(14F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(987, 1031);
            Controls.Add(BTN);
            Controls.Add(RTB);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox RTB;
        private Button BTN;
    }
}
