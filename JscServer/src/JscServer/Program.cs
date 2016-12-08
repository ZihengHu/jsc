using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace JscServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var isDebugging = Debugger.IsAttached || args.Contains("--debug");

            string currentDirectory = isDebugging ? Directory.GetCurrentDirectory() : Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            var config = new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    var sslSection = config.GetSection("ssl");
                    if (sslSection.GetValue<bool>("enable"))
                    {
                        options.UseHttps(
                            sslSection.GetValue<string>("file"),
                            sslSection.GetValue<string>("password"));
                    }
                })
                .UseConfiguration(config)
                .UseContentRoot(currentDirectory)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            if (isDebugging)
            {
                host.Run();
            }
            else
            {
                host.RunAsService();
        }
    }
    }
}