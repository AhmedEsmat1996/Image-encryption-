using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            // double sigma = double.Parse(txtGaussSigma.Text);
            // int maskSize = (int)nudMaskSize.Value ;
            //  ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);

           // Stopwatch sw = Stopwatch.StartNew();
            ImageMatrix = ImageOperations.encrypt_image(ImageMatrix, text_seed.Text, Convert.ToInt32(text_tap.Text));
         // sw.Stop();
           //MessageBox.Show(((double)sw.ElapsedMilliseconds / 1000).ToString());
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
            
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

       
       
    }
}