using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.WebHost;

namespace CrazyReciteApi
{

    public class NoBufferPolicySelector : WebHostBufferPolicySelector
    {
        public override bool UseBufferedInputStream(object hostContext)
        {
            var context = hostContext as HttpContextBase;

            if (context != null)
            {
                if (context.Request.HttpMethod == HttpMethod.Post.ToString() && context.Request.ContentLength > 200000)
                    return false;
            }

            return true;
        }
    }
}