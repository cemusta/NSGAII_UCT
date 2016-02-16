using ConsoleApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\n Usage: nsgaii random_seed \n");
                return;
            }

            string seedStr =
                args[0].Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                    .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            double seed;

            double.TryParse(seedStr, out seed);
            if (seed <= 0.0 || seed >= 1.0)
            {
                Console.WriteLine("\n Entered seed value is wrong, seed value must be in (0,1) \n");
                return;
            }

            int[,,] scheduling = new int[8, 5, 9]; //8 dönem, 5 gün, 9 ders            
            int[,] lab_scheduling = new int[5, 9]; // labda dönem tutulmuyor 
            string[] teacher_list = new string[70];
            int[,] meeting = new int[5, 9]; // bölüm hocalarının ortak meeting saatleri.
            string[] record_list1 = new string[2];

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
                Console.WriteLine($"Input file missing.\n EX: {ex.FileName}");
                return;
            }

            //deneme
            var deneme = File.OpenWrite("deneme.out");

            #region scan input collective
            Console.WriteLine("scanning input collective\n");
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
                Console.WriteLine(ex.Message);
                throw;
            }
            #endregion

            #region scan lab schedule
            Console.WriteLine("scanning lab scheduling\n");
            reader = new StreamReader(input_labs);
            for (int j = 0; j < 9; j++)
            {
                line = reader.ReadLine();
                var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int k = 0; k < 5; k++)
                {
                    lab_scheduling[k, j] = int.Parse(parts[k]);
                }
            }
            #endregion

            #region scan meeting file
            Console.WriteLine("scanning meeting file\n");
            reader = new StreamReader(meeting_file);
            for (int j = 0; j < 9; j++)
            {
                line = reader.ReadLine();
                var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int k = 0; k < 5; k++)
                {
                    meeting[k, j] = int.Parse(parts[k]);
                }
            }
            #endregion

            #region scan course list
            reader = new StreamReader(input_file);
            int teacher_list_size = 0;
            line = reader.ReadLine();
            int course_count = int.Parse(line); //43 gibi bir sayı dönüyor
            CourseDetail[] course_list = new CourseDetail[course_count]; //corse list için alan al.
            Console.WriteLine($"SIZE: {course_count} \n");

            int x = 0;
            for (int course_ID = 0; course_ID < course_count; course_ID++)
            {
                line = reader.ReadLine();
                //Console.WriteLine($"{line}\n");

                var parts = line.Split(new char[] { ';' });
                // token = strtok(record, ";");

                Console.WriteLine($"x={x}");
                for (int i = 0; i < parts.Length; i++)
                {
                    Console.WriteLine($"{i}.{parts[i]}");
                }

                course_list[course_ID] = new CourseDetail(parts[0], parts[1], int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]), int.Parse(parts[6]));

                int j;
                for (j = 0; j < course_ID; j++)
                {
                    if (course_list[j].teacher == parts[1])
                    {
                        break; //niye?
                    }
                }

                if (j == course_ID)
                {
                    teacher_list[teacher_list_size] = parts[1];
                    teacher_list_size++;
                }

            }
            Console.WriteLine($"teacher size: {teacher_list_size}");
            #endregion


            List<string> vec1 = new List<string>(course_count);

            reader = new StreamReader(prerequisite);

            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(new char[] { ';' });

                for (int i = 0; i < parts.Length; i++)
                {
                    record_list1[i] = parts[i]; //todo: burası ancak 2 olabilir yoksa çakacak...
                }

                for (int i = 0; i < course_count; i++)
                {
                    if (course_list[i].code == record_list1[0])
                    {
                        vec1.Add(record_list1[1]);
                    }
                }
            }

            StreamWriter writer1 = new StreamWriter(fpt1);
            StreamWriter writer2 = new StreamWriter(fpt2);
            StreamWriter writer3 = new StreamWriter(fpt3);
            StreamWriter writer4 = new StreamWriter(fpt4);
            StreamWriter writer5 = new StreamWriter(fpt5);

            writer1.Write("# This file contains the data of initial population\n");
            writer2.Write("# This file contains the data of final population\n");
            writer3.Write("# This file contains the data of final feasible population (if found)\n");
            writer4.Write("# This file contains the data of all generations\n");
            writer5.Write("# This file contains information about inputs as read by the program\n");


            writer1.Flush();
            writer2.Flush();
            writer3.Flush();
            writer4.Flush();
            writer5.Flush();

        } // main
    }
}
