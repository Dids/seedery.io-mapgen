using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.Web;

namespace SeederyIo
{
    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();

        public WebServer()
        {
            if (!HttpListener.IsSupported) throw new NotSupportedException("Jeez, get a newer version of Windows!");
            _listener.Prefixes.Add("http://localhost:5000/upload/");
            _listener.Start();
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine("Ready to receive maps..");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            if (ctx == null) return;
                            try
                            {
                                Console.WriteLine("Receiving..");

                                // Parse the raw data first
                                string rawData;
                                using (var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                                {
                                    rawData = reader.ReadToEnd();
                                }

                                // Next translate the raw data to a more usable dictionary
                                Dictionary<string, string> postParams = new Dictionary<string, string>();
                                string[] rawParams = rawData.Split('&');
                                foreach (string param in rawParams)
                                {
                                    string[] kvPair = param.Split('=');
                                    string key = kvPair[0];
                                    string value = Uri.UnescapeDataString(kvPair[1]);
                                    postParams.Add(key, value);
                                }

                                // Get the individual fields
                                var protocol = postParams["protocol"];
                                var size = postParams["size"];
                                var seed = postParams["seed"];
                                var monuments = postParams["monuments"];
                                var filename = postParams["filename"];
                                var data = postParams["data"];

                                // Create the images directory if it doesn't exist yet
                                var imagesDirectory = Directory.GetCurrentDirectory() + "\\images";
                                if (!Directory.Exists(imagesDirectory)) Directory.CreateDirectory(imagesDirectory);

                                // Save the image
                                var imageFilePath = $"{imagesDirectory}\\{protocol}_{size}_{seed}.png";
                                using (FileStream imageStream = new FileStream(imageFilePath, FileMode.Create, FileAccess.Write))
                                {
                                    // The image data is 64 encoded, so decode it first
                                    byte[] bytes = Convert.FromBase64String(data);

                                    // Next just write the bytes and close the stream
                                    imageStream.Write(bytes, 0, bytes.Length);
                                    imageStream.Close();

                                    Console.WriteLine($"Map image saved to {imageFilePath}");
                                }

                                // Create the monuments directory if it doesn't exist yet
                                var monumentsDirectory = Directory.GetCurrentDirectory() + "\\monuments";
                                if (!Directory.Exists(monumentsDirectory)) Directory.CreateDirectory(monumentsDirectory);

                                // Save the monument data
                                var monumentsFilePath = $"{monumentsDirectory}\\{protocol}_{size}_{seed}.json";
                                using (FileStream monumentsStream = new FileStream(monumentsFilePath, FileMode.Create, FileAccess.Write))
                                {
                                    // The monument data is a JSON string, so convert it to a byte array
                                    byte[] bytes = GetBytes(monuments);

                                    // Next just write the bytes and close the stream
                                    monumentsStream.Write(bytes, 0, bytes.Length);
                                    monumentsStream.Close();

                                    Console.WriteLine($"Map monuments saved to {monumentsFilePath}");
                                }

                                // Send a response
                                ctx.Response.StatusCode = 200;
                                ctx.Response.ContentType = "text/html";
                                using (StreamWriter writer = new StreamWriter(ctx.Response.OutputStream, Encoding.UTF8)) writer.WriteLine("OK");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Exception occurred: " + e);
                            }
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception occurred: " + e);
                }
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}
