using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Numerics;
using Huffman1;
using System.IO;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }


    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }
        public static byte GET_Key(ref long  seed, int tap,int len)
        {
            
            byte key = 0;
            int res;
            for (int i = 0; i < 8; i++)
            {
               res = ((((seed & (1 << len-1)) != 0)?1:0 )^ (((seed & (1 << tap)) != 0) ? 1 : 0));
                key =(byte)((key<<1)+res);
              
                seed = ((seed << 1) + res);
            }
            return key;
        }
        public static RGBPixel[,] encrypt_image(RGBPixel[,] ImageMatrix, string seed, int tap)
        {

            int hight = GetHeight(ImageMatrix);
            int width = GetWidth(ImageMatrix);
            int seed_alpha =0;
            long seed2 ;
            int len;
            for (int i = 0; i < seed.Length; i++)
            {
                if ((seed[i] != '0' && seed[i] != '1')||seed.Length>64)
                {

                    for (int j = 0; j < seed.Length; j++)
                        seed_alpha += seed[j];

                    seed =Convert.ToString(seed_alpha,2);
                    break;
                }
            }
             seed2 = Convert.ToInt64(seed, 2);
             len = seed.Length;
          
            byte Rkey;
            byte Gkey ;
            byte Bkey;
            for (int i = 0; i < hight; i++)
            {
                for (int j = 0; j < width; j++)
                {
                 
                     Rkey = GET_Key(ref seed2, tap,len);
                     Gkey = GET_Key(ref seed2, tap,len);
                     Bkey = GET_Key(ref seed2, tap,len);

                    ImageMatrix[i, j].red = (byte)(ImageMatrix[i, j].red ^ Rkey);
                    ImageMatrix[i, j].green = (byte)(ImageMatrix[i, j].green ^Gkey);
                    ImageMatrix[i, j].blue = (byte)(ImageMatrix[i, j].blue ^ Bkey);

                }
            }
           

            return ImageMatrix;
        }

        public static  List<KeyValuePair< RGBPixel[,],KeyValuePair<string,int>>> Hack(RGBPixel[,] ImageMatrix, string seed, int size) // complexity = O(N^2 * 2^size * size).
        {
            int hight = GetHeight(ImageMatrix);
            int width = GetWidth(ImageMatrix);
          
            int avg = 0;
            string ss = "";
            List<KeyValuePair<RGBPixel[,], KeyValuePair<string, int>>> t = new List<KeyValuePair<RGBPixel[,], KeyValuePair<string, int>>>();
            int n = Convert.ToInt16(seed, 2);
         
            RGBPixel[,] imagecpy = new RGBPixel[hight, width];



            for (int u = n; u <= Math.Pow(2, size) - 1; u++)
            {
                
                for (int a = 0; a < size; a++)
                {
                    RGBPixel[,] image = new RGBPixel[hight, width];
                    //seed = Convert.ToString(u, 2);
                    string after = Convert.ToString(u, 2);
                    string before = "";
                    for (int k = 0; k < size - after.Length; k++)
                    {
                        before += "0";
                    }

                    seed = before + after;
                    ss = seed;
                    int tab = a;
                    int coun1 = 0, coun2 = 0, coun3 = 0;
                    imagecpy = (RGBPixel[,])ImageMatrix.Clone();
                    // int temp = 0;
                    //for (int y = 0; y < hight; y++)
                    //    for (int g = 0; g < width; g++)
                    //        imagecpy[y, g] = ImageMatrix[y, g];

                    Dictionary<int, int> Rvalues = new Dictionary<int, int>();
                    Dictionary<int, int> Gvalues = new Dictionary<int, int>();
                    Dictionary<int, int> Bvalues = new Dictionary<int, int>();
                   
                    encrypt_image(imagecpy, seed, tab);
                    //if (u == 2)
                    //    return ImageMatrix;
                    for (int i = 0; i < hight; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            Rvalues[imagecpy[i, j].red] = 0;
                            Gvalues[imagecpy[i, j].green] = 0;
                            Bvalues[imagecpy[i, j].blue] = 0;
                        }
                    }
                  
                    for (int i = 0; i < hight; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            //when exist 
                            Rvalues[imagecpy[i, j].red]++;
                            Gvalues[imagecpy[i, j].green]++;
                            Bvalues[imagecpy[i, j].blue]++;

                        }
                    }
                    
                    foreach (KeyValuePair<int, int> kvp in Rvalues)
                    {
                       
                       coun1 += kvp.Value;

                    }
                   coun1 /= Rvalues.Count;
                    foreach (KeyValuePair<int, int> kvp in Gvalues)
                    {
                       
                            coun2 += kvp.Value;
                    }
                    coun2 /= Gvalues.Count;
                    foreach (KeyValuePair<int, int> kvp in Bvalues)
                    {
                       
                            coun3 += kvp.Value;
                    }
                    coun3 /= Bvalues.Count;
                    //   seed = Convert.ToString(f, 2);
                    //if (seed == "10001111")
                    //    MessageBox.Show("found");
                   
                    if (coun1+coun2+coun3==avg)
                    {

                        //if (seed == "1001" && tab == 1)
                        //{

                        //    MessageBox.Show("found");
                        //    //for (int y = 0; y < hight; y++)
                        //    //    for (int g = 0; g < width; g++)
                        //    //        image[y, g] = imagecpy[y, g];

                        //    //t.Add(image);
                        //    //return t;
                        //    //return ImageMatrix;
                        //}

                        //avg = coun1 + coun2 + coun3;
                        //for (int y = 0; y < hight; y++)
                        //    for (int g = 0; g < width; g++)
                                image = (RGBPixel[,])imagecpy.Clone();
                        // if(!t.Contains(image))

                        KeyValuePair<RGBPixel[,], KeyValuePair<string, int>> b = new KeyValuePair<RGBPixel[,], KeyValuePair<string, int>>(image, new KeyValuePair<string, int>(seed, tab));
                        t.Add(b);
                        //ss = seed;
                       
                        
                    }
                   else if (coun1 + coun2 + coun3 >avg)
                    {


                        avg = coun1 + coun2 + coun3;
                        //for (int y = 0; y < hight; y++)
                        //    for (int g = 0; g < width; g++)
                        image = (RGBPixel[,])imagecpy.Clone();


                        KeyValuePair<RGBPixel[,], KeyValuePair<string, int>> b = new KeyValuePair<RGBPixel[,], KeyValuePair<string, int>>(image,new KeyValuePair<string, int>(ss,tab));
                        t= new List<KeyValuePair<RGBPixel[,], KeyValuePair<string, int>>>();
                        t.Add(b);
                    


                    }

                }

            }
          
            return t;
        }
        
        public static void huffman_encoding(RGBPixel[,] ImageMatrix)
        {


            Dictionary<int, int> Rvalues = new Dictionary<int, int>();
            Dictionary<int, int> Gvalues = new Dictionary<int, int>();
            Dictionary<int, int> Bvalues = new Dictionary<int, int>();

            int hight = GetHeight(ImageMatrix);
            int width = GetWidth(ImageMatrix);

            ///initiallize dictionary with 0
            for (int i = 0; i < hight; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Rvalues[ImageMatrix[i, j].red] = 0;
                    Gvalues[ImageMatrix[i, j].green] = 0;
                    Bvalues[ImageMatrix[i, j].blue] = 0;
                }
            }
            for (int i = 0; i < hight; i++)
            {
                for (int j = 0; j < width; j++)
                {
                       //when exist 
                  Rvalues[ImageMatrix[i, j].red]++;
                  Gvalues[ImageMatrix[i, j].green]++;
                  Bvalues[ImageMatrix[i, j].blue]++;

                }
            }
            HuffmanTree tree1 = new HuffmanTree(Rvalues);
            HuffmanTree tree2 = new HuffmanTree(Gvalues);
            HuffmanTree tree3 = new HuffmanTree(Bvalues);
            Dictionary<int, string> Redtree = tree1.CreateEncodings();
            Dictionary<int, string> Greentree = tree2.CreateEncodings();
            Dictionary<int, string> bluetree = tree3.CreateEncodings();
            long Rcount = 0;
            long Gcount = 0;
            long Bcount = 0;
            FileStream fs = new FileStream("huffman_output.txt", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("--R--");
            sw.WriteLine("Color - Frequency - Huffman Representation - Total Bits");
             
            //red
            foreach (KeyValuePair<int, string> kvp in Redtree)
            {
                sw.Write(kvp.Key + " - " + Rvalues[kvp.Key] + " - " + kvp.Value + " - " + kvp.Value.Length * Rvalues[kvp.Key]);
                sw.WriteLine();
                Rcount += kvp.Value.Length * Rvalues[kvp.Key];
            }
            sw.WriteLine("*Total = "+ Rcount);
            sw.WriteLine();
            sw.WriteLine("--G--");
            //green
            foreach (KeyValuePair<int, string> kvp in Greentree)
            {
                sw.Write(kvp.Key + " - " + Gvalues[kvp.Key] + " - " + kvp.Value + " - " + kvp.Value.Length * Gvalues[kvp.Key]);
                sw.WriteLine();
                Gcount += kvp.Value.Length * Gvalues[kvp.Key];
            }
            sw.WriteLine("*Total = " + Gcount);
            sw.WriteLine();
            sw.WriteLine("--B--");

            //blue
            foreach (KeyValuePair<int, string> kvp in bluetree)
            {
                sw.Write(kvp.Key + " - " + Bvalues[kvp.Key] + " - " + kvp.Value + " - " + kvp.Value.Length * Bvalues[kvp.Key]);
                sw.WriteLine();
                Bcount += kvp.Value.Length * Bvalues[kvp.Key];
            }
            double comp_output = (double)(Rcount + Gcount + Bcount) / 8;

            sw.WriteLine("*Total = " + Bcount);
            sw.WriteLine();
            sw.WriteLine("**Compression Output**");
            sw.WriteLine(comp_output+ " bytes");
            sw.WriteLine();
            sw.WriteLine("**Compression Ratio**");
            sw.WriteLine(Math.Round(comp_output/(hight*width*3)*100,1) +"%");

            sw.Close();
            fs.Close();
            MessageBox.Show("Huffman tree has been saved successflly !");


        }
        //static public byte[] StringToBytesArray(string str)
        //{
        //    var bitsToPad = 8 - str.Length % 8;

        //    if (bitsToPad != 8)
        //    {
        //        var neededLength = bitsToPad + str.Length;
        //        str = str.PadLeft(neededLength, '0');
        //    }

        //    int size = str.Length / 8;
        //    byte[] arr = new byte[size];

        //    for (int a = 0; a < size; a++)
        //    {
        //        arr[a] = Convert.ToByte(str.Substring(a * 8, 8), 2);
        //    }

        //    return arr;
        //}
        public static List<byte> ConvertStringByte(List<string> Slist) // O(N)
        {
            int len = Slist.Count, index1 = 0, index2 = 0, index3 = 0;

            byte elem = 0;
            List<byte> bytes = new List<byte>();
            while (true)
            {
                if (index3 == Slist[index1].Length) 
                {
                    index1++;
                    index3 = 0;
                }

                if (index2 == 8) // byte length
                {
                    bytes.Add(elem);
                    elem = 0;
                    index2 = 0;
                }
                if (index1 == len) // string list length
                    break;
                elem <<= 1;
                elem += (byte)(Slist[index1][index3] - 48);
                index3++;
                index2++;
                //bitscount++;
            }
            if (index2 != 0)
            {
                elem <<= (8 - index2);
                bytes.Add(elem);
            }

            Slist = new List<string>();
            return bytes;
        }
        public static void Compress_image(RGBPixel[,] ImageMatrix,string seed,int tap) //O(N^2)
        {
            FileStream fs = new FileStream("Encrypted_image.bin", FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            int hight = GetHeight(ImageMatrix);
            int width = GetWidth(ImageMatrix);
            bw.Write(Convert.ToUInt16(hight));
            bw.Write(Convert.ToUInt16(width));
            bw.Write(seed);
            bw.Write(Convert.ToUInt16(tap));
            Dictionary<int, int> Rvalues = new Dictionary<int, int>();
            Dictionary<int, int> Gvalues = new Dictionary<int, int>();
            Dictionary<int, int> Bvalues = new Dictionary<int, int>();

           
           
            ///initiallize dictionary with 0
            for (int i = 0; i < hight; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Rvalues[ImageMatrix[i, j].red] = 0;
                    Gvalues[ImageMatrix[i, j].green] = 0;
                    Bvalues[ImageMatrix[i, j].blue] = 0;
                }
            }
            for (int i = 0; i < hight; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    //when exist 
                    Rvalues[ImageMatrix[i, j].red]++;
                    Gvalues[ImageMatrix[i, j].green]++;
                    Bvalues[ImageMatrix[i, j].blue]++;

                }
            }
            bw.Write(Convert.ToUInt16(Rvalues.Count));
            foreach (KeyValuePair<int,int> K in Rvalues)
            {
                byte[] bb = BitConverter.GetBytes(K.Value);

                bw.Write(Convert.ToByte(K.Key));
                bw.Write(Convert.ToByte(bb[0]));
                bw.Write(Convert.ToByte(bb[1]));
                bw.Write(Convert.ToByte(bb[2]));

            }
            bw.Write(Convert.ToUInt16(Gvalues.Count));
            foreach (KeyValuePair<int, int> K in Gvalues)
            {
                byte[] bb = BitConverter.GetBytes(K.Value);

                bw.Write(Convert.ToByte(K.Key));
                bw.Write(Convert.ToByte(bb[0]));
                bw.Write(Convert.ToByte(bb[1]));
                bw.Write(Convert.ToByte(bb[2]));
            }
            bw.Write(Convert.ToUInt16(Bvalues.Count));
            foreach (KeyValuePair<int, int> K in Bvalues)
            {
                byte[] bb = BitConverter.GetBytes(K.Value);

                bw.Write(Convert.ToByte(K.Key));
                bw.Write(Convert.ToByte(bb[0]));
                bw.Write(Convert.ToByte(bb[1]));
                bw.Write(Convert.ToByte(bb[2]));
            }

            HuffmanTree tree1 = new HuffmanTree(Rvalues);
            HuffmanTree tree2 = new HuffmanTree(Gvalues);
            HuffmanTree tree3 = new HuffmanTree(Bvalues);
            Dictionary<int, string> Redtree = tree1.CreateEncodings();
            Dictionary<int, string> Greentree = tree2.CreateEncodings();
            Dictionary<int, string> Bluetree = tree3.CreateEncodings();
            List<string> R_binstream = new List<string>();
            List<string> G_binstream = new List<string>();
            List<string> B_binstream = new List<string>();
            for (int i = 0; i < hight; i++)
                for (int j = 0; j < width; j++)
                {
                    R_binstream.Add(Redtree[ImageMatrix[i, j].red]);
                   
                    G_binstream.Add(Greentree[ImageMatrix[i, j].green]);

                    B_binstream.Add(Bluetree[ImageMatrix[i, j].blue]);

                }
          
            byte[] b = ConvertStringByte(R_binstream).ToArray();
            bw.Write(Convert.ToInt32(b.Length));
            bw.Write(b);
          
            b = ConvertStringByte(G_binstream).ToArray();
            bw.Write(Convert.ToInt32(b.Length));
            bw.Write(b);
            b = ConvertStringByte(B_binstream).ToArray();
            bw.Write(Convert.ToInt32(b.Length));
            bw.Write(b);

          
            bw.Close();
            fs.Close();
           
        }

        public static RGBPixel[,] Decompress_image(string path ,ref string  seed,ref int tap,ref int height,ref int width) //O(N^2)
        {
            
            FileStream fs = new FileStream(@path, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            height = br.ReadUInt16();
            width = br.ReadUInt16();
            seed = br.ReadString();
            tap = br.ReadUInt16();
            RGBPixel[,] ImageMatrix = new RGBPixel[height, width];
            Dictionary<int,int> RedTreeList = new Dictionary<int, int>();
            Dictionary<int, int> GreenTreeList = new Dictionary<int, int>();
            Dictionary<int, int> BlueTreeList = new Dictionary<int, int>();
            List<byte> Redstream = new List<byte>();
            List<byte> Greenstream = new List<byte>();
            List<byte> Bluestream = new List<byte>();
           uint b= br.ReadUInt16();
            while(b!=0)
            {
                int key = br.ReadByte();
                byte[] bb = new byte[4];
                bb[0] = br.ReadByte();
                bb[1] = br.ReadByte();
                bb[2] = br.ReadByte();
                bb[3] = 0;
                int value = BitConverter.ToInt32(bb, 0);
                RedTreeList.Add(key, value);
                b--;
            }
            b = br.ReadUInt16();
            while (b != 0)
            {
                int key = br.ReadByte();
                byte[] bb = new byte[4];
                bb[0] = br.ReadByte();
                bb[1] = br.ReadByte();
                bb[2] = br.ReadByte();
                bb[3] = 0;
                int value = BitConverter.ToInt32(bb, 0);
                GreenTreeList.Add(key, value);
                b--;
            }
            b = br.ReadUInt16();
            while (b != 0)
            {
                int key = br.ReadByte();
                byte[] bb = new byte[4];
                bb[0] = br.ReadByte();
                bb[1] = br.ReadByte();
                bb[2] = br.ReadByte();
                bb[3] = 0;
                int value = BitConverter.ToInt32(bb, 0);
                BlueTreeList.Add(key,value);
                b--;
            }
          
            int size = br.ReadInt32();
            Redstream.AddRange(br.ReadBytes(size));
            size = br.ReadInt32();
            Greenstream.AddRange(br.ReadBytes(size));
            size = br.ReadInt32();
            Bluestream.AddRange(br.ReadBytes(size));
            br.Close();
            fs.Close();
            HuffmanTree tree1 = new HuffmanTree(RedTreeList);
            HuffmanTree tree2 = new HuffmanTree(GreenTreeList);
            HuffmanTree tree3 = new HuffmanTree(BlueTreeList);
            HuffmanNode Redtree = tree1.root;
            HuffmanNode Greentree = tree2.root;
            HuffmanNode Bluetree = tree3.root;
            int index = 0,count=0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    HuffmanNode N = Redtree.Clone();
                    while(N.Left!=null)
                    {
                        if(count==8 )
                        {
                            count = 0;
                            index++;
                        }
                        if((Redstream[index]&(1<<7))!=0)
                            N = N.Right;
                        else
                            N = N.Left;
                        count++;
                        Redstream[index] <<= 1;
                    }
                    ImageMatrix[i, j].red =(byte) N.Value;
                }
            }
            count = 0;index = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    HuffmanNode N = Greentree.Clone();
                    while (N.Left != null)
                    {
                        if (count == 8)
                        {
                            count = 0;
                            
                            index++;
                           
                        }
                        if ((Greenstream[index] & (1 << 7)) != 0)
                            N = N.Right;
                        else
                            N = N.Left;
                        count++;
                        Greenstream[index] <<= 1;
                    }
                    ImageMatrix[i, j].green = (byte)N.Value;
                }
            }
            count = 0; index = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    HuffmanNode N = Bluetree.Clone();
                    while (N.Left != null)
                    {
                        if (count == 8)
                        {
                            count = 0;
                            index++;
                        }
                        if ((Bluestream[index] & (1 << 7)) != 0)
                            N = N.Right;
                        else
                            N = N.Left;
                        count++;
                        Bluestream[index] <<= 1;
                    }
                    ImageMatrix[i, j].blue = (byte)N.Value;
                }
            }
            return ImageMatrix;


        }
   
    }
}
