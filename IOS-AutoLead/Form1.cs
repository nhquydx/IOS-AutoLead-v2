using Newtonsoft.Json;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
namespace IOS_AutoLead
{
    public partial class Form1 : Form
    {

        public static string editadddgv = String.Empty;
        public static List<string> editdgvOffer = new List<string>();
        ThuVienDll.FuncFolder tvfile;
        Thread thrRun;
        List<SSH> lssh;
        List<Backup> lbackup;
        List<string> ldate;
        List<string> lapp;
        List<string> ldatebyapp;
        List<Run> lrun;
        //Func ------------------
        public  bool IsNumber( string aNumber)
        {
            
            int temp_big_int;
            var is_number = int.TryParse(aNumber, out temp_big_int);
            return is_number;
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
        public Form1()
        {
            InitializeComponent();
            tvfile = new ThuVienDll.FuncFolder();
            lssh = new List<SSH>();
            lbackup = new List<Backup>();
            ldate = new List<string>();
            lapp = new List<string>();
            ldatebyapp = new List<string>();
            lrun = new List<Run>();
        }
        public bool isipaddrees(string str)
        {
            string[] arr = str.Split('.');
            int dem=0;
            foreach (string stri in arr)
            {
               if(IsNumber(stri))
                {
                    if(int.Parse(stri)>=0 && int.Parse(stri) <= 255)
                    {
                        dem++;
                    }
                }
            }
            if (dem == 4)
            {
                return true;
            }
            return false;
        }
        private void button8_Click(object sender, EventArgs e)
        {
            if (File.Exists(iStatic.diraidIphone+"/proxy.txt"))
            {
                richTextBox1.Lines = File.ReadAllLines(iStatic.diraidIphone + "/proxy.txt");
            }
            File.WriteAllText("IPhone.txt", txtDeviceID.Text);
            btnConnect.Enabled = false;
            if (btnConnect.Text == "Connect")
            {
                
                this.thrRun = new Thread(new ThreadStart(this.connect));
                this.thrRun.Start();
            }
            else
            {
                btnConnect.Text = "Connect";
               
                bool flag = this.thrRun != null;
                if (flag)
                {
                    bool flag1 = this.thrRun.ThreadState != System.Threading.ThreadState.Unstarted;
                    if (flag1)
                    {
                        bool flag2 = this.thrRun.ThreadState == System.Threading.ThreadState.Suspended;
                        if (flag2)
                        {
                            this.thrRun.Resume();
                            Thread.Sleep(100);
                        }
                        try
                        {
                            this.thrRun.Abort();
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                btnConnect.Enabled = true;
            }
           
        }
        public void loadHome()
        {
            if (File.Exists(iStatic.diraidIphone + "/home.txt"))
            {
                foreach (string str in File.ReadAllLines(iStatic.diraidIphone + "/home.txt"))
                {
                    string[] arr = str.Split('|');
 
                    addDGVHome(arr[0], arr[1], arr[2], arr[3], arr[4]);
                }
            }
            dataGridView1.ClearSelection();
        }

        public void dgvToTxt(DataGridView dgv, string path)
        {
            try
            {
                if (dgv.RowCount > 1)
                {
                    tvfile = new ThuVienDll.FuncFolder();
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    for (int i = 0; i < dgv.RowCount - 1; i++)
                    {
                        string strRow = "";
                        for (int j = 0; j < dgv.Rows[0].Cells.Count - 1; j++)
                        {

                            strRow += dgv.Rows[i].Cells[j].Value.ToString() + "|";
                        }

                        strRow += dgv.Rows[i].Cells[dgv.Rows[0].Cells.Count - 1].Value.ToString();

                        tvfile.wireData(strRow, path);
                    }
                }
            }
            catch
            {

            }
        }
        public void disconnect()
        {
            sshNet sshnet = new sshNet(iStatic.ipIphone);
            sshnet.Enable(true, txtidPortProxy.Text, txtPortProxy.Text);
            enableALL(false);
            dataGridView3.Rows.Clear();
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
        }
        public void connect()
        {
            try
            {

                if (isipaddrees(txtDeviceID.Text))
                {
                    lblStatus.Invoke(new MethodInvoker(delegate
                    {
                        lblStatus.Text = "Đang Kết Nối Tới IPhone [" + txtDeviceID.Text + "]";
                    }));
                    iStatic.ipIphone = txtDeviceID.Text;
                    sshNet sshnet = new sshNet(iStatic.ipIphone);
                    sshnet.Enable(false, txtidPortProxy.Text, txtPortProxy.Text);
                    iStatic.diraidIphone = sshnet.getaid();
                    if (iStatic.diraidIphone != string.Empty)
                    {
                        if (!Directory.Exists(iStatic.diraidIphone))
                        {
                            Directory.CreateDirectory(iStatic.diraidIphone);
                        }
                        btnConnect.Invoke(new MethodInvoker(delegate { btnConnect.Text = "Disconnect"; }));
                        lblStatus.Invoke(new MethodInvoker(delegate { lblStatus.Text = "Kết Nối Thành Công."; }));
                        base.Invoke(new MethodInvoker(delegate
                        {
                           
                            enableALL(true);
                            LoadSSH(false);
                            if(File.Exists(iStatic.diraidIphone+ "/xyAppstrore.txt"))
                            {
                                string[] arr = File.ReadAllText(iStatic.diraidIphone + "/xyAppstrore.txt").Split('|');
                                txtx2.Text = arr[0];
                                txty2.Text = arr[1];
                            }
                            if (File.Exists(iStatic.diraidIphone + "/xySafari.txt"))
                            {
                                string[] arr = File.ReadAllText(iStatic.diraidIphone + "/xySafari.txt").Split('|');
                                txtx1.Text = arr[0];
                                txty1.Text = arr[1];
                            }
                            if (File.Exists(iStatic.diraidIphone + "/portProxy.txt"))
                            {
                                txtPortProxy.Text = File.ReadAllText(iStatic.diraidIphone + "/portProxy.txt");
                            }
                            loadHome();
                            button13_Click(null, null);
                        }));
                    }
                    else
                    {
                        iStatic.setStatus("Không lấy được serial", lblStatus);
                    }
                }
                else
                {
                    MessageBox.Show("IP WIFI IPHONE LỖI");
                }
            }
            catch
            {
                btnConnect.Invoke(new MethodInvoker(delegate { btnConnect.Enabled = true; }));
                lblStatus.Invoke(new MethodInvoker(delegate { lblStatus.Text = "Kết Nối Lỗi : Kiểm tra IP hoặc wifi"; }));
            }
        btnConnect.Invoke(new MethodInvoker(delegate { btnConnect.Enabled = true; }));

        }


        private void button1_Click(object sender, EventArgs e)
        {
            
                tvfile = new ThuVienDll.FuncFolder();
                editadddgv = "ADD";
                Form2 frm = new Form2();
                frm.ShowDialog();

                if (Form2.Listitemdgv.Count -1 > 3)
                {
                    addDGVHome(Form2.Listitemdgv[4], Form2.Listitemdgv[0], Form2.Listitemdgv[1], Form2.Listitemdgv[2], Form2.Listitemdgv[3]);
                    tvfile.wireData( Form2.Listitemdgv[4] + "|" + Form2.Listitemdgv[0] + "|" + Form2.Listitemdgv[1] + "|" + Form2.Listitemdgv[2] + "|" + Form2.Listitemdgv[3],  iStatic.diraidIphone + "/home.txt");
                }
                editadddgv = string.Empty;
         
           
        }
        

        public void addDGVHome(string bl,string name,string url,string nameapp,string script)
        {
            dataGridView1.Rows.Add(bool.Parse(bl),name, url,nameapp,script);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            LoadForm();
           
        }
        public void LoadForm()
        {
            btnApplyProxy.BackColor = ColorTranslator.FromHtml("#2ECC40");
            txtDeviceID.Text = "192.168.";
            txtidPortProxy.Text = GetLocalIPAddress();
            txtPortProxy.Text = new Random().Next(1111, 9999).ToString();
            textBox1.Text = "100";
            checkBox1.Checked = true;
            if (File.Exists("iPhone.txt"))
            {
                txtDeviceID.Text = File.ReadAllText("iPhone.txt");
            }
           
            enableALL(false);

        }
        public void enableALL(bool bl)
        {
            dataGridView1.Rows.Clear();
            dataGridView3.Rows.Clear();
            listView1.Items.Clear();
            listView2.Items.Clear();
            button1.Enabled = bl;
            button2.Enabled = bl;
            button3.Enabled = bl;
            btnResetLead.Enabled = bl;
            btnResetRRS.Enabled = bl;
            btnStartLead.Enabled = bl;
            btnStartRRS.Enabled = bl;
            btnApplyProxy.Enabled = bl;
            button4.Enabled = bl;
            button7.Enabled = bl;
            button9.Enabled = bl;
            button10.Enabled = bl;
            button11.Enabled = bl;
            button12.Enabled = bl;
            button13.Enabled = bl;
            button14.Enabled = bl;
            button15.Enabled = bl;
            button16.Enabled = bl;
            button17.Enabled = bl;
  
            button22.Enabled = bl;
            button21.Enabled = bl;
            button20.Enabled = bl;
            button24.Enabled = bl;
            button25.Enabled = bl;
            button26.Enabled = bl;
            txtidPortProxy.Enabled = bl;
            txtPortProxy.Enabled = bl;
            textBox1.Enabled = bl;
            txtx1.Enabled = bl;
            txty1.Enabled = bl;
            txty2.Enabled = bl;
            txtx2.Enabled = bl;
            textBox6.Enabled = bl;
            textBox7.Enabled = bl;
            cbCountry.Enabled = bl;
            cboxRDomScript.Enabled = bl;
            cboxUserScript.Enabled = bl;
            cbProxyTypeHome.Enabled = bl;
            cbSelectScript.Enabled = bl;

        }
        private void button2_Click(object sender, EventArgs e)
        {
            editadddgv = "EDIT";
            for(int i =0;i< dataGridView1.Rows[rowdgv].Cells.Count ; i++)
            {
                editdgvOffer.Add( dataGridView1.Rows[rowdgv].Cells[i].Value.ToString());
            }
            Form2 frm = new Form2();
            frm.ShowDialog();
            if (Form2.Listitemdgv.Count - 1 > 3)
            {
                dataGridView1.Rows[rowdgv].Cells[0].Value = Form2.Listitemdgv[4];
                dataGridView1.Rows[rowdgv].Cells[1].Value = Form2.Listitemdgv[0];
                dataGridView1.Rows[rowdgv].Cells[2].Value = Form2.Listitemdgv[1];
                dataGridView1.Rows[rowdgv].Cells[3].Value = Form2.Listitemdgv[2];
                dataGridView1.Rows[rowdgv].Cells[4].Value = Form2.Listitemdgv[3];

               
            }
        
            editadddgv = string.Empty;
            saveDGVHome();
        }
        int rowdgv;
        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            rowdgv = e.RowIndex;
        }
        public void saveDGVHome()
        {
            if (File.Exists(iStatic.diraidIphone + "/home.txt"))
            {
                File.Delete(iStatic.diraidIphone + "/home.txt");
            }
            for (int i = 0; i < dataGridView1.Rows.Count - 1;i++)
            {
                string data = "";
                data = dataGridView1.Rows[i].Cells[0].Value.ToString();
                for (int j = 1; j <= dataGridView1.Columns.Count - 1; j++)
                {
                    data+= "|"+ dataGridView1.Rows[i].Cells[j].Value.ToString();
                }
                File.WriteAllText(iStatic.diraidIphone + "/home.txt", data);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.RemoveAt(rowdgv);
            saveDGVHome();
        }

        private void button9_Click(object sender, EventArgs e)
        {

            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open Text File";
            theDialog.Filter = "TXT files|*.txt";
   
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(iStatic.diraidIphone + "/ssh.txt")) { File.Delete(iStatic.diraidIphone + "/ssh.txt"); }
                File.Copy(theDialog.FileName.ToString(), iStatic.diraidIphone + "/ssh.txt");
                cbCountry.Items.Clear();
                LoadSSH(true);



            }
        }
  
        public void totalCounty(string isCounty)
        {
            if (cbCountry.Items.Count == 0)
            {
                cbCountry.Items.Add(isCounty);
                cbCountry.SelectedIndex = 0;
            }
            int ch = 0;
            for (int i = 0; i < cbCountry.Items.Count ; i++)
            {
                if (cbCountry.Items[i].ToString().ToLower().Equals(isCounty.ToLower()))
                {
                    ch = 1;
                }
                
            }
            if (ch == 0)
            {
                cbCountry.Items.Add(isCounty);
            }
            cbCountry.SelectedIndex = 0;

        }
        public void LoadSSH(bool bl1)
        {
            try
            {
                bool bl = File.Exists(iStatic.diraidIphone + "/ssh.txt");
                if (bl)
                {
                    lssh = new List<SSH>();
                    string text = "";
                    string[] arrssh = tvfile.readData(iStatic.diraidIphone + "/ssh.txt");
                
                    foreach (string str in arrssh)
                    {
                        string[] arr = str.Split('|');
                        if (arr.Length - 1 > 2)
                        {
                            string[] arrCountry = arr[3].Split('(');
                            string Country = "";
                            if (arrCountry.Length - 1 > 0)
                            {
                                Country = arrCountry[0].Trim();
                            }
                            else
                            {
                                Country = arr[3];
                            }
                            
                            if (isipaddrees(arr[0]))
                            {
                                
                                totalCounty(Country);
                                if (!Country.ToLower().Equals("unknown"))
                                {
                                    dataGridView2.Rows.Add(arr[0], arr[1], arr[2], Country);
                                    if (bl1)
                                    {
                                        text = string.Concat(new string[]
                                        {
                                        text,
                                        arr[0],
                                        "|",
                                        arr[1],
                                        "|",
                                        arr[2],
                                        "|",
                                        Country,
                                        "|uncheck",
                                         });
                                        text += "\r\n";
                                        SSH ssh = new SSH();
                                        ssh.IP = arr[0];
                                        ssh.username = arr[1];
                                        ssh.password = arr[2];
                                        ssh.countrycode = Country;
                                        ssh.live = "uncheck";
                                        lssh.Add(ssh);
                                    }
                                    else
                                    {
                                        SSH ssh = new SSH();
                                        ssh.IP = arr[0];
                                        ssh.username = arr[1];
                                        ssh.password = arr[2];
                                        ssh.countrycode = Country;
                                        ssh.live = arr[4];
                                        lssh.Add(ssh);

                                    }



                                }
                            }
                        }

                        if (bl1)
                        {
                            File.WriteAllText(iStatic.diraidIphone + "/ssh.txt", text);
                        }
                       

                    }
                }
            }
            catch
            {
                MessageBox.Show("SSH Lỗi, Đã xóa dữ liệu SSH ");
                File.Delete(iStatic.diraidIphone + "/ssh.txt");
            }
        }
        public void SaveSSH()
        {
            string text="";
            foreach(SSH ssh in lssh)
            {
                text = string.Concat(new string[]
                {
                        text,
                        ssh.IP,
                        "|",
                        ssh.username,
                        "|",
                        ssh.password,
                        "|",
                        ssh.countrycode,
                        "|",
                        ssh.live
                });
                text += "\r\n";
            }
            File.WriteAllText(iStatic.diraidIphone + "/ssh.txt", text);
        
        }
        private void button27_Click(object sender, EventArgs e)
        {
            sshNet ssh = new sshNet(txtDeviceID.Text);
            ssh.ResetIP();
        }
        int sshLine = 0;
        public void LoadListssh()
        {
            int dem = 0;
            foreach (string str in tvfile.readData("ssh.txt"))
            {
                string[] arr = str.Split('|');
                SSH ssh = new SSH();
                ssh.IP = arr[0];
                ssh.username = arr[1];
                ssh.password = arr[2];
                ssh.countrycode = arr[3];
                ssh.live = arr[4];
                if (arr[4] == "uncheck")
                {
                    sshLine = dem;
                }
                else
                {
                    dem++;
                }
                lssh.Add(ssh);
                
            }
           
        }

     
      
        private void button7_Click(object sender, EventArgs e)
        {
            button7.Enabled = false ;
          
            new Thread(() =>
            {
                SSH.ConnectSSH(txtidPortProxy.Text, int.Parse(txtPortProxy.Text), lssh, sshLine,lblStatus);
                button7.Enabled = true;

            }).Start() ;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            foreach(DataGridViewRow row in dataGridView2.SelectedRows)
            {
                DialogResult Resuft= MessageBox.Show("Bạn Chắc Chắn Muốn Xóa Dòng SSH","Thông báo", MessageBoxButtons.OKCancel);
                if (Resuft == DialogResult.OK)
                {
                    lssh.RemoveAt(row.Index);
                   dataGridView2.Rows.RemoveAt(row.Index);
                }
               
            }
            SaveSSH();
            LoadSSH(false);
        }
      
        private void btnApplyProxy_Click(object sender, EventArgs e)
        {
            btnApplyProxy.Enabled = false;
            sshNet sshn = new sshNet(iStatic.ipIphone);
            if (btnApplyProxy.Text == "Enable Proxy")
            {
                sshn.Enable(true, txtidPortProxy.Text, txtPortProxy.Text);
             
                if (iStatic.diraidIphone != string.Empty)
                {
                    
                    File.WriteAllText(iStatic.diraidIphone+"/portProxy.txt", txtPortProxy.Text);
                }
                btnApplyProxy.BackColor = Color.Red;
                btnApplyProxy.Text = "Disable Proxy";
                btnApplyProxy.Enabled = true;
            }
            else
            {
                sshn.Enable(false, txtidPortProxy.Text, txtPortProxy.Text);
                btnApplyProxy.Text = "Enable Proxy";
                btnApplyProxy.BackColor = ColorTranslator.FromHtml("#2ECC40");
             
                btnApplyProxy.Enabled = true;
            }
            Thread.Sleep(1000);
            
        }

        private void btnStartLead_Click(object sender, EventArgs e)
        {
            bool flag = this.btnStartLead.Text == "START" || this.btnStartLead.Text == "RESUME";
            if (flag)
            {
                btnResetLead.Enabled = false;
                bool flag3 = this.btnStartLead.Text == "START";
                if (flag3)
                {
                   
                    this.thrRun = new Thread(new ThreadStart(this.runLead));
                    this.thrRun.Start();
                }
                else
                {
             
                    bool flag4 = this.thrRun == null || (this.thrRun.ThreadState & System.Threading.ThreadState.Stopped) == System.Threading.ThreadState.Stopped;
                    if (flag4)
                    {
                        this.thrRun = new Thread(new ThreadStart(this.runLead));
                    }
                    bool flag5 = (this.thrRun.ThreadState & System.Threading.ThreadState.Suspended) == System.Threading.ThreadState.Suspended;
                    if (flag5)
                    {
                        thrRun.Resume();
                    }
                    else
                    {
                        bool flag6 = (this.thrRun.ThreadState & System.Threading.ThreadState.Unstarted) == System.Threading.ThreadState.Unstarted || (this.thrRun.ThreadState & System.Threading.ThreadState.AbortRequested) == System.Threading.ThreadState.AbortRequested || (this.thrRun.ThreadState & System.Threading.ThreadState.Aborted) == System.Threading.ThreadState.Aborted || (this.thrRun.ThreadState & System.Threading.ThreadState.Stopped) == System.Threading.ThreadState.Stopped;
                        if (flag6)
                        {
                            this.thrRun = new Thread(new ThreadStart(this.runLead));
                            this.thrRun.Start();
                        }
                    }
                }
                this.btnStartLead.Text = "STOP";
                this.btnStartLead.Refresh();
                btnResetLead.Enabled = false;
            }
            else
            {
             
                try
                {
                    thrRun.Suspend();
                }
                catch (Exception)
                {
                }
           
                this.btnStartLead.Text = "RESUME";
                this.btnStartLead.Refresh();
                btnResetLead.Enabled = true;
            }
        }
        public string GetApp(string nApp)
        {
            string nameapp = "";
            ThreadRunLead runthr = new ThreadRunLead();
            foreach (string str in runthr.GetAppList())
            {
                string[] arrnameapp = str.Split('=');
                if (arrnameapp.Length - 1 > 0)
                {
                    if (arrnameapp[1].Trim().Equals(nApp))
                    {
                        nameapp = arrnameapp[0].Trim();
                        return nameapp;
                    }


                }
            }
            return nameapp;
        }
        public void runLead()
        {
            while (true)
            {
                for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.White;
                    if (dataGridView1.Rows[i].Cells[0].Value.ToString().Trim().ToLower() == "true")
                    {
                        
                        Offer off = new Offer();
                        off.Name = dataGridView1.Rows[i].Cells[1].Value.ToString();
                        off.Url = dataGridView1.Rows[i].Cells[2].Value.ToString();
                        off.NameApp = dataGridView1.Rows[i].Cells[3].Value.ToString();
                        off.Script = dataGridView1.Rows[i].Cells[4].Value.ToString();
                        int c = 0;
                        foreach (Run r in lrun)
                        {
                            if (r.applist.Trim().Equals(off.NameApp.Trim()))
                            {
                                r.totalrun += 1;
                                c = 1;
                            }
                        }
                        if (c == 0)
                        {
                            Run r = new Run();
                            r.applist = off.NameApp;
                            r.totalrun = 0;
                            r.totalbackup = 1;
                            lrun.Add(r);
                        }

                      
                        switch (cbProxyTypeHome.Text)
                        {
                            case "SSH":
                                if (SSH.ConnectSSH(txtidPortProxy.Text, int.Parse(txtPortProxy.Text), lssh, sshLine, lblStatus))
                                {
                                    string[] arr1 = null;
                                    if (File.Exists(iStatic.diraidIphone + "/Script.txt"))
                                    {
                                        arr1 = File.ReadAllLines(iStatic.diraidIphone + "/Script.txt");
                                    }
                                    ThreadRunLead(off, checkBox2.Checked, arr1, int.Parse(txtTimeLoadURL.Text), i);


                                }
                                break;
                            case "Proxy": break;
                            case "Direct":
                              
                                string[] arr = null;
                                if (File.Exists(iStatic.diraidIphone + "/Script.txt"))
                                {
                                    arr = File.ReadAllLines(iStatic.diraidIphone + "/Script.txt");
                                }
                                ThreadRunLead(off, checkBox2.Checked, arr, int.Parse(txtTimeLoadURL.Text), i);
                                break;
                        }



                    }

                }
            }
            //Connect ssh 

        }

        public void ThreadRunLead(Offer offer, bool blWipeFull, string[] arrScript, int timeoutUrl,int i)
        {
            ThreadRunLead runthr = new ThreadRunLead();
            dataGridView1.Rows[i].Cells[1].Style.BackColor = ColorTranslator.FromHtml("#2ECC40");
            iStatic.setStatus("Đang xóa dữ liệu Application",lblStatus);
            if (runthr.Wipe(offer.NameApp, blWipeFull))
            {
               
                iStatic.setStatus("Đang đổi thiết bị", lblStatus);
                InfoDevice iDevice = runthr.changeData();
                if (iDevice.Country != null)
                {
                    base.Invoke(new MethodInvoker(delegate
                    {
                        txtCountrySetting.Text = iDevice.Country;
                        txtLanguageSetting.Text = iDevice.Language;
                        txtMachineSetting.Text = iDevice.Machine;
                        txtNetWordSetting.Text = iDevice.NetworkInfo;
                        txtOSVesionSetting.Text = iDevice.OSVersion;
                        txtScreennHSetting.Text = iDevice.ScreenHeight;
                        txtScreenWSetting.Text = iDevice.ScreenWidth;
                        txtTimeZone.Text = iDevice.Timezone;
                        txtUserAgentSetting.Text = iDevice.UserAgent;
                    }));
                    dataGridView1.Rows[i].Cells[2].Style.BackColor = ColorTranslator.FromHtml("#2ECC40");
                    iStatic.setStatus("Đang mở Link", lblStatus);
                    runthr.fileOpenUrl(txtx1.Text, txty1.Text, txtx2.Text, txty2.Text);
                    if (runthr.openUrl(offer.Url, timeoutUrl))
                    {
                        dataGridView1.Rows[i].Cells[3].Style.BackColor = ColorTranslator.FromHtml("#2ECC40");
                        iStatic.setStatus("Mở Application", lblStatus);
                        runthr.openApp(GetApp(offer.NameApp.Trim()));
                        try
                        {
                            Thread.Sleep(int.Parse(txttimeoutopengame.Text));
                        }
                        catch
                        {

                        }
                        if (offer.Script.ToLower().Trim() == "true")
                        {
                            dataGridView1.Rows[i].Cells[4].Style.BackColor = ColorTranslator.FromHtml("#2ECC40");
                            iStatic.setStatus("Chạy Script", lblStatus);
                            runthr.script(arrScript);
                            iStatic.setStatus("Chạy Xong", lblStatus);
                        }

                        foreach (Run r in lrun)
                        {
                            Console.WriteLine(r.applist.Trim() + "Equals"+offer.NameApp.Trim());
                            if (r.applist.Trim().Equals(offer.NameApp.Trim()))
                            {
                                r.totalInstall += 1;
                                iStatic.setStatus(r.totalInstall.ToString(), label7);
                                double pt = ((double)r.totalbackup / (double)r.totalInstall) * 100;
                                Console.WriteLine("phần trăm:" + pt);
                                if (IsNumber(textBox1.Text))
                                {

                                    
                                    if (int.Parse(textBox1.Text)<= pt && pt<= int.Parse(textBox1.Text) + 4)
                                    {
                                        Console.WriteLine("Done: Backup");
                                        r.totalbackup += 1;
                                        iStatic.setStatus(r.totalbackup.ToString(), label9);
                                        iStatic.setStatus("Lưu dữ liệu Application", lblStatus);
                                        runthr.Backup(offer.NameApp, txtCountrySetting.Text);
                                    }
                                }
                             
                            }
                        }
                      
                    }
                }

            }
            iStatic.setStatus("Hoàn tất", lblStatus);
            for (int j = 0; j <= dataGridView1.Columns.Count - 1; j++)
            {
                dataGridView1.Rows[i].Cells[j].Style.BackColor = Color.White;
            }
        }
        private void btnResetLead_Click(object sender, EventArgs e)
        {
            bool flag = this.btnStartLead.Text == "RESUME";
            if (flag)
            {
    
                this.btnStartLead.Text = "START";
                this.btnStartLead.Refresh();
                bool flag2 = this.thrRun != null;
                if (flag2)
                {
                    bool flag3 = this.thrRun.ThreadState != System.Threading.ThreadState.Unstarted;
                    if (flag3)
                    {
                        bool flag4 = this.thrRun.ThreadState == System.Threading.ThreadState.Suspended;
                        if (flag4)
                        {
                            thrRun.Resume();
                            Thread.Sleep(100);
                        }
                        try
                        {
                            try
                            {
                                this.thrRun.Abort();
                            }
                            catch (Exception)
                            {
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
               
            }
        }

        private void dataGridView2_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView2.Rows[e.RowIndex].Selected = true;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            cbCountry.Items.Clear();
            dataGridView2.Rows.Clear();
            if(File.Exists(iStatic.diraidIphone + "/ssh.txt"))
            {
                File.Delete(iStatic.diraidIphone + "/ssh.txt");
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            int dem = 0;
            foreach(SSH ssh in lssh)
            {
                if (ssh.live == "Dead")
                {
                    lssh.RemoveAt(dem);
                }
                dem++;
            }
            SaveSSH();
            LoadSSH(false);
        }

        private void button5_Click(object sender, EventArgs e)
        {
           
            int dem = 1;
            iDem:
            if (iStatic.diraidIphone != "")
            {
                int check = 0;
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    if (listView1.Items[i].Text == "Slot " + (listView1.Items.Count + dem))
                    {
                        check = 1;
                    }
                }
                if (check == 0)
                {
                    if (!Directory.Exists(iStatic.diraidIphone + "/Script"))
                    {
                        Directory.CreateDirectory(iStatic.diraidIphone + "/Script");
                    }
                    if (!Directory.Exists(iStatic.diraidIphone + "/Script/Slot"))
                    {
                        Directory.CreateDirectory(iStatic.diraidIphone + "/Script/Slot");
                    }
                    listView1.Items.Add("Slot " + (listView1.Items.Count + dem));


                }
                else
                {
                    dem++;
                    goto iDem;
                }
            }
            
        }
        public int random(int min, int max)
        {
            return new Random().Next(min, max);
        }
        private void button6_Click(object sender, EventArgs e)
        {
            int check = 0;
            int dem = 1;
            iDem:
            for (int i = 0;i < listView2.Items.Count;i++)
            {
                if(listView2.Items[i].Text == "Script " + (listView2.Items.Count + dem))
                {
                    check = 1;
                }
            }
            if (check == 0)
            {
                int rd = random(100, 9999);
                tvfile.wireData("Script " + (listView2.Items.Count + dem) + rd, iStatic.diraidIphone + "/Script/Slot/" + listView1.Items[countindexListSLot].Text + ".txt");
                listView2.Items.Add("Script " + (listView2.Items.Count + dem)+ rd);
            }
            else
            {
                dem++;
                goto iDem;
            }
        

        }
        int countindexListSLot = 0;
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
  
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                  
                    listView2.Items.Clear();
                    countindexListSLot = listView1.SelectedItems[0].Index;
                    string strList = listView1.SelectedItems[0].Text;
                    listView2.Enabled = true;
                    button6.Enabled = true;
                    if (File.Exists(iStatic.diraidIphone + "/Script/Slot/" + strList + ".txt"))
                    {
                        string[] arr = tvfile.readData(iStatic.diraidIphone + "/Script/Slot/" + strList + ".txt");
                        if (arr.Length - 1 > 0)
                        {
                            foreach (string str in arr)
                            {
                                listView2.Items.Add(str);
                            }
                        }
                    }
                }
        }
            catch
            {
                lblStatus.Text = "Lỗi Chọn Slot";
            }

        }
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                string strList = listView2.SelectedItems[0].Text;
                if (File.Exists(iStatic.diraidIphone + "/Script/" + strList+".txt"))
                {
                    richTextBox1.Lines = File.ReadAllLines(iStatic.diraidIphone + "/Script/" + strList + ".txt");
                }
                else
                {
                    richTextBox1.Text = "";
                }
                textBox7.Text = strList;
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                string strList = listView2.SelectedItems[0].Text;
                File.WriteAllLines(iStatic.diraidIphone + "/Script/" + strList + ".txt", richTextBox1.Lines);
                Thread.Sleep(100);
            }
        }


        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                string[] arr = new string[listView2.Items.Count];
                int intList = listView2.SelectedItems[0].Index;
                listView2.Items[intList].Text = textBox7.Text;
                string strList1 = listView1.Items[countindexListSLot].Text;
                for (int i = 0; i < listView2.Items.Count; i++)
                {
                    arr[i] = listView2.Items[i].Text;

                }
                File.WriteAllLines(iStatic.diraidIphone + "/Script/Slot/" + strList1 + ".txt", arr);
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 1)
            {
                for (int i = 0; i < dataGridView1.Rows.Count-1; i++)
                {
                    MessageBox.Show(dataGridView1.Rows[i].Cells[0].Value.ToString());
                }
            }
            //ThuVienDll.BvSshIOS.closebitvise(5886);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            sshNet ssh = new sshNet(iStatic.ipIphone);
            ssh.Enable(true, txtidPortProxy.Text, txtPortProxy.Text);
            Application.Exit();
            Environment.Exit(0);
        }

        private void button21_Click(object sender, EventArgs e)
        {
            ThreadRunLead runthr = new ThreadRunLead();
            runthr.openApp(iStatic.nameApp9);
            Thread.Sleep(1000);
            if (comboBox1.Text != "")
            {
                new Thread(() =>
                {
                    base.Invoke(new MethodInvoker(delegate
                    {
                        iStatic.setStatus("Đang xóa dữ liệu Application", lblStatus);
                        if (runthr.Wipe(comboBox1.Text, true))
                        {
                            iStatic.setStatus("Xóa dữ liệu Application thành công", lblStatus);
                        }
                        else
                        {
                            iStatic.setStatus("Xóa dữ liệu Application thất bại", lblStatus);
                        }
                    }));
                }).Start();
            }
        }


        public void checkListby(List<string> list,string str)
        {
            int d = 0;
            foreach (string strdate in list)
            {
                if (str.Trim().Equals(strdate))
                {
                    d = 1;
                }
            }
            if (d == 0)
            {
                list.Add(str.Trim());
            }
            else
            {
                d = 0;
            }
        }
        private void button13_Click(object sender, EventArgs e)
        {
            dataGridView3.Rows.Clear();
            lapp = new List<string>();
            ldate = new List<string>();
            lbackup = new List<Backup>();
            sshNet sshNet = new sshNet(iStatic.ipIphone);
            foreach (string str in sshNet.getFileBackup())
            {
                string[] arrstr = str.Split('_');
              
                if (arrstr.Length - 1 > 2)
                {
                    if (arrstr[4].Split('.').Length - 1 > 0)
                    {
                        checkListby(ldate, arrstr[2]);
                        checkListby(lapp, arrstr[0]);
                        Backup bup = new Backup();
                        bup.appList = arrstr[0];
                        bup.filename = str;
                        bup.country = arrstr[1];
                        bup.total = arrstr[4].Split('.')[0];
                        bup.timecreate = arrstr[2];
                        lbackup.Add(bup);
                        dataGridView3.Rows.Add(str, arrstr[0], arrstr[4].Split('.')[0], arrstr[1], arrstr[2]);

                    }
                }

            }
            dataGridView3.ClearSelection();
        }

        private void button25_Click(object sender, EventArgs e)
        {
            ThreadRunLead runthr = new ThreadRunLead();
            runthr.openApp(iStatic.nameApp9);
            Thread.Sleep(1000);
            if (comboBox1.Text != "" && txtCountrySetting.Text !="")
            {
                new Thread(() =>
                {
                    base.Invoke(new MethodInvoker(delegate
                    {
                        iStatic.setStatus("Lưu dữ liệu Application", lblStatus);
                        if (runthr.Backup(comboBox1.Text, txtCountrySetting.Text))
                        {
                            iStatic.setStatus("Lưu dữ liệu Application thành công", lblStatus);
                        }
                        else
                        {
                            iStatic.setStatus("Lưu dữ liệu Application thất bại", lblStatus);
                        }
                    }));
                }).Start();
            }
            else
            {
                iStatic.setStatus("Bạn chưa change data hoặc chưa có tên Application", lblStatus);
            }
        }

        private void button26_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            sshNet sshnet = new sshNet(iStatic.ipIphone);
        
            sshnet.OpenApp(iStatic.nameApp9);
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
            File.WriteAllText(iStatic.diraidIphone + "/Applist.txt", text);
            comboBox1.Enabled = true;
        }

        private void button22_Click(object sender, EventArgs e)
        {
            ThreadRunLead runthr = new ThreadRunLead();
            runthr.openApp(iStatic.nameApp9);
            Thread.Sleep(1000);
            new Thread(() =>
            {
                iStatic.setStatus("Đang đổi thiết bị", lblStatus);
                InfoDevice iDevice = runthr.changeData();
                if (iDevice.Country != null && iDevice.Language!="")
                {
                    base.Invoke(new MethodInvoker(delegate
                    {
                        txtCountrySetting.Text = iDevice.Country;
                        txtLanguageSetting.Text = iDevice.Language;
                        txtMachineSetting.Text = iDevice.Machine;
                        txtNetWordSetting.Text = iDevice.NetworkInfo;
                        txtOSVesionSetting.Text = iDevice.OSVersion;
                        txtScreennHSetting.Text = iDevice.ScreenHeight;
                        txtScreenWSetting.Text = iDevice.ScreenWidth;
                        txtTimeZone.Text = iDevice.Timezone;
                        txtUserAgentSetting.Text = iDevice.UserAgent;
                    }));
                    iStatic.setStatus("Đang đổi thiết bị thành công", lblStatus);
                }
                else
                {
                    iStatic.setStatus("Đang đổi thiết bị thất bại", lblStatus);
                }
                
            }).Start();
            
        }

        private void button23_Click(object sender, EventArgs e)
        {
           
        }

        private void button20_Click(object sender, EventArgs e)
        {
            ThreadRunLead runthr = new ThreadRunLead();

            if (textBox6.Text != "")
            {
                new Thread(() =>
            {
                iStatic.setStatus("Đang mở Link", lblStatus);

                if (runthr.openUrl(textBox6.Text))
                {
                    iStatic.setStatus("Đang mở Link thành công", lblStatus);

                }
            }).Start();
            }
        }

        private void button18_Click_1(object sender, EventArgs e)
        {
            ThreadRunLead runthr = new ThreadRunLead();
            new Thread(() =>
            {
                string[] ar = richTextBox2.Lines;
                iStatic.setStatus("Chạy Script", lblStatus);
                runthr.script(ar);
                iStatic.setStatus("Chạy Script Xong", lblStatus);
            }).Start();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
           
            if (cbRRSbyAPP.Checked)
            {
        
                cbxRRSbyApp.Items.Clear();
                foreach (string str in lapp)
                {
                    cbxRRSbyApp.Items.Add(str);
                }
                cbxRRSbyApp.Enabled = true;
            }
            else
            {
               
                cbxRRSbyApp.Items.Clear();
                cbxRRSbyApp.Enabled = false;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRRSbyDay.Checked)
            {
                if (!cbRRSbyAPP.Checked)
                { 
                    cbxRRSbyDay.Items.Clear();
                    foreach (string str in ldate)
                    {
                        cbxRRSbyDay.Items.Add(str);
                    }
                  
                }
                cbxRRSbyDay.Enabled = true;
            }
            else
            {
                cbxRRSbyDay.Items.Clear();
                cbxRRSbyDay.Enabled = false;
             
               
                
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

            ldatebyapp = new List<string>();
            dataGridView3.Rows.Clear();
            cbxRRSbyDay.Items.Clear();
            foreach (Backup blup in lbackup)
            {
                int ck = 0;
                foreach (string str in ldatebyapp)
                {
                    if (str.Equals(blup.timecreate) )
                    {
                        ck = 1;
                    }
                }
             
                if (ck == 0 && blup.appList.Trim().Equals(cbxRRSbyApp.Text.Trim()))
                {
                    ldatebyapp.Add(blup.timecreate);
                    cbxRRSbyDay.Items.Add((blup.timecreate));
                }
                if (cbRRSbyDay.Checked)
                {
                    if (blup.appList.Trim().Equals(cbxRRSbyApp.Text.Trim()) && blup.timecreate.Trim().Equals(cbxRRSbyDay.Text.Trim()))
                    {
                        
                        dataGridView3.Rows.Add(blup.filename, blup.appList, blup.total, blup.country, blup.timecreate);
                    }
                }
                else
                {
       
                    if (blup.appList.Trim().Equals(cbxRRSbyApp.Text.Trim()))
                    {
                    
                        dataGridView3.Rows.Add(blup.filename, blup.appList, blup.total, blup.country, blup.timecreate);
                    }
                }
            }
            dataGridView3.ClearSelection();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            dataGridView3.Rows.Clear();
            foreach (Backup blup in lbackup)
            {
                if (cbRRSbyAPP.Checked)
                {
                    if (blup.appList.Trim().Equals(cbxRRSbyApp.Text.Trim()) && blup.timecreate.Trim().Equals(cbxRRSbyDay.Text.Trim()))
                    {
                        dataGridView3.Rows.Add(blup.filename, blup.appList, blup.total, blup.country, blup.timecreate);
                    }
                }
                else
                {
                    if (blup.timecreate.Trim().Equals(cbxRRSbyDay.Text.Trim()))
                    {
                        dataGridView3.Rows.Add(blup.filename, blup.appList, blup.total, blup.country, blup.timecreate);
                    }
                }
            }
            dataGridView3.ClearSelection();
        }
        Thread thrRunScript;
        private void button8_Click_1(object sender, EventArgs e)
        {
            if(button8.Text == "Test Script")
            {
                button8.Text = "Stop Script";
                ThreadRunLead r = new IOS_AutoLead.ThreadRunLead();
                string[] arr = richTextBox1.Lines;
                thrRunScript = new Thread(()=> {
                    r.script(arr);
                    button8.Invoke(new MethodInvoker(delegate { button8.Text = "Test Script"; }));
                });
                thrRunScript.Start();
            }
            else
            {
                bool flag2 = this.thrRunScript != null;
                if (flag2)
                {
                    bool flag3 = this.thrRunScript.ThreadState != System.Threading.ThreadState.Unstarted;
                    if (flag3)
                    {
                        bool flag4 = this.thrRunScript.ThreadState == System.Threading.ThreadState.Suspended;
                        if (flag4)
                        {
                            thrRunScript.Resume();
                            Thread.Sleep(100);
                        }
                        try
                        {
                            try
                            {
                                this.thrRunScript.Abort();
                            }
                            catch (Exception)
                            {
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                button8.Text = "Test Script";
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows.Count == 1)
            {
                ThreadRunLead runthr = new ThreadRunLead();

                new Thread(() =>
                {
                    base.Invoke(new MethodInvoker(delegate
                    {
                        int irow = dataGridView3.SelectedRows[0].Index;
                        iStatic.setStatus("Đang Restore File "+ dataGridView3.Rows[irow].Cells[0].Value.ToString().Trim(), lblStatus);
                        runthr.Restore(dataGridView3.Rows[irow].Cells[0].Value.ToString().Trim(), dataGridView3.Rows[irow].Cells[1].Value.ToString().Trim());
                        iStatic.setStatus("Restore Thành Công",lblStatus);
                    }));
                }).Start();
            }     
        }

        private void btnStartRRS_Click(object sender, EventArgs e)
        {
            bool flag = this.btnStartRRS.Text == "START" || this.btnStartRRS.Text == "RESUME";
            if (flag)
            {
                btnResetRRS.Enabled = false;
                
                bool flag3 = this.btnStartRRS.Text == "START";
                if (flag3)
                {
                   


                    this.thrRun = new Thread(new ThreadStart(this.runRRS));
                    this.thrRun.Start();
                }
                else
                {

                    bool flag4 = this.thrRun == null || (this.thrRun.ThreadState & System.Threading.ThreadState.Stopped) == System.Threading.ThreadState.Stopped;
                    if (flag4)
                    {
                        this.thrRun = new Thread(new ThreadStart(this.runRRS));
                    }
                    bool flag5 = (this.thrRun.ThreadState & System.Threading.ThreadState.Suspended) == System.Threading.ThreadState.Suspended;
                    if (flag5)
                    {
                        thrRun.Resume();
                    }
                    else
                    {
                        bool flag6 = (this.thrRun.ThreadState & System.Threading.ThreadState.Unstarted) == System.Threading.ThreadState.Unstarted || (this.thrRun.ThreadState & System.Threading.ThreadState.AbortRequested) == System.Threading.ThreadState.AbortRequested || (this.thrRun.ThreadState & System.Threading.ThreadState.Aborted) == System.Threading.ThreadState.Aborted || (this.thrRun.ThreadState & System.Threading.ThreadState.Stopped) == System.Threading.ThreadState.Stopped;
                        if (flag6)
                        {
                            this.thrRun = new Thread(new ThreadStart(this.runRRS));
                            this.thrRun.Start();
                        }
                    }
                }
                this.btnStartRRS.Text = "STOP";
                this.btnStartRRS.Refresh();
                btnResetRRS.Enabled = false;
            }
            else
            {

                try
                {
                    thrRun.Suspend();
                }
                catch (Exception)
                {
                }

                this.btnStartRRS.Text = "RESUME";
                this.btnStartRRS.Refresh();
                btnResetRRS.Enabled = true;
            }
        }
        public void connectProxy()
        {
            Proxy:
            switch (cbProxyTypeHome.Text)
            {
                case "SSH":
                    if (!SSH.ConnectSSH(txtidPortProxy.Text, int.Parse(txtPortProxy.Text), lssh, sshLine, lblStatus))
                    {
                        goto Proxy;
                    }
                    break;
                case "Proxy": break;
                case "Direct": break;
            }
        }
        public void runRRS()
        {
            ThreadRunLead run = new IOS_AutoLead.ThreadRunLead();
   
            if (dataGridView3.SelectedRows.Count > 0)
            {
                foreach(DataGridViewRow row in dataGridView3.SelectedRows)
                {




                    connectProxy();
                iStatic.setStatus("Đang Restore File" + row.Cells[0].Value.ToString(), lblStatus);

                    if (run.Restore(row.Cells[0].Value.ToString(), row.Cells[1].Value.ToString()))
                    {
                        iStatic.setStatus("Restore Done",lblStatus);
                        run.openApp(GetApp(row.Cells[1].Value.ToString().Trim()));
                        if (cbSelectScript.Text != "")
                        {
                            if (cboxRDomScript.Checked)
                            {
                                if (File.Exists(iStatic.diraidIphone + "/Script/Slot" + cbSelectScript.Text))
                                {
                                    string[] arrRdScript = File.ReadAllLines(iStatic.diraidIphone + "/Script/Slot" + cbSelectScript.Text);
                                    string dirscript = arrRdScript[new Random().Next(0, arrRdScript.Length - 1)];

                                    if (File.Exists(iStatic.diraidIphone + "/Script/" + dirscript))
                                    {
                                        iStatic.setStatus("Đang Chạy Script", lblStatus);
                                        string[] arrScript = File.ReadAllLines(iStatic.diraidIphone + "/Script/" + cbSelectScript.Text);
                                        run.script(arrScript);
                                    }
                                }

                            }
                            else
                            {
                                if (File.Exists(iStatic.diraidIphone + "/Script/" + cbSelectScript.Text))
                                {
                                    iStatic.setStatus("Đang Chạy Script", lblStatus);
                                    string[] arrScript = File.ReadAllLines(iStatic.diraidIphone + "/Script/" + cbSelectScript.Text);
                                    run.script(arrScript);
                                }
                            }
                        }
                        row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#2ECC40");
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.Red;
                    }
                }
            }
            else
            {
                for(int i=0;i< dataGridView3.Rows.Count - 1; i++)
                {
                    connectProxy();
                    iStatic.setStatus("Đang Restore File" + dataGridView3.Rows[i].Cells[0].Value.ToString(), lblStatus);
                    if (run.Restore(dataGridView3.Rows[i].Cells[0].Value.ToString(), dataGridView3.Rows[i].Cells[1].Value.ToString()))
                    {
                        iStatic.setStatus("Restore Done", lblStatus);
                        run.openApp(GetApp(dataGridView3.Rows[i].Cells[1].Value.ToString().Trim()));
                        if (cbSelectScript.Text != "")
                        {
                            if (cboxRDomScript.Checked)
                            {
                                if (File.Exists(iStatic.diraidIphone + "/Script/Slot" + cbSelectScript.Text))
                                {
                                    string[] arrRdScript = File.ReadAllLines(iStatic.diraidIphone + "/Script/Slot" + cbSelectScript.Text);
                                    string dirscript = arrRdScript[new Random().Next(0, arrRdScript.Length - 1)];

                                    if (File.Exists(iStatic.diraidIphone + "/Script/" + dirscript))
                                    {
                                        iStatic.setStatus("Đang Chạy Script", lblStatus);
                                        string[] arrScript = File.ReadAllLines(iStatic.diraidIphone + "/Script/" + cbSelectScript.Text);
                                        run.script(arrScript);
                                    }
                                }

                            }
                            else
                            {
                                if (File.Exists(iStatic.diraidIphone + "/Script/" + cbSelectScript.Text))
                                {
                                    iStatic.setStatus("Đang Chạy Script", lblStatus);
                                    string[] arrScript = File.ReadAllLines(iStatic.diraidIphone + "/Script/" + cbSelectScript.Text);
                                    run.script(arrScript);
                                }
                            }
                        }
                        //
                        dataGridView3.Rows[i].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#2ECC40");
                    }
                    else
                    {
                        dataGridView3.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    }

                }
            }
        }


        private void btnResetRRS_Click(object sender, EventArgs e)
        {
            bool flag = this.btnStartRRS.Text == "RESUME";
            if (flag)
            {

                this.btnStartRRS.Text = "START";
                this.btnStartRRS.Refresh();
                bool flag2 = this.thrRun != null;
                if (flag2)
                {
                    bool flag3 = this.thrRun.ThreadState != System.Threading.ThreadState.Unstarted;
                    if (flag3)
                    {
                        bool flag4 = this.thrRun.ThreadState == System.Threading.ThreadState.Suspended;
                        if (flag4)
                        {
                            thrRun.Resume();
                            Thread.Sleep(100);
                        }
                        try
                        {
                            try
                            {
                                this.thrRun.Abort();
                            }
                            catch (Exception)
                            {
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

            }
        }




        private void cboxUserScript_CheckedChanged(object sender, EventArgs e)
        {
            if (cboxUserScript.Checked)
            {
                DirectoryInfo d = new DirectoryInfo(iStatic.diraidIphone + "/Script");//Assuming Test is your Folder
                FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files
                cbSelectScript.Items.Clear();
                foreach (FileInfo file in Files)
                {
                    cbSelectScript.Items.Add(file.Name);
                }
            }
        }

        private void cboxRDomScript_CheckedChanged(object sender, EventArgs e)
        {
            if (cboxRDomScript.Checked)
            {
                DirectoryInfo d = new DirectoryInfo(iStatic.diraidIphone + "/Script/Slot");//Assuming Test is your Folder
                FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files
                cbSelectScript.Items.Clear();
                foreach (FileInfo file in Files)
                {
                    cbSelectScript.Items.Add(file.Name);
                }
            }
        }

        private void dataGridView3_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView3.Rows[e.RowIndex].Selected = true;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows.Count == 1)
            {
                int irow = dataGridView3.SelectedRows[0].Index;
                sshNet sshnet = new sshNet(iStatic.ipIphone);
                sshnet.deleteFileLuu(dataGridView3.Rows[irow].Cells[0].Value.ToString());
                dataGridView3.Rows.RemoveAt(irow);

            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < dataGridView3.Rows.Count - 1; i++)
            {
                sshNet sshnet = new sshNet(iStatic.ipIphone);
                sshnet.deleteFileLuu(dataGridView3.Rows[i].Cells[0].Value.ToString());
            }
            dataGridView3.Rows.Clear();
        }

        private void richTextBox3_TextChanged(object sender, EventArgs e)
        {
            File.WriteAllLines(iStatic.diraidIphone + "/proxy.txt", richTextBox1.Lines);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            sshNet ssh = new sshNet(iStatic.ipIphone);
            ssh.Enable(true, txtidPortProxy.Text, txtPortProxy.Text);
            Application.Exit();
            Environment.Exit(0);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (File.Exists(iStatic.diraidIphone + "/home.txt"))
            {
                File.Delete(iStatic.diraidIphone + "/home.txt");
            }
            dataGridView1.Rows.Clear();
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void txtx1_TextChanged(object sender, EventArgs e)
        {
            
            if(txty1.Text != "-1")
            {
                File.WriteAllText(iStatic.diraidIphone + "/xySafri.txt", txtx1.Text +"|"+txty1.Text);
            }
            else
            {
                File.WriteAllText(iStatic.diraidIphone + "/xySafri.txt", txtx1.Text + "|-1" );
            }
           
        }

        private void txty1_TextChanged(object sender, EventArgs e)
        {
            if (txtx1.Text != "-1")
            {
                File.WriteAllText(iStatic.diraidIphone + "/xySafari.txt", txtx1.Text + "|" + txty1.Text);
            }
            else
            {
                File.WriteAllText(iStatic.diraidIphone + "/xySafari.txt",  "-1|"+txty1.Text);
            }
        }

        private void txtx2_TextChanged(object sender, EventArgs e)
        {

            if (txty2.Text != "-1")
            {
                File.WriteAllText(iStatic.diraidIphone + "xyAppstrore.txt", txtx2.Text + "|" + txty2.Text);
            }
            else
            {
                File.WriteAllText(iStatic.diraidIphone + "xyAppstrore.txt", txtx2.Text + "|-1");
            }
        }

        private void txty2_TextChanged(object sender, EventArgs e)
        {
            if (txtx2.Text != "-1")
            {
                File.WriteAllText(iStatic.diraidIphone + "xyAppstrore.txt", txtx2.Text + "|" + txty2.Text);
            }
            else
            {
                File.WriteAllText(iStatic.diraidIphone + "xyAppstrore.txt", "-1|" + txty2.Text);
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            IDataObject iData = Clipboard.GetDataObject();
            
            if (iData.GetDataPresent(DataFormats.Text))
            {
                File.WriteAllText(iStatic.diraidIphone + "/ssh.txt", (String)iData.GetData(DataFormats.Text));
                cbCountry.Items.Clear();
                LoadSSH(true);

            }
           
            
          
        }
    }
}
