using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(DataRelay.Startup1))]

namespace DataRelay
{
    public class Startup1
    {
        public void Configuration(IAppBuilder app)
        {
           
        }
    }
}
