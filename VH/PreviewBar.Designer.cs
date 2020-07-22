namespace VH
{
    partial class PreviewBar
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
            this.Closepanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // Closepanel
            // 
            this.Closepanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Closepanel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Closepanel.BackgroundImage = global::VH.Properties.Resources.Close;
            this.Closepanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Closepanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Closepanel.Location = new System.Drawing.Point(304, 8);
            this.Closepanel.Name = "Closepanel";
            this.Closepanel.Size = new System.Drawing.Size(16, 16);
            this.Closepanel.TabIndex = 0;
            this.Closepanel.Click += new System.EventHandler(this.Closepanel_Click);
            // 
            // PreviewBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(328, 84);
            this.ControlBox = false;
            this.Controls.Add(this.Closepanel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PreviewBar";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Closepanel;
    }
}