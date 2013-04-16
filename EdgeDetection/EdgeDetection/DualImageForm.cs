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
 * Speed. The program runs relatively slowly
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
using System.Diagnostics;

namespace ImageRecognition
{
    public partial class DualImageForm : Form
    {
        Image pic; //the first image, which the user loads
        Image pic2; //the second image, which displays the edges
        float percentageInt; //the acceptance value, given by the user
        Stopwatch clock;
        Boolean useLockBits = true;

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

        public Boolean isEdgeOptimized(Color[] colors)
        {
            //colors[0] should be the checking pixel
            Boolean returnBool = true;
            float percentage = percentageInt; //the percentage used is set
            //equal to the global variable percentageInt
            if (isMatching(colors[0], colors[1], percentage) &&
               isMatching(colors[0], colors[2], percentage) &&
               isMatching(colors[0], colors[3], percentage) &&
               //isMatching(colors[0], colors[4], percentage) &&
               //isMatching(colors[0], colors[5], percentage) &&
               //isMatching(colors[0], colors[6], percentage) &&
               //isMatching(colors[0], colors[7], percentage) &&
               isMatching(colors[0], colors[8], percentage))
            {
                returnBool = false;
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

            ///* 
             
            ///* 
            
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

        private void button1_MouseClick(object sender, MouseEventArgs e) //Open button
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

        private void button3_MouseClick(object sender, MouseEventArgs e) //Analyze button
        {
            clock = Stopwatch.StartNew();
            if(checkBox1.Checked)
            {
                useLockBits = true;
            }
            else
            {
                useLockBits = false;
            }
            analyze();
            clock.Stop();
            float time = clock.ElapsedMilliseconds;
            label3.Text = time/1000 + " sec";
        }

        

        private void button2_Click(object sender, EventArgs e) //Help Button
        {
            //if the help button is pressed
            HelpForm temp = new HelpForm(); //create a new help form
            temp.ShowDialog(); //and show it
        }

        private void button4_Click(object sender, EventArgs e) //Save Button
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

        private void analyze()
        {
            //When the analyze button is pressed
            percentageInt = float.Parse(textBox1.Text);
            float scale = 1;



            if (comboBox1.SelectedItem == "Auto")
            {
                scale = pic.Width / pictureBox1.Width;
            }
            else if (comboBox1.SelectedItem == "1/2")
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
                scale = pic.Width / pictureBox1.Width;
            }

            int tempWidth = 1;
            int tempHeight = 1;
            if (scale >= 1)
            {
                tempWidth = (int)Math.Floor(pic.Width / scale);
                tempHeight = (int)Math.Floor(pic.Height / scale);
            }
            else
            {
                tempWidth = pic.Width;
                tempHeight = pic.Height;
            }

            Bitmap img = (Bitmap)resizeImage(pic, new Size(tempWidth, tempHeight));
            pic2 = new Bitmap(tempWidth, tempHeight);
            Bitmap img2 = (Bitmap)pic2;
            Color[] pixels = null;
            if (useLockBits)
            {

                BitmapData bmd = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                 System.Drawing.Imaging.ImageLockMode.ReadOnly, img.PixelFormat);

                // Get the address of the first line.
                IntPtr ptr = bmd.Scan0;

                // Declare an array to hold the bytes of the bitmap. 
                int bytes = Math.Abs(bmd.Stride) * img.Height;
                byte[] rgbValues = new byte[bytes];
                //Goes R1, G1, B1, R2, etc!

                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                pixels = new Color[bytes / 4];
                for (int z = 0; z < bytes / 4; z++)
                {
                    pixels[z] = Color.FromArgb(rgbValues[4 * z + 2], rgbValues[4 * z + 1], rgbValues[4 * z]);
                }

                img.UnlockBits(bmd);

            }
            
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    
                    if (i < img2.Width && j < img2.Height)
                    {
                        if (i - 1 > 1 && j - 1 > 1 && j + 1 < img.Height && i + 1 < img.Width)
                        {
                            if (useLockBits)
                            {
                                Color[] temp = new Color[9];
                                temp[0] = pixels[((j - 1) * img.Width) + i]; //i, j
                                temp[1] = pixels[((j - 2) * img.Width) + i + 1]; //i + 1, j - 1
                                temp[2] = pixels[((j - 1) * img.Width) + i + 1]; //i + 1, j
                                temp[3] = pixels[((j) * img.Width) + i + 1]; //i + 1, j + 1
                                temp[4] = pixels[((j) * img.Width) + i]; //i, j + 1
                                temp[5] = pixels[((j) * img.Width) + i - 1]; //i-1, j + 1
                                temp[6] = pixels[((j - 1) * img.Width) + i - 1]; //i -1, j
                                temp[7] = pixels[((j - 2) * img.Width) + i - 1]; //i-1, j-1
                                temp[8] = pixels[((j - 2) * img.Width) + i]; //i, j - 1
                                if (isEdgeOptimized(temp))
                                {
                                    Color nomatch = Color.Black;
                                    img2.SetPixel(i, j, nomatch);
                                }
                                else
                                {
                                    img2.SetPixel(i, j, Mix(Color.White, img.GetPixel(i, j), 0f));
                                }
                            }
                            else
                            {
                                if (isEdge(img, i,j))
                                {
                                    Color nomatch = Color.Black;
                                    img2.SetPixel(i, j, nomatch);
                                }
                                else
                                {
                                    img2.SetPixel(i, j, Mix(Color.White, img.GetPixel(i, j), 0f));
                                }
                            }
                            //img2.SetPixel(i,j,temp[0]);
                        }
                    }
                    else
                    {
                       
                    }
                }

            }
            pictureBox2.Image = img2;

        }
        



    }
}
