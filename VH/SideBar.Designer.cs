namespace VH
{
    partial class SideBar
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SideBar));
            this.Recordbutton = new System.Windows.Forms.Button();
            this.Runbutton = new System.Windows.Forms.Button();
            this.BarHandlertimer = new System.Windows.Forms.Timer(this.components);
            this.Playbutton = new System.Windows.Forms.Button();
            this.EditpictureBox = new System.Windows.Forms.PictureBox();
            this.SettingpictureBox = new System.Windows.Forms.PictureBox();
            this.LoadpictureBox = new System.Windows.Forms.PictureBox();
            this.SavepictureBox = new System.Windows.Forms.PictureBox();
            this.PinPintureBox = new System.Windows.Forms.PictureBox();
            this.ExitpictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.EditpictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SettingpictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LoadpictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SavepictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PinPintureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ExitpictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // Recordbutton
            // 
            this.Recordbutton.Location = new System.Drawing.Point(12, 26);
            this.Recordbutton.Name = "Recordbutton";
            this.Recordbutton.Size = new System.Drawing.Size(56, 23);
            this.Recordbutton.TabIndex = 4;
            this.Recordbutton.Text = "Record";
            this.Recordbutton.UseVisualStyleBackColor = true;
            this.Recordbutton.Click += new System.EventHandler(this.Recordbutton_Click);
            this.Recordbutton.MouseEnter += new System.EventHandler(this.SideBar_MouseEnter);
            this.Recordbutton.MouseLeave += new System.EventHandler(this.SideBar_MouseLeave);
            // 
            // Runbutton
            // 
            this.Runbutton.Location = new System.Drawing.Point(12, 84);
            this.Runbutton.Name = "Runbutton";
            this.Runbutton.Size = new System.Drawing.Size(56, 23);
            this.Runbutton.TabIndex = 7;
            this.Runbutton.Text = "R + P";
            this.Runbutton.UseVisualStyleBackColor = true;
            this.Runbutton.Click += new System.EventHandler(this.Runbutton_Click);
            this.Runbutton.MouseEnter += new System.EventHandler(this.SideBar_MouseEnter);
            this.Runbutton.MouseLeave += new System.EventHandler(this.SideBar_MouseLeave);
            // 
            // BarHandlertimer
            // 
            this.BarHandlertimer.Enabled = true;
            this.BarHandlertimer.Interval = 20;
            this.BarHandlertimer.Tick += new System.EventHandler(this.BarHandlertimer_Tick);
            // 
            // Playbutton
            // 
            this.Playbutton.Location = new System.Drawing.Point(12, 55);
            this.Playbutton.Name = "Playbutton";
            this.Playbutton.Size = new System.Drawing.Size(56, 23);
            this.Playbutton.TabIndex = 16;
            this.Playbutton.Text = "Play";
            this.Playbutton.UseVisualStyleBackColor = true;
            this.Playbutton.Click += new System.EventHandler(this.Playbutton_Click);
            // 
            // EditpictureBox
            // 
            this.EditpictureBox.BackColor = System.Drawing.Color.Transparent;
            this.EditpictureBox.BackgroundImage = global::VH.Properties.Resources.Edit;
            this.EditpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.EditpictureBox.Location = new System.Drawing.Point(15, 111);
            this.EditpictureBox.Name = "EditpictureBox";
            this.EditpictureBox.Size = new System.Drawing.Size(50, 25);
            this.EditpictureBox.TabIndex = 19;
            this.EditpictureBox.TabStop = false;
            this.EditpictureBox.Click += new System.EventHandler(this.EditpictureBox_Click);
            // 
            // SettingpictureBox
            // 
            this.SettingpictureBox.BackColor = System.Drawing.Color.Transparent;
            this.SettingpictureBox.BackgroundImage = global::VH.Properties.Resources.Setting;
            this.SettingpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.SettingpictureBox.Location = new System.Drawing.Point(15, 204);
            this.SettingpictureBox.Name = "SettingpictureBox";
            this.SettingpictureBox.Size = new System.Drawing.Size(50, 25);
            this.SettingpictureBox.TabIndex = 18;
            this.SettingpictureBox.TabStop = false;
            this.SettingpictureBox.Click += new System.EventHandler(this.SettingpictureBox_Click);
            // 
            // LoadpictureBox
            // 
            this.LoadpictureBox.BackColor = System.Drawing.Color.Transparent;
            this.LoadpictureBox.BackgroundImage = global::VH.Properties.Resources.Load;
            this.LoadpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.LoadpictureBox.Location = new System.Drawing.Point(15, 173);
            this.LoadpictureBox.Name = "LoadpictureBox";
            this.LoadpictureBox.Size = new System.Drawing.Size(50, 25);
            this.LoadpictureBox.TabIndex = 17;
            this.LoadpictureBox.TabStop = false;
            this.LoadpictureBox.Click += new System.EventHandler(this.LoadpictureBox_Click);
            // 
            // SavepictureBox
            // 
            this.SavepictureBox.BackColor = System.Drawing.Color.Transparent;
            this.SavepictureBox.BackgroundImage = global::VH.Properties.Resources.Save;
            this.SavepictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.SavepictureBox.Location = new System.Drawing.Point(15, 142);
            this.SavepictureBox.Name = "SavepictureBox";
            this.SavepictureBox.Size = new System.Drawing.Size(50, 25);
            this.SavepictureBox.TabIndex = 15;
            this.SavepictureBox.TabStop = false;
            this.SavepictureBox.Click += new System.EventHandler(this.SavepictureBox_Click);
            // 
            // PinPintureBox
            // 
            this.PinPintureBox.BackColor = System.Drawing.Color.Transparent;
            this.PinPintureBox.BackgroundImage = global::VH.Properties.Resources.Hide;
            this.PinPintureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.PinPintureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PinPintureBox.Location = new System.Drawing.Point(57, 4);
            this.PinPintureBox.Name = "PinPintureBox";
            this.PinPintureBox.Size = new System.Drawing.Size(16, 16);
            this.PinPintureBox.TabIndex = 14;
            this.PinPintureBox.TabStop = false;
            this.PinPintureBox.Tag = "";
            this.PinPintureBox.Click += new System.EventHandler(this.PinPintureBox_Click);
            this.PinPintureBox.MouseEnter += new System.EventHandler(this.SideBar_MouseEnter);
            this.PinPintureBox.MouseLeave += new System.EventHandler(this.SideBar_MouseLeave);
            // 
            // ExitpictureBox
            // 
            this.ExitpictureBox.BackColor = System.Drawing.Color.Transparent;
            this.ExitpictureBox.BackgroundImage = global::VH.Properties.Resources.Exit;
            this.ExitpictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ExitpictureBox.Location = new System.Drawing.Point(15, 240);
            this.ExitpictureBox.Name = "ExitpictureBox";
            this.ExitpictureBox.Size = new System.Drawing.Size(50, 25);
            this.ExitpictureBox.TabIndex = 12;
            this.ExitpictureBox.TabStop = false;
            this.ExitpictureBox.Click += new System.EventHandler(this.ExitpictureBox_Click);
            this.ExitpictureBox.MouseEnter += new System.EventHandler(this.SideBar_MouseEnter);
            this.ExitpictureBox.MouseLeave += new System.EventHandler(this.SideBar_MouseLeave);
            // 
            // SideBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(80, 274);
            this.ControlBox = false;
            this.Controls.Add(this.EditpictureBox);
            this.Controls.Add(this.SettingpictureBox);
            this.Controls.Add(this.LoadpictureBox);
            this.Controls.Add(this.Playbutton);
            this.Controls.Add(this.SavepictureBox);
            this.Controls.Add(this.PinPintureBox);
            this.Controls.Add(this.ExitpictureBox);
            this.Controls.Add(this.Runbutton);
            this.Controls.Add(this.Recordbutton);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SideBar";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "SideBar";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.SideBar_Load);
            this.MouseEnter += new System.EventHandler(this.SideBar_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.SideBar_MouseLeave);
            ((System.ComponentModel.ISupportInitialize)(this.EditpictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SettingpictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LoadpictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SavepictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PinPintureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ExitpictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button Recordbutton;
        private System.Windows.Forms.Button Runbutton;
        private System.Windows.Forms.Timer BarHandlertimer;
        private System.Windows.Forms.PictureBox ExitpictureBox;
        private System.Windows.Forms.PictureBox PinPintureBox;
        private System.Windows.Forms.PictureBox SavepictureBox;
        private System.Windows.Forms.Button Playbutton;
        private System.Windows.Forms.PictureBox LoadpictureBox;
        private System.Windows.Forms.PictureBox SettingpictureBox;
        private System.Windows.Forms.PictureBox EditpictureBox;
    }
}