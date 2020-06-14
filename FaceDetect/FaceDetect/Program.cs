using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Diagnostics;

namespace FaceDetect
{
    class Program
    {
        private static string msg = "Please provide the API key as first argument and the filename as the second argument";
        static void Main(string[] args)
        {
            //Process CMD args
            var apiKey = !string.IsNullOrWhiteSpace(args[0]) ? args[0] : throw new ArgumentException(msg, args[0]);
            var fileName = File.Exists(args[1]) ? args[1] : throw new FileNotFoundException(msg, args[1]);
            //Make request
            var region = "eastus";
            var target = new Uri($"https://{region}.api.cognitive.microsoft.com/face/v1.0/detect/?subscription-key={apiKey}");
            var httpPost = CreateHttpRequest(target, "POST", "application/octet-stream");
            //Load image
            using (var fs = File.OpenRead(fileName))
            {
                fs.CopyTo(httpPost.GetRequestStream());
            }
            //Sumbit image to HTTP endpoint
            string data = getResponse(httpPost);
            //Inspect the json
            var rectangles = getRectangles(data);
            //Draw rectangles on the image
            var img = Image.Load(fileName);

            var count = 0;
            foreach(var rectangle in getRectangles(data))
            {
                img.Mutate(a => a.DrawPolygon(Color.Aquamarine, 5, rectangle));
                count++;
            }

            Console.WriteLine($"Numer of faces detected: {count}");

            var outputFileName = $"{Environment.CurrentDirectory}\\{Path.GetFileNameWithoutExtension(fileName)}-out{Path.GetExtension(fileName)}";
            SaveImage(img, outputFileName);

            OpenWithDefaultApp(outputFileName);
        }

        private static void OpenWithDefaultApp(string fileName)
        {
            var si = new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = fileName,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(si);
        }

        private static void SaveImage(Image img, string outputFileName)
        {
            using (var fs = File.Create(outputFileName))
            {
                img.SaveAsJpeg(fs);
            }
        }

        private static IEnumerable<PointF[]> getRectangles(string data)
        {
            var faces = JArray.Parse(data);
            foreach (var face in faces)
            {
                var id = (string)face["faceId"];
                var top = (int)face["faceRectangle"]["top"];
                var width = (int)face["faceRectangle"]["width"];
                var left = (int)face["faceRectangle"]["left"];
                var height = (int)face["faceRectangle"]["height"];

                var rectangle = new PointF[]
                {
                    new PointF(left, top),
                    new PointF(left + width, top),
                    new PointF(left + width, top + height),
                    new PointF(left, top + height)
                };

                yield return rectangle;
            }
        }

        private static string getResponse(HttpWebRequest httpPost)
        {
            using (var response = httpPost.GetResponse())
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }

        private static HttpWebRequest CreateHttpRequest(Uri target, string method, string contentType)
        {
            var request = WebRequest.CreateHttp(target);
            request.Method = method;
            request.ContentType = contentType;
            return request;
        }
    }
}
