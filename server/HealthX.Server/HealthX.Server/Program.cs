using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HealthX.Server {

    public enum LoggingStatus {
        INFO,
        DEBUG,
        WARNING,
        ERROR
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

        public static void Log (LoggingStatus status, string task, string message) {
            string datetime = DateTime.Now.ToString();
            Console.WriteLine("[" + datetime + "][" + status.ToString() + "][" + task + "] " + message);
        }
    }
}
