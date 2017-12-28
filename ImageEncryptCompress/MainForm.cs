using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
           InitializeComponent();
           
            textBox_size.ReadOnly = true;
            button2.Enabled = false;
            button3.Enabled = false;
            //text_result.Enabled = false;
        }
        List<KeyValuePair<RGBPixel[,], KeyValuePair<string, int>>> d;
        int index = 0;
        RGBPixel[,] ImageMatrix;
        string seed;
        int height;
        int width;
        int tap;
        //string OpenedFilePath2;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if(!checkBox2.Checked)
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
            else
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
               
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    //Open the browsed image and display it
                   
                    string OpenedFilePath = openFileDialog1.FileName; //filepath
                   
                    Stopwatch sw = Stopwatch.StartNew();

            
                ImageMatrix = ImageOperations.Decompress_image(OpenedFilePath,ref seed,ref tap,ref height,ref width);
                    ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                    sw.Stop();
                    text_seed.Text = seed;
                    text_tap.Text = tap.ToString();
                    txtHeight.Text = height.ToString();
                    txtWidth.Text = width.ToString();
                    btnGaussSmooth.Enabled = true;
                    MessageBox.Show("Decompression time: " + ((double)sw.ElapsedMilliseconds / 1000).ToString() + " seconds");
                    //label1.Text = "Encrypted Image";



                }

            }

        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            

            
            if(!checkBox1.Checked && !checkBox2.Checked)
            {

                Stopwatch sw1 = Stopwatch.StartNew();
                ImageMatrix = ImageOperations.encrypt_image(ImageMatrix, text_seed.Text, Convert.ToInt32(text_tap.Text));
                sw1.Stop();
                MessageBox.Show("Encryption time :"+((double)sw1.ElapsedMilliseconds / 1000).ToString() + " Seconds");
                Stopwatch sw = Stopwatch.StartNew();
                ImageOperations.Compress_image(ImageMatrix, text_seed.Text, Convert.ToInt32(text_tap.Text));
                sw.Stop();
                ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
                MessageBox.Show("Compression time: " + ((double)sw.ElapsedMilliseconds / 1000).ToString() + " Seconds");
                MessageBox.Show("Image has been encrypted and compressed successfully !", "Successful Operation");
                
            }
            else if (!checkBox2.Checked)
            {
                string x = "";
                for(int i=0;i<Convert.ToInt32( textBox_size.Text);i++)
                {
                    x += "0";
                }
                Stopwatch sw = Stopwatch.StartNew(); ;
            
                 d = ImageOperations.Hack(ImageMatrix, x, Convert.ToInt32(textBox_size.Text));
                sw.Stop();
               

                ImageOperations.DisplayImage(d[index].Key, pictureBox2);
                    text_seed.Text = d[index].Value.Key;
                    text_tap.Text = d[index].Value.Value.ToString();
                    text_result.Text = d.Count.ToString();
                button2.Enabled = true;
                button3.Enabled = true;
                MessageBox.Show("Hack time :" + ((double)sw.ElapsedMilliseconds / 1000).ToString() + " Seconds");
                

            }
            else
            {
                Stopwatch sw = Stopwatch.StartNew();
                ImageMatrix = ImageOperations.encrypt_image(ImageMatrix, seed, tap);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
                sw.Stop();
                MessageBox.Show("Decryption time :" + ((double)sw.ElapsedMilliseconds / 1000).ToString() + " Seconds");

                

            }



        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            ImageOperations.huffman_encoding(ImageMatrix);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

            if(checkBox1.Checked==true)
            {
                checkBox2.Checked = false;
                text_seed.ReadOnly = true;
                text_tap.ReadOnly = true;
                label1.Text = "Encrypted Image";
                btnGaussSmooth.Text = "Hack image";
                label2.Text = "Possible results";
                textBox_size.ReadOnly = false;
                if (d!=null)
                {
                    button2.Enabled = true;
                    button3.Enabled = true;
                }
               


            }
            else 
            {
                text_seed.ReadOnly = false;
                text_tap.ReadOnly = false;
                btnGaussSmooth.Enabled = true;
                label1.Text = "Original Image";
                btnGaussSmooth.Text = "Encrypt";
                label2.Text = "Encrypted Image";
                textBox_size.ReadOnly = true;
                button2.Enabled = false;
                button3.Enabled = false;


            }

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            //if (index == -1)
            //    index = d.Count - 1;
            //else if (index == d.Count)
            //    index = d.Count - 1;
            index--;
            if (index ==-1)
                   index = d.Count - 1;
            ImageMatrix = d[index].Key;
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
            text_seed.Text = d[index].Value.Key;
            text_tap.Text = d[index].Value.Value.ToString();
            //index--;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //if (index == d.Count)
            //    index = 0;
            //else if (index == -1)
            //    index = d.Count - 1;
            index++;
            if (index == d.Count)
                index = 0;
            ImageMatrix = d[index].Key;
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
            text_seed.Text = d[index].Value.Key;
            text_tap.Text = d[index].Value.Value.ToString();
            
            //index++;
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void txtHeight_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox2.Checked)
            {
                btnOpen.Text = "Browse File";
                btnGaussSmooth.Text = "Decrypt";
                label1.Text = "Encrypted Image";
                label2.Text = "Decrypted Image";
                text_seed.ReadOnly = true;
                text_tap.ReadOnly=true;
                textBox_size.ReadOnly = true;
                checkBox1.Checked = false;
                btnGaussSmooth.Enabled = false;

            }
            else
            {
                btnOpen.Text = "Open Image";
                btnGaussSmooth.Text = "Encrypt";
                label1.Text = "Original Image";
                label2.Text = "Encrypted Image";
                text_seed.ReadOnly = false;
                text_tap.ReadOnly = false;
                btnGaussSmooth.Enabled = true;
                //textBox_size.ReadOnly = true;
            }
        }
    }
}