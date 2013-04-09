/* 
 * This program is created by Viktor Koves.
 * To see licensing information for this program,
 * please look in the readme file that is in the
 * github repository. 
 * 
 * The github repository can be found at:
 * https://github.com/vkoves/EdgeDetection
 */
/*
 * Current problems:
 * The edgified image looks pixelated, though it is high res,
 * because an edge pixel cannot form alone. Due to the algorithm
*/

//All the imports used
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
        Image pic; //the first image, which the user loads
        Image pic2; //the second image, which displays the edges
        float percentageInt; //the acceptance value, given by the user

        public DualImageForm()
        {
            InitializeComponent();
        }

        private static Image resizeImage(Image imgToResize, Size size) 
        {
            //this method is used to resize images to a new resolution
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
            //this method is used to mix colors, for a transparency effect
            //it is not currently used in the program
            float amountFrom = 1.0f - percent;

            return Color.FromArgb(
            (byte)(from.A * amountFrom + to.A * percent),
            (byte)(from.R * amountFrom + to.R * percent),
            (byte)(from.G * amountFrom + to.G * percent),
            (byte)(from.B * amountFrom + to.B * percent));
        }

        public Boolean isMatching(Color a, Color b, float percent)
        {
            //this method is used to identify whether two pixels, 
            //of color a and b match, as in they can be considered
            //a solid color based on the acceptance value (percent)

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
            //This method determines whether an inputed pixel,
            //of position (i, j) on the Bitmap bmp, is an edge
            //it uses the global variable percentageInt when
            //it calls is matching
            Boolean returnBool = true; //By default, we say the pixel is
            //an edge, and we try to disprove it by checking nearby pixels 
            float percentage = percentageInt; //the percentage used is set
            //equal to the global variable percentageInt

            Bitmap img = bmp; //creates a new bitmap from the inputted bitmap
            Color checking = bmp.GetPixel(i, j); //the pixel we are checking
            //to be an edge is located at (i, j), and thus we fetch the color
            //in the bitmap at (i, j)

            //Here the program checks if all eight nearby pixels match the
            //pixel that is being checked to be an edge
            if (isMatching(checking, img.GetPixel(i, j - 1), percentage) == true
                && isMatching(checking, img.GetPixel(i + 1, j - 1), percentage) == true
                && isMatching(checking, img.GetPixel(i + 1, j), percentage) == true
                //&& isMatching(checking, img.GetPixel(i + 1, j + 1), percentage) == true
                //&& isMatching(checking, img.GetPixel(i, j + 1), percentage) == true
                //&& isMatching(checking, img.GetPixel(i - 1, j + 1), percentage) == true
                //&& isMatching(checking, img.GetPixel(i - 1, j), percentage) == true
                && isMatching(checking, img.GetPixel(i - 1, j - 1), percentage) == true)
            {
                returnBool = false; //if all the nearby pixels match the 
                //pixel being checked, we say the pixel being check is not
                //an edge
            }
            return returnBool;
        }

        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            //if the "Open" button is pressed, let the user 
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

        private void button3_MouseClick(object sender, MouseEventArgs e)
        {
            //When the analyze button is pressed
            percentageInt = float.Parse(textBox1.Text);
            float scale = 1; 
            if (comboBox1.SelectedItem == "Auto")
            {
                scale = pic.Width/pictureBox1.Width;;
            }
            else if(comboBox1.SelectedItem == "1/2")
            {
                scale = 2;
            }
            else if (comboBox1.SelectedItem == "1/4")
            {
                scale = 4;
            }
            else if (comboBox1.SelectedItem == "Original")
            {
                scale = 1;
            }
            else
            {
                scale = pic.Width/pictureBox1.Width;
            }

            int tempWidth = (int) Math.Floor(pic.Width / scale);
            int tempHeight = (int)Math.Floor(pic.Height / scale);

            Bitmap img = (Bitmap)resizeImage(pic, new Size(tempWidth, tempHeight));
            pic2 = new Bitmap(tempWidth, tempHeight);
            Bitmap img2 = (Bitmap) pic2;
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
                            }
                            else
                            {
                                img2.SetPixel(i, j, Mix(Color.White, img.GetPixel(i, j), 0f));
                            }
                        }
                    }
                    else
                    {
                        img.SetPixel(i, j, Mix(pixel,Color.Red,0.5f));
                    }
                }
                
            }
            pictureBox2.Image = img2;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //if the help button is pressed
            HelpForm temp = new HelpForm(); //create a new help form
            temp.ShowDialog(); //and show it
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                System.IO.FileStream fs =
                   (System.IO.FileStream)saveFileDialog1.OpenFile();
                // Saves the Image in the appropriate ImageFormat based upon the
                // File type selected in the dialog box.
                // NOTE that the FilterIndex property is one-based.
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        pictureBox2.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        pictureBox2.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 3:
                        pictureBox2.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                }
                fs.Close();
            }
        }
    }
}
