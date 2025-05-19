using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Web
{
    public static class WebExtension
    {
        public static bool IsSuccess(this HttpStatusCode statusCode) => statusCode >= HttpStatusCode.OK && statusCode < HttpStatusCode.Ambiguous;
    }
}
