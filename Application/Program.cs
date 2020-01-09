#region

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

#endregion

namespace Application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseSetting(WebHostDefaults.DetailedErrorsKey, "true")
                .UseStartup<Startup>();
        }
    }
}