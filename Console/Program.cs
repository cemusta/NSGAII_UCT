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

        private static readonly ProblemDefinition Problem = new ProblemDefinition();
        public static readonly Randomization RandomizationObj = new Randomization();

        private static int GnuplotChoice;
        private static int GnuplotObjective1;
        private static int GnuplotObjective2;
        private static int GnuplotObjective3;
        private static int GnuplotAngle1;
        private static int GnuplotAngle2;

        private static int TeacherListSize;

        private static readonly List<int[,]> Scheduling = new List<int[,]>(8); //8 dönem, 5 gün, 9 ders            
        private static readonly int[,] LabScheduling = new int[5, 9]; // labda dönem tutulmuyor 
        private static readonly string[] TeacherList = new string[70]; //todo: dub hocalar olabiliyor.
        private static readonly int[,] Meeting = new int[5, 9]; // bölüm hocalarının ortak meeting saatleri.
        private static readonly string[] RecordList1 = new string[2];
        private static CourseDetail[] CourseList;

        private static List<List<string>> PrerequisteList;
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
                Scheduling.Add(new int[5, 9]);
            }

            Population parentPopulation;
            Population childPopulation;
            Population mixedPopulation;



            //Input files:
            FileStream courseListFile;
            FileStream inputSchedulingFile;
            FileStream inputLabsFile;
            FileStream meetingFile;
            FileStream prerequisiteFile;

            try  //todo better input handling
            {
                courseListFile = File.OpenRead("course_list.csv");
                inputSchedulingFile = File.OpenRead("scheduling.in");
                inputLabsFile = File.OpenRead("lab_list.in");
                meetingFile = File.OpenRead("Meeting.txt");
                prerequisiteFile = File.OpenRead("Onkosul-list.csv");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Input file missing.\n EX: {ex.FileName}");
                return;
            }

            #endregion

            #region scan input collective
            Console.WriteLine("scanning input collective\n");
            StreamReader reader = new StreamReader(inputSchedulingFile);
            //var parts = input_collective.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string line;
            try
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        line = reader.ReadLine();
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int k = 0; k < 5; k++)
                        {
                            //fscanf(input_collective, "%d", scheduling[i][k][j]);
                            Scheduling[i][k, j] = int.Parse(parts[k]);
                        }
                    }
                    reader.ReadLine(); //trailing \n
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
            reader = new StreamReader(inputLabsFile);
            for (int j = 0; j < 9; j++)
            {
                line = reader.ReadLine();
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int k = 0; k < 5; k++)
                {
                    LabScheduling[k, j] = int.Parse(parts[k]);
                }
            }
            #endregion

            #region scan meeting file
            Console.WriteLine("scanning meeting file\n");
            reader = new StreamReader(meetingFile);
            for (int j = 0; j < 9; j++)
            {
                line = reader.ReadLine();
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int k = 0; k < 5; k++)
                {
                    Meeting[k, j] = int.Parse(parts[k]);
                }
            }
            #endregion

            #region scan course list
            reader = new StreamReader(courseListFile);

            line = reader.ReadLine();
            int courseCount = int.Parse(line); //43 gibi bir sayı dönüyor
            CourseList = new CourseDetail[courseCount]; //corse list için alan al.
            Console.WriteLine($"SIZE: {courseCount} \n");

            for (int courseId = 0; courseId < courseCount; courseId++)
            {
                line = reader.ReadLine();
                //Console.WriteLine($"{line}\n");

                var parts = line.Split(';');
                // token = strtok(record, ";");

                for (int i = 0; i < parts.Length; i++)
                {
                    Console.WriteLine($"{i}.{parts[i]}");
                }
                Console.WriteLine();

                CourseList[courseId] = new CourseDetail(parts[0], parts[1], int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]), int.Parse(parts[6]));

                int j;
                for (j = 0; j < courseId; j++)
                {
                    if (CourseList[j].Teacher == parts[1])
                    {
                        break; //niye?
                    }
                }

                if (j == courseId)
                {
                    TeacherList[TeacherListSize] = parts[1];
                    TeacherListSize++;
                }

            }
            Console.WriteLine($"teacher size: {TeacherListSize}");
            #endregion

            #region scan preqeuiste courses
            PrerequisteList = new List<List<string>>(courseCount);
            for (int i = 0; i < courseCount; i++)
            {
                PrerequisteList.Add(new List<string>());
            }

            reader = new StreamReader(prerequisiteFile);

            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(new char[] { ';' });

                for (int i = 0; i < parts.Length; i++)
                {
                    RecordList1[i] = parts[i]; //todo: burası ancak 2 olabilir yoksa çakacak...
                }

                for (int i = 0; i < courseCount; i++)
                {
                    if (CourseList[i].Code == RecordList1[0])
                    {
                        PrerequisteList[i].Add(RecordList1[1]);
                    }
                }
            }
            #endregion

            #region init file writers
            // Output files:
            var fpt1 = File.OpenWrite("initial_pop.out");
            var fpt2 = File.OpenWrite("final_pop.out");
            var fpt3 = File.OpenWrite("best_pop.out");
            var fpt4 = File.OpenWrite("all_pop.out");
            var fpt5 = File.OpenWrite("params.out");

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
            #endregion

            #region problem relevant parameter inputs

            string consoleIn;

            Console.WriteLine(" Enter the problem relevant and algorithm relevant parameters ... ");
            Console.WriteLine(" Enter the population size (a multiple of 4) : ");
            consoleIn = Console.ReadLine();
            Problem.PopulationSize = int.Parse(consoleIn);
            if (Problem.PopulationSize < 4 || Problem.PopulationSize % 4 != 0)
            {
                Console.WriteLine($" population size read is : {Problem.PopulationSize}");
                Console.WriteLine(" Wrong population size entered, hence exiting \n");
                return;
            }

            Console.WriteLine(" Enter the number of generations : ");
            consoleIn = Console.ReadLine();
            Problem.GenCount = int.Parse(consoleIn);
            if (Problem.GenCount < 1)
            {
                Console.WriteLine($" number of generations read is : {Problem.GenCount}");
                Console.WriteLine(" Wrong nuber of generations entered, hence exiting \n");
                return;
            }

            Console.WriteLine(" Enter the number of objectives : ");
            consoleIn = Console.ReadLine();
            Problem.ObjectiveCount = int.Parse(consoleIn);
            if (Problem.ObjectiveCount < 1)
            {
                Console.WriteLine($" number of objectives entered is : {Problem.ObjectiveCount}");
                Console.WriteLine(" Wrong number of objectives entered, hence exiting \n");
                return;
            }

            Console.WriteLine("\n Enter the number of constraints : ");
            consoleIn = Console.ReadLine();
            Problem.ConstraintCount = int.Parse(consoleIn);
            if (Problem.ConstraintCount < 0)
            {
                Console.WriteLine($" number of constraints entered is : {Problem.ConstraintCount}");
                Console.WriteLine(" Wrong number of constraints enetered, hence exiting \n");
                return;
            }

            Console.WriteLine("\n Enter the number of real variables : ");
            consoleIn = Console.ReadLine();
            Problem.RealVariableCount = int.Parse(consoleIn);
            if (Problem.RealVariableCount < 0)
            {
                Console.WriteLine($" number of real variables entered is : {Problem.RealVariableCount}");
                Console.WriteLine(" Wrong number of variables entered, hence exiting \n");
                return;
            }


            if (Problem.RealVariableCount != 0)
            {
                Problem.min_realvar = new double[Problem.RealVariableCount];
                Problem.max_realvar = new double[Problem.RealVariableCount];
                for (int i = 0; i < Problem.RealVariableCount; i++)
                {
                    Console.WriteLine($" Enter the lower limit of real variable {i + 1} : ");
                    consoleIn = Console.ReadLine();
                    Problem.min_realvar[i] = double.Parse(consoleIn);
                    Console.WriteLine($" Enter the upper limit of real variable {i + 1} : ");
                    consoleIn = Console.ReadLine();
                    Problem.max_realvar[i] = double.Parse(consoleIn);
                    if (Problem.max_realvar[i] <= Problem.min_realvar[i])
                    {
                        Console.WriteLine(" Wrong limits entered for the min and max bounds of real variable, hence exiting \n");
                        return;
                    }
                }
                Console.WriteLine(" Enter the probability of Crossover of real variable (0.6-1.0) : ");
                consoleIn = Console.ReadLine();
                Problem.RealCrossoverProbability = double.Parse(consoleIn);
                if (Problem.RealCrossoverProbability < 0.0 || Problem.RealCrossoverProbability > 1.0)
                {
                    Console.WriteLine($" Probability of crossover entered is : {Problem.RealCrossoverProbability}");
                    Console.WriteLine(" Entered value of probability of Crossover of real variables is out of bounds, hence exiting \n");
                    return;
                }
                Console.WriteLine(" Enter the probablity of mutation of real variables (1/Problem.RealVariableCount) : ");
                consoleIn = Console.ReadLine();
                Problem.RealMutationProbability = double.Parse(consoleIn);
                if (Problem.RealMutationProbability < 0.0 || Problem.RealMutationProbability > 1.0)
                {
                    Console.WriteLine($" Probability of mutation entered is : {Problem.RealMutationProbability}");
                    Console.WriteLine(" Entered value of probability of mutation of real variables is out of bounds, hence exiting \n");
                    return;
                }
                Console.WriteLine(" Enter the value of distribution index for Crossover (5-20): ");
                consoleIn = Console.ReadLine();
                Problem.CrossoverDistributionIndex = double.Parse(consoleIn);
                if (Problem.CrossoverDistributionIndex <= 0)
                {
                    Console.WriteLine($" The value entered is : {Problem.CrossoverDistributionIndex}");
                    Console.WriteLine(" Wrong value of distribution index for Crossover entered, hence exiting \n");
                    return;
                }
                Console.WriteLine(" Enter the value of distribution index for mutation (5-50): ");
                consoleIn = Console.ReadLine();
                Problem.MutationDistributionIndex = double.Parse(consoleIn);
                if (Problem.MutationDistributionIndex <= 0)
                {
                    Console.WriteLine($" The value entered is : {Problem.MutationDistributionIndex}");
                    Console.WriteLine(" Wrong value of distribution index for mutation entered, hence exiting \n");
                    return;
                }
            }

            Console.WriteLine(" Enter the number of binary variables : ");
            consoleIn = Console.ReadLine();
            Problem.BinaryVariableCount = int.Parse(consoleIn);
            if (Problem.BinaryVariableCount < 0)
            {
                Console.WriteLine($" number of binary variables entered is : {Problem.BinaryVariableCount}");
                Console.WriteLine(" Wrong number of binary variables entered, hence exiting \n");
                return;
            }
            if (Problem.BinaryVariableCount != 0)
            {
                Problem.nbits = new int[Problem.BinaryVariableCount];
                Problem.min_binvar = new double[Problem.BinaryVariableCount];
                Problem.max_binvar = new double[Problem.BinaryVariableCount];
                for (int i = 0; i < Problem.BinaryVariableCount; i++)
                {
                    Console.WriteLine($" Enter the number of bits for binary variable {i + 1} :");
                    consoleIn = Console.ReadLine();
                    var parts = consoleIn.Split(new char[] { ' ' });
                    Problem.nbits[i] = int.Parse(parts[0]);
                    if (Problem.nbits[i] > Problem.MaxBitCount)
                        Problem.MaxBitCount = Problem.nbits[i];
                    if (Problem.nbits[i] < 1)
                    {
                        Console.WriteLine(" Wrong number of bits for binary variable entered, hence exiting");
                        return;
                    }
                    Console.WriteLine($" Enter the lower limit of binary variable {i + 1} :");
                    //consoleIn = Console.ReadLine();
                    Problem.min_binvar[i] = double.Parse(parts[1]);

                    Console.WriteLine($" Enter the upper limit of binary variable {i + 1} :");
                    //consoleIn = Console.ReadLine();
                    Problem.max_binvar[i] = double.Parse(parts[2]);
                    if (Problem.max_binvar[i] <= Problem.min_binvar[i])
                    {
                        Console.WriteLine(" Wrong limits entered for the min and max bounds of binary variable entered, hence exiting \n");
                        return;
                    }
                }
                Console.WriteLine(" Enter the probability of Crossover of binary variable (0.6-1.0): ");
                consoleIn = Console.ReadLine().Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                    .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                Problem.BinaryCrossoverProbability = double.Parse(consoleIn);
                if (Problem.BinaryCrossoverProbability < 0.0 || Problem.BinaryCrossoverProbability > 1.0)
                {
                    Console.WriteLine($" Probability of crossover entered is : {Problem.BinaryCrossoverProbability}");
                    Console.WriteLine(" Entered value of probability of Crossover of binary variables is out of bounds, hence exiting \n");
                    return;
                }
                Console.WriteLine(" Enter the probability of mutation of binary variables (1/Problem.nbits): ");
                consoleIn = Console.ReadLine().Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                    .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                Problem.BinaryMutationProbability = double.Parse(consoleIn);
                if (Problem.BinaryMutationProbability < 0.0 || Problem.BinaryMutationProbability > 1.0)
                {
                    Console.WriteLine($" Probability of mutation entered is :  {Problem.BinaryMutationProbability}");
                    Console.WriteLine(" Entered value of probability  of mutation of binary variables is out of bounds, hence exiting \n");
                    return;
                }
            }
            if (Problem.RealVariableCount == 0 && Problem.BinaryVariableCount == 0)
            {
                Console.WriteLine("\n Number of real as well as binary variables, both are zero, hence exiting \n");
                return;
            }

            Console.WriteLine(" Do you want to use gnuplot to display the results realtime (0 for NO) (1 for yes) : ");
            consoleIn = Console.ReadLine();
            GnuplotChoice = int.Parse(consoleIn);
            if (GnuplotChoice != 0 && GnuplotChoice != 1)
            {
                Console.WriteLine($" Entered the wrong choice, hence exiting, choice entered was {GnuplotChoice}\n");
                return;
            }
            if (GnuplotChoice == 1)
            {
                if (Problem.ObjectiveCount == 2)
                {
                    Console.WriteLine(" Enter the objective for X axis display : ");
                    consoleIn = Console.ReadLine();
                    GnuplotObjective1 = int.Parse(consoleIn);
                    if (GnuplotObjective1 < 1 || GnuplotObjective1 > Problem.ObjectiveCount)
                    {
                        Console.WriteLine($" Wrong value of X objective entered, value entered was {GnuplotObjective1}\n");
                        return;
                    }
                    Console.WriteLine(" Enter the objective for Y axis display : ");
                    consoleIn = Console.ReadLine();
                    GnuplotObjective2 = int.Parse(consoleIn);
                    if (GnuplotObjective2 < 1 || GnuplotObjective2 > Problem.ObjectiveCount)
                    {
                        Console.WriteLine($" Wrong value of Y objective entered, value entered was {GnuplotObjective2}\n");
                        return;
                    }
                    GnuplotObjective3 = -1;
                }
                else
                {
                    Console.WriteLine(" #obj > 2, 2D display or a 3D display ?, enter 2 for 2D and 3 for 3D :");

                    consoleIn = Console.ReadLine();
                    GnuplotChoice = int.Parse(consoleIn);
                    if (GnuplotChoice != 2 && GnuplotChoice != 3)
                    {
                        Console.WriteLine($" Entered the wrong choice, hence exiting, choice entered was {GnuplotChoice}\n");
                        return;
                    }
                    if (GnuplotChoice == 2)
                    {
                        Console.WriteLine(" Enter the objective for X axis display : ");
                        consoleIn = Console.ReadLine();
                        GnuplotObjective1 = int.Parse(consoleIn);
                        if (GnuplotObjective1 < 1 || GnuplotObjective1 > Problem.ObjectiveCount)
                        {
                            Console.WriteLine($" Wrong value of X objective entered, value entered was {GnuplotObjective1}\n");
                            return;
                        }
                        Console.WriteLine(" Enter the objective for Y axis display : ");
                        consoleIn = Console.ReadLine();
                        GnuplotObjective2 = int.Parse(consoleIn);
                        if (GnuplotObjective2 < 1 || GnuplotObjective2 > Problem.ObjectiveCount)
                        {
                            Console.WriteLine($" Wrong value of Y objective entered, value entered was {GnuplotObjective2}\n");
                            return;
                        }
                        GnuplotObjective3 = -1;
                    }
                    else
                    {
                        Console.WriteLine(" Enter the objective for X axis display : ");
                        consoleIn = Console.ReadLine();
                        GnuplotObjective1 = int.Parse(consoleIn);
                        if (GnuplotObjective1 < 1 || GnuplotObjective1 > Problem.ObjectiveCount)
                        {
                            Console.WriteLine($" Wrong value of X objective entered, value entered was {GnuplotObjective1}\n");
                            return;
                        }
                        Console.WriteLine(" Enter the objective for Y axis display : ");
                        consoleIn = Console.ReadLine();
                        GnuplotObjective2 = int.Parse(consoleIn);
                        if (GnuplotObjective2 < 1 || GnuplotObjective2 > Problem.ObjectiveCount)
                        {
                            Console.WriteLine($" Wrong value of Y objective entered, value entered was {GnuplotObjective2}\n");
                            return;
                        }
                        Console.WriteLine(" Enter the objective for Z axis display : ");
                        consoleIn = Console.ReadLine();
                        GnuplotObjective3 = int.Parse(consoleIn);
                        if (GnuplotObjective3 < 1 || GnuplotObjective3 > Problem.ObjectiveCount)
                        {
                            Console.WriteLine($" Wrong value of Z objective entered, value entered was {GnuplotObjective3}\n");
                            return;
                        }
                        Console.WriteLine(" You have chosen 3D display, hence location of eye required \n");
                        Console.WriteLine(" Enter the first angle (an integer in the range 0-180) (if not known, enter 60) :");
                        consoleIn = Console.ReadLine();
                        GnuplotAngle1 = int.Parse(consoleIn);
                        if (GnuplotAngle1 < 0 || GnuplotAngle1 > 180)
                        {
                            Console.WriteLine(" Wrong value for first angle entered, hence exiting \n");
                            return;
                        }
                        Console.WriteLine(" Enter the second angle (an integer in the range 0-360) (if not known, enter 30) :");
                        consoleIn = Console.ReadLine();
                        GnuplotAngle2 = int.Parse(consoleIn);
                        if (GnuplotAngle2 < 0 || GnuplotAngle2 > 360)
                        {
                            Console.WriteLine(" Wrong value for second angle entered, hence exiting \n");
                            return;
                        }
                    }
                }
            }
            Console.WriteLine("\n Input data successfully entered, now performing initialization \n");
            #endregion

            #region write down starting params into file

            writer5.WriteLine($" Population size = {Problem.PopulationSize}");
            writer5.WriteLine($" Number of generations = {Problem.GenCount}");
            writer5.WriteLine($" Number of objective functions = {Problem.ObjectiveCount}");
            writer5.WriteLine($" Number of constraints = {Problem.ConstraintCount}");
            writer5.WriteLine($" Number of real variables = {Problem.RealVariableCount}");
            if (Problem.RealVariableCount != 0)
            {
                for (int i = 0; i < Problem.RealVariableCount; i++)
                {
                    writer5.WriteLine($" Lower limit of real variable {i + 1} = {Problem.min_realvar[i]}");
                    writer5.WriteLine($" Upper limit of real variable {i + 1} = {Problem.max_realvar[i]}");
                }
                writer5.WriteLine($" Probability of crossover of real variable = {Problem.RealCrossoverProbability}");
                writer5.WriteLine($" Probability of mutation of real variable = {Problem.RealMutationProbability}");
                writer5.WriteLine($" Distribution index for crossover = {Problem.CrossoverDistributionIndex}");
                writer5.WriteLine($" Distribution index for mutation = {Problem.MutationDistributionIndex}");
            }
            writer5.Write($" Number of binary variables = {Problem.BinaryVariableCount}");
            if (Problem.BinaryVariableCount != 0)
            {
                for (int i = 0; i < Problem.BinaryVariableCount; i++)
                {
                    writer5.WriteLine($" Number of bits for binary variable {i + 1} = {Problem.nbits[i]}");
                    writer5.WriteLine($" Lower limit of binary variable {i + 1} = {Problem.min_binvar[i]}");
                    writer5.WriteLine($" Upper limit of binary variable {i + 1} = {Problem.max_binvar[i]}");
                }
                writer5.WriteLine($" Probability of crossover of binary variable = {Problem.BinaryCrossoverProbability}");
                writer5.WriteLine($" Probability of mutation of binary variable = {Problem.BinaryMutationProbability}");
            }
            writer5.Write($" Seed for random number generator = {seed}");
            Problem.TotalBinaryBitLength = 0;
            if (Problem.BinaryVariableCount != 0)
            {
                /*printf("Problem.BinaryVariableCount: %d \n", Problem.BinaryVariableCount);*/
                for (int i = 0; i < Problem.BinaryVariableCount; i++)
                {
                    Problem.TotalBinaryBitLength += Problem.nbits[i];
                    /*printf("Problem.nbits[%d]: %d \n", i,Problem.nbits[i]);*/
                }
            }

            writer1.Write($"# of objectives = {Problem.ObjectiveCount}, # of constraints = {Problem.ConstraintCount}, # of real_var = {Problem.RealVariableCount}, # of bits of bin_var = {Problem.TotalBinaryBitLength}, constr_violation, rank, crowding_distance\n");
            writer2.Write($"# of objectives = {Problem.ObjectiveCount}, # of constraints = {Problem.ConstraintCount}, # of real_var = {Problem.RealVariableCount}, # of bits of bin_var = {Problem.TotalBinaryBitLength}, constr_violation, rank, crowding_distance\n");
            writer3.Write($"# of objectives = {Problem.ObjectiveCount}, # of constraints = {Problem.ConstraintCount}, # of real_var = {Problem.RealVariableCount}, # of bits of bin_var = {Problem.TotalBinaryBitLength}, constr_violation, rank, crowding_distance\n");
            writer4.Write($"# of objectives = {Problem.ObjectiveCount}, # of constraints = {Problem.ConstraintCount}, # of real_var = {Problem.RealVariableCount}, # of bits of bin_var = {Problem.TotalBinaryBitLength}, constr_violation, rank, crowding_distance\n");
            Problem.BinaryMutationCount = 0;
            Problem.RealMutationCount = 0;
            Problem.BinaryCrossoverCount = 0;
            Problem.RealCrossoverCount = 0;

            writer1.Flush();
            writer2.Flush();
            writer3.Flush();
            writer4.Flush();
            writer5.Flush();

            #endregion

            #region first population
            parentPopulation = new Population(Problem.PopulationSize, Problem.RealVariableCount, Problem.BinaryVariableCount, Problem.MaxBitCount, Problem.ObjectiveCount, Problem.ConstraintCount);
            // (population*)malloc(sizeof(population));
            childPopulation = new Population(Problem.PopulationSize, Problem.RealVariableCount, Problem.BinaryVariableCount, Problem.MaxBitCount, Problem.ObjectiveCount, Problem.ConstraintCount);
            // (population*)malloc(sizeof(population));
            mixedPopulation = new Population(Problem.PopulationSize * 2, Problem.RealVariableCount, Problem.BinaryVariableCount, Problem.MaxBitCount, Problem.ObjectiveCount, Problem.ConstraintCount);
            // (population*)malloc(sizeof(population));


            RandomizationObj.Randomize();
            InitializePopulation(parentPopulation);
            Console.WriteLine(" Initialization done, now performing first generation");


            parentPopulation.Decode(Problem);

            evaluate_population(parentPopulation);
            assign_rank_and_crowding_distance(parentPopulation);
            ReportPopulation(parentPopulation, writer1);
            writer4.WriteLine("# gen = 1");
            ReportPopulation(parentPopulation, writer4);
            Console.WriteLine(" gen = 1");
            //fflush(stdout);
            if (GnuplotChoice != 0)
            {
                PlotPopulation(parentPopulation, 1);
            }
            writer1.Flush();
            writer2.Flush();
            writer3.Flush();
            writer4.Flush();
            writer5.Flush();

            #endregion

            #region generation loop
            for (int i = 2; i <= Problem.GenCount; i++)
            {
                Selection(parentPopulation, childPopulation);
                MutatePopulation(childPopulation);

                childPopulation.Decode(Problem);
                evaluate_population(childPopulation);
                MergePopulation(parentPopulation, childPopulation, mixedPopulation);
                fill_nondominated_sort(mixedPopulation, parentPopulation);

                /* Comment following four lines if information for all
                generations is not desired, it will speed up the execution */
                //*fprintf(fpt4,"# gen = %d\n",i);
                //ReportPopulation(parent_pop,fpt4);
                //fflush(fpt4);*/
                if (GnuplotChoice != 0)
                {
                    PlotPopulation(parentPopulation, i);
                }

                Console.WriteLine($" gen = {i}");
            }
            #endregion

            #region prepare final reports
            Console.WriteLine($" Generations finished, now reporting solutions");
            ReportPopulation(parentPopulation, writer2);
            ReportFeasiblePopulation(parentPopulation, writer3);

            if (Problem.RealVariableCount != 0)
            {
                writer5.WriteLine($" Number of crossover of real variable = {Problem.RealCrossoverCount}");
                writer5.WriteLine($" Number of mutation of real variable = {Problem.RealMutationCount}");
            }
            if (Problem.BinaryVariableCount != 0)
            {
                writer5.WriteLine($" Number of crossover of binary variable = {Problem.BinaryCrossoverCount}");
                writer5.WriteLine($" Number of mutation of binary variable = {Problem.BinaryMutationCount}");
            }
            #endregion

            #region close files
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

            courseListFile.Close();
            inputSchedulingFile.Close();
            inputLabsFile.Close();
            meetingFile.Close();
            prerequisiteFile.Close();
            #endregion

            Console.WriteLine("\n Routine successfully exited \n");

        } // main


        #region initialize.c
        /* Function to initialize a population randomly */
        static void InitializePopulation(Population pop)
        {
            for (int i = 0; i < Problem.PopulationSize; i++)
            {
                InitializeIndividual(pop.IndList[i]);
            }
        }

        /* Function to initialize an individual randomly */
        static void InitializeIndividual(Individual ind)
        {
            int j;
            if (Problem.RealVariableCount != 0)
            {
                for (j = 0; j < Problem.RealVariableCount; j++)
                {
                    ind.Xreal[j] = RandomizationObj.RandomDouble(Problem.min_realvar[j], Problem.max_realvar[j]);
                }
            }
            if (Problem.BinaryVariableCount != 0)
            {
                for (j = 0; j < Problem.BinaryVariableCount; j++)
                {
                    for (int k = 0; k < Problem.nbits[j]; k++)
                    {
                        if (RandomizationObj.RandomPercent() <= 0.5)
                        {
                            ind.Gene[j, k] = 0;
                        }
                        else
                        {
                            ind.Gene[j, k] = 1;
                        }
                    }
                }
            }
        }
        #endregion

        #region eval.c
        /* Routine to evaluate objective function values and constraints for a population */
        static void evaluate_population(Population pop)
        {
            for (int i = 0; i < Problem.PopulationSize; i++)
            {
                evaluate_individual(pop.IndList[i]);
            }
        }

        /* Routine to evaluate objective function values and constraints for an individual */
        static void evaluate_individual(Individual ind)
        {
            UCT_Evaluate(ind.Xbin, ind.Obj);
            if (Problem.ConstraintCount == 0)
            {
                ind.ConstrViolation = 0.0;
            }
            else
            {
                ind.ConstrViolation = 0.0;
                for (int j = 0; j < Problem.ConstraintCount; j++)
                {
                    if (ind.Constr[j] < 0.0)
                    {
                        ind.ConstrViolation += ind.Constr[j];
                    }
                }
            }
        }
        #endregion

        #region problemdef.c
        static void UCT_Evaluate(double[] xbin, double[] obj)
        {
            #region init variables
            // todo fix fix these stuff. dinamik olmamalı bunlar her seferinde? emin degilim...
            // en azından pop kadar kere yaratılmalı? pop içine taşınabilir?
            int sum, i, j, k;
            List<int[,]> teacherSchedulingCounter = new List<int[,]>(50); //todo teacher no kadar...
            for (i = 0; i < 50; i++)
            {
                teacherSchedulingCounter.Add(new int[5, 9]);
            }
            int teacherIndex = 0;
            int[,] labCounter = new int[5, 9];
            List<int>[][,] schedulingOnlyCse = new List<int>[8][,];
            for (i = 0; i < 8; i++)
            {
                schedulingOnlyCse[i] = new List<int>[5, 9];

                for (j = 0; j < 5; j++)
                {
                    for (k = 0; k < 9; k++)
                    {
                        schedulingOnlyCse[i][j, k] = new List<int>();
                    }
                }
            }
            List<int>[,] electiveCourses = new List<int>[5, 9];
            for (i = 0; i < 5; i++)
            {
                for (j = 0; j < 9; j++)
                {
                    electiveCourses[i, j] = new List<int>();
                }
            }

            obj[0] = 0;
            obj[1] = 0;
            obj[2] = 0;

            //reset scheduling, tearchers_scheduling and lab_scheduling
            for (i = 0; i < TeacherListSize; ++i)
            {
                Array.Copy(teacherSchedulingCounter[i], Meeting, 5 * 9);
            }

            //copy diaconate lab lessons
            Array.Copy(labCounter, LabScheduling, 5 * 9);
            #endregion

            #region fill variables
            for (j = 0; j < Problem.BinaryVariableCount; j++) //ders syaisi kadar.
            {
                for (i = 0; i < TeacherListSize; i++)
                {
                    if (TeacherList[i].Equals(CourseList[j].Teacher))
                    {
                        teacherIndex = i;  //todo: hashlist'e veya dict.'e çevirebiliriz?
                        break;
                    }
                } //ögretmen indeksini buluyor.


                sum = (int)xbin[j];

                if (CourseList[j].Duration == 1)
                {
                    if (CourseList[j].Elective == 0)
                    {
                        adding_course_1_slot(schedulingOnlyCse[CourseList[j].Semester - 1], sum, j);
                    }
                    else if (CourseList[j].Elective == 1)
                    {
                        adding_course_1_slot(electiveCourses, sum, j);
                    }

                    adding_course_1_slot(teacherSchedulingCounter[teacherIndex], sum);
                    if (CourseList[j].Type == 1)
                    {
                        for (k = 0; k < CourseList[j].LabHour; k++)
                        {
                            adding_course_1_slot(labCounter, sum);
                        }
                    }
                }
                else if (CourseList[j].Duration == 2)
                {
                    if (CourseList[j].Elective == 0)
                    {
                        adding_course_2_slot(schedulingOnlyCse[CourseList[j].Semester - 1], sum, j);
                    }
                    else if (CourseList[j].Elective == 1)
                    {
                        adding_course_2_slot(electiveCourses, sum, j);
                    }

                    adding_course_2_slot(teacherSchedulingCounter[teacherIndex], sum);
                    if (CourseList[j].Type == 1)
                    {
                        for (k = 0; k < CourseList[j].LabHour; k++)
                        {
                            adding_course_2_slot(labCounter, sum);
                        }
                    }
                }
                else if (CourseList[j].Duration == 3)
                {
                    if (CourseList[j].Elective == 0)
                    {
                        adding_course_3_slot(schedulingOnlyCse[CourseList[j].Semester - 1], sum, j);
                    }
                    else if (CourseList[j].Elective == 1)
                    {
                        adding_course_3_slot(electiveCourses, sum, j);
                    }

                    adding_course_3_slot(teacherSchedulingCounter[teacherIndex], sum);
                    if (CourseList[j].Type == 1)
                    {
                        for (k = 0; k < CourseList[j].LabHour; k++)
                        {
                            adding_course_3_slot(labCounter, sum);
                        }
                    }
                }
            }
            #endregion

            //+TODO   dönem ici dekanlik/bolum dersi cakismasi
            for (j = 0; j < 8; j++)
            {
                //collision of CSE&fac courses in semester
                obj[0] += calculate_collision2(schedulingOnlyCse[j], Scheduling[j], 0);
            }
            //+TODO	 donem ici bolum dersi cakismasi
            for (j = 0; j < 8; j++)
            {
                obj[0] += calculate_collision1(schedulingOnlyCse[j], 1);                          /*collision of only CSE courses in semester*/
            }
            //+TODO	dönemler arasi dekanlik/bolum dersi cakismasi--------------buna bak tekrar
            for (j = 1; j < 8; j++)
            {
                // 1-2  2-3  3-4  4-5  5-6  6-7  7-8
                // 2-1  3-2  4-3  5-4  6-5  7-6  8-7     consecutive CSE&faculty courses
                obj[1] += calculate_collision2(schedulingOnlyCse[j - 1], Scheduling[j], 0);  //cse derslerini bir sonraki dönem ile     
                obj[1] += calculate_collision2(schedulingOnlyCse[j], Scheduling[j - 1], 0);  //cse derslerini bir önceki dönem ile               
            }
            //+TODO	dönemler arası CSE çakışmaları
            for (j = 1; j < 8; j++)
            {
                obj[1] += calculate_collision7(schedulingOnlyCse[j - 1], schedulingOnlyCse[j], 0);  /*consecutive only CSE courses*/
            }
            //+TODO	aynı saatte 3'ten fazla lab olmaması lazim
            obj[0] += calculate_collision1(labCounter, 4);
            //# of lab at most 4 //todo: make input param.

            for (j = 0; j < TeacherListSize; j++)
            {
                if (!TeacherList[j].Equals("ASSISTANT"))
                {
                    //+TODO	og. gor. aynı saatte baska dersinin olmaması
                    obj[0] += calculate_collision1(teacherSchedulingCounter[j], 1);
                    /*teacher course collision*/
                    //+TODO	og. gor. gunluk 4 saatten fazla pespese dersinin olmamasi
                    obj[2] += calculate_collision3(teacherSchedulingCounter[j], 4);
                    /*teacher have at most 4 consective lesson per day*/
                    //+TODO	og. gor. boş gununun olması
                    obj[2] += calculate_collision4(teacherSchedulingCounter[j]);
                    /* teacher have free day*/
                }
            }
            //+TODO	lab ve lecture farklı günlerde olsun
            for (j = 0; j < 8; j++)
            {
                obj[2] += calculate_collision6(schedulingOnlyCse[j]);    /*lab lecture hours must be in seperate day*/
            }
            //+TODO	lab miktarı kadar lab_scheduling'i artır
            //+TODO	seçmeliler için ayrı tablo tutup ayrı fonksiyonlarla çakışmaları kontrol et.
            //+TODO	secmelilerin hangi donemlere eklenecegi ve hangi donemlerle cakismamasi istendiği?
            obj[0] += calculate_collision1(electiveCourses, 1);                            /*elective courses*/
            obj[2] += calculate_collision2(electiveCourses, Scheduling[5], 0);             /*elective+faculty courses in semester(consecutive)*/
            obj[2] += calculate_collision2(electiveCourses, Scheduling[6], 0);             /*elective+faculty courses in semester*/
            obj[2] += calculate_collision2(electiveCourses, Scheduling[7], 0);             /*elective+faculty courses in semester*/
            obj[1] += calculate_collision7(schedulingOnlyCse[5], electiveCourses, 0);    /*CSE+elective courses(consecutive)*/
            obj[0] += calculate_collision7(schedulingOnlyCse[6], electiveCourses, 0);    /*CSE+elective courses*/
            obj[0] += calculate_collision7(schedulingOnlyCse[7], electiveCourses, 0);    /*CSE+elective courses*/
            //+TODO	toplanti saatleri hocaların tablosuna da eklensin
            //TODO	dekanlık derslerinin sectionları??
            //+TODO	obj[2] kontrol et.
        }
        #endregion

        #region functions.c

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
                array[slot / 5, slot % 5 + 2]++;
            else
                array[slot / 5, slot % 5 + 4]++;
        }
        static void adding_course_1_slot(List<int>[,] array, int slot, int i)
        {
            if (slot % 5 < 3)
                array[slot / 5, slot % 5 + 2].Add(i);
            else
                array[slot / 5, slot % 5 + 4].Add(i);
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

        static bool is_prerequisite(int preIndexOfCourseList, int postIndexOfCourseList)
        {
            int i;
            for (i = 0; i < PrerequisteList[postIndexOfCourseList].Count; i++)
            {
                if (PrerequisteList[postIndexOfCourseList][i] == CourseList[preIndexOfCourseList].Code)
                {
                    return true;
                }
            }
            return false;
        }

        /* collision of CSE courses at the same time*/
        static int calculate_collision1(int[,] array, int minimumCollision)
        {
            int i, j, result = 0;
            for (i = 0; i < 5; i++)
            {
                for (j = 0; j < 9; j++)
                {
                    if (array[i, j] > minimumCollision)
                    {
                        result += array[i, j] - 1;
                    }
                }
            }
            return result;
        }
        static int calculate_collision1(List<int>[,] array, int minimumCollision)
        {
            int i, j, result = 0;
            for (i = 0; i < 5; i++)
            {
                for (j = 0; j < 9; j++)
                {
                    if (array[i, j].Count > minimumCollision)
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
        static int calculate_collision2(List<int>[,] array1, int[,] array2, int minimumCollision)
        {
            int i, j, result = 0;
            for (i = 0; i < 5; i++)
            {
                for (j = 0; j < 9; j++)
                {
                    if (array1[i, j].Count > minimumCollision && array2[i, j] > minimumCollision)
                    {
                        result += array1[i, j].Count + array2[i, j] - 1;
                    }
                }
            }
            return result;
        }
        /*count consecutive 4(can be changed) hour for teachers table*/
        static int calculate_collision3(int[,] array, int maxConsecutiveHour)
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
                    if (counter >= maxConsecutiveHour)
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
                        if (j != k && CourseList[j].Code.Equals(CourseList[k].Code))
                        {
                            //result++;
                            type1 = CourseList[j].Type;
                            type2 = CourseList[k].Type;
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

        static int calculate_collision7(List<int>[,] array1, List<int>[,] array2, int minimumCollision)
        {
            int result = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (array1[i, j].Count > minimumCollision && array2[i, j].Count > minimumCollision)
                    {

                        for (int k = 0; k < array2[i, j].Count; k++)
                        {
                            for (int l = 0; l < array1[i, j].Count; l++)
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
        /* Routine for Tournament Selection, it creates a newPopulation from oldPopulation by performing Tournament Selection and the Crossover */
        static void Selection(Population oldPopulation, Population newPopulation)
        {
            int[] a1, a2; //todo: optmizasyon
            int temp;
            int i;
            int rand;
            Individual parent1, parent2;
            a1 = new int[Problem.PopulationSize];
            a2 = new int[Problem.PopulationSize];
            for (i = 0; i < Problem.PopulationSize; i++)
            {
                a1[i] = a2[i] = i;
            }
            for (i = 0; i < Problem.PopulationSize; i++)
            {
                rand = RandomizationObj.RandomInteger(i, Problem.PopulationSize - 1);
                temp = a1[rand];
                a1[rand] = a1[i];
                a1[i] = temp;
                rand = RandomizationObj.RandomInteger(i, Problem.PopulationSize - 1);
                temp = a2[rand];
                a2[rand] = a2[i];
                a2[i] = temp;
            }
            for (i = 0; i < Problem.PopulationSize; i += 4)
            {
                parent1 = Tournament(oldPopulation.IndList[a1[i]], oldPopulation.IndList[a1[i + 1]]);
                parent2 = Tournament(oldPopulation.IndList[a1[i + 2]], oldPopulation.IndList[a1[i + 3]]);
                Crossover(parent1, parent2, newPopulation.IndList[i], newPopulation.IndList[i + 1]);
                parent1 = Tournament(oldPopulation.IndList[a2[i]], oldPopulation.IndList[a2[i + 1]]);
                parent2 = Tournament(oldPopulation.IndList[a2[i + 2]], oldPopulation.IndList[a2[i + 3]]);
                Crossover(parent1, parent2, newPopulation.IndList[i + 2], newPopulation.IndList[i + 3]);
            }

        }

        /* Routine for binary Tournament */
        static Individual Tournament(Individual ind1, Individual ind2)
        {
            int flag;
            flag = CheckDominance(ind1, ind2);
            if (flag == 1)
            {
                return ind1;
            }
            if (flag == -1)
            {
                return ind2;
            }
            if (ind1.CrowdDist > ind2.CrowdDist)
            {
                return ind1;
            }
            if (ind2.CrowdDist > ind1.CrowdDist)
            {
                return ind2;
            }
            if (RandomizationObj.RandomPercent() <= 0.5)
            {
                return ind1;
            }
            else
            {
                return ind2;
            }
        }
        #endregion

        #region Crossover.c
        /* Function to cross two individuals */
        static void Crossover(Individual parent1, Individual parent2, Individual child1, Individual child2)
        {
            if (Problem.RealVariableCount != 0)
            {
                RealCrossover(parent1, parent2, child1, child2);
            }
            if (Problem.BinaryVariableCount != 0)
            {
                BinaryCrossover(parent1, parent2, child1, child2);
            }
        }

        /* Routine for real variable SBX Crossover */
        static void RealCrossover(Individual parent1, Individual parent2, Individual child1, Individual child2)
        {
            int i;
            double rand;
            double y1, y2, yl, yu;
            double c1, c2;
            double alpha, beta, betaq;
            if (RandomizationObj.RandomPercent() <= Problem.RealCrossoverProbability)
            {
                Problem.RealCrossoverCount++;
                for (i = 0; i < Problem.RealVariableCount; i++)
                {
                    if (RandomizationObj.RandomPercent() <= 0.5)
                    {
                        if (Math.Abs(parent1.Xreal[i] - parent2.Xreal[i]) > Problem.EPS)
                        {
                            if (parent1.Xreal[i] < parent2.Xreal[i])
                            {
                                y1 = parent1.Xreal[i];
                                y2 = parent2.Xreal[i];
                            }
                            else
                            {
                                y1 = parent2.Xreal[i];
                                y2 = parent1.Xreal[i];
                            }
                            yl = Problem.min_realvar[i];
                            yu = Problem.max_realvar[i];
                            rand = RandomizationObj.RandomPercent();
                            beta = 1.0 + 2.0 * (y1 - yl) / (y2 - y1);
                            alpha = 2.0 - Math.Pow(beta, -(Problem.CrossoverDistributionIndex + 1.0));
                            if (rand <= 1.0 / alpha)
                            {
                                betaq = Math.Pow(rand * alpha, 1.0 / (Problem.CrossoverDistributionIndex + 1.0));
                            }
                            else
                            {
                                betaq = Math.Pow(1.0 / (2.0 - rand * alpha), 1.0 / (Problem.CrossoverDistributionIndex + 1.0));
                            }
                            c1 = 0.5 * (y1 + y2 - betaq * (y2 - y1));
                            beta = 1.0 + 2.0 * (yu - y2) / (y2 - y1);
                            alpha = 2.0 - Math.Pow(beta, -(Problem.CrossoverDistributionIndex + 1.0));
                            if (rand <= 1.0 / alpha)
                            {
                                betaq = Math.Pow(rand * alpha, 1.0 / (Problem.CrossoverDistributionIndex + 1.0));
                            }
                            else
                            {
                                betaq = Math.Pow(1.0 / (2.0 - rand * alpha), 1.0 / (Problem.CrossoverDistributionIndex + 1.0));
                            }
                            c2 = 0.5 * (y1 + y2 + betaq * (y2 - y1));
                            if (c1 < yl)
                                c1 = yl;
                            if (c2 < yl)
                                c2 = yl;
                            if (c1 > yu)
                                c1 = yu;
                            if (c2 > yu)
                                c2 = yu;
                            if (RandomizationObj.RandomPercent() <= 0.5)
                            {
                                child1.Xreal[i] = c2;
                                child2.Xreal[i] = c1;
                            }
                            else
                            {
                                child1.Xreal[i] = c1;
                                child2.Xreal[i] = c2;
                            }
                        }
                        else
                        {
                            child1.Xreal[i] = parent1.Xreal[i];
                            child2.Xreal[i] = parent2.Xreal[i];
                        }
                    }
                    else
                    {
                        child1.Xreal[i] = parent1.Xreal[i];
                        child2.Xreal[i] = parent2.Xreal[i];
                    }
                }
            }
            else
            {
                for (i = 0; i < Problem.RealVariableCount; i++)
                {
                    child1.Xreal[i] = parent1.Xreal[i];
                    child2.Xreal[i] = parent2.Xreal[i];
                }
            }
        }

        /* Routine for two point binary Crossover */
        static void BinaryCrossover(Individual parent1, Individual parent2, Individual child1, Individual child2)
        {
            int i, j;
            double rand;
            int temp, site1, site2;
            for (i = 0; i < Problem.BinaryVariableCount; i++)
            {
                rand = RandomizationObj.RandomPercent();
                if (rand <= Problem.BinaryCrossoverProbability)
                {
                    Problem.BinaryCrossoverCount++;
                    site1 = RandomizationObj.RandomInteger(0, Problem.nbits[i] - 1);
                    site2 = RandomizationObj.RandomInteger(0, Problem.nbits[i] - 1);
                    if (site1 > site2)
                    {
                        temp = site1;
                        site1 = site2;
                        site2 = temp;
                    }
                    for (j = 0; j < site1; j++)
                    {
                        child1.Gene[i, j] = parent1.Gene[i, j];
                        child2.Gene[i, j] = parent2.Gene[i, j];
                    }
                    for (j = site1; j < site2; j++)
                    {
                        child1.Gene[i, j] = parent2.Gene[i, j];
                        child2.Gene[i, j] = parent1.Gene[i, j];
                    }
                    for (j = site2; j < Problem.nbits[i]; j++)
                    {
                        child1.Gene[i, j] = parent1.Gene[i, j];
                        child2.Gene[i, j] = parent2.Gene[i, j];
                    }
                }
                else
                {
                    for (j = 0; j < Problem.nbits[i]; j++)
                    {
                        child1.Gene[i, j] = parent1.Gene[i, j];
                        child2.Gene[i, j] = parent2.Gene[i, j];
                    }
                }
            }

        }
        #endregion

        #region dominance.c
        /* Routine for usual non-domination checking
   It will return the following values
   1 if a dominates b
   -1 if b dominates a
   0 if both a and b are non-dominated */
        static int CheckDominance(Individual a, Individual b)
        {
            int i;
            int flag1;
            int flag2;
            flag1 = 0;
            flag2 = 0;
            if (a.ConstrViolation < 0 && b.ConstrViolation < 0)
            {
                if (a.ConstrViolation > b.ConstrViolation)
                {
                    return 1;
                }
                if (a.ConstrViolation < b.ConstrViolation)
                {
                    return -1;
                }
                return 0;
            }
            if (a.ConstrViolation < 0 && b.ConstrViolation == 0)
            {
                return -1;
            }

            if (a.ConstrViolation == 0 && b.ConstrViolation < 0)
            {
                return 1;
            }

            for (i = 0; i < Problem.ObjectiveCount; i++)
            {
                if (a.Obj[i] < b.Obj[i])
                {
                    flag1 = 1;

                }
                else
                {
                    if (a.Obj[i] > b.Obj[i])
                    {
                        flag2 = 1;
                    }
                }
            }
            if (flag1 == 1 && flag2 == 0)
            {
                return 1;
            }
            if (flag1 == 0 && flag2 == 1)
            {
                return -1;
            }
            return 0;
        }
        #endregion

        #region mutation.c
        /* Function to perform mutation in a population */
        static void MutatePopulation(Population pop)
        {
            int i;
            for (i = 0; i < Problem.PopulationSize; i++)
            {
                MutateIndividual(pop.IndList[i]);
            }
        }

        /* Function to perform mutation of an individual */
        static void MutateIndividual(Individual ind)
        {
            if (Problem.RealVariableCount != 0)
            {
                RealMutate(ind);
            }
            if (Problem.BinaryVariableCount != 0)
            {
                BinaryMutate(ind);
            }
            return;
        }

        /* Routine for binary mutation of an individual */
        static void BinaryMutate(Individual ind)
        {
            int j, k;
            double prob;
            for (j = 0; j < Problem.BinaryVariableCount; j++)
            {
                for (k = 0; k < Problem.nbits[j]; k++)
                {
                    prob = RandomizationObj.RandomPercent();
                    if (prob <= Problem.BinaryMutationProbability)
                    {
                        if (ind.Gene[j, k] == 0)
                        {
                            ind.Gene[j, k] = 1;
                        }
                        else
                        {
                            ind.Gene[j, k] = 0;
                        }
                        Problem.BinaryMutationCount += 1;
                    }
                }
            }
        }

        /* Routine for real polynomial mutation of an individual */
        static void RealMutate(Individual ind)
        {
            int j;
            double rnd, delta1, delta2, mut_pow, deltaq;
            double y, yl, yu, val, xy;
            for (j = 0; j < Problem.RealVariableCount; j++)
            {
                if (RandomizationObj.RandomPercent() <= Problem.RealMutationProbability)
                {
                    y = ind.Xreal[j];
                    yl = Problem.min_realvar[j];
                    yu = Problem.max_realvar[j];
                    delta1 = (y - yl) / (yu - yl);
                    delta2 = (yu - y) / (yu - yl);
                    rnd = RandomizationObj.RandomPercent();
                    mut_pow = 1.0 / (Problem.MutationDistributionIndex + 1.0);
                    if (rnd <= 0.5)
                    {
                        xy = 1.0 - delta1;
                        val = 2.0 * rnd + (1.0 - 2.0 * rnd) * Math.Pow(xy, Problem.MutationDistributionIndex + 1.0);
                        deltaq = Math.Pow(val, mut_pow) - 1.0;
                    }
                    else
                    {
                        xy = 1.0 - delta2;
                        val = 2.0 * (1.0 - rnd) + 2.0 * (rnd - 0.5) * Math.Pow(xy, Problem.MutationDistributionIndex + 1.0);
                        deltaq = 1.0 - Math.Pow(val, mut_pow);
                    }
                    y = y + deltaq * (yu - yl);
                    if (y < yl)
                        y = yl;
                    if (y > yu)
                        y = yu;
                    ind.Xreal[j] = y;
                    Problem.RealMutationCount += 1;
                }
            }
        }
        #endregion

        #region report.c

        /* Function to print the information of a population in a file */
        static void ReportPopulation(Population pop, StreamWriter writer)
        {
            int i, j, k;
            for (i = 0; i < Problem.PopulationSize; i++)
            {
                for (j = 0; j < Problem.ObjectiveCount; j++)
                {
                    writer.Write($"{pop.IndList[i].Obj[j].ToString("E")}\t");
                }
                if (Problem.ConstraintCount != 0)
                {
                    for (j = 0; j < Problem.ConstraintCount; j++)
                    {
                        writer.Write($"{pop.IndList[i].Constr[j].ToString("E")}\t");
                    }
                }
                if (Problem.RealVariableCount != 0)
                {
                    for (j = 0; j < Problem.RealVariableCount; j++)
                    {
                        writer.Write($"{pop.IndList[i].Xreal[j].ToString("E")}\t");
                    }
                }
                if (Problem.BinaryVariableCount != 0)
                {
                    for (j = 0; j < Problem.BinaryVariableCount; j++)
                    {
                        for (k = 0; k < Problem.nbits[j]; k++)
                        {
                            writer.Write($"{pop.IndList[i].Gene[j, k]}\t");
                        }
                    }
                }
                writer.Write($"{pop.IndList[i].ConstrViolation.ToString("E")}\t");
                writer.Write($"{pop.IndList[i].Rank}\t");
                writer.Write($"{pop.IndList[i].CrowdDist.ToString("E")}\n");
            }

        }

        /* Function to print the information of feasible and non-dominated population in a file */
        static void ReportFeasiblePopulation(Population pop, StreamWriter writer)
        {
            int i, j, k;
            for (i = 0; i < Problem.PopulationSize; i++)
            {
                if (pop.IndList[i].ConstrViolation == 0.0 && pop.IndList[i].Rank == 1)
                {
                    for (j = 0; j < Problem.ObjectiveCount; j++)
                    {
                        writer.Write($"{pop.IndList[i].Obj[j].ToString("E")}\t");
                    }
                    if (Problem.ConstraintCount != 0)
                    {
                        for (j = 0; j < Problem.ConstraintCount; j++)
                        {
                            writer.Write($"{pop.IndList[i].Constr[j].ToString("E")}\t");
                        }
                    }
                    if (Problem.RealVariableCount != 0)
                    {
                        for (j = 0; j < Problem.RealVariableCount; j++)
                        {
                            writer.Write($"{pop.IndList[i].Xreal[j].ToString("E")}\t");
                        }
                    }
                    if (Problem.BinaryVariableCount != 0)
                    {
                        for (j = 0; j < Problem.BinaryVariableCount; j++)
                        {
                            for (k = 0; k < Problem.nbits[j]; k++)
                            {
                                writer.Write($"{pop.IndList[i].Gene[j, k]}\t");
                            }
                        }
                    }
                    writer.Write($"{pop.IndList[i].ConstrViolation.ToString("E")}\t");
                    writer.Write($"{pop.IndList[i].Rank}\t");
                    writer.Write($"{ pop.IndList[i].CrowdDist.ToString("E")}\n");
                }
            }
            return;
        }

        #endregion

        #region list.c
        /* Insert an element X into the list at location specified by NODE */
        static void Insert(Lists node, int x)
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
        }

        /* Delete the node NODE from the list */
        static Lists Delete(Lists node)
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
            return temp;
        }
        #endregion

        #region rank.c
        /* Function to assign rank and crowding distance to a population of size pop_size*/
        static void assign_rank_and_crowding_distance(Population newPopulation)
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

            orig.index = -1;
            orig.parent = null;
            orig.child = null;
            cur.index = -1;
            cur.parent = null;
            cur.child = null;
            temp1 = orig;
            for (i = 0; i < Problem.PopulationSize; i++)
            {
                Insert(temp1, i);
                temp1 = temp1.child;
            }
            do
            {
                if (orig.child.child == null)
                {
                    newPopulation.IndList[orig.child.index].Rank = rank;
                    newPopulation.IndList[orig.child.index].CrowdDist = Problem.INF;
                    break;
                }
                temp1 = orig.child;
                Insert(cur, temp1.index);
                front_size = 1;
                temp2 = cur.child;
                temp1 = Delete(temp1);
                temp1 = temp1.child;
                do
                {
                    temp2 = cur.child;
                    do
                    {
                        end = 0;
                        flag = CheckDominance(newPopulation.IndList[temp1.index], newPopulation.IndList[temp2.index]);
                        if (flag == 1)
                        {
                            Insert(orig, temp2.index);
                            temp2 = Delete(temp2);
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
                        Insert(cur, temp1.index);
                        front_size++;
                        temp1 = Delete(temp1);
                    }
                    temp1 = temp1.child;
                }
                while (temp1 != null);
                temp2 = cur.child;
                do
                {
                    newPopulation.IndList[temp2.index].Rank = rank;
                    temp2 = temp2.child;
                }
                while (temp2 != null);
                assign_crowding_distance_list(newPopulation, cur.child, front_size);
                temp2 = cur.child;
                do
                {
                    temp2 = Delete(temp2);
                    temp2 = temp2.child;
                }
                while (cur.child != null);
                rank += 1;
            }
            while (orig.child != null);

        }
        #endregion

        #region crowddist.c
        /* Routine to compute crowding distance based on ojbective function values when the population in in the form of a list */
        static void assign_crowding_distance_list(Population pop, Lists lst, int frontSize)
        {
            int[][] obj_array;
            int[] dist;
            int i, j;
            Lists temp;
            temp = lst;
            if (frontSize == 1)
            {
                pop.IndList[lst.index].CrowdDist = Problem.INF;
                return;
            }
            if (frontSize == 2)
            {
                pop.IndList[lst.index].CrowdDist = Problem.INF;
                pop.IndList[lst.child.index].CrowdDist = Problem.INF;
                return;
            }
            dist = new int[frontSize];
            obj_array = new int[Problem.ObjectiveCount][];
            //obj_array = (int**)malloc(Problem.ObjectiveCount * sizeof(int*));
            for (i = 0; i < Problem.ObjectiveCount; i++)
            {
                obj_array[i] = new int[frontSize];
            }
            for (j = 0; j < frontSize; j++)
            {
                dist[j] = temp.index;
                temp = temp.child;
            }
            assign_crowding_distance(pop, dist, obj_array, frontSize);

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
                pop.IndList[c1].CrowdDist = Problem.INF;
                return;
            }
            if (front_size == 2)
            {
                pop.IndList[c1].CrowdDist = Problem.INF;
                pop.IndList[c2].CrowdDist = Problem.INF;
                return;
            }
            dist = new int[front_size];
            obj_array = new int[Problem.ObjectiveCount][];
            //obj_array = (int**)malloc(Problem.ObjectiveCount * sizeof(int*));
            for (i = 0; i < Problem.ObjectiveCount; i++)
            {
                obj_array[i] = new int[front_size];
            }

            for (j = 0; j < front_size; j++)
            {
                dist[j] = c1++;
            }
            assign_crowding_distance(pop, dist, obj_array, front_size);
            //free(dist);
            //for (i = 0; i < Problem.ObjectiveCount; i++)
            //{
            //    free(obj_array[i]);
            //}
            //free(obj_array);
            return;
        }

        /* Routine to compute crowding distances */
        static void assign_crowding_distance(Population pop, int[] dist, int[][] objArray, int frontSize)
        {
            int i, j;
            for (i = 0; i < Problem.ObjectiveCount; i++)
            {
                for (j = 0; j < frontSize; j++)
                {
                    objArray[i][j] = dist[j];
                }
                quicksort_front_obj(pop, i, objArray[i], frontSize);
            }
            for (j = 0; j < frontSize; j++)
            {
                pop.IndList[dist[j]].CrowdDist = 0.0;
            }
            for (i = 0; i < Problem.ObjectiveCount; i++)
            {
                pop.IndList[objArray[i][0]].CrowdDist = Problem.INF;
            }
            for (i = 0; i < Problem.ObjectiveCount; i++)
            {
                for (j = 1; j < frontSize - 1; j++)
                {
                    if (pop.IndList[objArray[i][j]].CrowdDist != Problem.INF)
                    {
                        if (pop.IndList[objArray[i][frontSize - 1]].Obj[i] == pop.IndList[objArray[i][0]].Obj[i])
                        {
                            pop.IndList[objArray[i][j]].CrowdDist += 0.0;
                        }
                        else
                        {
                            pop.IndList[objArray[i][j]].CrowdDist += (pop.IndList[objArray[i][j + 1]].Obj[i] - pop.IndList[objArray[i][j - 1]].Obj[i]) / (pop.IndList[objArray[i][frontSize - 1]].Obj[i] - pop.IndList[objArray[i][0]].Obj[i]);
                        }
                    }
                }
            }
            for (j = 0; j < frontSize; j++)
            {
                if (pop.IndList[dist[j]].CrowdDist != Problem.INF)
                {
                    pop.IndList[dist[j]].CrowdDist = pop.IndList[dist[j]].CrowdDist / Problem.ObjectiveCount;
                }
            }
            return;
        }
        #endregion

        #region sort.c
        /* Randomized quick sort routine to sort a population based on a particular objective chosen */
        static void quicksort_front_obj(Population pop, int objcount, int[] objArray, int objArraySize)
        {
            q_sort_front_obj(pop, objcount, objArray, 0, objArraySize - 1);
            return;
        }

        /* Actual implementation of the randomized quick sort used to sort a population based on a particular objective chosen */
        static void q_sort_front_obj(Population pop, int objcount, int[] objArray, int left, int right)
        {
            int index;
            int temp;
            int i, j;
            double pivot;
            if (left < right)
            {
                index = RandomizationObj.RandomInteger(left, right);
                temp = objArray[right];
                objArray[right] = objArray[index];
                objArray[index] = temp;
                pivot = pop.IndList[objArray[right]].Obj[objcount];
                i = left - 1;
                for (j = left; j < right; j++)
                {
                    if (pop.IndList[objArray[j]].Obj[objcount] <= pivot)
                    {
                        i += 1;
                        temp = objArray[j];
                        objArray[j] = objArray[i];
                        objArray[i] = temp;
                    }
                }
                index = i + 1;
                temp = objArray[index];
                objArray[index] = objArray[right];
                objArray[right] = temp;
                q_sort_front_obj(pop, objcount, objArray, left, index - 1);
                q_sort_front_obj(pop, objcount, objArray, index + 1, right);
            }
        }

        /* Randomized quick sort routine to sort a population based on crowding distance */
        static void quicksort_dist(Population pop, int[] dist, int frontSize)
        {
            q_sort_dist(pop, dist, 0, frontSize - 1);
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
                index = RandomizationObj.RandomInteger(left, right);
                temp = dist[right];
                dist[right] = dist[index];
                dist[index] = temp;
                pivot = pop.IndList[dist[right]].CrowdDist;
                i = left - 1;
                for (j = left; j < right; j++)
                {
                    if (pop.IndList[dist[j]].CrowdDist <= pivot)
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

        }
        #endregion

        #region MergePopulation.c

        /* Routine to MergePopulation two populations into one */
        static void MergePopulation(Population pop1, Population pop2, Population pop3)
        {
            int i, k;
            for (i = 0; i < Problem.PopulationSize; i++)
            {
                CopyIndividual(pop1.IndList[i], pop3.IndList[i]);
            }
            for (i = 0, k = Problem.PopulationSize; i < Problem.PopulationSize; i++, k++)
            {
                CopyIndividual(pop2.IndList[i], pop3.IndList[k]);
            }
        }

        /* Routine to copy an individual 'ind1' into another individual 'ind2' */
        static void CopyIndividual(Individual ind1, Individual ind2)
        {
            int i, j;
            ind2.Rank = ind1.Rank;
            ind2.ConstrViolation = ind1.ConstrViolation;
            ind2.CrowdDist = ind1.CrowdDist;
            if (Problem.RealVariableCount != 0)
            {
                for (i = 0; i < Problem.RealVariableCount; i++)
                {
                    ind2.Xreal[i] = ind1.Xreal[i];
                }
            }
            if (Problem.BinaryVariableCount != 0)
            {
                for (i = 0; i < Problem.BinaryVariableCount; i++)
                {
                    ind2.Xbin[i] = ind1.Xbin[i];
                    for (j = 0; j < Problem.nbits[i]; j++)
                    {
                        ind2.Gene[i, j] = ind1.Gene[i, j];
                    }
                }
            }
            for (i = 0; i < Problem.ObjectiveCount; i++)
            {
                ind2.Obj[i] = ind1.Obj[i];
            }
            if (Problem.ConstraintCount != 0)
            {
                for (i = 0; i < Problem.ConstraintCount; i++)
                {
                    ind2.Constr[i] = ind1.Constr[i];
                }
            }

        }

        #endregion

        #region fillnds.c

        /* Routine to perform non-dominated sorting */
        static void fill_nondominated_sort(Population mixedPop, Population newPop)
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
            for (i = 0; i < 2 * Problem.PopulationSize; i++)
            {
                Insert(temp1, i);
                temp1 = temp1.child;
            }
            i = 0;
            do
            {
                temp1 = pool.child;
                Insert(elite, temp1.index);
                front_size = 1;
                temp2 = elite.child;
                temp1 = Delete(temp1);
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
                        flag = CheckDominance(mixedPop.IndList[temp1.index], mixedPop.IndList[temp2.index]);
                        if (flag == 1)
                        {
                            Insert(pool, temp2.index);
                            temp2 = Delete(temp2);
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
                        Insert(elite, temp1.index);
                        front_size++;
                        temp1 = Delete(temp1);
                    }
                    temp1 = temp1.child;
                }
                while (temp1 != null);
                temp2 = elite.child;
                j = i;
                if (archieve_size + front_size <= Problem.PopulationSize)
                {
                    do
                    {
                        CopyIndividual(mixedPop.IndList[temp2.index], newPop.IndList[i]);
                        newPop.IndList[i].Rank = rank;
                        archieve_size += 1;
                        temp2 = temp2.child;
                        i += 1;
                    }
                    while (temp2 != null);
                    assign_crowding_distance_indices(newPop, j, i - 1);
                    rank += 1;
                }
                else
                {
                    crowding_fill(mixedPop, newPop, i, front_size, elite);
                    archieve_size = Problem.PopulationSize;
                    for (j = i; j < Problem.PopulationSize; j++)
                    {
                        newPop.IndList[j].Rank = rank;
                    }
                }
                temp2 = elite.child;
                do
                {
                    temp2 = Delete(temp2);
                    temp2 = temp2.child;
                }
                while (elite.child != null);
            }
            while (archieve_size < Problem.PopulationSize);

        }

        /* Routine to fill a population with individuals in the decreasing order of crowding distance */
        static void crowding_fill(Population mixedPop, Population newPop, int count, int frontSize, Lists elite)
        {
            int[] dist;
            Lists temp;
            int i, j;
            assign_crowding_distance_list(mixedPop, elite.child, frontSize);
            dist = new int[frontSize];
            temp = elite.child;
            for (j = 0; j < frontSize; j++)
            {
                dist[j] = temp.index;
                temp = temp.child;
            }
            quicksort_dist(mixedPop, dist, frontSize);
            for (i = count, j = frontSize - 1; i < Problem.PopulationSize; i++, j--)
            {
                CopyIndividual(mixedPop.IndList[dist[j]], newPop.IndList[i]);
            }

        }

        #endregion

        #region display.c
        ///* Function to display the current population for the subsequent generation */
        static void PlotPopulation(Population pop, int genNo)
        {
            Console.WriteLine(" printing gnuplot");
            if (GnuplotChoice != 3)
            {
                var xr = new double[Problem.PopulationSize];
                var yr = new double[Problem.PopulationSize];

                for (int x = 0; x < Problem.PopulationSize; x++)
                {
                    xr[x] = pop.IndList[x].Obj[GnuplotObjective1 - 1];
                    yr[x] = pop.IndList[x].Obj[GnuplotObjective2 - 1];
                }

                GnuPlot.Plot(xr, yr, $"title 'Generation #{genNo} of {Problem.GenCount}' pt 1");
                GnuPlot.Set($"xlabel \"obj[{GnuplotObjective1 - 1}]\"");
                GnuPlot.Set($"ylabel \"obj[{GnuplotObjective2 - 1}]\"");

            }
            else
            {
                var xr = new double[Problem.PopulationSize];
                var yr = new double[Problem.PopulationSize];
                var zr = new double[Problem.PopulationSize];

                for (int x = 0; x < Problem.PopulationSize; x++)
                {
                    xr[x] = pop.IndList[x].Obj[GnuplotObjective1 - 1];
                    yr[x] = pop.IndList[x].Obj[GnuplotObjective2 - 1];
                    zr[x] = pop.IndList[x].Obj[GnuplotObjective3 - 1];
                }

                GnuPlot.SPlot(xr, yr, zr, $"title 'Generation #{genNo} of {Problem.GenCount}' with points pointtype 8 lc rgb 'blue'");
                GnuPlot.Set($"xlabel \"obj[{GnuplotObjective1 - 1}]\"");
                GnuPlot.Set($"ylabel \"obj[{GnuplotObjective2 - 1}]\"");
                GnuPlot.Set($"zlabel \"obj[{GnuplotObjective3 - 1}]\"");
            }
        }

        #endregion

    }
}
