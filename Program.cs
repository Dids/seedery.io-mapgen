using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SeederyIo
{
    class Program
    {
        static void Main(string[] args)
        {
            WebServer ws = new WebServer();
            ws.Start();
            Console.WriteLine("Web server is running. Press any key to quit.");
            Console.ReadKey();
            ws.Stop();
        }
    }
}
