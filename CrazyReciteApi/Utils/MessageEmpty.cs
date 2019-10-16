using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CrazyReciteApi.Utils
{
    public class MessageEmpty
    {
        static readonly Dictionary<ResultTypes,string> resultDic=new Dictionary<ResultTypes, string>();

        public static string GetResultMsg(ResultTypes code)
        {
            if (!resultDic.ContainsKey(code))
                return null;

            string message = resultDic[code];
            return message;
        }
    }
}