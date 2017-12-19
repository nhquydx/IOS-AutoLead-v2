using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOS_LeadMobile
{
    public static class StringFunctions
    {

        public static string[] ChrSplitOpTion(string strValue, char chrValue)
        {
            string[] result;
            try
            {
                string[] array = strValue.Split(new char[]
            {
                chrValue
            });
                result = array;
            }
            catch
            {
                result = null;
            }
            return result;
        }

        public static string StrReplaceSpace(string replace)
        {
            return replace.Replace(" ", "");
        }

        public static string[] ArrSplitOpTion(string strValue, string strOpTion)
        {
            string[] result;
            try
            {
                string[] array = strValue.Split(new string[]
            {
                strOpTion
            }, StringSplitOptions.None);
                result = array;
            }
            catch
            {
                result = null;
            }
            return result;
        }
    }
}
