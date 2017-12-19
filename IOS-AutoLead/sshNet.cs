using IOS_LeadMobile;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IOS_AutoLead
{
    public class sshNet
    {
        string ip;
        string app9Logo = "com.mins.iPhoneChanger";
        ThuVienDll.FuncFolder tvFile;
       
        public sshNet(string ip)
        {
            this.ip = ip;
        }

        //Func 
        public void ErrorSaveLog(string str)
        {
            ThuVienDll.FuncFolder file = new ThuVienDll.FuncFolder();
            file.wireData(str, "log.txt");
        }
        public bool deleteFile(string pathosap, string nameos)
        {

            try
            {
                using (var client = new SftpClient(ip, "root", "alpine"))
                {
                    client.Connect();
                    if (client.IsConnected)
                    {

                        var files = client.ListDirectory(pathosap);
                        //   Console.WriteLine(nameos + ": --------------------------");
                        foreach (var file in files)
                        {

                            //Console.WriteLine(file.Name +"---"+ nameos);
                            if (file.Name == nameos)
                            {
                                runcmd("rm -rf " + file.FullName);
                                client.Disconnect();
                                return true;
                            }
                        }
                        //    Console.WriteLine(nameos + ": --------------------------");
                    }

                    return false;
                }

            }
            catch (Exception ex)
            {
                ErrorSaveLog("ERROR:" + ex);
                return false;
            }
        }
        public bool copy(string pathto, string pathfrom)
        {

            try
            {
                ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
               {
                    new PasswordAuthenticationMethod("root", "alpine")
               });
                using (SftpClient client = new SftpClient(connectionInfo))
                {
                    client.Connect();
                    if (client.IsConnected)
                    {

                        var files = client.ListDirectory(pathto);

                        foreach (var file in files)
                        {
                            if ((!file.Name.StartsWith(".")))
                            {
                                Console.WriteLine("cp -rf '" + file.FullName + "' '" + pathfrom+"'");
                                runcmd("cp -rf '" + file.FullName + "' '" + pathfrom+"'");
                            }

                        }

                    }

                    return false;
                }

            }
            catch (Exception ex)
            {
                ErrorSaveLog("ERROR:" + ex);
                return false;
            }
        }
        public bool ProxyAdd(string ipaddress, string port)
        {
            try
            {
                string strResuftMain = string.Concat(new object[]
                {
                "function FindProxyForURL(url, host)\r\n{\r\nreturn \"SOCKS ",
                ipaddress,
                ":",
                port,
                "\";\r\n}"
                 });
                string strFileProxyPac = "./iTune/proxy.pac";
                StreamWriter streamWriter = new StreamWriter(strFileProxyPac);
                streamWriter.WriteLine(strResuftMain);
                streamWriter.Close();
                string strResuftMain2 = string.Concat(new object[]
            {
            "strict_chain\r\nproxy_dns\r\nremote_dns_subnet 224\r\ntcp_read_time_out 15000\r\ntcp_connect_time_out 8000\r\n[ProxyList]\r\nsocks5\t",
            ipaddress,
            " ",
            port,
            });
                string strFileProxyChain = "./iTune..proxychains.conf";
                StreamWriter streamWriter2 = new StreamWriter(strFileProxyChain);
                streamWriter2.WriteLine(strResuftMain2);
                streamWriter2.Close();
                ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
                {
                new PasswordAuthenticationMethod("root", "alpine")
                });
                using (SftpClient sftpClient = new SftpClient(connectionInfo))
                {
                    sftpClient.Connect();
                    sftpClient.ChangeDirectory("/private/var/");
                    using (FileStream fileStream = File.OpenRead(strFileProxyPac))
                    {
                        sftpClient.UploadFile(fileStream, "proxy.pac", true, null);
                    }
                    sftpClient.Disconnect();
                }
                using (SftpClient sftpClient2 = new SftpClient(connectionInfo))
                {
                    sftpClient2.Connect();
                    sftpClient2.ChangeDirectory("/private/etc/");
                    using (FileStream fileStream2 = File.OpenRead(strFileProxyChain))
                    {
                        sftpClient2.UploadFile(fileStream2, "proxychains.conf", true, null);
                    }
                    sftpClient2.Disconnect();
                }
                using (SshClient sshClient = new SshClient(connectionInfo))
                {
                    sshClient.Connect();
                    sshClient.CreatenExecute("killall -9 networkd");
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void sendIOS(string text)
        {
            new Thread(() =>
            {
                tvFile = new ThuVienDll.FuncFolder();
                if (File.Exists("./quy/send.lua"))
                {
                    File.Delete("./quy/send.lua");
                }
                tvFile.wireData("inputText('" + text + "')", "./quy/send.lua");
                UploadFile("/var/mobile/Library/AutoTouch/Scripts/", "./quy/send.lua");
                ThuVienDll.RequestServer.HTTP_GET("http://" + ip + ":8080/control/start_playing?path=send.lua", "");
            }).Start();
        }
        public bool isDirAndFileApp(string path, string nameos)
        {
            try
            {
                using (var client = new SftpClient(ip, "root", "alpine"))
                {
                    client.Connect();
                    if (client.IsConnected)
                    {
                        var files = client.ListDirectory(path);
                        //   Console.WriteLine(nameos + ": --------------------------");
                        foreach (var file in files)
                        {
                            //     Console.WriteLine(file.Name +"---"+ nameos);
                            if (file.Name == nameos)
                            {

                                client.Disconnect();
                                return true;
                            }
                        }
                        //    Console.WriteLine(nameos + ": --------------------------");
                    }
                    return false;
                }
            }
            catch
            {

                return false;
            }
        }
        public int random(int min, int max)
        {
            return new Random().Next(min, max);
        }
        public void UploadFile(string pathos, string pathpc)
        {
            try
            {
                ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
                {
                    new PasswordAuthenticationMethod("root", "alpine")
                });
                using (SftpClient sftp = new SftpClient(connectionInfo))
                {
                    sftp.Connect();
                    string workingdirectory = pathos;
                    string uploadfile = pathpc;
                    Stream fileStream = new FileStream(uploadfile, FileMode.Open);
                    sftp.ChangeDirectory(workingdirectory);
                    var listDirectory = sftp.ListDirectory(workingdirectory);
                    sftp.BufferSize = 4 * 1024; // bypass Payload error large files
                    sftp.UploadFile(fileStream, Path.GetFileName(uploadfile));
                    sftp.Disconnect();
                    fileStream.Close();
                }
            }
            catch
            {

            }
        }
        public void DownFile(string pathosap, string namfile, string path)
        {
            if (File.Exists(path + "/" + namfile))
            {
                File.Delete(path + "/" + namfile);
            }
            ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
              {
                    new PasswordAuthenticationMethod("root", "alpine")
              });
            using (ScpClient scp = new ScpClient(connectionInfo))
            {

                scp.Connect();
             
                scp.Download(pathosap + "/" + namfile, new FileInfo(path + "/" + namfile));
                scp.Disconnect();
            }


        }
        public string[] getApplication()
        {
            string St_ResuftCommand;
            try
            {
                ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
            {
            new PasswordAuthenticationMethod("root", "alpine")
            });
                using (SshClient sshClient = new SshClient(connectionInfo))
                {
                    sshClient.Connect();
                    St_ResuftCommand = sshClient.CreatenExecute("ipainstaller -l");
                    string[] StArray_ResuftCommand = StringFunctions.ArrSplitOpTion(St_ResuftCommand, "\n");
                    sshClient.Disconnect();
                    return StArray_ResuftCommand;
                }
            }
            catch
            {
                return null;
            }
        }
        public string[] getFileBackup()
        {
            string[] arrResuft = null;
            try
            {
                ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
                {
                  new PasswordAuthenticationMethod("root", "alpine")
                });
                using (SshClient sshClient = new SshClient(connectionInfo))
                {
                    sshClient.Connect();
                    string resuft = sshClient.CreatenExecute("ls /private/var/root/backup/");
                    
                    arrResuft = StringFunctions.ArrSplitOpTion(resuft, "\n");
                    sshClient.Disconnect();
                    return arrResuft;

                }
            }
            catch
            {
                return arrResuft;
            }

        }

        public string getdirapp(string app, bool isBundle)
        {
            ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
            {
                new PasswordAuthenticationMethod("root", "alpine")
            });
            using (SshClient sshClient = new SshClient(connectionInfo))
            {
                sshClient.Connect();
                string findapplication;
                if (isBundle)
                {
                    findapplication = runcmd("find /var/containers/Bundle/Application/*/ -name '.com.apple.*'");
                }
                else
                {
                    findapplication = runcmd("find /private/var/mobile/Containers/Data/Application/*/ -name '.com.apple.*'");
                }

                string[] arrResuftCommand = StringFunctions.ArrSplitOpTion(findapplication, "\n");
                foreach (string strresuft in arrResuftCommand)
                {

                    if (runcmd("cat " + strresuft).Contains(app))
                    {
                        return StringFunctions.ArrSplitOpTion(strresuft, ".com.apple.mobile_container_manager.metadata.plist")[0];
                    }
                }
            }
            return string.Empty;
        }


        public void ClearSafari()
        {
            string strResuftCommand;
            try
            {
                ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
            {
            new PasswordAuthenticationMethod("root", "alpine")
            });
                using (SshClient sshClient = new SshClient(connectionInfo))
                {
                    sshClient.Connect();
                    sshClient.CreatenExecute("killall -9 AppStore");
                    sshClient.CreatenExecute("killall -9 MobileSafari");
                    sshClient.CreatenExecute("rm -r /private/var/mobile/Library/Safari");
                    sshClient.CreatenExecute("ls /private/var/mobile/Library/Cookies/");
                    strResuftCommand = sshClient.CreatenExecute("ls /private/var/mobile/Library/Cookies/");
                    string[] arrResuftFile = StringFunctions.ArrSplitOpTion(strResuftCommand, "\n");
                    for (int i = 0; i < arrResuftFile.Count<string>(); i++)
                    {
                        if (arrResuftFile[i] != null && arrResuftFile[i] != "" && arrResuftFile[i].IndexOf("sqlitedb") <= 0)
                        {
                            sshClient.CreatenExecute("rm /private/var/mobile/Library/Cookies/" + arrResuftFile[i]);
                        }
                    }
                    strResuftCommand = sshClient.CreatenExecute("find /private/var/mobile/Containers/Data/Application/ -name \"SuspendState.plist\"");
                    string[] arrResuftCommand = StringFunctions.ArrSplitOpTion(strResuftCommand, "\n");
                    if (arrResuftCommand.Count<string>() > 0)
                    {
                        for (int j = 0; j < arrResuftCommand.Count<string>(); j++)
                        {
                            arrResuftCommand[j] = arrResuftCommand[j].Encode();
                            sshClient.CreatenExecute("rm " + arrResuftCommand[j]);
                        }
                    }
                    Thread.Sleep(500);
                    strResuftCommand = sshClient.CreatenExecute("find /private/var/mobile/Containers/Data/Application/ -name \"History.db-wal\"");
                    arrResuftCommand = StringFunctions.ArrSplitOpTion(strResuftCommand, "\n");
                    if (arrResuftCommand.Count<string>() > 0)
                    {
                        for (int k = 0; k < arrResuftCommand.Count<string>(); k++)
                        {
                            arrResuftCommand[k] = arrResuftCommand[k].Encode();
                            using (SshCommand sshCommand = sshClient.CreateCommand("rm " + arrResuftCommand[k]))
                            {
                                sshCommand.Execute();
                            }
                        }
                    }
                    strResuftCommand = sshClient.CreatenExecute("find /private/var/mobile/Containers/Data/Application/ -name \"History.db\"");
                    arrResuftCommand = StringFunctions.ArrSplitOpTion(strResuftCommand, "\n");
                    if (arrResuftCommand.Count<string>() > 0)
                    {
                        for (int l = 0; l < arrResuftCommand.Count<string>(); l++)
                        {
                            arrResuftCommand[l] = arrResuftCommand[l].Encode();
                            sshClient.CreatenExecute("rm " + arrResuftCommand[l]);
                        }
                    }
                    strResuftCommand = sshClient.CreatenExecute("find /private/var/mobile/Containers/Data/Application/ -name \"History.db-shm\"");
                    arrResuftCommand = StringFunctions.ArrSplitOpTion(strResuftCommand, "\n");
                    if (arrResuftCommand.Count<string>() > 0)
                    {
                        for (int m = 0; m < arrResuftCommand.Count<string>(); m++)
                        {
                            arrResuftCommand[m] = arrResuftCommand[m].Encode();
                            sshClient.CreatenExecute("rm " + arrResuftCommand[m]);
                        }
                    }
                    strResuftCommand = sshClient.CreatenExecute("find /private/var/mobile/Containers/Data/Application/ -name fsCachedData");
                    string[] arrResuftCommand2 = StringFunctions.ArrSplitOpTion(strResuftCommand, "\n");
                    for (int n = 0; n < arrResuftCommand2.Count<string>(); n++)
                    {
                        if (arrResuftCommand2[n].IndexOf("mobilesafari") > 0)
                        {
                            string strResuftNop = arrResuftCommand2[n];
                            string[] arrResuftNop = StringFunctions.ArrSplitOpTion(strResuftNop, "/");
                            if (arrResuftNop != null)
                            {
                                sshClient.CreatenExecute(string.Concat(new string[]
                            {
                            "rm -rf /private/var/mobile/Containers/Data/Application/",
                            arrResuftNop[7],
                            "/",
                            arrResuftNop[8],
                            "/",
                            arrResuftNop[9],
                            "/",
                            arrResuftNop[10],
                            "/Cache.db-wal"
                             }));
                                sshClient.CreatenExecute(string.Concat(new string[]
                            {
                            "rm -rf /private/var/mobile/Containers/Data/Application/",
                            arrResuftNop[7],
                            "/",
                            arrResuftNop[8],
                            "/",
                            arrResuftNop[9],
                            "/",
                            arrResuftNop[10],
                            "/Cache.db-shm"
                            }));
                                sshClient.CreatenExecute(string.Concat(new string[]
                            {
                            "rm -rf /private/var/mobile/Containers/Data/Application/",
                            arrResuftNop[7],
                            "/",
                            arrResuftNop[8],
                            "/",
                            arrResuftNop[9],
                            "/",
                            arrResuftNop[10],
                            "/fsCachedData"
                             }));
                            }
                        }
                    }
                    sshClient.Disconnect();
                }
            }
            catch(Exception ex) { Console.WriteLine(ex); }
        }

        //ADD Proxy--------------

        public void Enable(bool bl,string ip, string port)
        {
            if (bl)
            {
               runcmd("iProxyManager "+ip + " " + port + " " +"1");
            }
            else
            {
                runcmd("iProxyManager " + ip + " " + port + " " + "0");
            }
        }


        //Open App
        public void OpenApp(string strnameapp)
        {
            runcmd("open " + strnameapp);
        }

        //Reset Iphone 
        public void ResetIP()
        {
            runcmd("killall -9 SpringBoard");
        }

        //KillApp
        public void KillAll()
        {
            try
            {
                ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
            {
            new PasswordAuthenticationMethod("root", "alpine")
            });
                using (SshClient sshClient = new SshClient(connectionInfo))
                {
                    sshClient.Connect();
                    sshClient.CreatenExecute("killall -9 AppStore");
                    sshClient.CreatenExecute("killall -9 MobileSafari");
                    Thread.Sleep(200);
                    string strResuftCommand;
                    strResuftCommand = sshClient.CreatenExecute("find /private/var/mobile/Containers/Bundle/Application -name '*.app'");
                    string[] arrResuftCommand = StringFunctions.ArrSplitOpTion(strResuftCommand, "\n");
                    for (int i = 0; i < arrResuftCommand.Count<string>(); i++)
                    {
                        arrResuftCommand[i] = arrResuftCommand[i].Replace(" ", "\\ ");
                        string[] arrResuftCommand2 = StringFunctions.ArrSplitOpTion(arrResuftCommand[i], "/");
                        for (int j = 0; j < arrResuftCommand2.Count<string>(); j++)
                        {
                            if (arrResuftCommand2[j] != "" && arrResuftCommand2[j].IndexOf("app") > 0)
                            {
                                string strResuftCommand2 = arrResuftCommand2[j].Replace(".app", "");
                                sshClient.CreatenExecute("killall -9 " + strResuftCommand2);
                            }
                        }
                    }
                    strResuftCommand = sshClient.CreatenExecute("find /private/var/*/Bundle/Application -name '*.app'");
                    arrResuftCommand = StringFunctions.ArrSplitOpTion(strResuftCommand, "\n");
                    for (int k = 0; k < arrResuftCommand.Count<string>(); k++)
                    {
                        arrResuftCommand[k] = arrResuftCommand[k].Replace(" ", "\\ ");
                        string[] arrResuftCommand3 = StringFunctions.ArrSplitOpTion(arrResuftCommand[k], "/");
                        for (int l = 0; l < arrResuftCommand3.Count<string>(); l++)
                        {
                            if (arrResuftCommand3[l] != "" && arrResuftCommand3[l].IndexOf("app") > 0)
                            {
                                string strResuftCommand2 = arrResuftCommand3[l].Replace(".app", "");
                                sshClient.CreatenExecute("killall -9 " + strResuftCommand2);
                            }
                        }
                    }
               
                    sshClient.Disconnect();
                    randominfo();

                    UploadFile("/var/mobile/Library/AutoTouch/Scripts/", iStatic.diraidIphone+"/info.txt");
                    runcmd("activator send libactivator.system.homebutton");
                    runcmd("activator send libactivator.system.clear-switcher");
                    UploadFile("/var/mobile/Library/AutoTouch/Scripts/", iStatic.diraidIphone + "openlink.lua");


                }
            }
            catch { }
        }
        public void randominfo()
        {
            try
            {
                ThuVienDll.FuncFolder file = new ThuVienDll.FuncFolder();
                if (File.Exists(iStatic.diraidIphone+"/info.txt"))
                {
                    File.Delete(iStatic.diraidIphone+"/info.txt");
                }
                string[] firstName = file.readData("./quy/FirstName.txt");
                string[] lastName = file.readData("./quy/LastName.txt");
                string[] Address = file.readData("./quy/Address.txt");
                string[] Phone = file.readData("./quy/Phone.txt");
                string[] Mail = file.readData("./quy/Mail.txt");
                string[] city = file.readData("./quy/City.txt");
                string citystate = city[random(0, city.Length - 2)];
                string[] arrcitystate = city[random(0, city.Length - 2)].Split('|');
                file.wireData(firstName[random(0, firstName.Length - 2)], iStatic.diraidIphone+"/info.txt");
                file.wireData(lastName[random(0, lastName.Length - 2)], iStatic.diraidIphone+"/info.txt");
                file.wireData(Address[random(0, Address.Length - 2)], iStatic.diraidIphone+"/info.txt");
                file.wireData(Mail[random(0, Mail.Length - 2)], iStatic.diraidIphone+"/info.txt");
                file.wireData(Phone[random(0, Phone.Length - 2)], iStatic.diraidIphone+"/info.txt");
                file.wireData(arrcitystate[0], iStatic.diraidIphone+"/info.txt");
                file.wireData(arrcitystate[1], iStatic.diraidIphone+"/info.txt");
                file.wireData(arrcitystate[2], iStatic.diraidIphone+"/info.txt");
            }
            catch
            {

            }
        }


        public string runcmd(string cmd)
        {
            try
            {
                ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
              {
                    new PasswordAuthenticationMethod("root", "alpine")
              });
                using (SshClient client = new SshClient(connectionInfo))
                {
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(4);
                    client.Connect();

                    //        Console.WriteLine(output);
                    return client.CreatenExecute(cmd);

                }
            }
            catch (Exception ex)
            {
                ErrorSaveLog(ex.Message);
                return "";
            }
        }
        public bool checkconnect()
        {
            try
            {
                ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
            {
                  new PasswordAuthenticationMethod("root", "alpine")
            });
                using (SshClient client = new SshClient(connectionInfo))
                {
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(2);
                    client.Connect();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        // connect ip get 

        //Get Applist
        public string[] GetAppList()
        {
            string[] arrapplist = null;
            try
            {
     
                string Applist = getData("getapplist").Replace("\"", "").Replace(";","");
                arrapplist=StringFunctions.ArrSplitOpTion(Applist, "\n");
                return arrapplist;
            }
            catch
            {
                return arrapplist;
            }
        }
        public string getaid()
        {
            if (checkconnect())
            {
                string str = string.Empty;
                OpenApp(app9Logo);
                Thread.Sleep(1000);
                string seri = getData("getserial");
                if (seri != string.Empty)
                {
                    return seri;
                }
            }
            return string.Empty;
        }
        //wipe App

        private void ClearDataApp(string Application)
        {
           
            try
            {
                ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
            {
            new PasswordAuthenticationMethod("root", "alpine")
            });
                using (SshClient sshClient = new SshClient(connectionInfo))
                {
                    sshClient.Connect();
                    string nameapp = getPackerapp(Application);
                    string bundle=getbundle(nameapp);
                   
                    if (bundle != "")
                    {


                        sshClient.CreatenExecute("killall -9 "+ nameapp);
                        sshClient.CreatenExecute("rm -rf " + bundle + "/tmp/*");
                        sshClient.CreatenExecute("rm -rf " + bundle + "/Library/*");
                        sshClient.CreatenExecute("rm -rf " + bundle + "/Documents/*");
                        sshClient.CreatenExecute("chmod 777 " + bundle + "/tmp/*");
                        sshClient.CreatenExecute("chmod 777 " + bundle + "Library/*");
                        sshClient.CreatenExecute("chmod 777 " + bundle + "/Documents/*");
                    }
                    sshClient.Disconnect();
                }
            }
            catch { }
        }
        public string getPackerapp(string Application)
        {
            string nameapp = "";
            foreach (string str in GetAppList())
            {
                
                string[] arrnameapp = str.Split('=');
                if (arrnameapp.Length - 1 > 0)
                {
                   
                    if (arrnameapp[1].Trim().ToLower().Equals(Application.ToLower().Trim()))
                    {
                       
                        nameapp = arrnameapp[0].Trim();
                        return nameapp;
                    }


                }
            }
            return nameapp;
        }
        public bool WipeApp(string Application,bool bl)
        {
            try
            {
                
                ClearSafari();
                string nameapp= getPackerapp(Application);
         
                string[] arrApplication = nameapp.Split('.');
                if (arrApplication.Length - 1 > 1)
                {
                    string d = "sqlite3 /var/Keychains/keychain-2.db \"delete from genp where agrp like '%" + arrApplication[0] + "." + arrApplication[1] + "%';\"";
                    runcmd(d);
                }
                else
                {
                    return false;
                }
                if (bl)
                {
                    ClearDataApp(Application);
                }
                else
                {
                    runcmd("ipainstaller -u " + Application.Trim());
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
        public bool isnotKeychains()
        {
            try
            {
                if (!isDirAndFileApp("/var/Data", "Keychains"))
                {
                    ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
                    {
                       new PasswordAuthenticationMethod("root", "alpine")
                    });
                    using (SshClient sshClient = new SshClient(connectionInfo))
                    {
                        sshClient.Connect();
                        sshClient.CreatenExecute("mkdir /private/var/Data");
                        sshClient.CreatenExecute("chmod 777 /private/var/Data");
                        sshClient.CreatenExecute("mkdir /private/var/Data/Keychains");
                        sshClient.CreatenExecute("chmod 777 /private/var/Data/Keychains");
                        runcmd("cp -rf  /var/Keychains/ /var/Data/");
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public void resetKeyChains()
        {
            try
            {
                if (!isnotKeychains())
                {

                    runcmd("yes | cp -rf /var/Data/Keychains/ /var/");
                }
            }
            catch { }
        }
        //Change Device
        public string getData(string parameter)
        {
            string Out = String.Empty;
            try
            {
              
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://"+ip+":6969/"+ parameter);
                request.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (System.IO.Stream stream = response.GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(stream))
                    {
                        Out = sr.ReadToEnd();
                        sr.Close();
                    }
                }
                request.Abort();
                response.Close();
                
                return Out;
            }
            catch (Exception ex)
            {
                ErrorSaveLog(ex.Message);
                return Out;
            }

        }
        public void writeFileProxy(string bl)
        {
            if (!Directory.Exists("quy"))
            {
                Directory.CreateDirectory("quy");
            }
            if (File.Exists("./quy/com.linusyang.MobileShadowSocks.plist"))
            {
                File.Delete("./quy/com.linusyang.MobileShadowSocks.plist");
            }
            string pathstr = "./quy/com.linusyang.MobileShadowSocks.plist";
            tvFile = new ThuVienDll.FuncFolder();
            tvFile.wireData("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", pathstr);
            tvFile.wireData("<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">", pathstr);
            tvFile.wireData("<plist version=\"1.0\">", pathstr);
            tvFile.wireData("  <dict>", pathstr);
            tvFile.wireData("    <key>AUTO_PROXY</key>", pathstr);
            tvFile.wireData("    <"+ bl + "/>", pathstr);
            tvFile.wireData("    <key>PAC_FILE</key>", pathstr);
            tvFile.wireData("    <string>/Applications/MobileShadowSocks.app/auto.pac</string>", pathstr);
            tvFile.wireData("    <key>PROFILE_LIST</key>", pathstr);
            tvFile.wireData("    <array>", pathstr);
            tvFile.wireData("    <dict>", pathstr);
            tvFile.wireData("    <key>AUTO_PROXY</key>", pathstr);
            tvFile.wireData("  <true/>", pathstr);
            tvFile.wireData("    <key>PAC_FILE</key>", pathstr);
            tvFile.wireData("    <string>/Applications/MobileShadowSocks.app/auto.pac</string>", pathstr);
            tvFile.wireData("    </dict>", pathstr);
            tvFile.wireData("    </array>", pathstr);
            tvFile.wireData("    <key>PROXY_ENABLED</key>", pathstr);
            tvFile.wireData("    <" + bl + "/>", pathstr);
            tvFile.wireData("    <key>SELECTED_PROFILE</key>", pathstr);
            tvFile.wireData("    <integer>-1</integer>", pathstr);
            tvFile.wireData("    </dict>", pathstr);
            tvFile.wireData("    </plist>", pathstr);
        }
        public void writeFileplit(string useragent)
        {
            if (!Directory.Exists("quy"))
            {
                Directory.CreateDirectory("quy");
            }
            if (File.Exists("./quy/com.rbt.userAgentChanger.plist"))
            {
                File.Delete("./quy/com.rbt.userAgentChanger.plist");
            }
            string pathstr = "./quy/com.rbt.userAgentChanger.plist";
            tvFile = new ThuVienDll.FuncFolder();
            tvFile.wireData("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", pathstr);
            tvFile.wireData("<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">", pathstr);
            tvFile.wireData("<plist version=\"1.0\">", pathstr);
            tvFile.wireData("  <dict>", pathstr);
            tvFile.wireData("    <key>UACUserAgent</key>", pathstr);
            tvFile.wireData("    <integer>-1</integer>", pathstr);
            tvFile.wireData("    <key>customuseragent</key>", pathstr);
            tvFile.wireData("    <string>" + useragent + "</string>", pathstr);
            tvFile.wireData("    <key>enabled</key>", pathstr);
            tvFile.wireData("    <true/>", pathstr);
            tvFile.wireData("    <key>settingsCopied</key>", pathstr);
            tvFile.wireData("    <string>1</string>", pathstr);
            tvFile.wireData("  </dict>", pathstr);
            tvFile.wireData("</plist>", pathstr);
        }
        public void writeFileplitlocation(string latitude, string longitude)
        {
            if (!Directory.Exists("./quy"))
            {
                Directory.CreateDirectory("quy");
            }
            if (File.Exists("./quy/com.cunstuck.locationchanger.plist"))
            {
                File.Delete("./quy/com.cunstuck.locationchanger.plist");
            }
            string pathstr = "./quy/com.cunstuck.locationchanger.plist";
            tvFile = new ThuVienDll.FuncFolder();
            tvFile.wireData("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", pathstr);
            tvFile.wireData("<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">", pathstr);
            tvFile.wireData("<plist version=\"1.0\">", pathstr);
            tvFile.wireData("  <dict>", pathstr);
            tvFile.wireData("	<key>a</key>", pathstr);
            tvFile.wireData("	<string>1</string>", pathstr);
            tvFile.wireData("	<key>enable</key>", pathstr);
            tvFile.wireData("	<string>1</string>", pathstr);
            tvFile.wireData("	<key>key</key>", pathstr);
            tvFile.wireData("	<string>8638f8e5</string>", pathstr);
            tvFile.wireData("	<key>latitude</key>", pathstr);
            tvFile.wireData("	<string>" + latitude + "</string>", pathstr);
            tvFile.wireData("	<key>longitude</key>", pathstr);
            tvFile.wireData("	<string>" + longitude + "</string>", pathstr);
            tvFile.wireData("	<key>s</key>", pathstr);
            tvFile.wireData("	<string>0.644040</string>", pathstr);
            tvFile.wireData("  </dict>", pathstr);
            tvFile.wireData("</plist>", pathstr);
        }

        public string ChangeDevice()
        {
            string Long = "";
            string Lat = "";
            string UserAgent = "";
            string datajson = getData("getdata");
            if (datajson != string.Empty)
            {
                dynamic data = JObject.Parse(datajson);
                Long = data.Device.Longitude;
                Lat = data.Device.Latitude;
                UserAgent = data.Device.UserAgent;
            }
            else
            {
                return datajson;
            }
            //if (Long != "" && Lat != "")
            //{
            //    writeFileplitlocation(Long + "" + random(10, 99), Lat + "" + random(10, 99));
            //    Thread.Sleep(1000);
            //    UploadFile("/private/var/mobile/Library/Preferences/", "./quy/com.cunstuck.locationchanger.plist");
            //    Console.WriteLine("UP Location DONE");
            //}
            //if (UserAgent != "")
            //{
            //    writeFileplit(UserAgent);
            //    Thread.Sleep(1000);
            //    UploadFile("/private/var/mobile/Library/Preferences/", "./quy/com.rbt.userAgentChanger.plist");
            //    Console.WriteLine("UP UserAgent DONE");
            //}

            return datajson;
        }


        //public bool ChangeDevice(string proxy, string geo)
        //{
        //    tvFile = new ThuVienDll.FuncFolder();
        //    try
        //    {
        //        if (proxy.Split(':').Length - 1 > 0)
        //        {
        //            string checkip = checkLocationIP(proxy);
        //            string[] xylocation = null;
        //            if (checkip != String.Empty)
        //            {
        //                xylocation = checkip.Split(new string[] { "\"latitude\":", ",\"longitude\":", ",\"metro_code\"" }, StringSplitOptions.None);
        //            }
        //            if (xylocation != null)
        //            {
        //                writeFileplitlocation(xylocation[1] + "" + random(10, 99), xylocation[2] + "" + random(10, 99));
        //                UploadFile("/private/var/mobile/Library/Preferences/", "./quy/com.cunstuck.locationchanger.plist");
        //                Console.WriteLine("UP Location DONE");
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //        if (!Directory.Exists("./quy/" + ip))
        //        {
        //            Directory.CreateDirectory("./quy/" + ip);
        //        }
        //        if (File.Exists("./quy/" + ip + "/data.json"))
        //        {
        //            UploadFile("/private/var/mobile/Library/Preferences/", "./quy/" + ip + "/data.json");
        //            Console.WriteLine("Json Done");
        //        }
        //        else
        //        {
        //            DownFile("/private/var/mobile/Library/Preferences/", "data.json", "./quy/" + ip);
        //            Console.WriteLine("down Json Done");
        //        }
        //        runcmd("open 'com.sontung.SystemAppInfo'");
        //        if (ThuVienDll.RequestServer.HTTP_GET("http://" + ip + ":9090/getUserID?countrycode=" + geo.ToLower(), "") != "")
        //        {

        //            DownFile("/private/var/mobile/Library/Preferences/", "data.json", "./quy/" + ip);
        //            Console.WriteLine("Down Json Done");
        //            Thread.Sleep(1000);
        //            json js = new json();
        //            string vs = js.jsToRRS("./quy/" + ip);

        //            string[] arrvsos = vs.Split('.');

        //            string useragent = "";
        //            if (File.Exists("./UserAgent/" + vs + ".txt"))
        //            {

        //                useragent = tvFile.readData("./UserAgent/" + vs + ".txt")[random(0, (tvFile.readData("./UserAgent/" + vs + ".txt").Length - 2))];
        //            }
        //            else
        //            {
        //                if (File.Exists("./UserAgent/" + arrvsos[0] + "." + arrvsos[1] + ".txt"))
        //                {
        //                    useragent = tvFile.readData("./UserAgent/" + arrvsos[0] + "." + arrvsos[1] + ".txt")[random(0, (tvFile.readData("./UserAgent/" + arrvsos[0] + "." + arrvsos[1] + ".txt").Length - 2))];
        //                }
        //                else
        //                {
        //                    if (File.Exists("./UserAgent/" + arrvsos[0] + ".0" + ".txt"))
        //                    {
        //                        useragent = tvFile.readData("./UserAgent/" + arrvsos[0] + ".0" + ".txt")[random(0, (tvFile.readData("./UserAgent/" + arrvsos[0] + ".0" + ".txt").Length - 2))];
        //                    }
        //                }
        //            }
        //            writeFileplit(useragent.Substring(0, useragent.Length - 3));
        //            Thread.Sleep(1000);

        //            UploadFile("/private/var/mobile/Library/Preferences/", "./quy/com.rbt.userAgentChanger.plist");
        //            Thread.Sleep(1500);
        //            return true;
        //        }
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorSaveLog(ex.Message);
        //        return false;
        //    }
        //}
        //Open url
        public bool openlink(string url)
        {
            try
            {
                ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
                {
                new PasswordAuthenticationMethod("root", "alpine")
                });
                using (SshClient sshClient = new SshClient(connectionInfo))
                {
                    sshClient.Connect();
                    sshClient.CreatenExecute("uiopen '" + url.Trim() + "'");
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        //Install App---------
        public bool installApp(string Application)
        {

            try
            {
                if (runcmd("ipainstaller -c /private/var/Data/" + Application + ".ipa").Contains("success"))
                {
                    return true;
                }
                return false;
            }
            catch
            {

                return false;
            }
        }
        public int checkinstallApp(string Application)
        {
            if (!isDirAndFileApp("/private/var/Data", Application + ".ipa"))
            {
                try
                {
                    ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
                    {
                      new PasswordAuthenticationMethod("root", "alpine")
                    });

                    using (SshClient sshClient = new SshClient(connectionInfo))
                    {
                        sshClient.Connect();
                        sshClient.CreatenExecute("mkdir /private/var/Data");
                        sshClient.CreatenExecute("chmod 777 /private/var/Data");
                        runcmd(string.Concat(new string[]
                        {
                        "ipainstaller -b ",
                        Application,
                        " -o /private/var/Data/",
                        Application,
                        ".ipa"
                        }));
                        sshClient.CreatenExecute("chmod 777 /private/var/Data/" + Application + ".ipa");
                    }
                    return 0;
                }
                catch
                {
                    return 1;
                }
            }
            else
            {
                return 2;
            }
        }

        public string getbundle(string Application)
        {
            return getData("bundle?id=" + Application);
        }

        //Backup App
        public bool backUpApp(string Application,string country ,string Date)
        {
            try
            {
                string nameapp = getPackerapp(Application);
                string bundleApp = getbundle(nameapp);
                string applist = Application;
               
                if (bundleApp != string.Empty)
                {
                    ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
                {
                    new PasswordAuthenticationMethod("root", "alpine")
                });
                    using (SshClient sshClient = new SshClient(connectionInfo))
                    {
                        sshClient.Connect();

                        sshClient.CreatenExecute("mkdir /private/var/root/backup");
                  
                        sshClient.CreatenExecute("chmod 777 /private/var/root/backup");
                        sshClient.CreatenExecute("mkdir '/private/var/root/backup/" + applist + "_"+ country + Date+"'");
                        sshClient.CreatenExecute("chmod 777 '/private/var/root/backup/" + applist + "_" + country + Date+"'");                     
                        string stt = sshClient.CreatenExecute("sqlite3 ex1 \"ATTACH DATABASE '/var/root/backup/" + applist + "_" + country + Date + "/keychain' As 'quy';ATTACH DATABASE '/var/Keychains/keychain-2.db' As 'quy1';CREATE TABLE quy.quydaica as Select * from quy1.genp where agrp like '%" + nameapp.Split('.')[0] + "." + nameapp.Split('.')[1] + "%';\"");
                        // runcmd("sqlite3 ex1 \"ATTACH DATABASE '/var/root/backup/" + Application + Date + "/keychain' As 'quy';ATTACH DATABASE '/var/Keychains/keychain-2.db' As 'quy1';CREATE TABLE quy.quydaica as Select * from quy1.genp where agrp like '%QKNZB3E3BF%';\"");
                        sshClient.CreatenExecute("cp -rf /private/var/mobile/Library/Cookies '/private/var/root/backup/" + applist + Date + "/Cookies'");
                        sshClient.CreatenExecute("cp -rf /private/var/mobile/Library/Preferences/data.json " + "'/private/var/root/backup/" + applist + "_" + country + Date+"'");
                        sshClient.CreatenExecute("cp -rf /private/var/mobile/Library/Safari '/private/var/root/backup/" + applist + "_" + country + Date + "/Safari'");
                        sshClient.CreatenExecute("mkdir '/private/var/root/backup/" + applist + "_" + country + Date + "/" + applist+"'");
                        sshClient.CreatenExecute("chmod 777 '/private/var/root/backup/" + applist + "_" + country + Date + "/" + applist+"'");
                        copy(bundleApp, "/var/root/backup/" + applist + "_" + country + Date + "/" + applist);
                        sshClient.CreatenExecute("cd /var/root/backup/;zip -r '" + applist + "_" + country + Date + ".zip' '" + applist + "_" + country + Date+"'");
                        sshClient.CreatenExecute("rm -rf '/var/root/backup/" + applist + "_" + country + Date+"'");
                        sshClient.Disconnect();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorSaveLog(ex.Message);
                return false;
            }
        }



        //Restore app
        public bool RestoreApp(string dirApplication, string app)
        {
            string ApplicationRRS = dirApplication.Split(new string[] { ".zip" }, StringSplitOptions.None)[0];
            deleteFile("/var/root/backup", ApplicationRRS);
            try
            {
                
                ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
                {
                    new PasswordAuthenticationMethod("root", "alpine")
                });
                using (SshClient sshClient = new SshClient(connectionInfo))
                {
                    sshClient.Connect();
  
                    string nameapp = getPackerapp(app);
                 
                    string bundleApp = getbundle(nameapp);
                    
                    if (bundleApp != string.Empty)
                    {
                    
                 
                        sshClient.CreatenExecute("unzip /var/root/backup/" + ApplicationRRS + ".zip -d /var/root/backup/");
                

                        sshClient.CreatenExecute("sqlite3 ex2 \"ATTACH DATABASE '/private/var/root/backup/" + ApplicationRRS + "/keychain' As 'quy';ATTACH DATABASE '/var/Keychains/keychain-2.db' As 'quy1';Delete from quy1.genp where agrp like '%" + nameapp.Split('.')[0] + "." + nameapp.Split('.')[1] + "%';Insert into quy1.genp  Select * from quy.quydaica where agrp like '%" + nameapp.Split('.')[0] + "." + nameapp.Split('.')[1] + "%';\"");
                        sshClient.CreatenExecute("yes | cp -rf '/private/var/root/backup/" + ApplicationRRS + "/data.json' /private/var/mobile/Library/Preferences");
                        sshClient.CreatenExecute("yes | cp -rf  '/private/var/root/backup/" + ApplicationRRS + "/Cookies/*' /private/var/mobile/Library/");
                        sshClient.CreatenExecute("yes | cp -rf '/private/var/root/backup/" + ApplicationRRS + "/Safari/*' /private/var/mobile/Library/");
                        ClearDataApp(app.Trim());
                       
                        sshClient.CreatenExecute("rsync -avze ssh '/private/var/root/backup/" + ApplicationRRS + "/" + app.Trim() + "/Library' " + bundleApp);
                        sshClient.CreatenExecute("rsync -avze ssh '/private/var/root/backup/" + ApplicationRRS + "/" + app.Trim() + "/Documents' " + bundleApp);
                        sshClient.CreatenExecute("chown -HR mobile:mobile " + bundleApp);
                       
                        if (isDirAndFileApp("'/var/root/backup/", ApplicationRRS+"'"))
                        {
                        
                            sshClient.CreatenExecute("rm -rf '/var/root/backup/" + ApplicationRRS+"'");
                        }
                  
                    }
                    else
                    {
                        return false;
                    }
                   
                    sshClient.Disconnect();
                }
                return true;
            }
            catch (Exception ex)
            {
                deleteFileLuu(ApplicationRRS);
                ErrorSaveLog(ex.ToString());
                return false;
            }
        }
        public void deleteFileLuu(string ApplicationRRS)
        {
            if (isDirAndFileApp("/var/root/backup", ApplicationRRS))
            {
                runcmd("rm -rf /var/root/backup/" + ApplicationRRS);
            }
        }





















































        //public bool backUpApp(string Application, string Date)
        //{
        //    try
        //    {

        //        if (getbundle(Application) != string.Empty)
        //        {
        //            ConnectionInfo connectionInfo = new ConnectionInfo(ip, 22, "root", new AuthenticationMethod[]
        //        {
        //            new PasswordAuthenticationMethod("root", "alpine")
        //        });
        //            using (SshClient sshClient = new SshClient(connectionInfo))
        //            {
        //                sshClient.Connect();
        //                //  string Null;
        //                //Null = sshClient.CreatenExecute("find /private/var/mobile/Containers/Data/Application/*/ -name '" + Application + "'");
        //                //string[] arrResuftCommand = StringFunctions.ArrSplitOpTion(Null, "\n");
        //                //if (arrResuftCommand.Count<string>() <= 0 || !(arrResuftCommand[0] != ""))
        //                //{
        //                //}
        //                sshClient.CreatenExecute("mkdir /private/var/root/backup");
        //                sshClient.CreatenExecute("chmod 777 /private/var/root/backup");
        //                sshClient.CreatenExecute("mkdir /private/var/root/backup/" + Application + Date);
        //                sshClient.CreatenExecute("chmod 777 /private/var/root/backup/" + Application + Date);
        //                //   string[] arrResuftCommand2 = StringFunctions.ArrSplitOpTion(arrResuftCommand[0], "Library");
        //                string strResuftCommand = getdirapp(Application, false);
        //                string stt = sshClient.CreatenExecute("sqlite3 ex1 \"ATTACH DATABASE '/var/root/backup/" + Application + Date + "/keychain' As 'quy';ATTACH DATABASE '/var/Keychains/keychain-2.db' As 'quy1';CREATE TABLE quy.quydaica as Select * from quy1.genp where agrp like '%" + Application.Split('.')[0] + "." + Application.Split('.')[1] + "%';\"");
        //                // runcmd("sqlite3 ex1 \"ATTACH DATABASE '/var/root/backup/" + Application + Date + "/keychain' As 'quy';ATTACH DATABASE '/var/Keychains/keychain-2.db' As 'quy1';CREATE TABLE quy.quydaica as Select * from quy1.genp where agrp like '%QKNZB3E3BF%';\"");
        //                // sshClient.CreatenExecute("cp -rf /private/var/mobile/Library/Cookies /private/var/root/backup/" + Application + Date + "/Cookies");
        //                sshClient.CreatenExecute("cp -rf /private/var/mobile/Library/Preferences/data.json /private/var/root/backup/" + Application + Date);
        //                sshClient.CreatenExecute("cp -rf /private/var/mobile/Library/Safari /private/var/root/backup/" + Application + Date + "/Safari");
        //                string dirappbundle = getdirapp(Application, true);
        //                sshClient.CreatenExecute("cp -rf " + dirappbundle + "/iTunesArtwork /var/root/backup/" + Application + Date);
        //                sshClient.CreatenExecute("cp -rf " + dirappbundle + "/iTunesMetadata.plist /var/root/backup/" + Application + Date);
        //                sshClient.CreatenExecute("mkdir /private/var/root/backup/" + Application + Date + "/" + Application);
        //                sshClient.CreatenExecute("chmod 777 /private/var/root/backup/" + Application + Date + "/" + Application);
        //                copy(strResuftCommand, "/var/root/backup/" + Application + Date + "/" + Application);
        //                sshClient.CreatenExecute("cd /var/root/backup/;zip -r " + Application + Date + ".zip " + Application + Date);
        //                sshClient.CreatenExecute("rm -rf /var/root/backup/" + Application + Date);
        //                sshClient.Disconnect();
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorSaveLog(ex.Message);
        //        return false;
        //    }
        //}

    }
}
