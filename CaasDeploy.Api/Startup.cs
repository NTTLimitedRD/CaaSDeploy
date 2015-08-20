using Hangfire;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(CaasDeploy.Api.Startup))]

namespace CaasDeploy.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
		    GlobalConfiguration.Configuration
			    .UseSqlServerStorage("HangFireDB");

			app.UseHangfireDashboard();
			app.UseHangfireServer();
        }
    }
}
