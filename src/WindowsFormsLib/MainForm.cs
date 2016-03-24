﻿using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsLib
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            foreach(var dir in Directory.GetDirectories(@"c:\"))
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                this.treeView2.Nodes.Add(di.Name);
            }

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.progressBar1.Value++;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.label5.Text = comboBox1.SelectedItem as string;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            this.label6.Text = dateTimePicker1.Value.ToString();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if( radioButton1.Checked )
            {
                this.label7.Text = radioButton1.Name + " checked";
            }
            else
            {
                this.label7.Text = radioButton2.Name + " checked";
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.label10.Text = checkBox1.Checked.ToString();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            this.label18.Text = textBox2.Text;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.label17.Text = textBox1.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PictureBrowser frm = new PictureBrowser();
            frm.ShowDialog();
        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.listView1.Items.Clear();
            foreach (var dir in Directory.GetDirectories(@"c:\" + e.Node.Name))
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                var li = this.listView1.Items.Add(di.Name);
                li.Tag = di;
            }
            foreach (var dir in Directory.GetFiles(@"c:\" + e.Node.Name))
            {
                this.listView1.Items.Add(Path.GetFileName(dir));
            }
        }
    }
}
