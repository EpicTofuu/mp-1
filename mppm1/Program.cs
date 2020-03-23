using FFmpeg.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Diagnostics;

namespace mppm1
{
    class Program
    {
        public static string ffmpegPath = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
        public static Dictionary<string, string> Mappings;

        [STAThread]
        static void Main(string[] args)
        {
            Converter converter;

            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export")))
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export"));
            }

            if (!File.Exists(ffmpegPath))
            {
                Console.WriteLine("an instance of ffmpeg could not be found on your machine.");
                Console.WriteLine("This program requires an instance of ffmpeg to be installed");
                Console.WriteLine("Simply download the files and extract the contents in the program's directory");
                Console.WriteLine("https://www.ffmpeg.org/");
                Console.ReadLine();
                Environment.Exit(1);
            }

            Mappings = new Dictionary<string, string>();
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mappings.txt")))
            {
                string[] lines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mappings.txt"));

                foreach (string line in lines)
                {
                    string[] formats = line.Split(' ');

                    //Add them going both ways
                    if (!Mappings.Keys.Contains (formats[0]))
                        Mappings.Add(formats[0], formats[1]);   

                    if (!Mappings.Keys.Contains (formats[1]))
                        Mappings.Add(formats[1], formats[0]);
                }
            }
            
            if (args.Length > 0 && File.Exists(args[0]))
            {
                string path = args[0];
                converter = new Converter(Mappings, ffmpegPath);
                converter.Convert(path);
            }
            else
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    string fil = "All files (*.*)|*.*";

                    List<string> uniqueExtension = new List<string>();

                    foreach (KeyValuePair<string, string> entry in Mappings)
                    {
                        if (uniqueExtension.Contains(entry.Key) || uniqueExtension.Contains(entry.Value))   //remove duplicates
                            break;

                        fil = fil + $"|{entry.Key} files (*.{entry.Key})|*.{entry.Key}" + $"|{entry.Value} files (*.{entry.Value})|*.{entry.Value}";

                        uniqueExtension.Add(entry.Key);
                        uniqueExtension.Add(entry.Value);
                    }

                    ofd.Filter = fil;
                    ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        Console.WriteLine(ofd.FileName);
                        converter = new Converter(Mappings, ffmpegPath);
                        converter.Convert(ofd.FileName);
                    }
                }
            }
        }
        
    }

    public class Converter
    {
        Dictionary<string, string> Mappings;
        string ffmpegPath;

        public Converter(Dictionary <string, string> Mappings, string ffmpegPath)
        {
            this.Mappings = Mappings;
            this.ffmpegPath = ffmpegPath;
        }

        /// <summary>
        /// Convert an mp4 (video) into mp3 (audio)
        /// </summary>
        /// <param name="inp"></param>
        public async void Convert(string inp)
        {
            string ext = Mappings[Path.GetExtension(inp).Trim('.')];      //extension of file taken from dictionary

            var ffmpeg = new Engine(ffmpegPath);
            
            ffmpeg.Error += Ffmpeg_Error;
            ffmpeg.Progress += Ffmpeg_Progress;

            string output = $"{Path.GetFileNameWithoutExtension(inp)}" + "." + ext;
            var inputFile = new MediaFile(inp);
            var outputFile = new MediaFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export", output));

            Console.WriteLine("converting file");
            await ffmpeg.ConvertAsync(inputFile, outputFile);
            Console.WriteLine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export", output));
            Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export"));
        }

        private void Ffmpeg_Progress(object sender, FFmpeg.NET.Events.ConversionProgressEventArgs e)
        {
            Console.Clear();
            float perc = (float)Math.Round((decimal)(e.ProcessedDuration.TotalSeconds / e.TotalDuration.TotalSeconds * 100), 2);
            Console.WriteLine ($"Progress: {perc}%");
            Console.WriteLine("if you decide to close the program now, you must also kill it from the task manager");
        }

        private void Ffmpeg_Error(object sender, FFmpeg.NET.Events.ConversionErrorEventArgs e)
        {
            Console.WriteLine("[{0} => {1}]: Error: {2}\n{3}", e.Input.FileInfo.Name, e.Output.FileInfo.Name, e.Exception.ExitCode, e.Exception.InnerException);
            Console.ReadLine();
        }
    }
}