using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenCvSharp;

namespace Trimer
{
    class Program
    {
        static void Main(string[] args)
        {
            var videoFile = args.FirstOrDefault() ?? @"C:\Users\huser\Documents\WallStudio\3D\mogumi\making_video\video(3).mp4";
            // var videoFile = args.FirstOrDefault() ?? @"video.mp4";
            using var capture = new VideoCapture(videoFile);
            var frameDuration = TimeSpan.FromSeconds(1 / capture.Fps);
            var mats = new Queue<Mat>(Enumerable.Range(0, 60).Select(i => new Mat()));
            using(var sw = new StreamWriter(Path.GetFileName(videoFile) + ".csv", false))
            {
                for(int i = 0; true; i++)
                {
                    var mat = new Mat();
                    mats.Enqueue(mat);
                    mats.Dequeue().Dispose();
                    if (!capture.Read(mat))
                    {
                        break;
                    }
                    Cv2.CvtColor(mat, mat, ColorConversionCodes.RGB2GRAY);

                    if(i % (int)capture.Fps == 0)
                    {
                        double sim;
                        using(var display = mat.Clone())
                        {
                            sim = CalcSimilality(mats, display);
                            // mat.CopyTo(display);
                            // display.PutText($"{i:D7}, {sim:f3}", new Point(15, 15), HersheyFonts.HersheyPlain, 1, Scalar.Blue);
                            // Cv2.ImShow("frame", display);
                            // if ((Cv2.WaitKey(10) & 0xFF) == 'q')
                            // {
                            //     break;
                            // }
                        }

                        var dump = new []
                        {
                            $"{i}",
                            $"{frameDuration * i}",
                            $"{sim}",
                        };
                        sw.WriteLine(string.Join(',', dump));
                        Console.CursorLeft = 0;
                        Console.Write($"{i / (double)capture.FrameCount:p1}");
                    }
                }
            }
            Cv2.DestroyAllWindows();
        }

        static double CalcSimilality(IEnumerable<Mat> mats, Mat externalDiff)
        {
            var score = 0D;
            var array = mats.ToArray();
            for (int i = 0; i < array.Length - 1; i++)
            {
                var a = array[i];
                var b = array[i + 1];
                if(a.Size().Height == 0 || a.Size().Width == 0 || a.Size() != b.Size() || a.Type() != b.Type())
                {
                    continue;
                }

                var diff = externalDiff ?? new Mat();
                Cv2.Subtract(a, b, diff);
                score += Cv2.Sum(diff).Val0 / (a.Size().Width * a.Size().Height);
            }
            return score / (array.Length - 1);
        }
    }
}
