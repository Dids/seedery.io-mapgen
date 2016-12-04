using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace SeederyIo
{
    class Program
    {
        static void Main(string[] args)
        {
            WebServer webServer = new WebServer();
            webServer?.Start();
            Console.WriteLine("Web server is running. Press any key to quit.");
            Console.ReadKey();
            webServer?.Stop();
        }
    }
}
