using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IOS_AutoLead
{
    public  class ThreadRunLead
    {
        string app9Logo = "com.mins.iPhoneChanger";

        sshNet ssh = new sshNet(iStatic.ipIphone);
        //GETAPPLIST
        public string[] GetAppList()
        {
            string[] arrApplist = ssh.GetAppList();
            return arrApplist;
        }
        //WIPE---------------------------------------------------------------------------------
        public bool Wipe(string App,bool bl)
        {
            ssh.KillAll();
            ssh.OpenApp(app9Logo);
            Thread.Sleep(3000);
             return ssh.WipeApp(App, bl);
        }
        //CHANGE-------------------------------------------------------------------------------
        public InfoDevice changeData()
        {
            InfoDevice dvc = new InfoDevice();
            try
            {
               
                string devicejs = ssh.ChangeDevice();
                dynamic data = JObject.Parse(devicejs);
                dvc.Country = data.Device.IOS;
                dvc.NetworkInfo = data.Device.NetworkInfo;
                dvc.Machine = data.Device.HWMachine;
                dvc.Timezone = data.Device.Timezone;
                dvc.ScreenHeight = data.Device.ScreenHeight;
                dvc.ScreenWidth = data.Device.ScreenWidth;
                dvc.OSVersion = data.Device.OSVersion;
                dvc.Language = data.Device.Language;
                dvc.UserAgent = data.Device.UserAgent;
            }
            catch
            {
                return dvc;
            }
            return dvc;
        }
        //OPEN URL-----------------------------------------------------------------------------
        public void fileOpenUrl(string x1,string y1,string x2,string y2)
        {
            string text = "tap("+x1+ ", " + y1 + ")";
                   text += "/r/n";
                   text += "tap(" + x2 + ", " + y2 + ")";
            File.WriteAllText(iStatic.diraidIphone + "/openlink.lua", text);
        }
        public bool openUrl(string url)
        {
            if (!ssh.openlink(url)) { return false; }
  
            return true;
        }
        public bool openUrl(string url,int timeoutUrl)
        {
            ssh.UploadFile("/var/mobile/Library/AutoTouch/Scripts/", "./"+iStatic.ipIphone+"/openlink.lua");
            if (!ssh.openlink(url)) { return false; }
            //check appstore open
            DateTime startTime = DateTime.Now;
            while (true)
            {
                ThuVienDll.RequestServer.HTTP_GET("http://" + iStatic.ipIphone + ":8080/control/start_playing?path=openlink.lua", "");
                Thread.Sleep(1000);
                if (int.Parse(DateTime.Now.Subtract(startTime).TotalSeconds.ToString().Split('.')[0]) > timeoutUrl)
                {
                    return false;
                }
                if(ThuVienDll.RequestServer.HTTP_GET("http://" + iStatic.ipIphone + ":6969/checkapps", "").Trim()== "App Store")
                {

                    return true;
                }

            }
          
        }
       
        //OpenApp------------------------------------------------------------------------------
        public void openApp(string App)
        {
            ssh.OpenApp(App);
        }
        //Script-------------------------------------------------------------------------------
        public bool script(string[] arr)
        {
            int nsleepMouse = 0;
            int cTouchUp = 0;
            Thread.Sleep(2000);
            ThuVienDll.FuncFolder file = new ThuVienDll.FuncFolder();
            foreach (string str in arr)
            {
               
               
            
                Console.WriteLine(nsleepMouse);
                if (str.Contains("usleep"))
                {

                    string[] arrsleep = str.Split(new string[] { "usleep(", ")" }, StringSplitOptions.None);
                    double nsleep = 0;
                    if (double.TryParse(arrsleep[1], out nsleep))
                    {
                        nsleepMouse += (int)(nsleep / 1000);
                    }
                }
               
                if (str.Contains("touchDown"))
                {
                    cTouchUp = 0;
                    nsleepMouse = 0;
                    File.WriteAllText(iStatic.diraidIphone + "/Script.lua", str +"\r\n");
                }
                else
                {
                    if (str.Contains("touchMove"))
                    {
                        file.wireData(str, iStatic.diraidIphone + "/Script.lua");
                    }
                    else
                    {
                        if (str.Contains("touchUp"))
                        {
                            cTouchUp = 1;
                            file.wireData(str, iStatic.diraidIphone + "/Script.lua");
                            ssh.UploadFile("/var/mobile/Library/AutoTouch/Scripts/", "./" + iStatic.diraidIphone + "/Script.lua");
                            ThuVienDll.RequestServer.HTTP_GET("http://" + iStatic.ipIphone + ":8080/control/start_playing?path=Script.lua", "");
                            Thread.Sleep(nsleepMouse);
                        }
                        else
                        {
                            if (cTouchUp == 0)
                            {
                                if (str.Contains("usleep"))
                                {
                                    file.wireData(str, iStatic.diraidIphone + "/Script.lua");
                                }
                            }
                            else
                            {
                                File.WriteAllText(iStatic.diraidIphone + "/Script.lua", str);
                                ssh.UploadFile("/var/mobile/Library/AutoTouch/Scripts/", "./" + iStatic.diraidIphone + "/Script.lua");
                                ThuVienDll.RequestServer.HTTP_GET("http://" + iStatic.ipIphone + ":8080/control/start_playing?path=Script.lua", "");
                                if (str.Contains("usleep"))
                                {
                                    string[] arrsleep = str.Split(new string[] { "usleep(", ")" }, StringSplitOptions.None);
                                    double nsleep = 0;
                                    if (double.TryParse(arrsleep[1], out nsleep))
                                    {
                                        Thread.Sleep((int)(nsleep / 1000));
                                    }
                                }
                            }
                        }
                    }
                }
                
           
               


            }
            
            return true;
        }
        //Backup--------------------------------------------------------------------------------
        public bool Backup(string app,string country)
        {
           
            string date = "_" + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + "_" + ssh.random(1111, 99999) + "_0";
            if (!ssh.backUpApp(app, country, date)) {  ssh.deleteFile("/var/root/backup/", app + "_"+ country + date); return false; } else { return true; } ;

        }
        //Restore---------------------------------------------------------------------------------
        public bool Restore(string dir,string app)
        {
            if (!ssh.RestoreApp(dir, app))
            {
                ssh.deleteFile("/var/root/backup/", dir.Split('.')[0]);
                return false;
            }
            return true;
        }
        
    }
}
