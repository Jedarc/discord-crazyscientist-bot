using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace CrazyScientist.Bot
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
                .Build()
                .RunAsync();
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var startup = new Startup(hostContext.Configuration, hostContext.HostingEnvironment);
                    startup.ConfigureServices(services);
                });
    }
}
