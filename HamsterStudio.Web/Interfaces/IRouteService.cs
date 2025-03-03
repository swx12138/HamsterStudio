using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Web.Interfaces
{
    public interface IRouteService
    {
        void RegisterRoute(IRoute route);

        bool Response(HttpListenerRequest request, ref HttpListenerResponse response);
    }
}
