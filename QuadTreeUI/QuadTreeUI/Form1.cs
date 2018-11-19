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
            for (int i = 0; i <= 10; i++)
            {
                g.DrawLine(pen, 0 + (100 * i), 0, 0 + (100 * i), 500);
            }
            for (int i = 0; i <= 5; i++)
            {
                g.DrawLine(pen, 0, (100*i), 1000, (100*i));
            }

            subroutine subroutine = new subroutine();

            g.Dispose();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
