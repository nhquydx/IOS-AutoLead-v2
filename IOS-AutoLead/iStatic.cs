using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IOS_AutoLead
{
    public class iStatic
    {
        public static string status =  string.Empty;
        public static string ipIphone = string.Empty;
        public static string diraidIphone = string.Empty;
        public static string dirExe = AppDomain.CurrentDomain.BaseDirectory;
        public static string nameApp9 = "com.mins.iPhoneChanger";


        public static void setStatus(string Stt, Label lb)
        {
            lb.Invoke(new MethodInvoker(delegate { lb.Text = Stt; }));
        }
    }
}
