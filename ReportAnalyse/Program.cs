using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportAnalyse
{
    class Program
    {
        static void Main(string[] args)
        {
            string path;
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: app <report path>");
                return;
            }
            else
            {
                path = args[0];
                if (!path.EndsWith("\\"))
                    path += "\\";

                var pathok = Directory.Exists(path);

                if (!pathok)
                {
                    Console.WriteLine("folder path doesn't exist.");
                    return;
                }


            }

            var methodFiles = Directory.GetFiles(path).Where(x => x.Contains("method")).ToList();

            List<Report> RepList = new List<Report>();

            foreach (var methodfile in methodFiles)
            {
                var lines = File.ReadLines(methodfile).ToList();

                var seedline = lines[0].Replace("seed: ", "");
                var popline = lines[1].Replace("pop.: ", "");
                var genline = lines[2].Replace("gen.: ", "");
                var methodline = lines[3].Replace("Method: ", "");

                Report temp = new Report
                {
                    Title = Path.GetFileName(methodfile).Substring(0, 6),
                    Seed = double.Parse(seedline),
                    Pop = int.Parse(popline),
                    Gen = int.Parse(genline),
                    Method = methodline
                };

                RepList.Add(temp);
            }

            foreach (var report in RepList)
            {
                string filename = path + report.Title + "_best_pop.out";
                if (File.Exists(filename))
                {
                    var lines = File.ReadAllLines(filename);

                    report.Finalcount = lines.Length - 2;

                    int best = 10000;
                    int bestCount = 0;
                    for (int i = 2; i < lines.Length; i++)
                    {
                        var test = lines[i].Split('\t');

                        int temp = int.Parse(test[0]) + int.Parse(test[1]) + int.Parse(test[2]);
                        if (temp < best)
                        {
                            best = temp;
                            bestCount = 1;
                        }
                        else if (temp == best)
                        {
                            bestCount++;
                        }

                    }

                    report.Best = best;
                    report.Bestcount = bestCount;
                }
            }


            var file = File.OpenWrite($"{path}Analysis.csv");
            StreamWriter writer = new StreamWriter(file);

            writer.WriteLine("Title, Seed, Pop., Gen., Method, Best, # of Best, Finak Rank 1");

            foreach (var report in RepList)
            {

                writer.Write($"{report.Title},");
                writer.Write($"{report.Seed.ToString(CultureInfo.InvariantCulture)},");
                writer.Write($"{report.Pop},");
                writer.Write($"{report.Gen},");
                writer.Write($"{report.Method},");

                writer.Write($"{report.Best},");
                writer.Write($"{report.Bestcount},");
                writer.Write($"{report.Finalcount}");
                writer.Write("\n");
            }

            writer.Flush();
            writer.Close();

        }
    }

    public class Report
    {
        public string Title;
        public double Seed;
        public int Pop;
        public int Gen;
        public string Method;
        public int Best;
        public int Bestcount;
        public int Finalcount;



    }
}
