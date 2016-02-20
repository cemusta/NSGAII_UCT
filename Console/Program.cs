using AwokeKnowing.GnuplotCSharp;
using ConsoleApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;


namespace ConsoleApp
{
    class Program
    {
        #region Variable 
        static double INF = 1.0e14;
        static double EPS = 1.0e-14;
        static double E = 2.71828182845905;
        static double PI = 3.14159265358979;
        static string GNUPLOT_COMMAND = "gnuplot -persist";

        static int nreal;
        static int nbin;
        static int maxnbit = 0;
        static int nobj;
        static int ncon;
        static int popsize;
        static double pcross_real = 0;
        static double pcross_bin = 0;
        static double pmut_real = 0;
        static double pmut_bin = 0;
        static double eta_c = 0;
        static double eta_m = 0;
        static int ngen;
        static int nbinmut;
        static int nrealmut;
        static int nbincross;
        static int nrealcross;

        static int[] nbits = new int[50];
        static double[] min_realvar = new double[50];
        static double[] max_realvar = new double[50];
        static double[] min_binvar = new double[50];
        static double[] max_binvar = new double[50];

        static int bitlength;
        static int choice;
        static int obj1;
        static int obj2;
        static int obj3;
        static int angle1;
        static int angle2;

        static rand randObj = new rand();

        static int teacher_list_size = 0;

        static List<int[,]> scheduling = new List<int[,]>(8); //8 dönem, 5 gün, 9 ders            
        static int[,] lab_scheduling = new int[5, 9]; // labda dönem tutulmuyor 
        static string[] teacher_list = new string[70]; //todo: dub hocalar olabiliyor.
        static int[,] meeting = new int[5, 9]; // bölüm hocalarının ortak meeting saatleri.
        static string[] record_list1 = new string[2];
        static CourseDetail[] course_list;

        static List<List<string>> prerequisteList;
        #endregion

        static void Main(string[] args)
        {

            #region initialize stuff

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


            for (int i = 0; i < 8; i++)
            {
                scheduling.Add(new int[5, 9]);
            }

            Population parent_pop;
            Population child_pop;
            Population mixed_pop;

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

            #endregion

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
                            scheduling[i][k, j] = int.Parse(parts[k]);
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

            line = reader.ReadLine();
            int course_count = int.Parse(line); //43 gibi bir sayı dönüyor
            course_list = new CourseDetail[course_count]; //corse list için alan al.
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


            prerequisteList = new List<List<string>>(course_count);
            for (int i = 0; i < course_count; i++)
            {
                prerequisteList.Add(new List<string>());
            }

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
                        prerequisteList[i].Add(record_list1[1]);
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
                    if (nbits[i] > maxnbit)
                        maxnbit = nbits[i];
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
            writer5.Write($" Seed for random number generator = {seed}");
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



            parent_pop = new Population(popsize, nreal, nbin, maxnbit, nobj, ncon); // (population*)malloc(sizeof(population));
            child_pop = new Population(popsize, nreal, nbin, maxnbit, nobj, ncon); // (population*)malloc(sizeof(population));
            mixed_pop = new Population(popsize * 2, nreal, nbin, maxnbit, nobj, ncon); // (population*)malloc(sizeof(population));
                                                                                       //allocate_memory_pop(parent_pop, popsize);
                                                                                       //allocate_memory_pop(child_pop, popsize);
                                                                                       //allocate_memory_pop(mixed_pop, 2 * popsize);


            randObj.randomize();
            initialize_pop(parent_pop);
            Console.WriteLine(" Initialization done, now performing first generation");



            decode_pop(parent_pop);
            evaluate_population(parent_pop);
            assign_rank_and_crowding_distance(parent_pop);
            report_pop(parent_pop, writer1);
            writer4.WriteLine("# gen = 1");
            report_pop(parent_pop, writer4);
            Console.WriteLine(" gen = 1");
            //fflush(stdout);
            if (choice != 0)
            {
                onthefly_display(parent_pop, 1);
            }
            writer1.Flush();
            writer2.Flush();
            writer3.Flush();
            writer4.Flush();
            writer5.Flush();


            for (int i = 2; i <= ngen; i++)
            {
                selection(parent_pop, child_pop);
                mutation_pop(child_pop);
                decode_pop(child_pop);
                evaluate_population(child_pop);
                merge(parent_pop, child_pop, mixed_pop);
                fill_nondominated_sort(mixed_pop, parent_pop);

                /* Comment following four lines if information for all
                generations is not desired, it will speed up the execution */
                ///*fprintf(fpt4,"# gen = %d\n",i);
                //report_pop(parent_pop,fpt4);
                //fflush(fpt4);*/
                if (choice != 0)
                {
                    onthefly_display(parent_pop, i);
                }

                Console.WriteLine($" gen = {i}");
            }



            Console.WriteLine($" Generations finished, now reporting solutions");
            report_pop(parent_pop, writer2);
            report_feasible(parent_pop, writer3);

            if (nreal != 0)
            {
                writer5.WriteLine($" Number of crossover of real variable = {nrealcross}");
                writer5.WriteLine($" Number of mutation of real variable = {nrealmut}");
            }
            if (nbin != 0)
            {
                writer5.WriteLine($" Number of crossover of binary variable = {nbincross}");
                writer5.WriteLine($" Number of mutation of binary variable = {nbinmut}");
            }
            //fflush(stdout);
            writer1.Flush();
            writer2.Flush();
            writer3.Flush();
            writer4.Flush();
            writer5.Flush();
            writer1.Close();
            writer2.Close();
            writer3.Close();
            writer4.Close();
            writer5.Close();
            fpt1.Close();
            fpt2.Close();
            fpt3.Close();
            fpt4.Close();
            fpt5.Close();

            input_file.Close();
            input_collective.Close();
            input_labs.Close();
            meeting_file.Close();
            prerequisite.Close();

            Console.WriteLine("\n Routine successfully exited \n");

        } // main


        #region initialize.c
        /* Function to initialize a population randomly */
        static void initialize_pop(Population pop)
        {
            for (int i = 0; i < popsize; i++)
            {
                initialize_ind(pop.indList[i]);
            }
            return;
        }

        /* Function to initialize an individual randomly */
        static void initialize_ind(Individual ind)
        {
            int j, k;
            if (nreal != 0)
            {
                for (j = 0; j < nreal; j++)
                {
                    ind.xreal[j] = randObj.rndreal(min_realvar[j], max_realvar[j]);
                }
            }
            if (nbin != 0)
            {
                for (j = 0; j < nbin; j++)
                {
                    for (k = 0; k < nbits[j]; k++)
                    {
                        if (randObj.randomperc() <= 0.5)
                        {
                            ind.gene[j, k] = 0;
                        }
                        else
                        {
                            ind.gene[j, k] = 1;
                        }
                    }
                }
            }
            return;
        }
        #endregion

        #region decode.c
        /* Function to decode a population to find out the binary variable values based on its bit pattern */
        static void decode_pop(Population pop)
        {
            int i;
            if (nbin != 0)
            {
                for (i = 0; i < popsize; i++)
                {
                    decode_ind(pop.indList[i]);
                }
            }
            return;
        }

