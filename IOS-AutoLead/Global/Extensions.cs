using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOS_LeadMobile
{
    public static class Extensions
    {
        public static string Encode(this string text)
        {
            return text.Replace("(", "\\(").Replace(")", "\\)").Replace(" ", "\\ ");
        }
        public static string CreatenExecute(this SshClient client , string command)
        {
            string result = "";

            using (SshCommand sshCommand = client.CreateCommand(command))
            {
                sshCommand.Execute();
                result = sshCommand.Result;
            }
         
            return result;
        }
    }
}
