using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VH
{
    public partial class PreviewBar : Form
    {
        int DefaultHeight = 150;
        public PreviewBar()
        {
            InitializeComponent();
            Height = DefaultHeight;
            Width =  Graphic.WorkingArea_rect.Width;
            Left = 0;
            Top = Graphic.WorkingArea_rect.Bottom - Height; 
        }

        private void Closepanel_Click(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