        /* Function to decode an individual to find out the binary variable values based on its bit pattern */
        static void decode_ind(Individual ind)
        {
            int j, k;
            int sum;
            if (nbin != 0)
            {
                for (j = 0; j < nbin; j++)
                {
                    sum = 0;
                    for (k = 0; k < nbits[j]; k++)
                    {
                        if (ind.gene[j, k] == 1)
                        {
                            sum += (int)Math.Pow(2, nbits[j] - 1 - k);
                        }
                    }
                    ind.xbin[j] = min_binvar[j] + (double)sum * (max_binvar[j] - min_binvar[j]) / (double)(Math.Pow(2, nbits[j]) - 1);
                }
            }
            return;
        }
        #endregion

        #region eval.c
        /* Routine to evaluate objective function values and constraints for a population */
        static void evaluate_population(Population pop)
        {
            for (int i = 0; i < popsize; i++)
            {
                evaluate_individual(pop.indList[i]);
            }
            return;
        }

        /* Routine to evaluate objective function values and constraints for an individual */
        static void evaluate_individual(Individual ind)
        {
            test_problem(ind.xreal, ind.xbin, ind.gene, ind.obj, ind.constr);
            if (ncon == 0)
            {
                ind.constr_violation = 0.0;
            }
            else
            {
                ind.constr_violation = 0.0;
                for (int j = 0; j < ncon; j++)
                {
                    if (ind.constr[j] < 0.0)
                    {
                        ind.constr_violation += ind.constr[j];
                    }
                }
            }
            return;
        }
        #endregion

        #region problemdef.c
        static void test_problem(double[] xreal, double[] xbin, int[,] gene, double[] obj, double[] constr)
        {
            // todo fix fix these stuff. dinamik olmamalı bunlar her seferinde? emin degilim...
            // en azından pop kadar kere yaratılmalı? pop içine taşınabilir?
            int sum, i, j, k;
            List<int[,]> teacher_scheduling_counter = new List<int[,]>(50); //todo teacher no kadar...
            for (i = 0; i < 50; i++)
            {
                teacher_scheduling_counter.Add(new int[5, 9]);
            }
            int teacher_index = 0;
            int[,] lab_counter = new int[5, 9];
            List<int>[][,] scheduling_only_CSE = new List<int>[8][,];
            for (i = 0; i < 8; i++)
            {
                scheduling_only_CSE[i] = new List<int>[5, 9];

                for (j = 0; j < 5; j++)
                {
                    for (k = 0; k < 9; k++)
                    {
                        scheduling_only_CSE[i][j, k] = new List<int>();
                    }
                }
            }
            List<int>[,] elective_courses = new List<int>[5, 9];
            for (i = 0; i < 5; i++)
            {
                for (j = 0; j < 9; j++)
                {
                    elective_courses[i, j] = new List<int>();
                }
            }
            obj[0] = 0;
            obj[1] = 0;
            obj[2] = 0;

            /*reset scheduling, tearchers_scheduling and lab_scheduling*/
            for (i = 0; i < teacher_list_size; ++i)
            {
                copy_array(teacher_scheduling_counter[i], meeting);
            }
            //for (j = 0; j < 8; j++) //c#'da int init'te hep 0
            //{
            //    reset(scheduling_only_CSE[j]);
            //}
            //reset(elective_courses);

            copy_array(lab_counter, lab_scheduling);       /*copy diaconate lab lessons*/
            for (j = 0; j < nbin; j++)
            {
                for (i = 0; i < teacher_list_size; i++)
                {
                    if (teacher_list[i].Equals(course_list[j].teacher))
                    {
                        teacher_index = i;
                        break;
                    }
                }
                sum = (int)xbin[j];

                if (course_list[j].duration == 1)
                {
                    if (course_list[j].elective == 0)
                    {
                        adding_course_1_slot(scheduling_only_CSE[course_list[j].semester - 1], sum, j);
                    }
                    else if (course_list[j].elective == 1)
                    {
                        adding_course_1_slot(elective_courses, sum, j);
                    }

                    adding_course_1_slot(teacher_scheduling_counter[teacher_index], sum);
                    if (course_list[j].type == 1)
                    {
                        for (k = 0; k < course_list[j].labHour; k++)
                        {
                            adding_course_1_slot(lab_counter, sum);
                        }
                    }
                }
                else if (course_list[j].duration == 2)
                {
                    if (course_list[j].elective == 0)
                    {
                        adding_course_2_slot(scheduling_only_CSE[course_list[j].semester - 1], sum, j);
                    }
                    else if (course_list[j].elective == 1)
                    {
                        adding_course_2_slot(elective_courses, sum, j);
                    }

                    adding_course_2_slot(teacher_scheduling_counter[teacher_index], sum);
                    if (course_list[j].type == 1)
                    {
                        for (k = 0; k < course_list[j].labHour; k++)
                        {
                            adding_course_2_slot(lab_counter, sum);
                        }
                    }
                }
                else if (course_list[j].duration == 3)
                {
                    if (course_list[j].elective == 0)
                    {
                        adding_course_3_slot(scheduling_only_CSE[course_list[j].semester - 1], sum, j);
                    }
                    else if (course_list[j].elective == 1)
                    {
                        adding_course_3_slot(elective_courses, sum, j);
                    }

                    adding_course_3_slot(teacher_scheduling_counter[teacher_index], sum);
                    if (course_list[j].type == 1)
                    {
                        for (k = 0; k < course_list[j].labHour; k++)
                        {
                            adding_course_3_slot(lab_counter, sum);
                        }
                    }
                }
            }

            //+TODO   dönem ici dekanlik/bolum dersi cakismasi
            for (j = 0; j < 8; j++)
            {
                obj[0] += calculate_collision2(scheduling_only_CSE[j], scheduling[j], 0);           /*collision of CSE&fac courses in semester*/
            }
            //+TODO	 donem ici bolum dersi cakismasi
            for (j = 0; j < 8; j++)
            {
                obj[0] += calculate_collision1(scheduling_only_CSE[j], 1);                          /*collision of only CSE courses in semester*/
            }
            //+TODO	dönemler arasi dekanlik/bolum dersi cakismasi--------------buna bak tekrar
            for (j = 1; j < 8; j++)
            {
                /*1-2  2-3  3-4  4-5  5-6  6-7  7-8
                2-1  3-2  4-3  5-4  6-5  7-6  8-7*/
                obj[1] += calculate_collision2(scheduling_only_CSE[j - 1], scheduling[j], 0);           /*consecutive CSE&faculty courses*/
                obj[1] += calculate_collision2(scheduling_only_CSE[j], scheduling[j - 1], 0);           /*consecutive CSE&faculty courses*/
            }
            //+TODO	dönemler arası CSE çakışmaları
            for (j = 1; j < 8; j++)
            {
                obj[1] += calculate_collision7(scheduling_only_CSE[j - 1], scheduling_only_CSE[j], 0);  /*consecutive only CSE courses*/
            }
            //+TODO	aynı saatte 3'ten fazla lab olmaması lazim
            obj[0] += calculate_collision1(lab_counter, 4);                                         /*# of lab at most 4*/
            for (j = 0; j < teacher_list_size; j++)
            {
                if (!(teacher_list[j].Equals("ASSISTANT")))
                {
                    //+TODO	og. gor. aynı saatte baska dersinin olmaması
                    obj[0] += calculate_collision1(teacher_scheduling_counter[j], 1);
                    /*teacher course collision*/
                    //+TODO	og. gor. gunluk 4 saatten fazla pespese dersinin olmamasi
                    obj[2] += calculate_collision3(teacher_scheduling_counter[j], 4);
                    /*teacher have at most 4 consective lesson per day*/
                    //+TODO	og. gor. boş gununun olması
                    obj[2] += calculate_collision4(teacher_scheduling_counter[j]);
                    /* teacher have free day*/
                }
            }
            //+TODO	lab ve lecture farklı günlerde olsun
            for (j = 0; j < 8; j++)
            {
                obj[2] += calculate_collision6(scheduling_only_CSE[j]);                             /*lab lecture hours must be in seperate day*/
            }
            //+TODO	lab miktarı kadar lab_scheduling'i artır
            //+TODO	seçmeliler için ayrı tablo tutup ayrı fonksiyonlarla çakışmaları kontrol et.
            //+TODO	secmelilerin hangi donemlere eklenecegi ve hangi donemlerle cakismamasi istendiği?
            obj[0] += calculate_collision1(elective_courses, 1);                            /*elective courses*/
            obj[2] += calculate_collision2(elective_courses, scheduling[5], 0);             /*elective+faculty courses in semester(consecutive)*/
            obj[2] += calculate_collision2(elective_courses, scheduling[6], 0);             /*elective+faculty courses in semester*/
            obj[2] += calculate_collision2(elective_courses, scheduling[7], 0);             /*elective+faculty courses in semester*/
            obj[1] += calculate_collision7(scheduling_only_CSE[5], elective_courses, 0);    /*CSE+elective courses(consecutive)*/
            obj[0] += calculate_collision7(scheduling_only_CSE[6], elective_courses, 0);    /*CSE+elective courses*/
            obj[0] += calculate_collision7(scheduling_only_CSE[7], elective_courses, 0);    /*CSE+elective courses*/
                                                                                            //+TODO	toplanti saatleri hocaların tablosuna da eklensin
                                                                                            //TODO	dekanlık derslerinin sectionları
                                                                                            //+TODO	obj[2] kontrol et.
            return;
        }
        #endregion

