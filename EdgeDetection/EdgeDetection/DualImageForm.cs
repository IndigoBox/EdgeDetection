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
using System.Collections;
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
        Boolean[,] edgeData;
        ArrayList edges = new ArrayList();
        int width;
        int height;
        int pixelThresh;
        Bitmap img;

        public DualImageForm()
        {
            Debug.WriteLine("form loaded");
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
            analyze();
            clock.Stop();
            float time = clock.ElapsedMilliseconds;
            label3.Text = time / 1000 + " sec";
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

        private unsafe static bool IsEdgeOptimized(byte* pp, byte* cp, byte* np, int scaledPercent)
        {
            return !IsMatching(cp, pp - 3, scaledPercent) &&
                   //!IsMatching(cp, pp, scaledPercent) &&
                   //!IsMatching(cp, pp + 3, scaledPercent) &&
                   //!IsMatching(cp, cp - 3, scaledPercent) &&
                   //!IsMatching(cp, cp + 3, scaledPercent) &&
                   //!IsMatching(cp, np - 3, scaledPercent) &&
                   !IsMatching(cp, np, scaledPercent) &&
                   !IsMatching(cp, np + 3, scaledPercent);
        }

        private unsafe static bool IsMatching(byte* p1, byte* p2, int thresh)
        {
            if (false)
            {
                Debug.WriteLine("Pixel 1: B " + *p1++ + " G " + *p1++ + " R " + *p1++);
                Debug.WriteLine("Pixel 2: B " + *p2++ + " G " + *p2++ + " R " + *p2++);
                Debug.WriteLine("thresh " + thresh);
                Debug.WriteLine(Math.Abs(*p1++ - *p2++) < thresh && Math.Abs(*p1++ - *p2++) < thresh && Math.Abs(*p1++ - *p2++) < thresh);
            }
            return Math.Abs(*p1++ - *p2++) < thresh && Math.Abs(*p1++ - *p2++) < thresh && Math.Abs(*p1++ - *p2++) < thresh;
            //return true;
        }

        private void button5_MouseClick(object sender, MouseEventArgs e)
        {
            pixelThresh = int.Parse(textBox2.Text);
            separateEdges();
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

            width = pic.Width;
            height = pic.Height;
            edgeData = new Boolean[pic.Width, pic.Height];

            img = (Bitmap)resizeImage(pic, new Size(tempWidth, tempHeight));
            pic2 = new Bitmap(tempWidth, tempHeight);
            Bitmap img2 = (Bitmap)pic2;
            Color[] pixels = null;
            
            BitmapData data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
            ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int size = Math.Abs(data.Stride) * img.Height;
            Byte[] bytes = new byte[size];

            int scaledPercent = (int)(Math.Round(percentageInt * 255));
            Debug.WriteLine("percent " + scaledPercent);
            unsafe
            {
                    Debug.WriteLine("Woah there, unsafe stuff");
                    byte* prevLine = (byte*)data.Scan0;
                    byte* currLine = prevLine + data.Stride;
                    byte* nextLine = currLine + data.Stride;

                    for (int y = 1; y < img.Height - 1; y++)
                    {

                        byte* pp = prevLine + 3;
                        byte* cp = currLine + 3;
                        byte* np = nextLine + 3;
                        for (int x = 1; x < img.Width - 1; x++)
                        {
                            if (IsEdgeOptimized(pp, cp, np, scaledPercent))
                            {
                                edgeData[x, y] = true;
                                //Debug.WriteLine("x " + x + "y " + y);
                                
                                //img2.SetPixel(x, y, Color.Black);
                                //bytes[(y * img.Width + x) * 3 + 2] = 255;
                            }
                            else
                            {
                                bytes[(y * data.Stride) + (x * 3)] = 255;
                                bytes[(y * data.Stride) + (x * 3) +1] = 255;
                                bytes[(y * data.Stride) + (x * 3) + 2] = 255;
                                //img2.SetPixel(x, y, Color.White);
                            }
                            pp += 3; cp += 3; np += 3;
                        }
                        prevLine = currLine;
                        currLine = nextLine;
                        nextLine += data.Stride;
                    }
                }
            System.Runtime.InteropServices.Marshal.Copy(bytes, 0, data.Scan0, size);
            img.UnlockBits(data);
            pictureBox2.Image = img;
        } // end analyze

        private void separateEdges()
        {
            Boolean red = false;
            Color temp = Color.Blue;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (edgeData[x, y])
                    {
                        red = !red;
                        if (red)
                            temp = Color.Red;
                        else
                            temp = Color.Blue;
                        //cycle clockwise from (x-1, y-1)
                        int[] next = new int[2]{x,y};
                        edgeData[next[0], next[1]] = false;
                        img.SetPixel(next[0], next[1], temp);
                        while (next != null && next[0] != -1)
                        {
                            //Debug.WriteLine("while");
                            try
                            {
                                //Debug.WriteLine("next x " + next[0] + " y " + next[1]);
                                edgeData[next[0], next[1]] = false;
                                img.SetPixel(next[0], next[1], temp);
                            }
                            catch (IndexOutOfRangeException)
                            {
                                Debug.WriteLine("index out of range    x " + next[0] + " y " + next[1]);
                            }
                            next = scan(next, pixelThresh);
                        }
                    }
                }
            }
            pictureBox1.Image = img;
        } //end separate

        private int[] scan(int[] temp, int thresh)
        {
            
            int x = temp[0];
            int y = temp[1];
            int d = 2;
            int[] next = new int[2]{-1, -1};
            //Debug.WriteLine("array " + next[0]);
            while (next != null && next[0] == -1 &&  d < thresh)
            {
                //Debug.WriteLine("scan depth " + d);
                next = search(x, y, d);
                if (next == null || next[0] == -1)
                    Debug.WriteLine("proceeding to deeper scan");
                d += 1;
            }
            return next;
        }

        private int[] search(int x, int y, int thresh)
        {
            //Debug.WriteLine("search " + thresh);
            int d = thresh;
            int xtemp = x - d;
            int ytemp = y - d;
            int[] test = new int[2];
            test[0] = -1;
            test[0] = -1;
            if (thresh > 0)
            {
                while (xtemp <= x + d && (test == null || test[0] == -1))
                {
                    //Debug.WriteLine("while 1 ");
                    test = check(new int[2]{xtemp, ytemp});
                    xtemp += 1;
                }
                xtemp = x + d; ytemp = y - d + 1;
                while (ytemp <= y + d - 1 && (test == null || test[0] == -1))
                {
                    //Debug.WriteLine("while 2 ");
                    test = check(new int[2] { xtemp, ytemp });
                    ytemp += 1;
                }
                ytemp = y + d;
                while (xtemp >= x - d && (test == null || test[0] == -1))
                {
                    //Debug.WriteLine("while 3 ");
                    test = check(new int[2] { xtemp, ytemp });
                    xtemp -= 1;
                }
                xtemp = x - d; ytemp = y + d - 1;
                while (ytemp >= y - d + 1 && (test == null || test[0] == -1))
                {
                    //Debug.WriteLine("while 4 ");
                    test = check(new int[2] { xtemp, ytemp });
                    ytemp -= 1;
                }
            }
            return test;
        }

        private int[] check(int[] temp)
        {
            int x = temp[0];
            int y = temp[1];
            if(x-1 >= 0 && (y-1) >= 0 && x+1 < width && y+1 < height)
            {
                //Debug.WriteLine("X " + x + "   Y " + y);
                if (edgeData[x-1, y-1])
                {
                    return new int[2] { x-1, y-1};
                }
                else if (edgeData[x, y-1])
                {
                    return new int[2] { x, y-1};
                }
                else if (edgeData[x-1, y])
                {
                    return new int[2] { x-1, y };
                }
                else if (edgeData[x+1, y])
                {
                    return new int[2] { x+1, y };
                }
                else if (edgeData[x+1, y+1])
                {
                    return new int[2] { x+1, y+1 };
                }
                else if (edgeData[x, y+1])
                {
                    return new int[2] { x, y+1 };
                }
                else if (edgeData[x+1, y-1])
                {
                    return new int[2] { x+1, y-1};
                }
                else if (edgeData[x-1, y+1])
                {
                    return new int[2] { x-1, y+1};
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}

