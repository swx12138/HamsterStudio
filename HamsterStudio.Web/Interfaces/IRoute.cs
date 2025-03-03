using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Web.Interfaces
{
    public interface IRoute
    {
        bool IsMyCake(string url);

        public void Response(HttpListenerRequest request, ref HttpListenerResponse response);
    }
}
