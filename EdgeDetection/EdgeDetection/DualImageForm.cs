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
        Stopwatch clock; //timer1 for the analyze method
        Stopwatch clock2; //timer2 for the separate method
        Boolean[,] edgeData;
        Boolean[,] edgeDataHolder; //holds edge data while edge data is being modified so it can reset
        Color[] groupColors; //colors for edge groups
        ArrayList edgeGroups = new ArrayList();
        int width;
        int height;
        int pixelThresh;
        Bitmap img;

        public DualImageForm()
        {
            Debug.WriteLine("form loaded");
            InitializeComponent();
            groupColors = new Color[] { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Pink, Color.Purple, Color.Orange, Color.Brown };
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
            label3.Text = "Analyze: " + time / 1000 + " sec";
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
            clock2 = Stopwatch.StartNew();
            separateEdges();
            clock2.Stop();
            float time = clock2.ElapsedMilliseconds;
            label5.Text = "Separate: " + time / 1000 + " sec";
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
                            bytes[(y * data.Stride) + (x * 3) + 1] = 255;
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
            int color = -1;
            Color temp = Color.Blue;
            edgeDataHolder = edgeData;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (edgeData[x, y])
                    {
                        color++;
                        if (color == groupColors.Length)
                        {
                            color = 0;
                        }
                        temp = groupColors[color];

                        //make a new edge group, and add all edges to the group as they are found
                        EdgeGroup group = new EdgeGroup(new Edge(x, y));
                        edgeGroups.Add(group);

                        //cycle clockwise from (x-1, y-1)
                        ArrayList edgeGroup = new ArrayList();

                        ArrayList toScan = new ArrayList(); //contains all pixels that need scanning
                        toScan.Add(new Edge(x, y));

                        ArrayList tempE = new ArrayList(); //contains all pixels to be added to toScan at the end of cycle
                        Edge[] holder; //holds edges from scan


                        int[] next = new int[2] { x, y };
                        edgeData[x, y] = false;
                        img.SetPixel(x, y, temp);

                        while (toScan.Count > 0) //one run of the inside of this loop is a cycle
                        {
                            for (int i = 0; i < toScan.Count; i++)
                            {
                                int xT = ((Edge)toScan[i]).getX();
                                int yT = ((Edge)toScan[i]).getY();
                                try
                                {
                                    img.SetPixel(xT, yT, temp);
                                }
                                catch
                                {

                                }
                                holder = scan(new int[] { xT, yT }, pixelThresh);

                                for (int q = 0; q < holder.Length; q++)
                                {
                                    tempE.Add(holder[q]);
                                    //Debug.WriteLine("edge [" + q + "]" + "x" + holder[q].getX() + " y " + holder[0].getY());
                                }
                            }

                            toScan = (ArrayList)tempE.Clone();
                            tempE = new ArrayList();
                        }
                    }
                }
            }
            edgeData = edgeDataHolder;
            pictureBox1.Image = img;
        } //end separate

        private Edge[] scan(int[] temp, int thresh)
        {
            Edge[] edgeT;
            ArrayList edges = new ArrayList();

            int x = temp[0];
            int y = temp[1];
            int d = 1;
            //Debug.WriteLine("array " + next[0]);
            while (d < thresh)
            {
                //Debug.WriteLine("scan depth " + d);
                edgeT = search(x, y, d);
                if (edgeT != null && edgeT.Length != 0)
                {
                    for (int i = 0; i < edgeT.Length; i++)
                    {
                        edges.Add(edgeT[i]);
                    }
                }
                d += 1;
            }

            Edge[] output = new Edge[edges.Count];
            for (int i = 0; i < edges.Count; i++)
            {
                output[i] = (Edge)edges[i];
            }

            return output;
        }

        private Edge[] search(int x, int y, int thresh)
        {
            //Debug.WriteLine("search " + thresh);
            ArrayList edgeTemp = new ArrayList();
            Edge[] output;
            int edgeNumber = 0; //the number of discovered edges

            int d = thresh;
            int xtemp = x - d;
            if (xtemp < 0)
            {
                xtemp = 0;
            }
            int ytemp = y - d;
            if (ytemp < 0)
            {
                ytemp = 0;
            }
            if (thresh > 0)
            {
                while (xtemp <= x + d)
                {
                    //Debug.WriteLine("while 1 ");
                    if (xtemp >= 0 && ytemp >= 0 && edgeData[xtemp, ytemp])
                    {
                        edgeTemp.Add(new Edge(xtemp, ytemp));
                        edgeNumber += 1;
                    }
                    xtemp += 1;
                }
                xtemp = x + d; ytemp = y - d + 1;
                while (ytemp <= y + d - 1)
                {
                    //Debug.WriteLine("while 2 ");
                    if (xtemp >= 0 && ytemp >= 0 && edgeData[xtemp, ytemp])
                    {
                        edgeTemp.Add(new Edge(xtemp, ytemp));
                        edgeNumber += 1;
                    }
                    ytemp += 1;
                }
                ytemp = y + d;
                while (xtemp >= x - d)
                {
                    //Debug.WriteLine("while 3 ");
                    if (xtemp >= 0 && ytemp >= 0 && edgeData[xtemp, ytemp])
                    {
                        edgeTemp.Add(new Edge(xtemp, ytemp));
                        edgeNumber += 1;
                    }
                    xtemp -= 1;
                }
                xtemp = x - d; ytemp = y + d - 1;
                while (ytemp >= y - d + 1)
                {
                    //Debug.WriteLine("while 4 ");
                    if (xtemp >= 0 && ytemp >= 0 && edgeData[xtemp, ytemp])
                    {
                        edgeTemp.Add(new Edge(xtemp, ytemp));
                        edgeNumber += 1;
                    }
                    ytemp -= 1;
                }
            }

            output = new Edge[edgeNumber];
            for (int i = 0; i < edgeNumber; i++)
            {
                output[i] = (Edge)edgeTemp[i];
                edgeData[output[i].getX(), output[i].getY()] = false;
                //Debug.WriteLine("edge [" + i + "]" + "x" + output[i].getX() + " y " + output[0].getY());
            }

            return output;
        }
    } // end class dual Image form

    public class EdgeGroup
    {
        ArrayList edges;
        Color color;

        public EdgeGroup(Edge first)
        {
            edges = new ArrayList();
            edges.Add(first);
        }

        public void add(Edge addEdge)
        {
            edges.Add(addEdge);
        }
    }

    public class Edge
    {
        int x; int y;

        public Edge(int x_in, int y_in)
        {
            x = x_in;
            y = y_in;
        }

        public int getX()
        {
            return x;
        }

        public int getY()
        {
            return y;
        }
    }
}//end namespace

