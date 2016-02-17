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


            int nreal;
            int nbin;
            int nobj;
            int ncon;
            int popsize;
            double pcross_real = 0;
            double pcross_bin = 0;
            double pmut_real = 0;
            double pmut_bin = 0;
            double eta_c = 0;
            double eta_m = 0;
            int ngen;
            int nbinmut;
            int nrealmut;
            int nbincross;
            int nrealcross;

            int[] nbits = new int[50];
            double[] min_realvar = new double[50];
            double[] max_realvar = new double[50];
            double[] min_binvar = new double[50];
            double[] max_binvar = new double[50];

            int bitlength;
            int choice;
            int obj1;
            int obj2;
            int obj3;
            int angle1;
            int angle2;


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

            for (int course_ID = 0; course_ID < course_count; course_ID++)
            {
                line = reader.ReadLine();
                //Console.WriteLine($"{line}\n");

                var parts = line.Split(new char[] { ';' });
                // token = strtok(record, ";");

                for (int i = 0; i < parts.Length; i++)
                {
                    Console.WriteLine($"{i}.{parts[i]}");
                }
                Console.WriteLine();

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


            #region problem relevant parameter inputs

            string consoleIn;

            Console.WriteLine(" Enter the problem relevant and algorithm relevant parameters ... ");
            Console.WriteLine(" Enter the population size (a multiple of 4) : ");
            consoleIn = Console.ReadLine();
            popsize = int.Parse(consoleIn);
            if (popsize < 4 || (popsize % 4) != 0)
            {
                Console.WriteLine($" population size read is : {popsize}");
                Console.WriteLine(" Wrong population size entered, hence exiting \n");
                return;
            }

            Console.WriteLine(" Enter the number of generations : ");
            consoleIn = Console.ReadLine();
            ngen = int.Parse(consoleIn);
            if (ngen < 1)
            {
                Console.WriteLine($" number of generations read is : {ngen}");
                Console.WriteLine(" Wrong nuber of generations entered, hence exiting \n");
                return;
            }

            Console.WriteLine(" Enter the number of objectives : ");
            consoleIn = Console.ReadLine();
            nobj = int.Parse(consoleIn);
            if (nobj < 1)
            {
                Console.WriteLine($" number of objectives entered is : {nobj}");
                Console.WriteLine(" Wrong number of objectives entered, hence exiting \n");
                return;
            }

            Console.WriteLine("\n Enter the number of constraints : ");
            consoleIn = Console.ReadLine();
            ncon = int.Parse(consoleIn);
            if (ncon < 0)
            {
                Console.WriteLine($" number of constraints entered is : {ncon}");
                Console.WriteLine(" Wrong number of constraints enetered, hence exiting \n");
                return;
            }

            Console.WriteLine("\n Enter the number of real variables : ");
            consoleIn = Console.ReadLine();
            nreal = int.Parse(consoleIn);
            if (nreal < 0)
            {
                Console.WriteLine($" number of real variables entered is : {nreal}");
                Console.WriteLine(" Wrong number of variables entered, hence exiting \n");
                return;
            }


            if (nreal != 0)
            {
                min_realvar = new double[nreal];
                max_realvar = new double[nreal];
                for (int i = 0; i < nreal; i++)
                {
                    Console.WriteLine($" Enter the lower limit of real variable %d : ", i + 1);
                    consoleIn = Console.ReadLine();
                    min_realvar[i] = double.Parse(consoleIn);
                    Console.WriteLine($" Enter the upper limit of real variable %d : ", i + 1);
                    consoleIn = Console.ReadLine();
                    max_realvar[i] = double.Parse(consoleIn);
                    if (max_realvar[i] <= min_realvar[i])
                    {
                        Console.WriteLine(" Wrong limits entered for the min and max bounds of real variable, hence exiting \n");
                        return;
                    }
                }
                Console.WriteLine(" Enter the probability of crossover of real variable (0.6-1.0) : ");
                consoleIn = Console.ReadLine();
                pcross_real = double.Parse(consoleIn);
                if (pcross_real < 0.0 || pcross_real > 1.0)
                {
                    Console.WriteLine($" Probability of crossover entered is : {pcross_real}");
                    Console.WriteLine(" Entered value of probability of crossover of real variables is out of bounds, hence exiting \n");
                    return;
                }
                Console.WriteLine(" Enter the probablity of mutation of real variables (1/nreal) : ");
                consoleIn = Console.ReadLine();
                pmut_real = double.Parse(consoleIn);
                if (pmut_real < 0.0 || pmut_real > 1.0)
                {
                    Console.WriteLine($" Probability of mutation entered is : {pmut_real}");
                    Console.WriteLine(" Entered value of probability of mutation of real variables is out of bounds, hence exiting \n");
                    return;
                }
                Console.WriteLine(" Enter the value of distribution index for crossover (5-20): ");
                consoleIn = Console.ReadLine();
                eta_c = double.Parse(consoleIn);
                if (eta_c <= 0)
                {
                    Console.WriteLine($" The value entered is : {eta_c}");
                    Console.WriteLine(" Wrong value of distribution index for crossover entered, hence exiting \n");
                    return;
                }
                Console.WriteLine(" Enter the value of distribution index for mutation (5-50): ");
                consoleIn = Console.ReadLine();
                eta_m = double.Parse(consoleIn);
                if (eta_m <= 0)
                {
                    Console.WriteLine($" The value entered is : {eta_m}");
                    Console.WriteLine(" Wrong value of distribution index for mutation entered, hence exiting \n");
                    return;
                }
            }

            Console.WriteLine(" Enter the number of binary variables : ");
            consoleIn = Console.ReadLine();
            nbin = int.Parse(consoleIn);
            if (nbin < 0)
            {
                Console.WriteLine($" number of binary variables entered is : {nbin}");
                Console.WriteLine(" Wrong number of binary variables entered, hence exiting \n");
                return;
            }
            if (nbin != 0)
            {
                nbits = new int[nbin];
                min_binvar = new double[nbin];
                max_binvar = new double[nbin];
                for (int i = 0; i < nbin; i++)
                {
                    Console.WriteLine($" Enter the number of bits for binary variable {i + 1} : ");
                    consoleIn = Console.ReadLine();
                    var parts = consoleIn.Split(new char[] { ' ' });
                    nbits[i] = int.Parse(parts[0]);
                    if (nbits[i] < 1)
                    {
                        Console.WriteLine(" Wrong number of bits for binary variable entered, hence exiting");
                        return;
                    }
                    Console.WriteLine($" Enter the lower limit of binary variable {i + 1}  : ");
                    //consoleIn = Console.ReadLine();
                    min_binvar[i] = double.Parse(parts[1]);

                    Console.WriteLine($" Enter the upper limit of binary variable {i + 1}  : ");
                    //consoleIn = Console.ReadLine();
                    max_binvar[i] = double.Parse(parts[2]);
                    if (max_binvar[i] <= min_binvar[i])
                    {
                        Console.WriteLine(" Wrong limits entered for the min and max bounds of binary variable entered, hence exiting \n");
                        return;
                    }
                }
                Console.WriteLine(" Enter the probability of crossover of binary variable (0.6-1.0): ");
                consoleIn = Console.ReadLine().Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                    .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                pcross_bin = double.Parse(consoleIn);
                if (pcross_bin < 0.0 || pcross_bin > 1.0)
                {
                    Console.WriteLine($" Probability of crossover entered is : {pcross_bin}");
                    Console.WriteLine(" Entered value of probability of crossover of binary variables is out of bounds, hence exiting \n");
                    return;
                }
                Console.WriteLine(" Enter the probability of mutation of binary variables (1/nbits): ");
                consoleIn = Console.ReadLine().Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                    .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                pmut_bin = double.Parse(consoleIn);
                if (pmut_bin < 0.0 || pmut_bin > 1.0)
                {
                    Console.WriteLine($" Probability of mutation entered is :  {pmut_bin}");
                    Console.WriteLine(" Entered value of probability  of mutation of binary variables is out of bounds, hence exiting \n");
                    return;
                }
            }
            if (nreal == 0 && nbin == 0)
            {
                Console.WriteLine("\n Number of real as well as binary variables, both are zero, hence exiting \n");
                return;
            }

            Console.WriteLine(" Do you want to use gnuplot to display the results realtime (0 for NO) (1 for yes) : ");
            consoleIn = Console.ReadLine();
            choice = int.Parse(consoleIn);
            if (choice != 0 && choice != 1)
            {
                Console.WriteLine($" Entered the wrong choice, hence exiting, choice entered was {choice}\n");
                return;
            }
            if (choice == 1)
            {
                //object gp = null;  // _popen(GNUPLOT_COMMAND, "w");
                //if (gp == null)
                //{
                //    Console.WriteLine(" Could not open a pipe to gnuplot, check the definition of GNUPLOT_COMMAND in file global.h\n");
                //    Console.WriteLine(" Edit the string to suit your system configuration and rerun the program\n");
                //    return;
                //}
                if (nobj == 2)
                {
                    Console.WriteLine(" Enter the objective for X axis display : ");
                    consoleIn = Console.ReadLine();
                    obj1 = int.Parse(consoleIn);
                    if (obj1 < 1 || obj1 > nobj)
                    {
                        Console.WriteLine($" Wrong value of X objective entered, value entered was {obj1}\n");
                        return;
                    }
                    Console.WriteLine(" Enter the objective for Y axis display : ");
                    consoleIn = Console.ReadLine();
                    obj2 = int.Parse(consoleIn);
                    if (obj2 < 1 || obj2 > nobj)
                    {
                        Console.WriteLine($" Wrong value of Y objective entered, value entered was {obj2}\n");
                        return;
                    }
                    obj3 = -1;
                }
                else
                {
                    Console.WriteLine(" #obj > 2, 2D display or a 3D display ?, enter 2 for 2D and 3 for 3D :");

                    consoleIn = Console.ReadLine();
                    choice = int.Parse(consoleIn);
                    if (choice != 2 && choice != 3)
                    {
                        Console.WriteLine($" Entered the wrong choice, hence exiting, choice entered was {choice}\n");
                        return;
                    }
                    if (choice == 2)
                    {
                        Console.WriteLine(" Enter the objective for X axis display : ");
                        consoleIn = Console.ReadLine();
                        obj1 = int.Parse(consoleIn);
                        if (obj1 < 1 || obj1 > nobj)
                        {
                            Console.WriteLine($" Wrong value of X objective entered, value entered was {obj1}\n");
                            return;
                        }
                        Console.WriteLine(" Enter the objective for Y axis display : ");
                        consoleIn = Console.ReadLine();
                        obj2 = int.Parse(consoleIn);
                        if (obj2 < 1 || obj2 > nobj)
                        {
                            Console.WriteLine($" Wrong value of Y objective entered, value entered was {obj2}\n");
                            return;
                        }
                        obj3 = -1;
                    }
                    else
                    {
                        Console.WriteLine(" Enter the objective for X axis display : ");
                        consoleIn = Console.ReadLine();
                        obj1 = int.Parse(consoleIn);
                        if (obj1 < 1 || obj1 > nobj)
                        {
                            Console.WriteLine($" Wrong value of X objective entered, value entered was {obj1}\n");
                            return;
                        }
                        Console.WriteLine(" Enter the objective for Y axis display : ");
                        consoleIn = Console.ReadLine();
                        obj2 = int.Parse(consoleIn);
                        if (obj2 < 1 || obj2 > nobj)
                        {
                            Console.WriteLine($" Wrong value of Y objective entered, value entered was {obj2}\n");
                            return;
                        }
                        Console.WriteLine(" Enter the objective for Z axis display : ");
                        consoleIn = Console.ReadLine();
                        obj3 = int.Parse(consoleIn);
                        if (obj3 < 1 || obj3 > nobj)
                        {
                            Console.WriteLine($" Wrong value of Z objective entered, value entered was {obj3}\n");
                            return;
                        }
                        Console.WriteLine(" You have chosen 3D display, hence location of eye required \n");
                        Console.WriteLine(" Enter the first angle (an integer in the range 0-180) (if not known, enter 60) :");
                        consoleIn = Console.ReadLine();
                        angle1 = int.Parse(consoleIn);
                        if (angle1 < 0 || angle1 > 180)
                        {
                            Console.WriteLine(" Wrong value for first angle entered, hence exiting \n");
                            return;
                        }
                        Console.WriteLine(" Enter the second angle (an integer in the range 0-360) (if not known, enter 30) :");
                        consoleIn = Console.ReadLine();
                        angle2 = int.Parse(consoleIn);
                        if (angle2 < 0 || angle2 > 360)
                        {
                            Console.WriteLine(" Wrong value for second angle entered, hence exiting \n");
                            return;
                        }
                    }
                }
            }
            Console.WriteLine("\n Input data successfully entered, now performing initialization \n");
            #endregion




            writer5.WriteLine($" Population size = {popsize}");
            writer5.WriteLine($" Number of generations = {ngen}");
            writer5.WriteLine($" Number of objective functions = {nobj}");
            writer5.WriteLine($" Number of constraints = {ncon}");
            writer5.WriteLine($" Number of real variables = {nreal}");
            if (nreal != 0)
            {
                for (int i = 0; i < nreal; i++)
                {
                    writer5.WriteLine($" Lower limit of real variable {i + 1} = {min_realvar[i]}");
                    writer5.WriteLine($" Upper limit of real variable {i + 1} = {max_realvar[i]}");
                }
                writer5.WriteLine($" Probability of crossover of real variable = {pcross_real}");
                writer5.WriteLine($" Probability of mutation of real variable = {pmut_real}");
                writer5.WriteLine($" Distribution index for crossover = {eta_c}");
                writer5.WriteLine($" Distribution index for mutation = {eta_m}");
            }
            writer5.Write($" Number of binary variables = {nbin}");
            if (nbin != 0)
            {
                for (int i = 0; i < nbin; i++)
                {
                    writer5.WriteLine($" Number of bits for binary variable {i + 1} = {nbits[i]}");
                    writer5.WriteLine($" Lower limit of binary variable {i + 1} = {min_binvar[i]}");
                    writer5.WriteLine($" Upper limit of binary variable {i + 1} = {max_binvar[i]}");
                }
                writer5.WriteLine($" Probability of crossover of binary variable = {pcross_bin}");
                writer5.WriteLine($" Probability of mutation of binary variable = {pmut_bin}");
            }
            writer5.Write($" Seed for random number generator = {seed}" );
            bitlength = 0;
            if (nbin != 0)
            {
                /*printf("nbin: %d \n", nbin);*/
                for (int i = 0; i < nbin; i++)
                {
                    bitlength += nbits[i];
                    /*printf("nbits[%d]: %d \n", i,nbits[i]);*/
                }
            }

            writer1.Write($"# of objectives = {0}, # of constraints = {1}, # of real_var = {2}, # of bits of bin_var = {3}, constr_violation, rank, crowding_distance\n", nobj, ncon, nreal, bitlength);
            writer2.Write($"# of objectives = {0}, # of constraints = {1}, # of real_var = {2}, # of bits of bin_var = {3}, constr_violation, rank, crowding_distance\n", nobj, ncon, nreal, bitlength);
            writer3.Write($"# of objectives = {0}, # of constraints = {1}, # of real_var = {2}, # of bits of bin_var = {3}, constr_violation, rank, crowding_distance\n", nobj, ncon, nreal, bitlength);
            writer4.Write($"# of objectives = {0}, # of constraints = {1}, # of real_var = {2}, # of bits of bin_var = {3}, constr_violation, rank, crowding_distance\n", nobj, ncon, nreal, bitlength);
            nbinmut = 0;
            nrealmut = 0;
            nbincross = 0;
            nrealcross = 0;


            writer1.Flush();
            writer2.Flush();
            writer3.Flush();
            writer4.Flush();
            writer5.Flush();



            //parent_pop = (population*)malloc(sizeof(population));
            //child_pop = (population*)malloc(sizeof(population));
            //mixed_pop = (population*)malloc(sizeof(population));
            //allocate_memory_pop(parent_pop, popsize);
            //allocate_memory_pop(child_pop, popsize);
            //allocate_memory_pop(mixed_pop, 2 * popsize);
            //randomize();
            //initialize_pop(parent_pop);
            Console.WriteLine(" Initialization done, now performing first generation");



            //decode_pop(parent_pop);
            //evaluate_pop(parent_pop);
            //assign_rank_and_crowding_distance(parent_pop);
            //report_pop(parent_pop, fpt1);
            //fprintf(fpt4, "# gen = 1\n");
            //report_pop(parent_pop, fpt4);
            Console.WriteLine(" gen = 1");
            //fflush(stdout);
            //if (choice != 0) onthefly_display(parent_pop, gp, 1);
            //fflush(fpt1);
            //fflush(fpt2);
            //fflush(fpt3);
            //fflush(fpt4);
            //fflush(fpt5);


            for (int i = 2; i <= ngen; i++)
            {
                //selection(parent_pop, child_pop);
                //mutation_pop(child_pop);
                //decode_pop(child_pop);
                //evaluate_pop(child_pop);
                //merge(parent_pop, child_pop, mixed_pop);
                //fill_nondominated_sort(mixed_pop, parent_pop);
                ///* Comment following four lines if information for all
                //generations is not desired, it will speed up the execution */
                ///*fprintf(fpt4,"# gen = %d\n",i);
                //report_pop(parent_pop,fpt4);
                //fflush(fpt4);*/
                //if (choice != 0) onthefly_display(parent_pop, gp, i);
                Console.WriteLine($" gen = {i}");
            }





            //printf("\n Generations finished, now reporting solutions");
            //report_pop(parent_pop, fpt2);
            //report_feasible(parent_pop, fpt3);
            //if (nreal != 0)
            //{
            //    fprintf(fpt5, "\n Number of crossover of real variable = %d", nrealcross);
            //    fprintf(fpt5, "\n Number of mutation of real variable = %d", nrealmut);
            //}
            //if (nbin != 0)
            //{
            //    fprintf(fpt5, "\n Number of crossover of binary variable = %d", nbincross);
            //    fprintf(fpt5, "\n Number of mutation of binary variable = %d", nbinmut);
            //}
            //fflush(stdout);
            //fflush(fpt1);
            //fflush(fpt2);
            //fflush(fpt3);
            //fflush(fpt4);
            //fflush(fpt5);
            //fclose(fpt1);
            //fclose(fpt2);
            //fclose(fpt3);
            //fclose(fpt4);
            //fclose(fpt5);
            //fclose(input_file);
            //fclose(input_collective);
            //fclose(input_labs);
            //fclose(prerequisite);
            //fclose(deneme);
            //if (choice != 0)
            //{
            //    _pclose(gp);
            //}
            //if (nreal != 0)
            //{
            //    free(min_realvar);
            //    free(max_realvar);
            //}
            //if (nbin != 0)
            //{
            //    free(min_binvar);
            //    free(max_binvar);
            //    free(nbits);
            //}
            //deallocate_memory_pop(parent_pop, popsize);
            //deallocate_memory_pop(child_pop, popsize);
            //deallocate_memory_pop(mixed_pop, 2 * popsize);
            //free(parent_pop);
            //free(child_pop);
            //free(mixed_pop);


            Console.WriteLine("\n Routine successfully exited \n");

        } // main
    }
}
