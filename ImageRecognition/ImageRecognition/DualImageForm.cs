using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace ImageRecognition
{
    public partial class DualImageForm : Form
    {
        Image pic;
        Image pic2;
        float percentageInt;

        public DualImageForm()
        {
            InitializeComponent();
        }

        private static Image resizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.Bicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return (Image)b;
        }

        public static Color Mix(Color from, Color to, float percent)
        {
            float amountFrom = 1.0f - percent;

            return Color.FromArgb(
            (byte)(from.A * amountFrom + to.A * percent),
            (byte)(from.R * amountFrom + to.R * percent),
            (byte)(from.G * amountFrom + to.G * percent),
            (byte)(from.B * amountFrom + to.B * percent));
        }

        public Boolean isMatching(Color a, Color b, float percent)
        {
            Boolean returnBool = false;
            Boolean Rmatches = false;
            Boolean Gmatches = false;
            Boolean Bmatches = false;
            if (Math.Abs(a.R - b.R) < percent * 255)
            {
                Rmatches = true;
            }
            if (Math.Abs(a.G - b.G) < percent * 255)
            {
                Gmatches = true;
            }
            if (Math.Abs(a.B - b.B) < percent * 255)
            {
                Bmatches = true;
            }
            if (Rmatches && Bmatches && Gmatches)
            {
                returnBool = true;
            }
            return returnBool;
        }

        public Boolean isEdge(Bitmap bmp, int i, int j)
        {
            Boolean returnBool = true;
            float percentage = percentageInt;

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr,
                           rgbValues, 0, bytes);

            byte red = 0;
            byte green = 0;
            byte blue = 0;

            //Color checking = bmp.GetPixel(i, j);
            /*if (isMatching(checking, img.GetPixel(i, j - 1), percentage) == true
                && isMatching(checking, img.GetPixel(i + 1, j - 1), percentage) == true
                && isMatching(checking, img.GetPixel(i + 1, j), percentage) == true
                && isMatching(checking, img.GetPixel(i + 1, j + 1), percentage) == true
                && isMatching(checking, img.GetPixel(i, j + 1), percentage) == true
                && isMatching(checking, img.GetPixel(i - 1, j + 1), percentage) == true
                && isMatching(checking, img.GetPixel(i - 1, j), percentage) == true
                && isMatching(checking, img.GetPixel(i - 1, j - 1), percentage) == true)*/
            if(true)
            {
                returnBool = false;
            }
            return returnBool;
        }

        private void button2_MouseClick(object sender, MouseEventArgs e)
        {
            openFileDialog1.Filter = "Image Files (*.jpeg;*.jpg;*.png;*.gif)|(*.jpeg;*.jpg;*.png;*.gif|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pic2 = new Bitmap(openFileDialog1.FileName);
                pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox2.Image = pic2;
            }
        }

        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            openFileDialog1.Filter = "Image Files (*.jpeg;*.jpg;*.png;*.gif)|(*.jpeg;*.jpg;*.png;*.gif|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pic = new Bitmap(openFileDialog1.FileName);
                pic2 = new Bitmap(pic.Width, pic.Height);
                pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox2.Image = pic2;
                pictureBox1.Image = pic;
                
            }
        }

        private void button3_MouseClick(object sender, MouseEventArgs e) //ANALYZE
        {
            percentageInt = float.Parse(textBox1.Text);
            //int scale = pic.Width/pictureBox1.Width /2;
            int scale = 2;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            if (scale == 0)
            {
                scale = 1;
            }
            Bitmap img = (Bitmap) resizeImage(pic, new Size(pic.Width/scale,pic.Height/scale));
            img.LockBits(new Rectangle(0, 0, img.Width, img.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Canonical);
            pic2 = new Bitmap(pic.Width/scale, pic.Height/scale);
            //pictureBox2.Image = pic2;
            Bitmap img2 = (Bitmap) pic2;
            long matchingPixels = 0;
            Boolean useEdgeDetection = true;
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    Color pixel = img.GetPixel(i,j);
                    Color pixel2 = Color.Empty;
                    if (i < img2.Width && j < img2.Height)
                    {
                        if (i-1 > 1 && j-1 > 1 && j+1 < img.Height && i+1 < img.Width)
                        {
                            if (isEdge(img, i, j))
                            {
                                Color nomatch = Color.Black;
                                img2.SetPixel(i, j, Mix(nomatch, img.GetPixel(i, j), 0f));
                                img2.SetPixel(i, j, Mix(nomatch, img.GetPixel(i, j), 0f));
                                
                            }
                        }
                    }
                    else
                    {
                        img.SetPixel(i, j, Mix(pixel,Color.Red,0.5f));
                    }
                    
                }
                
            }
            //pictureBox1.Image = img;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.Image = img2;

        }

        private void DualImageForm_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            HelpForm temp = new HelpForm();
            temp.ShowDialog();
        }

    }
}
