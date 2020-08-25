using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trimer
{
    class Program
    {
        static (int w, int h) GetVideoResolution(string fileName)
        {
            var p = Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = "ffmpeg",
                Arguments = " -i video.mp4"
            });
            p.WaitForExit();
            var output = p.StandardError.ReadToEnd();
            var resolitionText = Regex.Match(output, @"Stream #0:0.*Video:.* ([1-9][0-9]*)x([1-9][0-9]*)\,");
            return (
                w: int.Parse(resolitionText.Groups[1].Value),
                h: int.Parse(resolitionText.Groups[2].Value));
        }

        static void Main(string[] args)
        {
            var size = GetVideoResolution("video.mp4");
            var p = Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = "ffmpeg",
                Arguments = " -i video.mp4  -f image2pipe -pix_fmt rgb32 -vcodec rawvideo  -"
            });
            var error = new Thread(() =>
            {
                while(!p.HasExited)
                {
                    Console.WriteLine(p.StandardError.ReadLine());
                }
            });
            var count = 0L;
            var buff = new byte[4 * size.w * size.h];
            var stdout = new Thread(() =>
            {
                while(!p.HasExited)
                {
                    count += p.StandardOutput.BaseStream.Read(buff, 0, buff.Length);
                    // Console.WriteLine();
                }
            });
            error.Start();
            stdout.Start();
            error.Join();
            stdout.Join();


            Console.WriteLine($"recived {count} byte {count / (4 * 1280 * 720)} frame");
            var gray = new byte[buff.Length / 4];
            for (int i = 0; i < gray.Length; i++)
            {
                gray[i] = buff[i * 4 + 1];
            }
            File.WriteAllBytes("out.raw", gray);
            var bmp = ByteToImage(buff, size.w, size.h);
            bmp.Save("out.bmp");
            Console.ReadKey();
        }

        public static Bitmap ByteToImage(byte[] blob, int w, int h)
        {
            using MemoryStream mStream = new MemoryStream();
            mStream.Write(blob, 0, blob.Length);
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var index = 0;
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    var color = Color.Yellow;// Color.FromArgb(blob[index++], blob[index++], blob[index++], blob[index++]);
                    bmp.SetPixel(j, i, color);
                }
            }
            return bmp;
        }
    }


}
