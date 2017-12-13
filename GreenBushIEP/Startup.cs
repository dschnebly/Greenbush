using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GreenBushIEP.Startup))]
namespace GreenBushIEP
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
