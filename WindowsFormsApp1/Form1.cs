using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            newImg();
        }

        private Pen left = new Pen(Color.Black);

        private void button2_Click(object sender, EventArgs e)
        {
            newImg();
        }

        private void newImg()
        {
            Bitmap im = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(im))
                g.Clear(Color.White);

            if (pictureBox1.Image != null)
                pictureBox1.Image.Dispose();
            pictureBox1.Image = im;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                left.Color = button1.BackColor = colorDialog1.Color;
        }

        private Point start;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (radioButton2.Checked)
                filling(e.X, e.Y);
            else if (radioButton3.Checked)
                fillingImg(e.X, e.Y);
            else
                start = e.Location;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                if (radioButton1.Checked)
                    pencil(sender, e);
        }

        private void pencil(object sender, MouseEventArgs e)
        {
            Bitmap im = new Bitmap(pictureBox1.Image);
            Graphics g = Graphics.FromImage(im);
            g.DrawLine(left, start, e.Location);
            start = e.Location;
            pictureBox1.Image = im;
        }

        private void filling(int x, int y)
        {
            Bitmap im = new Bitmap(pictureBox1.Image);
            Color cp = im.GetPixel(x, y);
            fill(im, cp, left, x, y);
            pictureBox1.Image = im;
        }

        private void fillingImg(int x, int y)
        {
            Bitmap fillImg = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            openFileDialog1.Filter = "ALL Image|*.png;*.bmp;*.jpg;*.gif";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                fillImg = new Bitmap(openFileDialog1.FileName);

            Bitmap im = new Bitmap(pictureBox1.Image);
            Color cp = im.GetPixel(x, y);
            fillFromImg(im, cp, fillImg, x, y);
            pictureBox1.Image = im;
        }

        private void fillFromImg(Bitmap im, Color col, Bitmap fillImg, int x, int y)
        {
            if ((x < 0) || (y < 0) || (x >= im.Width) || (y >= im.Height))
                return;
            Color cur = im.GetPixel(x, y);
            Color icol = fillImg.GetPixel(x % fillImg.Width, y % fillImg.Height);
            if ((!cur.Equals(col)) || (cur.Equals(icol)))
                return;
            int xL, xR;
            xL = xR = x;
            while ((xL > 0) && (col.Equals(cur)))
            {
                xL--;
                cur = im.GetPixel(xL, y);
            }
            cur = col;
            while ((xR < im.Width - 1) && (cur.Equals(col)))
            {
                xR++;
                cur = im.GetPixel(xR, y);
            }
            if (xL != 0)
                xL++;
            if (xR != im.Width - 1)
                xR--;
            for (int i = xL; i <= xR; i++)
            {
                icol = fillImg.GetPixel(i % fillImg.Width, y % fillImg.Height);
                im.SetPixel(i, y, icol);
            }

            for (int i = xL; i <= xR; i++)
                fillFromImg(im, col, fillImg, i, y + 1);

            for (int i = xL; i <= xR; i++)
                fillFromImg(im, col, fillImg, i, y - 1);
        }

        private void fill(Bitmap im, Color col, Pen p, int x, int y)
        {
            if ((x < 0) || (y < 0) || (x >= im.Width) || (y >= im.Height))
                return;
            Color cur = im.GetPixel(x, y);
            if ((!cur.Equals(col)) || (cur.Equals(p.Color)))
                return;
            int xL, xR;
            xL = xR = x;
            while  ((xL > 0) && (col.Equals(cur)))
            {
                xL--;
                cur = im.GetPixel(xL, y);
            }
            cur = col;
            while ((xR < im.Width - 1) && (cur.Equals(col)))
            {
                xR++;
                cur = im.GetPixel(xR, y);
            }
            if (xL != 0)
                xL++;
            if (xR != im.Width - 1)
                xR--;
            Graphics g = Graphics.FromImage(im);
            g.DrawLine(p, xL, y, xR, y);

            for (int i = xL; i <= xR; i++)
                fill(im, col, p, i, y + 1);

            for (int i = xL; i <= xR; i++)
                fill(im, col, p, i, y - 1);
        }

        private Bitmap img;
        private void button3_Click(object sender, EventArgs e)
        {
            img = new Bitmap(pictureBox1.Image);
            List<Point> points = findContour();
            foreach (Point p in points)
                img.SetPixel(p.X, p.Y, Color.Red);
            pictureBox1.Image = img;
        }

        private int dir;
        private List<Point> findContour()
        {
            List<Point> points = new List<Point>();
            Point startP = findRightTop(img.GetPixel(img.Width - 1, 0));
            if (startP.X == 0 && startP.Y == 0)
                return points;
            points.Add(startP);
            dir = 6;

            Color col = img.GetPixel(startP.X,  startP.Y);
            Point nextP = nextPoint(startP, col);
            points.Add(nextP);
            while (nextP != startP)
            {
                nextP = nextPoint(nextP, col);
                points.Add(nextP);
            }
            return points;
        }

        private Point findRightTop(Color col)
        {
            for (int i = img.Width - 1; i >= 0; i--)
                for (int j = 0; j < img.Height; j++)
                    if (col != img.GetPixel(i, j))
                        return new Point(i, j);
            return new Point(0, 0);
        }

        private Point nextPoint(Point curP, Color col)
        {
            int startD = (dir + 2) % 8;
            Point nextP = nextStep(curP, startD);
            int newD = startD;
            while(img.GetPixel(nextP.X,nextP.Y) != col)
            {
                if (newD < 0)
                    newD += 8;
                nextP = nextStep(curP, newD);
                newD--;
            }
            dir = newD;
            return nextP;
        }
        private Point nextStep(Point curP, int d)
        {
            switch (d)
            {
                case 0: return new Point(curP.X + 1, curP.Y);
                case 1: return new Point(curP.X + 1, curP.Y - 1);
                case 2: return new Point(curP.X, curP.Y - 1);
                case 3: return new Point(curP.X - 1, curP.Y - 1);
                case 4: return new Point(curP.X - 1, curP.Y);
                case 5: return new Point(curP.X - 1, curP.Y + 1);
                case 6: return new Point(curP.X, curP.Y + 1);
                case 7: return new Point(curP.X + 1, curP.Y + 1);
                default: return curP;
            }
        }
      
    }
}
