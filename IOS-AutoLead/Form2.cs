
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IOS_AutoLead
{
    public partial class Form2 : Form
    {
        public static List<String> Listitemdgv = new List<string>();
        public Form2()
        {
            InitializeComponent();
        }
        public bool IsValidUrl(string urlString)
        {
            Uri uri;
            bool flag3 = Uri.TryCreate(this.textBox2.Text, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            return flag3;
        }
        private void button2_Click(object sender, EventArgs e)
        {
          
           
            Listitemdgv = new List<string>();
           
            if (textBox1.Text != "" && textBox2.Text != ""  && comboBox1.Text != "")
            {
                if (IsValidUrl(textBox2.Text))
                {
                    bool check2 = false;
                    if (checkBox2.Checked)
                    {
                        check2 = true;
                    }
                    bool check1 = false;
                    if (checkBox1.Checked)
                    {
                        check1 = true;
                    }
                    Listitemdgv.Add(textBox1.Text);
                    Listitemdgv.Add(textBox2.Text);
                    Listitemdgv.Add(comboBox1.Text);
                    Listitemdgv.Add(check2.ToString());
                    Listitemdgv.Add(check1.ToString());
                    File.WriteAllLines(iStatic.diraidIphone + "/Script.txt", richTextBox1.Lines);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Url Lỗi kiểm tra lại");
                }
            }
            
            

        }

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            sshNet sshnet = new sshNet(iStatic.ipIphone);
            sshnet.OpenApp(iStatic.nameApp9);
            Thread.Sleep(1000);
            comboBox1.Enabled = false;
            string text = string.Empty;
            ThuVienDll.FuncFolder file = new ThuVienDll.FuncFolder();
           
            foreach (string str in sshnet.GetAppList())
            {
              
                string[] arrnameapp = str.Split('=');
                
                if (arrnameapp.Length - 1 > 0)
                {
                    text += arrnameapp[1] + "|";
                    comboBox1.Items.Add(arrnameapp[1].Trim());
                }
            }

            File.WriteAllText( iStatic.diraidIphone + "/Applist.txt",text);
            comboBox1.Enabled = true;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            if (Form1.editadddgv == "EDIT")
            {
                checkBox1.Checked = bool.Parse(Form1.editdgvOffer[0]);
                textBox1.Text = Form1.editdgvOffer[1];
                textBox2.Text = Form1.editdgvOffer[2];
                comboBox1.Text= Form1.editdgvOffer[3];
                checkBox2.Checked=bool.Parse(Form1.editdgvOffer[4]);
                if (File.Exists(iStatic.diraidIphone + "/Script.txt"))
                {
                    richTextBox1.Lines = File.ReadAllLines(iStatic.diraidIphone + "/Script.txt");
                }
            }
            if (File.Exists(iStatic.diraidIphone + "/Applist.txt"))
            {
                foreach (string str in File.ReadAllText(iStatic.diraidIphone + "/Applist.txt").Split('|'))
                {
                    comboBox1.Items.Add(str);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

       
    }
}