        #region functions.c
        /* Reset ith semester table*/
        static void reset(int[,] array)
        {
            int i, j;
            for (i = 0; i < 5; i++)
            {
                for (j = 0; j < 9; j++)
                {
                    array[i, j] = 0;
                }
            }
        }

        /* Copy second array to first array*/
        static void copy_array(int[,] array2, int[,] array1)
        {
            Array.Copy(array1, array2, 5 * 9);
            //int i, j;
            //for (i = 0; i < 5; i++)
            //{
            //    for (j = 0; j < 9; j++)
            //    {
            //        array[i][j] = array1[i][j];
            //    }
            //}
        }

        /* filling scheduling table for 1-hour class by using slot number 
        9-10	-	-	-	-	-
        10-11	-	-	-	-	-
        11-12	0	5	10	15	20
        12-13	1	6	11	16	21
        13-14	2	7	12	17	22
        14-15	-	-	-	-	-
        15-16	-	-	-	-	-
        16-17	3	8	13	18	23
        17-18	4	9	14	19	24
        */
        static void adding_course_1_slot(int[,] array, int slot)
        {
            if (slot % 5 < 3)
                array[slot / 5, (slot % 5) + 2]++;
            else
                array[slot / 5, (slot % 5) + 4]++;
        }
        static void adding_course_1_slot(List<int>[,] array, int slot, int i)
        {
            if (slot % 5 < 3)
                array[slot / 5, (slot % 5) + 2].Add(i);
            else
                array[slot / 5, (slot % 5) + 4].Add(i);
        }
        /*///////////////////////////////////////////////////////*/
        /* filling scheduling table for 2-hour class by using slot number 
        9-10	0	5	10	15	20
        10-11	-	-	-	-	-
        11-12	1	6	11	16	21
        12-13	2	7	12	17	22
        13-14	-	-	-	-	-
        14-15	3	8	13	18	23
        15-16	-	-	-	-	-
        16-17	4	9	14	19	24
        17-18	-	-	-	-	-
        */
        static void adding_course_2_slot(int[,] array, int slot)
        {
            int j = 0;
            if (slot % 5 == 0)
            {
                j = 0;
            }
            if (slot % 5 == 1)
            {
                j = 2;
            }
            if (slot % 5 == 2)
            {
                j = 3;
            }
            if (slot % 5 == 3)
            {
                j = 5;
            }
            if (slot % 5 == 4)
            {
                j = 7;
            }
            array[slot / 5, j]++;
            array[slot / 5, j + 1]++;
        }
        static void adding_course_2_slot(List<int>[,] array, int slot, int i)
        {
            int j = 0;
            if (slot % 5 == 0)
            {
                j = 0;
            }
            if (slot % 5 == 1)
            {
                j = 2;
            }
            if (slot % 5 == 2)
            {
                j = 3;
            }
            if (slot % 5 == 3)
            {
                j = 5;
            }
            if (slot % 5 == 4)
            {
                j = 7;
            }
            array[slot / 5, j].Add(i);
            array[slot / 5, j + 1].Add(i);
        }
        /*///////////////////////////////////////////////////////*/
        /* filling scheduling table for 3-hour class by using slot number 
        9-10	0	4	8	12	16
        10-11	-	-	-	-	-
        11-12	1	5	9	13	17
        12-13	-	-	-	-	-
        13-14	2	6	10	14	18
        14-15	3	7	11	15	19
        15-16	-	-	-	-	-
        16-17	-	-	-	-	-
        17-18	-	-	-	-	-
        */
        static void adding_course_3_slot(int[,] array, int slot)
        {
            int j = 0;
            if (slot % 4 == 0)
            {
                j = 0;
            }
            if (slot % 4 == 1)
            {
                j = 2;
            }
            if (slot % 4 == 2)
            {
                j = 4;
            }
            if (slot % 4 == 3)
            {
                j = 5;
            }
            array[slot / 4, j]++;
            array[slot / 4, j + 1]++;
            array[slot / 4, j + 2]++;
        }
        static void adding_course_3_slot(List<int>[,] array, int slot, int i)
        {
            int j = 0;
            if (slot % 4 == 0)
            {
                j = 0;
            }
            if (slot % 4 == 1)
            {
                j = 2;
            }
            if (slot % 4 == 2)
            {
                j = 4;
            }
            if (slot % 4 == 3)
            {
                j = 5;
            }
            array[slot / 4, j].Add(i);
            array[slot / 4, j + 1].Add(i);
            array[slot / 4, j + 2].Add(i);
        }

        static bool is_prerequisite(int pre_index_of_course_list, int post_index_of_course_list)
        {
            int i;
            for (i = 0; i < prerequisteList[post_index_of_course_list].Count; i++)
            {
                if (prerequisteList[post_index_of_course_list][i] == course_list[pre_index_of_course_list].code)
                {
                    return true;
                }
            }
            return false;
        }

