namespace VH
{
    partial class RnD
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RnD));
            this.Quitbutton = new System.Windows.Forms.Button();
            this.fps_label = new System.Windows.Forms.Label();
            this.report_timer = new System.Windows.Forms.Timer(this.components);
            this.ScreencheckBox = new System.Windows.Forms.CheckBox();
            this.MovecheckBox = new System.Windows.Forms.CheckBox();
            this.ObjectcheckBox = new System.Windows.Forms.CheckBox();
            this.XtextBox = new System.Windows.Forms.TextBox();
            this.Xlabel = new System.Windows.Forms.Label();
            this.Ylabel = new System.Windows.Forms.Label();
            this.YtextBox = new System.Windows.Forms.TextBox();
            this.Runbutton = new System.Windows.Forms.Button();
            this.MEventscheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.Picturebutton = new System.Windows.Forms.Button();
            this.Picturepanel = new System.Windows.Forms.Panel();
            this.pointObjectcheckBox = new System.Windows.Forms.CheckBox();
            this.Resetbutton = new System.Windows.Forms.Button();
            this.Savebutton = new System.Windows.Forms.Button();
            this.Modelbutton = new System.Windows.Forms.Button();
            this.ResetModelbutton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Quitbutton
            // 
            this.Quitbutton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Quitbutton.Location = new System.Drawing.Point(281, 9);
            this.Quitbutton.Name = "Quitbutton";
            this.Quitbutton.Size = new System.Drawing.Size(50, 23);
            this.Quitbutton.TabIndex = 0;
            this.Quitbutton.Text = "Quit";
            this.Quitbutton.UseVisualStyleBackColor = true;
            this.Quitbutton.Click += new System.EventHandler(this.Quitbutton_Click);
            // 
            // fps_label
            // 
            this.fps_label.ForeColor = System.Drawing.Color.White;
            this.fps_label.Location = new System.Drawing.Point(12, 9);
            this.fps_label.Name = "fps_label";
            this.fps_label.Size = new System.Drawing.Size(76, 60);
            this.fps_label.TabIndex = 1;
            // 
            // report_timer
            // 
            this.report_timer.Enabled = true;
            this.report_timer.Tick += new System.EventHandler(this.report_timer_Tick);
            // 
            // ScreencheckBox
            // 
            this.ScreencheckBox.AutoSize = true;
            this.ScreencheckBox.Checked = true;
            this.ScreencheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ScreencheckBox.ForeColor = System.Drawing.Color.Yellow;
            this.ScreencheckBox.Location = new System.Drawing.Point(95, 9);
            this.ScreencheckBox.Name = "ScreencheckBox";
            this.ScreencheckBox.Size = new System.Drawing.Size(81, 17);
            this.ScreencheckBox.TabIndex = 2;
            this.ScreencheckBox.Text = "Screen Box";
            this.ScreencheckBox.UseVisualStyleBackColor = true;
            this.ScreencheckBox.CheckedChanged += new System.EventHandler(this.ScreencheckBox_CheckedChanged);
            // 
            // MovecheckBox
            // 
            this.MovecheckBox.AutoSize = true;
            this.MovecheckBox.Checked = true;
            this.MovecheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MovecheckBox.ForeColor = System.Drawing.Color.Yellow;
            this.MovecheckBox.Location = new System.Drawing.Point(95, 23);
            this.MovecheckBox.Name = "MovecheckBox";
            this.MovecheckBox.Size = new System.Drawing.Size(74, 17);
            this.MovecheckBox.TabIndex = 3;
            this.MovecheckBox.Text = "Move Box";
            this.MovecheckBox.UseVisualStyleBackColor = true;
            this.MovecheckBox.CheckedChanged += new System.EventHandler(this.MovecheckBox_CheckedChanged);
            // 
            // ObjectcheckBox
            // 
            this.ObjectcheckBox.AutoSize = true;
            this.ObjectcheckBox.Checked = true;
            this.ObjectcheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ObjectcheckBox.ForeColor = System.Drawing.Color.Yellow;
            this.ObjectcheckBox.Location = new System.Drawing.Point(95, 37);
            this.ObjectcheckBox.Name = "ObjectcheckBox";
            this.ObjectcheckBox.Size = new System.Drawing.Size(78, 17);
            this.ObjectcheckBox.TabIndex = 4;
            this.ObjectcheckBox.Text = "Object Box";
            this.ObjectcheckBox.UseVisualStyleBackColor = true;
            this.ObjectcheckBox.CheckedChanged += new System.EventHandler(this.ObjectcheckBox_CheckedChanged);
            // 
            // XtextBox
            // 
            this.XtextBox.Location = new System.Drawing.Point(30, 82);
            this.XtextBox.Name = "XtextBox";
            this.XtextBox.Size = new System.Drawing.Size(28, 20);
            this.XtextBox.TabIndex = 5;
            // 
            // Xlabel
            // 
            this.Xlabel.AutoSize = true;
            this.Xlabel.Location = new System.Drawing.Point(10, 85);
            this.Xlabel.Name = "Xlabel";
            this.Xlabel.Size = new System.Drawing.Size(17, 13);
            this.Xlabel.TabIndex = 6;
            this.Xlabel.Text = "X:";
            // 
            // Ylabel
            // 
            this.Ylabel.AutoSize = true;
            this.Ylabel.Location = new System.Drawing.Point(63, 85);
            this.Ylabel.Name = "Ylabel";
            this.Ylabel.Size = new System.Drawing.Size(17, 13);
            this.Ylabel.TabIndex = 8;
            this.Ylabel.Text = "Y:";
            // 
            // YtextBox
            // 
            this.YtextBox.Location = new System.Drawing.Point(83, 82);
            this.YtextBox.Name = "YtextBox";
            this.YtextBox.Size = new System.Drawing.Size(28, 20);
            this.YtextBox.TabIndex = 7;
            // 
            // Runbutton
            // 
            this.Runbutton.Location = new System.Drawing.Point(17, 110);
            this.Runbutton.Name = "Runbutton";
            this.Runbutton.Size = new System.Drawing.Size(43, 23);
            this.Runbutton.TabIndex = 9;
            this.Runbutton.Text = "Run";
            this.Runbutton.UseVisualStyleBackColor = true;
            this.Runbutton.Click += new System.EventHandler(this.Runbutton_Click);
            // 
            // MEventscheckedListBox
            // 
            this.MEventscheckedListBox.CheckOnClick = true;
            this.MEventscheckedListBox.FormattingEnabled = true;
            this.MEventscheckedListBox.Location = new System.Drawing.Point(163, 84);
            this.MEventscheckedListBox.Name = "MEventscheckedListBox";
            this.MEventscheckedListBox.Size = new System.Drawing.Size(168, 199);
            this.MEventscheckedListBox.TabIndex = 11;
            // 
            // Picturebutton
            // 
            this.Picturebutton.Location = new System.Drawing.Point(17, 139);
            this.Picturebutton.Name = "Picturebutton";
            this.Picturebutton.Size = new System.Drawing.Size(41, 23);
            this.Picturebutton.TabIndex = 12;
            this.Picturebutton.Text = "Pic";
            this.Picturebutton.UseVisualStyleBackColor = true;
            this.Picturebutton.Click += new System.EventHandler(this.Picturebutton_Click);
            // 
            // Picturepanel
            // 
            this.Picturepanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Picturepanel.Location = new System.Drawing.Point(17, 169);
            this.Picturepanel.Name = "Picturepanel";
            this.Picturepanel.Size = new System.Drawing.Size(140, 114);
            this.Picturepanel.TabIndex = 13;
            // 
            // pointObjectcheckBox
            // 
            this.pointObjectcheckBox.AutoSize = true;
            this.pointObjectcheckBox.Checked = true;
            this.pointObjectcheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.pointObjectcheckBox.ForeColor = System.Drawing.Color.Yellow;
            this.pointObjectcheckBox.Location = new System.Drawing.Point(95, 52);
            this.pointObjectcheckBox.Name = "pointObjectcheckBox";
            this.pointObjectcheckBox.Size = new System.Drawing.Size(84, 17);
            this.pointObjectcheckBox.TabIndex = 14;
            this.pointObjectcheckBox.Text = "Point Object";
            this.pointObjectcheckBox.UseVisualStyleBackColor = true;
            // 
            // Resetbutton
            // 
            this.Resetbutton.Location = new System.Drawing.Point(64, 139);
            this.Resetbutton.Name = "Resetbutton";
            this.Resetbutton.Size = new System.Drawing.Size(41, 23);
            this.Resetbutton.TabIndex = 15;
            this.Resetbutton.Text = "Res";
            this.Resetbutton.UseVisualStyleBackColor = true;
            this.Resetbutton.Click += new System.EventHandler(this.Resetbutton_Click);
            // 
            // Savebutton
            // 
            this.Savebutton.Location = new System.Drawing.Point(111, 139);
            this.Savebutton.Name = "Savebutton";
            this.Savebutton.Size = new System.Drawing.Size(41, 23);
            this.Savebutton.TabIndex = 16;
            this.Savebutton.Text = "Save";
            this.Savebutton.UseVisualStyleBackColor = true;
            this.Savebutton.Click += new System.EventHandler(this.Savebutton_Click);
            // 
            // Modelbutton
            // 
            this.Modelbutton.Location = new System.Drawing.Point(18, 311);
            this.Modelbutton.Name = "Modelbutton";
            this.Modelbutton.Size = new System.Drawing.Size(62, 23);
            this.Modelbutton.TabIndex = 17;
            this.Modelbutton.Text = "Create";
            this.Modelbutton.UseVisualStyleBackColor = true;
            this.Modelbutton.Click += new System.EventHandler(this.Modelbutton_Click);
            // 
            // ResetModelbutton
            // 
            this.ResetModelbutton.Location = new System.Drawing.Point(86, 311);
            this.ResetModelbutton.Name = "ResetModelbutton";
            this.ResetModelbutton.Size = new System.Drawing.Size(62, 23);
            this.ResetModelbutton.TabIndex = 18;
            this.ResetModelbutton.Text = "Reset";
            this.ResetModelbutton.UseVisualStyleBackColor = true;
            this.ResetModelbutton.Click += new System.EventHandler(this.ResetModelbutton_Click);
            // 
            // RnD
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(343, 346);
            this.Controls.Add(this.ResetModelbutton);
            this.Controls.Add(this.Modelbutton);
            this.Controls.Add(this.Savebutton);
            this.Controls.Add(this.Resetbutton);
            this.Controls.Add(this.pointObjectcheckBox);
            this.Controls.Add(this.Picturepanel);
            this.Controls.Add(this.Picturebutton);
            this.Controls.Add(this.MEventscheckedListBox);
            this.Controls.Add(this.Runbutton);
            this.Controls.Add(this.Ylabel);
            this.Controls.Add(this.YtextBox);
            this.Controls.Add(this.Xlabel);
            this.Controls.Add(this.XtextBox);
            this.Controls.Add(this.ObjectcheckBox);
            this.Controls.Add(this.MovecheckBox);
            this.Controls.Add(this.ScreencheckBox);
            this.Controls.Add(this.fps_label);
            this.Controls.Add(this.Quitbutton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "RnD";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "VH R&D";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RnD_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Quitbutton;
        private System.Windows.Forms.Label fps_label;
        private System.Windows.Forms.Timer report_timer;
        private System.Windows.Forms.CheckBox ScreencheckBox;
        private System.Windows.Forms.CheckBox MovecheckBox;
        private System.Windows.Forms.CheckBox ObjectcheckBox;
        private System.Windows.Forms.TextBox XtextBox;
        private System.Windows.Forms.Label Xlabel;
        private System.Windows.Forms.Label Ylabel;
        private System.Windows.Forms.TextBox YtextBox;
        private System.Windows.Forms.Button Runbutton;
        private System.Windows.Forms.CheckedListBox MEventscheckedListBox;
        private System.Windows.Forms.Button Picturebutton;
        private System.Windows.Forms.Panel Picturepanel;
        private System.Windows.Forms.CheckBox pointObjectcheckBox;
        private System.Windows.Forms.Button Resetbutton;
        private System.Windows.Forms.Button Savebutton;
        private System.Windows.Forms.Button Modelbutton;
        private System.Windows.Forms.Button ResetModelbutton;
    }
}