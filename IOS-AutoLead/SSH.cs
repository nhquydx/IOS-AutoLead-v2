using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace IOS_AutoLead
{
    public class SSH
    {
        static int sshLine = 0;
        public SSH() { }
        public static void SaveSSH(List<SSH> lssh)
        {
            string text = "";
            foreach (SSH ssh in lssh)
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
        public static bool ConnectSSH(string ipaddress, int portaddress,List<SSH> lssh,int sshline,Label lbl)
        {
            Form1 frm = new Form1();
            sshLine = sshline-1;

            ConnectSSH:
            ThuVienDll.BvSshIOS.closebitvise(portaddress);
            sshNet sshnet = new sshNet(iStatic.ipIphone);
            SSH sshproxy= nextSSH(lssh);
            if (sshproxy == null)
            {
                iStatic.setStatus("SSH Het",lbl);
                return false;
            }
            if (sshproxy.live == "uncheck")
            {
                iStatic.setStatus("Kết Nối SSH : " + sshproxy.IP + "|" + sshproxy.username + "|" + sshproxy.password, lbl);
                ThuVienDll.FuncFolder tvfile = new ThuVienDll.FuncFolder();
                if (!ThuVienDll.BvSshIOS.SetSSH(sshproxy.IP, sshproxy.username, sshproxy.password, ipaddress, portaddress.ToString(), 15))
                {
                    ThuVienDll.BvSshIOS.closebitvise(portaddress);
                    Console.WriteLine(sshline);
                    lssh[sshLine].live = "dead";
                    goto ConnectSSH;
                }
                lssh[sshLine].live = "live";
                SaveSSH(lssh);
                iStatic.setStatus("Kết Nối SSH Thành Công ", lbl);

                return true;
            }
            else
            {
                goto ConnectSSH;
            }

        }
     
        public  static SSH nextSSH(List<SSH> lssh)
        {
            if (sshLine < lssh.Count-1)
            {
                sshLine++;
                return lssh[sshLine];
            }
            return null;
        }
        public string IP
        {
            get;
            set;
        }


        public string username
        {
            get;
            set;
        }


        public string password
        {
            get;
            set;
        }


        public string country
        {
            get;
            set;
        }

        public string countrycode
        {
            get;
            set;
        }

      
        public string live
        {
            get;
            set;
        }
    }
}