        /* collision of CSE courses at the same time*/
        static int calculate_collision1(int[,] array, int minimum_collision)
        {
            int i, j, result = 0;
            for (i = 0; i < 5; i++)
            {
                for (j = 0; j < 9; j++)
                {
                    if (array[i, j] > minimum_collision)
                    {
                        result += array[i, j] - 1;
                    }
                }
            }
            return result;
        }
        static int calculate_collision1(List<int>[,] array, int minimum_collision)
        {
            int i, j, result = 0;
            for (i = 0; i < 5; i++)
            {
                for (j = 0; j < 9; j++)
                {
                    if (array[i, j].Count > minimum_collision)
                    {
                        result += array[i, j].Count - 1;
                    }
                }
            }
            return result;
        }
        ///*///////////////////////////////////////////////////////*/
        ///* collision of CSE courses -1 0(calculate_collision1) +1 semester*/
        /*
        int calculate_collision2(int[,] array1, int[,] array2, int minimum_collision)
        {
            int i, j, result = 0;
            for (i = 0; i < 5; i++)
            {
                for (j = 0; j < 9; j++)
                {
                    if (array1[i,j] > minimum_collision && array2[i,j] > minimum_collision)
                    {
                        result++;
                    }
                }
            }
            return result;
        }
        */
        static int calculate_collision2(List<int>[,] array1, int[,] array2, int minimum_collision)
        {
            int i, j, result = 0;
            for (i = 0; i < 5; i++)
            {
                for (j = 0; j < 9; j++)
                {
                    if ((int)array1[i, j].Count > minimum_collision && array2[i, j] > minimum_collision)
                    {
                        result += array1[i, j].Count + array2[i, j] - 1;
                    }
                }
            }
            return result;
        }
        /*count consecutive 4(can be changed) hour for teachers table*/
        static int calculate_collision3(int[,] array, int max_consecutive_hour)
        {
            int counter;
            int i, j, result = 0;
            for (i = 0; i < 5; i++)
            {
                counter = 0;
                for (j = 0; j < 9; j++)
                {
                    if (array[i, j] > 0)
                    {
                        counter++;
                    }
                    else {
                        counter = 0;
                    }
                    if (counter >= max_consecutive_hour)
                    {
                        result++;
                    }
                }
            }
            return result;
        }

        /* if 1 day (or more) whole day is empty for teachers table   
            return 0  else return 1*/
        static int calculate_collision4(int[,] array)
        {
            int i, j, counter, tmp = 0;
            for (i = 0; i < 5; i++)
            {
                counter = 0;
                for (j = 0; j < 9; j++)
                {
                    if (array[i, j] > 0)
                    {
                        counter = 0;
                        break;
                    }
                    else
                        counter++;
                }
                if (counter == 9)
                    tmp++;
            }
            if (tmp == 0)
                return 1;
            else
                return 0;
        }

