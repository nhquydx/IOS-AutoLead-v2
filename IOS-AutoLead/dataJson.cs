using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IOS_AutoLead
{
    public class dataJson
    {
        ThuVienDll.RequestServer request = new ThuVienDll.RequestServer();
        static string latitude = "";
        static string longitude = "";
        static string time_zone = "";
        public static string GetJson()
        {
           string str= ThuVienDll.RequestServer.HTTP_GET(@"http://no1affiliate.net/index.php?Countrycode="+ Getneworkname(), "");
           dynamic data = JObject.Parse(str);
           data.Device.Add("Latitude", latitude);
           data.Device.Add("Longitude", longitude);
           data.Device.Add("TimeZone", time_zone);
           dynamic parsedJson = JsonConvert.DeserializeObject(data.ToString());
           return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
        public static string Getneworkname()
        {
            string Out = String.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://freegeoip.net/json/");
                WebProxy myproxy = new WebProxy("163.172.110.217:2194", false);
                myproxy.BypassProxyOnLocal = false;
                request.Proxy = myproxy;
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
                string source = Out;
                dynamic data = JObject.Parse(source);
                latitude = data.latitude;
                longitude= data.longitude;
                time_zone = data.time_zone;
                return data.country_code;
            }
            catch 
            {
                 return Out;
            }
        }
    }
}
