using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console
{
    class Program
    {
        
        static void Main(string[] args)
        {


            if (args.Length < 1)
            {
                System.Console.WriteLine("\n Usage: nsgaii random_seed \n");
                return;
            }

            string seedStr =
                args[0].Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                    .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            double seed;

            double.TryParse(seedStr, out seed);
            if (seed <= 0.0 || seed >= 1.0)
            {
                System.Console.WriteLine("\n Entered seed value is wrong, seed value must be in (0,1) \n");
                return;
            }

            int[,,] scheduling = new int[8, 5, 9]; //8 dönem, 5 gün, 9 ders            
            int[,] lab_scheduling = new int[5,9]; // labda dönem tutulmuyor 

            // Output files:

            var fpt1 = File.OpenWrite("initial_pop.out");
            var fpt2 = File.OpenWrite("final_pop.out");
            var fpt3 = File.OpenWrite("best_pop.out");
            var fpt4 = File.OpenWrite("all_pop.out");
            var fpt5 = File.OpenWrite("params.out");

            //Input files:
            FileStream input_file;
            FileStream input_collective;
            FileStream input_labs;
            FileStream meeting_file;
            FileStream prerequisite;

            try  //todo better input handling
            {
                input_file = File.OpenRead("course_list.csv");
                input_collective = File.OpenRead("scheduling.in");
                input_labs = File.OpenRead("lab_list.in");
                meeting_file = File.OpenRead("Meeting.txt");
                prerequisite = File.OpenRead("Onkosul-list.csv");
            }
            catch (FileNotFoundException ex)
            {
                System.Console.WriteLine($"Input file missing.\n EX: {ex.FileName}");
                return;
            }

            //deneme
            var deneme = File.OpenWrite("deneme.out");


            System.Console.WriteLine("scanning input collective\n");

            StreamReader reader = new StreamReader(input_collective);
            //var parts = input_collective.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string line;
            try
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        line = reader.ReadLine();
                        var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int k = 0; k < 5; k++)
                        {
                            //fscanf(input_collective, "%d", scheduling[i][k][j]);
                            scheduling[i, k, j] = int.Parse(parts[k]);
                        }
                    }
                    line = reader.ReadLine(); //trailing \n
                }
            }
            catch (Exception ex)
            {
                
                throw;
            }


            //System.Console.WriteLine("scanning lab scheduling\n");
            //for (int j = 0; j < 9; j++)
            //{
            //    for (int k = 0; k < 5; k++)
            //    {
            //        fscanf(input_labs, "%d", &lab_scheduling[k][j]);
            //    }
            //}

        }
    }
}