        /*if lecture and lab have been at the day slot return result; else 0; */
        static int calculate_collision5(int[,] array, int[,] array1)
        {
            int i, j, k, result = 0;
            for (i = 0; i < 5; i++)
            {
                for (j = 0; j < 9; j++)
                {
                    if (array[i, j] > 0)
                    {
                        for (k = j; k < 9; k++)
                        {
                            if (array1[i, k] > 0)
                            {
                                result++;
                                break;
                            }
                        }
                        break;
                    }
                    if (array1[i, j] > 0)
                    {
                        for (k = j; k < 9; k++)
                        {
                            if (array[i, k] > 0)
                            {
                                result++;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            return result;
        }

        static int calculate_collision6(List<int>[,] array)
        {
            int result = 0, i, j, k, type1, type2;
            List<int> day = new List<int>(5);
            for (i = 0; i < 5; i++)
            {
                for (j = 0; j < 9; j++)
                {
                    for (k = 0; k < (int)array[i, j].Count; k++)
                    {
                        day.Add(array[i, j][k]);
                    }
                }
                for (j = 0; j < (int)day.Count; j++)
                {
                    for (k = 0; k < (int)day.Count; k++)
                    {
                        if (j != k && course_list[j].code.Equals(course_list[k].code))
                        {
                            //result++;
                            type1 = course_list[j].type;
                            type2 = course_list[k].type;
                            if (type1 != type2 && type1 + type2 <= 1)
                            {
                                result++;
                            }
                        }
                    }
                }
                //day.clear();
            }
            return result;
        }

        static int calculate_collision7(List<int>[,] array1, List<int>[,] array2, int minimum_collision)
        {
            int i, j, k, l, result = 0;
            for (i = 0; i < 5; i++)
            {
                for (j = 0; j < 9; j++)
                {
                    if ((int)array1[i, j].Count > minimum_collision && (int)array2[i, j].Count > minimum_collision)
                    {

                        for (k = 0; k < (int)array2[i, j].Count; k++)
                        {
                            for (l = 0; l < (int)array1[i, j].Count; l++)
                            {
                                if (!is_prerequisite(array1[i, j][l], array2[i, j][k]))
                                {
                                    result++;
                                }
                            }
                        }

                    }
                }
            }
            return result;
        }
        #endregion

        #region tourselect.c
        /* Routine for tournament selection, it creates a new_pop from old_pop by performing tournament selection and the crossover */
        static void selection(Population old_pop, Population new_pop)
        {
            int[] a1, a2; //todo: optmizasyon
            int temp;
            int i;
            int rand;
            Individual parent1, parent2;
            a1 = new int[popsize];
            a2 = new int[popsize];
            for (i = 0; i < popsize; i++)
            {
                a1[i] = a2[i] = i;
            }
            for (i = 0; i < popsize; i++)
            {
                rand = randObj.rnd(i, popsize - 1);
                temp = a1[rand];
                a1[rand] = a1[i];
                a1[i] = temp;
                rand = randObj.rnd(i, popsize - 1);
                temp = a2[rand];
                a2[rand] = a2[i];
                a2[i] = temp;
            }
            for (i = 0; i < popsize; i += 4)
            {
                parent1 = tournament(old_pop.indList[a1[i]], old_pop.indList[a1[i + 1]]);
                parent2 = tournament(old_pop.indList[a1[i + 2]], old_pop.indList[a1[i + 3]]);
                crossover(parent1, parent2, new_pop.indList[i], new_pop.indList[i + 1]);
                parent1 = tournament(old_pop.indList[a2[i]], old_pop.indList[a2[i + 1]]);
                parent2 = tournament(old_pop.indList[a2[i + 2]], old_pop.indList[a2[i + 3]]);
                crossover(parent1, parent2, new_pop.indList[i + 2], new_pop.indList[i + 3]);
            }

            return;
        }

        /* Routine for binary tournament */
        static Individual tournament(Individual ind1, Individual ind2)
        {
            int flag;
            flag = check_dominance(ind1, ind2);
            if (flag == 1)
            {
                return (ind1);
            }
            if (flag == -1)
            {
                return (ind2);
            }
            if (ind1.crowd_dist > ind2.crowd_dist)
            {
                return (ind1);
            }
            if (ind2.crowd_dist > ind1.crowd_dist)
            {
                return (ind2);
            }
            if ((randObj.randomperc()) <= 0.5)
            {
                return (ind1);
            }
            else
            {
                return (ind2);
            }
        }
        #endregion

        #region crossover.c
        /* Function to cross two individuals */
        static void crossover(Individual parent1, Individual parent2, Individual child1, Individual child2)
        {
            if (nreal != 0)
            {
                realcross(parent1, parent2, child1, child2);
            }
            if (nbin != 0)
            {
                bincross(parent1, parent2, child1, child2);
            }
            return;
        }

        /* Routine for real variable SBX crossover */
        static void realcross(Individual parent1, Individual parent2, Individual child1, Individual child2)
        {
            int i;
            double rand;
            double y1, y2, yl, yu;
            double c1, c2;
            double alpha, beta, betaq;
            if (randObj.randomperc() <= pcross_real)
            {
                nrealcross++;
                for (i = 0; i < nreal; i++)
                {
                    if (randObj.randomperc() <= 0.5)
                    {
                        if (Math.Abs(parent1.xreal[i] - parent2.xreal[i]) > EPS)
                        {
                            if (parent1.xreal[i] < parent2.xreal[i])
                            {
                                y1 = parent1.xreal[i];
                                y2 = parent2.xreal[i];
                            }
                            else
                            {
                                y1 = parent2.xreal[i];
                                y2 = parent1.xreal[i];
                            }
                            yl = min_realvar[i];
                            yu = max_realvar[i];
                            rand = randObj.randomperc();
                            beta = 1.0 + (2.0 * (y1 - yl) / (y2 - y1));
                            alpha = 2.0 - Math.Pow(beta, -(eta_c + 1.0));
                            if (rand <= (1.0 / alpha))
                            {
                                betaq = Math.Pow((rand * alpha), (1.0 / (eta_c + 1.0)));
                            }
                            else
                            {
                                betaq = Math.Pow((1.0 / (2.0 - rand * alpha)), (1.0 / (eta_c + 1.0)));
                            }
                            c1 = 0.5 * ((y1 + y2) - betaq * (y2 - y1));
                            beta = 1.0 + (2.0 * (yu - y2) / (y2 - y1));
                            alpha = 2.0 - Math.Pow(beta, -(eta_c + 1.0));
                            if (rand <= (1.0 / alpha))
                            {
                                betaq = Math.Pow((rand * alpha), (1.0 / (eta_c + 1.0)));
                            }
                            else
                            {
                                betaq = Math.Pow((1.0 / (2.0 - rand * alpha)), (1.0 / (eta_c + 1.0)));
                            }
                            c2 = 0.5 * ((y1 + y2) + betaq * (y2 - y1));
                            if (c1 < yl)
                                c1 = yl;
                            if (c2 < yl)
                                c2 = yl;
                            if (c1 > yu)
                                c1 = yu;
                            if (c2 > yu)
                                c2 = yu;
                            if (randObj.randomperc() <= 0.5)
                            {
                                child1.xreal[i] = c2;
                                child2.xreal[i] = c1;
                            }
                            else
                            {
                                child1.xreal[i] = c1;
                                child2.xreal[i] = c2;
                            }
                        }
                        else
                        {
                            child1.xreal[i] = parent1.xreal[i];
                            child2.xreal[i] = parent2.xreal[i];
                        }
                    }
                    else
                    {
                        child1.xreal[i] = parent1.xreal[i];
                        child2.xreal[i] = parent2.xreal[i];
                    }
                }
            }
            else
            {
                for (i = 0; i < nreal; i++)
                {
                    child1.xreal[i] = parent1.xreal[i];
                    child2.xreal[i] = parent2.xreal[i];
                }
            }
            return;
        }

        /* Routine for two point binary crossover */
        static void bincross(Individual parent1, Individual parent2, Individual child1, Individual child2)
        {
            int i, j;
            double rand;
            int temp, site1, site2;
            for (i = 0; i < nbin; i++)
            {
                rand = randObj.randomperc();
                if (rand <= pcross_bin)
                {
                    nbincross++;
                    site1 = randObj.rnd(0, nbits[i] - 1);
                    site2 = randObj.rnd(0, nbits[i] - 1);
                    if (site1 > site2)
                    {
                        temp = site1;
                        site1 = site2;
                        site2 = temp;
                    }
                    for (j = 0; j < site1; j++)
                    {
                        child1.gene[i, j] = parent1.gene[i, j];
                        child2.gene[i, j] = parent2.gene[i, j];
                    }
                    for (j = site1; j < site2; j++)
                    {
                        child1.gene[i, j] = parent2.gene[i, j];
                        child2.gene[i, j] = parent1.gene[i, j];
                    }
                    for (j = site2; j < nbits[i]; j++)
                    {
                        child1.gene[i, j] = parent1.gene[i, j];
                        child2.gene[i, j] = parent2.gene[i, j];
                    }
                }
                else
                {
                    for (j = 0; j < nbits[i]; j++)
                    {
                        child1.gene[i, j] = parent1.gene[i, j];
                        child2.gene[i, j] = parent2.gene[i, j];
                    }
                }
            }
            return;
        }
        #endregion

        #region dominance.c
        /* Routine for usual non-domination checking
   It will return the following values
   1 if a dominates b
   -1 if b dominates a
   0 if both a and b are non-dominated */
        static int check_dominance(Individual a, Individual b)
        {
            int i;
            int flag1;
            int flag2;
            flag1 = 0;
            flag2 = 0;
            if (a.constr_violation < 0 && b.constr_violation < 0)
            {
                if (a.constr_violation > b.constr_violation)
                {
                    return (1);
                }
                else
                {
                    if (a.constr_violation < b.constr_violation)
                    {
                        return (-1);
                    }
                    else
                    {
                        return (0);
                    }
                }
            }
            else
            {
                if (a.constr_violation < 0 && b.constr_violation == 0)
                {
                    return (-1);
                }
                else
                {
                    if (a.constr_violation == 0 && b.constr_violation < 0)
                    {
                        return (1);
                    }
                    else
                    {
                        for (i = 0; i < nobj; i++)
                        {
                            if (a.obj[i] < b.obj[i])
                            {
                                flag1 = 1;

                            }
                            else
                            {
                                if (a.obj[i] > b.obj[i])
                                {
                                    flag2 = 1;
                                }
                            }
                        }
                        if (flag1 == 1 && flag2 == 0)
                        {
                            return (1);
                        }
                        else
                        {
                            if (flag1 == 0 && flag2 == 1)
                            {
                                return (-1);
                            }
                            else
                            {
                                return (0);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region mutation.c
        /* Function to perform mutation in a population */
        static void mutation_pop(Population pop)
        {
            int i;
            for (i = 0; i < popsize; i++)
            {
                mutation_ind(pop.indList[i]);
            }
            return;
        }

        /* Function to perform mutation of an individual */
        static void mutation_ind(Individual ind)
        {
            if (nreal != 0)
            {
                real_mutate_ind(ind);
            }
            if (nbin != 0)
            {
                bin_mutate_ind(ind);
            }
            return;
        }

        /* Routine for binary mutation of an individual */
        static void bin_mutate_ind(Individual ind)
        {
            int j, k;
            double prob;
            for (j = 0; j < nbin; j++)
            {
                for (k = 0; k < nbits[j]; k++)
                {
                    prob = randObj.randomperc();
                    if (prob <= pmut_bin)
                    {
                        if (ind.gene[j, k] == 0)
                        {
                            ind.gene[j, k] = 1;
                        }
                        else
                        {
                            ind.gene[j, k] = 0;
                        }
                        nbinmut += 1;
                    }
                }
            }
            return;
        }

        /* Routine for real polynomial mutation of an individual */
        static void real_mutate_ind(Individual ind)
        {
            int j;
            double rnd, delta1, delta2, mut_pow, deltaq;
            double y, yl, yu, val, xy;
            for (j = 0; j < nreal; j++)
            {
                if (randObj.randomperc() <= pmut_real)
                {
                    y = ind.xreal[j];
                    yl = min_realvar[j];
                    yu = max_realvar[j];
                    delta1 = (y - yl) / (yu - yl);
                    delta2 = (yu - y) / (yu - yl);
                    rnd = randObj.randomperc();
                    mut_pow = 1.0 / (eta_m + 1.0);
                    if (rnd <= 0.5)
                    {
                        xy = 1.0 - delta1;
                        val = 2.0 * rnd + (1.0 - 2.0 * rnd) * (Math.Pow(xy, (eta_m + 1.0)));
                        deltaq = Math.Pow(val, mut_pow) - 1.0;
                    }
                    else
                    {
                        xy = 1.0 - delta2;
                        val = 2.0 * (1.0 - rnd) + 2.0 * (rnd - 0.5) * (Math.Pow(xy, (eta_m + 1.0)));
                        deltaq = 1.0 - (Math.Pow(val, mut_pow));
                    }
                    y = y + deltaq * (yu - yl);
                    if (y < yl)
                        y = yl;
                    if (y > yu)
                        y = yu;
                    ind.xreal[j] = y;
                    nrealmut += 1;
                }
            }
            return;
        }
        #endregion

        #region report.c

        /* Function to print the information of a population in a file */
        static void report_pop(Population pop, StreamWriter writer)
        {
            int i, j, k;
            for (i = 0; i < popsize; i++)
            {
                for (j = 0; j < nobj; j++)
                {
                    writer.Write($"{pop.indList[i].obj[j].ToString("E")}\t");
                }
                if (ncon != 0)
                {
                    for (j = 0; j < ncon; j++)
                    {
                        writer.Write($"{pop.indList[i].constr[j].ToString("E")}\t");
                    }
                }
                if (nreal != 0)
                {
                    for (j = 0; j < nreal; j++)
                    {
                        writer.Write($"{pop.indList[i].xreal[j].ToString("E")}\t");
                    }
                }
                if (nbin != 0)
                {
                    for (j = 0; j < nbin; j++)
                    {
                        for (k = 0; k < nbits[j]; k++)
                        {
                            writer.Write($"{pop.indList[i].gene[j, k]}\t");
                        }
                    }
                }
                writer.Write($"{pop.indList[i].constr_violation.ToString("E")}\t");
                writer.Write($"{pop.indList[i].rank}\t");
                writer.Write($"{pop.indList[i].crowd_dist.ToString("E")}\n");
            }
            return;
        }

        /* Function to print the information of feasible and non-dominated population in a file */
        static void report_feasible(Population pop, StreamWriter writer)
        {
            int i, j, k;
            for (i = 0; i < popsize; i++)
            {
                if (pop.indList[i].constr_violation == 0.0 && pop.indList[i].rank == 1)
                {
                    for (j = 0; j < nobj; j++)
                    {
                        writer.Write($"{pop.indList[i].obj[j].ToString("E")}\t");
                    }
                    if (ncon != 0)
                    {
                        for (j = 0; j < ncon; j++)
                        {
                            writer.Write($"{pop.indList[i].constr[j].ToString("E")}\t");
                        }
                    }
                    if (nreal != 0)
                    {
                        for (j = 0; j < nreal; j++)
                        {
                            writer.Write($"{pop.indList[i].xreal[j].ToString("E")}\t");
                        }
                    }
                    if (nbin != 0)
                    {
                        for (j = 0; j < nbin; j++)
                        {
                            for (k = 0; k < nbits[j]; k++)
                            {
                                writer.Write($"{pop.indList[i].gene[j, k]}\t");
                            }
                        }
                    }
                    writer.Write($"{pop.indList[i].constr_violation.ToString("E")}\t");
                    writer.Write($"{pop.indList[i].rank}\t");
                    writer.Write($"{ pop.indList[i].crowd_dist.ToString("E")}\n");
                }
            }
            return;
        }

        #endregion

        #region list.c
        /* Insert an element X into the list at location specified by NODE */
        static void insert(Lists node, int x)
        {
            Lists temp;
            if (node == null)
            {
                Console.WriteLine(" Error!! asked to enter after a null pointer, hence exiting \n");
                Environment.Exit(1);
            }
            temp = new Lists();
            temp.index = x;
            temp.child = node.child;
            temp.parent = node;
            if (node.child != null)
            {
                node.child.parent = temp;
            }
            node.child = temp;
            return;
        }

        /* Delete the node NODE from the list */
        static Lists del(Lists node)
        {
            Lists temp;
            if (node == null)
            {
                Console.WriteLine(" Error!! asked to enter after a null pointer, hence exiting \n");
                Environment.Exit(1);
            }
            temp = node.parent;
            temp.child = node.child;
            if (temp.child != null)
            {
                temp.child.parent = temp;
            }
            //free(node);
            return (temp);
        }
        #endregion

        #region rank.c
        /* Function to assign rank and crowding distance to a population of size pop_size*/
        static void assign_rank_and_crowding_distance(Population new_pop)
        {
            int flag;
            int i;
            int end;
            int front_size;
            int rank = 1;
            Lists orig;
            Lists cur;
            Lists temp1, temp2;
            orig = new Lists();
            cur = new Lists();

            front_size = 0;
            orig.index = -1;
            orig.parent = null;
            orig.child = null;
            cur.index = -1;
            cur.parent = null;
            cur.child = null;
            temp1 = orig;
            for (i = 0; i < popsize; i++)
            {
                insert(temp1, i);
                temp1 = temp1.child;
            }
            do
            {
                if (orig.child.child == null)
                {
                    new_pop.indList[orig.child.index].rank = rank;
                    new_pop.indList[orig.child.index].crowd_dist = INF;
                    break;
                }
                temp1 = orig.child;
                insert(cur, temp1.index);
                front_size = 1;
                temp2 = cur.child;
                temp1 = del(temp1);
                temp1 = temp1.child;
                do
                {
                    temp2 = cur.child;
                    do
                    {
                        end = 0;
                        flag = check_dominance((new_pop.indList[temp1.index]), (new_pop.indList[temp2.index]));
                        if (flag == 1)
                        {
                            insert(orig, temp2.index);
                            temp2 = del(temp2);
                            front_size--;
                            temp2 = temp2.child;
                        }
                        if (flag == 0)
                        {
                            temp2 = temp2.child;
                        }
                        if (flag == -1)
                        {
                            end = 1;
                        }
                    }
                    while (end != 1 && temp2 != null);
                    if (flag == 0 || flag == 1)
                    {
                        insert(cur, temp1.index);
                        front_size++;
                        temp1 = del(temp1);
                    }
                    temp1 = temp1.child;
                }
                while (temp1 != null);
                temp2 = cur.child;
                do
                {
                    new_pop.indList[temp2.index].rank = rank;
                    temp2 = temp2.child;
                }
                while (temp2 != null);
                assign_crowding_distance_list(new_pop, cur.child, front_size);
                temp2 = cur.child;
                do
                {
                    temp2 = del(temp2);
                    temp2 = temp2.child;
                }
                while (cur.child != null);
                rank += 1;
            }
            while (orig.child != null);

            //free(orig);
            //free(cur);
            return;
        }
        #endregion

        #region crowddist.c
        /* Routine to compute crowding distance based on ojbective function values when the population in in the form of a list */
        static void assign_crowding_distance_list(Population pop, Lists lst, int front_size)
        {
            int[][] obj_array;
            int[] dist;
            int i, j;
            Lists temp;
            temp = lst;
            if (front_size == 1)
            {
                pop.indList[lst.index].crowd_dist = INF;
                return;
            }
            if (front_size == 2)
            {
                pop.indList[lst.index].crowd_dist = INF;
                pop.indList[lst.child.index].crowd_dist = INF;
                return;
            }
            dist = new int[front_size];
            obj_array = new int[nobj][];
            //obj_array = (int**)malloc(nobj * sizeof(int*));
            for (i = 0; i < nobj; i++)
            {
                obj_array[i] = new int[front_size];
            }
            for (j = 0; j < front_size; j++)
            {
                dist[j] = temp.index;
                temp = temp.child;
            }
            assign_crowding_distance(pop, dist, obj_array, front_size);
            //free(dist);
            //for (i = 0; i < nobj; i++)
            //{
            //    free(obj_array[i]);
            //}
            //free(obj_array);
            return;
        }

        /* Routine to compute crowding distance based on objective function values when the population in in the form of an array */
        static void assign_crowding_distance_indices(Population pop, int c1, int c2)
        {
            int[][] obj_array;
            int[] dist;
            int i, j;
            int front_size;
            front_size = c2 - c1 + 1;
            if (front_size == 1)
            {
                pop.indList[c1].crowd_dist = INF;
                return;
            }
            if (front_size == 2)
            {
                pop.indList[c1].crowd_dist = INF;
                pop.indList[c2].crowd_dist = INF;
                return;
            }
            dist = new int[front_size];
            obj_array = new int[nobj][];
            //obj_array = (int**)malloc(nobj * sizeof(int*));
            for (i = 0; i < nobj; i++)
            {
                obj_array[i] = new int[front_size];
            }

            for (j = 0; j < front_size; j++)
            {
                dist[j] = c1++;
            }
            assign_crowding_distance(pop, dist, obj_array, front_size);
            //free(dist);
            //for (i = 0; i < nobj; i++)
            //{
            //    free(obj_array[i]);
            //}
            //free(obj_array);
            return;
        }

        /* Routine to compute crowding distances */
        static void assign_crowding_distance(Population pop, int[] dist, int[][] obj_array, int front_size)
        {
            int i, j;
            for (i = 0; i < nobj; i++)
            {
                for (j = 0; j < front_size; j++)
                {
                    obj_array[i][j] = dist[j];
                }
                quicksort_front_obj(pop, i, obj_array[i], front_size);
            }
            for (j = 0; j < front_size; j++)
            {
                pop.indList[dist[j]].crowd_dist = 0.0;
            }
            for (i = 0; i < nobj; i++)
            {
                pop.indList[obj_array[i][0]].crowd_dist = INF;
            }
            for (i = 0; i < nobj; i++)
            {
                for (j = 1; j < front_size - 1; j++)
                {
                    if (pop.indList[obj_array[i][j]].crowd_dist != INF)
                    {
                        if (pop.indList[obj_array[i][front_size - 1]].obj[i] == pop.indList[obj_array[i][0]].obj[i])
                        {
                            pop.indList[obj_array[i][j]].crowd_dist += 0.0;
                        }
                        else
                        {
                            pop.indList[obj_array[i][j]].crowd_dist += (pop.indList[obj_array[i][j + 1]].obj[i] - pop.indList[obj_array[i][j - 1]].obj[i]) / (pop.indList[obj_array[i][front_size - 1]].obj[i] - pop.indList[obj_array[i][0]].obj[i]);
                        }
                    }
                }
            }
            for (j = 0; j < front_size; j++)
            {
                if (pop.indList[dist[j]].crowd_dist != INF)
                {
                    pop.indList[dist[j]].crowd_dist = (pop.indList[dist[j]].crowd_dist) / nobj;
                }
            }
            return;
        }
        #endregion

        #region sort.c
        /* Randomized quick sort routine to sort a population based on a particular objective chosen */
        static void quicksort_front_obj(Population pop, int objcount, int[] obj_array, int obj_array_size)
        {
            q_sort_front_obj(pop, objcount, obj_array, 0, obj_array_size - 1);
            return;
        }

        /* Actual implementation of the randomized quick sort used to sort a population based on a particular objective chosen */
        static void q_sort_front_obj(Population pop, int objcount, int[] obj_array, int left, int right)
        {
            int index;
            int temp;
            int i, j;
            double pivot;
            if (left < right)
            {
                index = randObj.rnd(left, right);
                temp = obj_array[right];
                obj_array[right] = obj_array[index];
                obj_array[index] = temp;
                pivot = pop.indList[obj_array[right]].obj[objcount];
                i = left - 1;
                for (j = left; j < right; j++)
                {
                    if (pop.indList[obj_array[j]].obj[objcount] <= pivot)
                    {
                        i += 1;
                        temp = obj_array[j];
                        obj_array[j] = obj_array[i];
                        obj_array[i] = temp;
                    }
                }
                index = i + 1;
                temp = obj_array[index];
                obj_array[index] = obj_array[right];
                obj_array[right] = temp;
                q_sort_front_obj(pop, objcount, obj_array, left, index - 1);
                q_sort_front_obj(pop, objcount, obj_array, index + 1, right);
            }
            return;
        }

        /* Randomized quick sort routine to sort a population based on crowding distance */
        static void quicksort_dist(Population pop, int[] dist, int front_size)
        {
            q_sort_dist(pop, dist, 0, front_size - 1);
            return;
        }

        /* Actual implementation of the randomized quick sort used to sort a population based on crowding distance */
        static void q_sort_dist(Population pop, int[] dist, int left, int right)
        {
            int index;
            int temp;
            int i, j;
            double pivot;
            if (left < right)
            {
                index = randObj.rnd(left, right);
                temp = dist[right];
                dist[right] = dist[index];
                dist[index] = temp;
                pivot = pop.indList[dist[right]].crowd_dist;
                i = left - 1;
                for (j = left; j < right; j++)
                {
                    if (pop.indList[dist[j]].crowd_dist <= pivot)
                    {
                        i += 1;
                        temp = dist[j];
                        dist[j] = dist[i];
                        dist[i] = temp;
                    }
                }
                index = i + 1;
                temp = dist[index];
                dist[index] = dist[right];
                dist[right] = temp;
                q_sort_dist(pop, dist, left, index - 1);
                q_sort_dist(pop, dist, index + 1, right);
            }
            return;
        }
        #endregion

        #region merge.c

        /* Routine to merge two populations into one */
        static void merge(Population pop1, Population pop2, Population pop3)
        {
            int i, k;
            for (i = 0; i < popsize; i++)
            {
                copy_ind(pop1.indList[i], pop3.indList[i]);
            }
            for (i = 0, k = popsize; i < popsize; i++, k++)
            {
                copy_ind(pop2.indList[i], pop3.indList[k]);
            }
            return;
        }

        /* Routine to copy an individual 'ind1' into another individual 'ind2' */
        static void copy_ind(Individual ind1, Individual ind2)
        {
            int i, j;
            ind2.rank = ind1.rank;
            ind2.constr_violation = ind1.constr_violation;
            ind2.crowd_dist = ind1.crowd_dist;
            if (nreal != 0)
            {
                for (i = 0; i < nreal; i++)
                {
                    ind2.xreal[i] = ind1.xreal[i];
                }
            }
            if (nbin != 0)
            {
                for (i = 0; i < nbin; i++)
                {
                    ind2.xbin[i] = ind1.xbin[i];
                    for (j = 0; j < nbits[i]; j++)
                    {
                        ind2.gene[i, j] = ind1.gene[i, j];
                    }
                }
            }
            for (i = 0; i < nobj; i++)
            {
                ind2.obj[i] = ind1.obj[i];
            }
            if (ncon != 0)
            {
                for (i = 0; i < ncon; i++)
                {
                    ind2.constr[i] = ind1.constr[i];
                }
            }
            return;
        }

        #endregion

        #region fillnds.c

        /* Routine to perform non-dominated sorting */
        static void fill_nondominated_sort(Population mixed_pop, Population new_pop)
        {
            int flag;
            int i, j;
            int end;
            int front_size;
            int archieve_size;
            int rank = 1;
            Lists pool;
            Lists elite;
            Lists temp1, temp2;
            pool = new Lists();
            elite = new Lists();
            front_size = 0;
            archieve_size = 0;
            pool.index = -1;
            pool.parent = null;
            pool.child = null;
            elite.index = -1;
            elite.parent = null;
            elite.child = null;
            temp1 = pool;
            for (i = 0; i < 2 * popsize; i++)
            {
                insert(temp1, i);
                temp1 = temp1.child;
            }
            i = 0;
            do
            {
                temp1 = pool.child;
                insert(elite, temp1.index);
                front_size = 1;
                temp2 = elite.child;
                temp1 = del(temp1);
                temp1 = temp1.child;
                do
                {
                    temp2 = elite.child;
                    if (temp1 == null)
                    {
                        break;
                    }
                    do
                    {
                        end = 0;
                        flag = check_dominance(mixed_pop.indList[temp1.index], mixed_pop.indList[temp2.index]);
                        if (flag == 1)
                        {
                            insert(pool, temp2.index);
                            temp2 = del(temp2);
                            front_size--;
                            temp2 = temp2.child;
                        }
                        if (flag == 0)
                        {
                            temp2 = temp2.child;
                        }
                        if (flag == -1)
                        {
                            end = 1;
                        }
                    }
                    while (end != 1 && temp2 != null);
                    if (flag == 0 || flag == 1)
                    {
                        insert(elite, temp1.index);
                        front_size++;
                        temp1 = del(temp1);
                    }
                    temp1 = temp1.child;
                }
                while (temp1 != null);
                temp2 = elite.child;
                j = i;
                if ((archieve_size + front_size) <= popsize)
                {
                    do
                    {
                        copy_ind(mixed_pop.indList[temp2.index], new_pop.indList[i]);
                        new_pop.indList[i].rank = rank;
                        archieve_size += 1;
                        temp2 = temp2.child;
                        i += 1;
                    }
                    while (temp2 != null);
                    assign_crowding_distance_indices(new_pop, j, i - 1);
                    rank += 1;
                }
                else
                {
                    crowding_fill(mixed_pop, new_pop, i, front_size, elite);
                    archieve_size = popsize;
                    for (j = i; j < popsize; j++)
                    {
                        new_pop.indList[j].rank = rank;
                    }
                }
                temp2 = elite.child;
                do
                {
                    temp2 = del(temp2);
                    temp2 = temp2.child;
                }
                while (elite.child != null);
            }
            while (archieve_size < popsize);
            //while (pool != null)
            //{
            //    temp1 = pool;
            //    pool = pool.child;
            //    free(temp1);
            //}
            //while (elite != null)
            //{
            //    temp1 = elite;
            //    elite = elite.child;
            //    free(temp1);
            //}
            return;
        }

        /* Routine to fill a population with individuals in the decreasing order of crowding distance */
        static void crowding_fill(Population mixed_pop, Population new_pop, int count, int front_size, Lists elite)
        {
            int[] dist;
            Lists temp;
            int i, j;
            assign_crowding_distance_list(mixed_pop, elite.child, front_size);
            dist = new int[front_size];
            temp = elite.child;
            for (j = 0; j < front_size; j++)
            {
                dist[j] = temp.index;
                temp = temp.child;
            }
            quicksort_dist(mixed_pop, dist, front_size);
            for (i = count, j = front_size - 1; i < popsize; i++, j--)
            {
                copy_ind(mixed_pop.indList[dist[j]], new_pop.indList[i]);
            }
            //free(dist);
            return;
        }

        #endregion

        #region display.c
        ///* Function to display the current population for the subsequent generation */
        static void onthefly_display(Population pop, int genNo)
        {
            Console.WriteLine(" printing gnuplot");
            if (choice != 3)
            {
                var Xr = new double[popsize];
                var Yr = new double[popsize];

                for (int x = 0; x < popsize; x++)
                {
                    Xr[x] = pop.indList[x].obj[obj1 - 1];
                    Yr[x] = pop.indList[x].obj[obj2 - 1];
                }

                GnuPlot.Plot(Xr, Yr, $"title 'Generation #{genNo}' pt 1");
            }
            else
            {
                var Xr = new double[popsize];
                var Yr = new double[popsize];
                var Zr = new double[popsize];

                for (int x = 0; x < popsize; x++)
                {
                    Xr[x] = pop.indList[x].obj[obj1 - 1];
                    Yr[x] = pop.indList[x].obj[obj2 - 1];
                    Zr[x] = pop.indList[x].obj[obj3 - 1];
                }

                GnuPlot.SPlot(Xr, Yr, Zr, $"title 'Generation #{genNo}' with points pointtype 8 lc rgb 'blue'");
            }
        }

        //void onthefly_display(Population pop, FILE* gp, int ii)
        //{
        //    int i;
        //    int flag;
        //    FILE* fpt;
        //    fpt = fopen("plot.out", "w");
        //    flag = 0;
        //    for (i = 0; i < popsize; i++)
        //    {
        //        if (pop.ind[i].constr_violation == 0)
        //        {
        //            if (choice != 3)
        //                fprintf(fpt, "%e\t%e\n", pop.ind[i].obj[obj1 - 1], pop.ind[i].obj[obj2 - 1]);
        //            else
        //                fprintf(fpt, "%e\t%e\t%e\n", pop.ind[i].obj[obj1 - 1], pop.ind[i].obj[obj2 - 1], pop.ind[i].obj[obj3 - 1]);
        //            fflush(fpt);
        //            flag = 1;
        //        }
        //    }
        //    if (flag == 0)
        //    {
        //        Console.WriteLine(" No feasible soln in this pop, hence no display");
        //    }
        //    else
        //    {
        //        Console.WriteLine(" printing gnuplot");
        //        if (choice != 3)
        //            fprintf(gp, "set title 'Generation #%d'\n unset key\n plot 'plot.out' w points pointtype 6 pointsize 1\n", ii);
        //        else
        //            fprintf(gp, "set title 'Generation #%d'\n set view %d,%d\n unset key\n splot 'plot.out' w points pointtype 6 pointsize 1\n", ii, angle1, angle2);
        //        fflush(gp);
        //    }
        //    fclose(fpt);
        //    return;
        //}

        #endregion

    }
}
