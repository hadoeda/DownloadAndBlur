using SuperfastBlur;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DownloadAndBlur
{
    class Program
    {
        static void Main(string[] args)
        {
            var spinner = new Thread(Spin)
            {
                IsBackground = true
            };

            try
            {
                if (args.Length < 1) throw new Exception("Не ввели адрес файла");

                var uri = new Uri(args[0]);
                int blur = 10;
                if (args.Length >= 2) int.TryParse(args[1], out blur);

                _ = DownloadAndBlur(uri, blur);
                spinner.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }

        static async Task DownloadAndBlur(Uri url, int blur)
        {
            var image = await Download(url);
            var bluredImage = await BlurAsync(image, blur);
            await SaveImage(bluredImage, url.Segments[url.Segments.Length - 1]);
            spin = false;
        }


        static async Task<Bitmap> Download(Uri uri)
        {
            Bitmap bmp = null; 
            using(var client = new HttpClient())
            {
                using (var stream = await client.GetStreamAsync(uri))
                {
                    await Task.Run(() => bmp = new Bitmap(stream));
                }
            }

            return bmp;
        }


        static Task<Bitmap> BlurAsync(Bitmap bmp, int blur)
        {
            return Task.Run(() => {
                var gblur = new GaussianBlur(bmp);
                return gblur.Process(blur);
            });
        }


        static Task SaveImage(Bitmap bmp, string filename)
        {
            return Task.Run(
                () => bmp.Save(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename))
            );
        }

        static bool spin = false;
        static void Spin()
        {
            spin = true;
            
            while (spin)
            {
                Console.Write("\rЗагрузка " + "\\");
                Thread.Sleep(500);
                Console.Write("\rЗагрузка " + "|");
                Thread.Sleep(500);
                Console.Write("\rЗагрузка " + "/");
                Thread.Sleep(500);
                Console.Write("\rЗагрузка " + "-");
                Thread.Sleep(500);
            }

            Console.Write("\r" + "Загрузка завершена...");
        }
    }
}
