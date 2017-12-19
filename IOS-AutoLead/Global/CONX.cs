using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace Lead_IOS
{

    public static class IOS_AutoLead

    {

        public static string ReadHTMLCode(string url)
        {
            try
            {
                WebClient webClient = new WebClient();
                byte[] reqHTML = webClient.DownloadData(url);
                UTF8Encoding objUTF8 = new UTF8Encoding();
                return objUTF8.GetString(reqHTML);
            }
            catch
            {
                return null;
            }
        }


        public static string NetworkInterFaces()
        {
            string strInterFaces;
            try
            {
                NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                for (int i = 0; i < allNetworkInterfaces.Length; i++)
                {
                    NetworkInterface networkInterface = allNetworkInterfaces[i];
                    bool flag = networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet;
                    if (flag)
                    {
                        foreach (UnicastIPAddressInformation current in networkInterface.GetIPProperties().UnicastAddresses)
                        {
                            strInterFaces = networkInterface.Name;
                            return strInterFaces;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

    }
}
