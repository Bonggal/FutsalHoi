using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FutsalHoi.Startup))]
namespace FutsalHoi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
