using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuadTreeUI
{
    public partial class Form1 : Form
    {
        public int ROW = 5;
        public int COL = 10;
        public static Graphics g;
        public static Pen pen;
        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(1200, 700);

            button1.Top = 550;
            button1.Left = 100;
            button1.Width = 300;
            button1.Text = "Create Tile";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // button1.Enabled = false;
            g = CreateGraphics();
            g.Clear(Color.White);
            pen = new Pen(Color.Black);
            for (int i = 0; i <= this.COL; i++)
            {
                g.DrawLine(pen, 0 + (Program.WINDOW_CONST * i), 0, 0 + (Program.WINDOW_CONST * i), Program.WINDOW_CONST * this.ROW);
            }
            for (int i = 0; i <= this.ROW; i++)
            {
                g.DrawLine(pen, 0, (Program.WINDOW_CONST*i), Program.WINDOW_CONST*this.COL, (Program.WINDOW_CONST*i));
            }

            subroutine subroutine = new subroutine();

            g.Dispose();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
