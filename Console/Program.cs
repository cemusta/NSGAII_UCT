using ConsoleApp.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace ConsoleApp
{
    class Program
    {
        #region Variable 

        private static readonly ProblemDefinition ProblemObj = new ProblemDefinition();
        private static readonly Randomization RandomizationObj = new Randomization();
        private static readonly Display DisplayObj = new Display();


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
                ProblemObj.Scheduling.Add(new int[5, 9]);
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
                            ProblemObj.Scheduling[i][k, j] = int.Parse(parts[k]);
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
                    ProblemObj.LabScheduling[k, j] = int.Parse(parts[k]);
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
                    ProblemObj.Meeting[k, j] = int.Parse(parts[k]);
                }
            }
            #endregion

            #region scan course list
            reader = new StreamReader(courseListFile);

            line = reader.ReadLine();
            int courseCount = int.Parse(line); //43 gibi bir sayı dönüyor

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

                var teacherName = parts[1];
                if (!ProblemObj.TeacherList.Contains(teacherName))
                {
                    ProblemObj.TeacherList.Add(teacherName);
                }

                ProblemObj.CourseList.Add(new Course(courseId, parts[0], parts[1], ProblemObj.TeacherList.IndexOf(teacherName), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]), (int.Parse(parts[6]) == 1)));

            }
            Console.WriteLine($"teacher size: {ProblemObj.TeacherList.Count}");
            #endregion

            #region scan preqeuiste courses

            reader = new StreamReader(prerequisiteFile);

            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(new char[] { ';' });

                var preCourse = ProblemObj.CourseList.Find(x => x.Code == parts[0]);
                if (preCourse == null)
                {
                    Console.WriteLine($"Preqeuiste Scan: non existing preq. course {parts[0]}");
                    continue;
                }

                var courseToAdd = ProblemObj.CourseList.Find(x => x.Code == parts[1]);
                if (courseToAdd == null)
                {
                    Console.WriteLine($"Preqeuiste Scan: non existing course {parts[1]}");
                    continue;
                }

                courseToAdd.prerequisites.Add(preCourse.Id);
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
            ProblemObj.PopulationSize = int.Parse(consoleIn);
            if (ProblemObj.PopulationSize < 4 || ProblemObj.PopulationSize % 4 != 0)
            {
                Console.WriteLine($" population size read is : {ProblemObj.PopulationSize}");
                Console.WriteLine(" Wrong population size entered, hence exiting \n");
                return;
            }

            Console.WriteLine(" Enter the number of generations : ");
            consoleIn = Console.ReadLine();
            ProblemObj.GenCount = int.Parse(consoleIn);
            if (ProblemObj.GenCount < 1)
            {
                Console.WriteLine($" number of generations read is : {ProblemObj.GenCount}");
                Console.WriteLine(" Wrong nuber of generations entered, hence exiting \n");
                return;
            }

            Console.WriteLine(" Enter the number of objectives : ");
            consoleIn = Console.ReadLine();
            ProblemObj.ObjectiveCount = int.Parse(consoleIn);
            if (ProblemObj.ObjectiveCount < 1)
            {
                Console.WriteLine($" number of objectives entered is : {ProblemObj.ObjectiveCount}");
                Console.WriteLine(" Wrong number of objectives entered, hence exiting \n");
                return;
            }

            Console.WriteLine("\n Enter the number of constraints : ");
            consoleIn = Console.ReadLine();
            ProblemObj.ConstraintCount = int.Parse(consoleIn);
            if (ProblemObj.ConstraintCount < 0)
            {
                Console.WriteLine($" number of constraints entered is : {ProblemObj.ConstraintCount}");
                Console.WriteLine(" Wrong number of constraints enetered, hence exiting \n");
                return;
            }

            Console.WriteLine("\n Enter the number of real variables : ");
            consoleIn = Console.ReadLine();
            ProblemObj.RealVariableCount = int.Parse(consoleIn);
            if (ProblemObj.RealVariableCount < 0)
            {
                Console.WriteLine($" number of real variables entered is : {ProblemObj.RealVariableCount}");
                Console.WriteLine(" Wrong number of variables entered, hence exiting \n");
                return;
            }


            if (ProblemObj.RealVariableCount != 0)
            {
                ProblemObj.min_realvar = new double[ProblemObj.RealVariableCount];
                ProblemObj.max_realvar = new double[ProblemObj.RealVariableCount];
                for (int i = 0; i < ProblemObj.RealVariableCount; i++)
                {
                    Console.WriteLine($" Enter the lower limit of real variable {i + 1} : ");
                    consoleIn = Console.ReadLine();
                    ProblemObj.min_realvar[i] = double.Parse(consoleIn);
                    Console.WriteLine($" Enter the upper limit of real variable {i + 1} : ");
                    consoleIn = Console.ReadLine();
                    ProblemObj.max_realvar[i] = double.Parse(consoleIn);
                    if (ProblemObj.max_realvar[i] <= ProblemObj.min_realvar[i])
                    {
                        Console.WriteLine(" Wrong limits entered for the min and max bounds of real variable, hence exiting \n");
                        return;
                    }
                }
                Console.WriteLine(" Enter the probability of Crossover of real variable (0.6-1.0) : ");
                consoleIn = Console.ReadLine();
                ProblemObj.RealCrossoverProbability = double.Parse(consoleIn);
                if (ProblemObj.RealCrossoverProbability < 0.0 || ProblemObj.RealCrossoverProbability > 1.0)
                {
                    Console.WriteLine($" Probability of crossover entered is : {ProblemObj.RealCrossoverProbability}");
                    Console.WriteLine(" Entered value of probability of Crossover of real variables is out of bounds, hence exiting \n");
                    return;
                }
                Console.WriteLine(" Enter the probablity of mutation of real variables (1/ProblemObj.RealVariableCount) : ");
                consoleIn = Console.ReadLine();
                ProblemObj.RealMutationProbability = double.Parse(consoleIn);
                if (ProblemObj.RealMutationProbability < 0.0 || ProblemObj.RealMutationProbability > 1.0)
                {
                    Console.WriteLine($" Probability of mutation entered is : {ProblemObj.RealMutationProbability}");
                    Console.WriteLine(" Entered value of probability of mutation of real variables is out of bounds, hence exiting \n");
                    return;
                }
                Console.WriteLine(" Enter the value of distribution index for Crossover (5-20): ");
                consoleIn = Console.ReadLine();
                ProblemObj.CrossoverDistributionIndex = double.Parse(consoleIn);
                if (ProblemObj.CrossoverDistributionIndex <= 0)
                {
                    Console.WriteLine($" The value entered is : {ProblemObj.CrossoverDistributionIndex}");
                    Console.WriteLine(" Wrong value of distribution index for Crossover entered, hence exiting \n");
                    return;
                }
                Console.WriteLine(" Enter the value of distribution index for mutation (5-50): ");
                consoleIn = Console.ReadLine();
                ProblemObj.MutationDistributionIndex = double.Parse(consoleIn);
                if (ProblemObj.MutationDistributionIndex <= 0)
                {
                    Console.WriteLine($" The value entered is : {ProblemObj.MutationDistributionIndex}");
                    Console.WriteLine(" Wrong value of distribution index for mutation entered, hence exiting \n");
                    return;
                }
            }

            Console.WriteLine(" Enter the number of binary variables : ");
            consoleIn = Console.ReadLine();
            ProblemObj.BinaryVariableCount = int.Parse(consoleIn);
            if (ProblemObj.BinaryVariableCount < 0)
            {
                Console.WriteLine($" number of binary variables entered is : {ProblemObj.BinaryVariableCount}");
                Console.WriteLine(" Wrong number of binary variables entered, hence exiting \n");
                return;
            }
            if (ProblemObj.BinaryVariableCount != 0)
            {
                ProblemObj.nbits = new int[ProblemObj.BinaryVariableCount];
                ProblemObj.min_binvar = new double[ProblemObj.BinaryVariableCount];
                ProblemObj.max_binvar = new double[ProblemObj.BinaryVariableCount];
                for (int i = 0; i < ProblemObj.BinaryVariableCount; i++)
                {
                    Console.WriteLine($" Enter the number of bits for binary variable {i + 1} :");
                    consoleIn = Console.ReadLine();
                    var parts = consoleIn.Split(new char[] { ' ' });
                    ProblemObj.nbits[i] = int.Parse(parts[0]);
                    if (ProblemObj.nbits[i] > ProblemObj.MaxBitCount)
                        ProblemObj.MaxBitCount = ProblemObj.nbits[i];
                    if (ProblemObj.nbits[i] < 1)
                    {
                        Console.WriteLine(" Wrong number of bits for binary variable entered, hence exiting");
                        return;
                    }
                    Console.WriteLine($" Enter the lower limit of binary variable {i + 1} :");
                    //consoleIn = Console.ReadLine();
                    ProblemObj.min_binvar[i] = double.Parse(parts[1]);

                    Console.WriteLine($" Enter the upper limit of binary variable {i + 1} :");
                    //consoleIn = Console.ReadLine();
                    ProblemObj.max_binvar[i] = double.Parse(parts[2]);
                    if (ProblemObj.max_binvar[i] <= ProblemObj.min_binvar[i])
                    {
                        Console.WriteLine(" Wrong limits entered for the min and max bounds of binary variable entered, hence exiting \n");
                        return;
                    }
                }
                Console.WriteLine(" Enter the probability of Crossover of binary variable (0.6-1.0): ");
                consoleIn = Console.ReadLine().Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                    .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                ProblemObj.BinaryCrossoverProbability = double.Parse(consoleIn);
                if (ProblemObj.BinaryCrossoverProbability < 0.0 || ProblemObj.BinaryCrossoverProbability > 1.0)
                {
                    Console.WriteLine($" Probability of crossover entered is : {ProblemObj.BinaryCrossoverProbability}");
                    Console.WriteLine(" Entered value of probability of Crossover of binary variables is out of bounds, hence exiting \n");
                    return;
                }
                Console.WriteLine(" Enter the probability of mutation of binary variables (1/ProblemObj.nbits): ");
                consoleIn = Console.ReadLine().Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                    .Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                ProblemObj.BinaryMutationProbability = double.Parse(consoleIn);
                if (ProblemObj.BinaryMutationProbability < 0.0 || ProblemObj.BinaryMutationProbability > 1.0)
                {
                    Console.WriteLine($" Probability of mutation entered is :  {ProblemObj.BinaryMutationProbability}");
                    Console.WriteLine(" Entered value of probability  of mutation of binary variables is out of bounds, hence exiting \n");
                    return;
                }
            }
            if (ProblemObj.RealVariableCount == 0 && ProblemObj.BinaryVariableCount == 0)
            {
                Console.WriteLine("\n Number of real as well as binary variables, both are zero, hence exiting \n");
                return;
            }

            Console.WriteLine(" Do you want to use gnuplot to display the results realtime (0 for NO) (1 for yes) : ");
            consoleIn = Console.ReadLine();
            DisplayObj.GnuplotChoice = int.Parse(consoleIn);
            if (DisplayObj.GnuplotChoice != 0 && DisplayObj.GnuplotChoice != 1)
            {
                Console.WriteLine($" Entered the wrong choice, hence exiting, choice entered was {DisplayObj.GnuplotChoice}\n");
                return;
            }
            if (DisplayObj.GnuplotChoice == 1)
            {
                if (ProblemObj.ObjectiveCount == 2)
                {
                    Console.WriteLine(" Enter the objective for X axis display : ");
                    consoleIn = Console.ReadLine();
                    DisplayObj.GnuplotObjective1 = int.Parse(consoleIn);
                    if (DisplayObj.GnuplotObjective1 < 1 || DisplayObj.GnuplotObjective1 > ProblemObj.ObjectiveCount)
                    {
                        Console.WriteLine($" Wrong value of X objective entered, value entered was {DisplayObj.GnuplotObjective1}\n");
                        return;
                    }
                    Console.WriteLine(" Enter the objective for Y axis display : ");
                    consoleIn = Console.ReadLine();
                    DisplayObj.GnuplotObjective2 = int.Parse(consoleIn);
                    if (DisplayObj.GnuplotObjective2 < 1 || DisplayObj.GnuplotObjective2 > ProblemObj.ObjectiveCount)
                    {
                        Console.WriteLine($" Wrong value of Y objective entered, value entered was {DisplayObj.GnuplotObjective2}\n");
                        return;
                    }
                    DisplayObj.GnuplotObjective3 = -1;
                }
                else
                {
                    Console.WriteLine(" #obj > 2, 2D display or a 3D display ?, enter 2 for 2D and 3 for 3D :");

                    consoleIn = Console.ReadLine();
                    DisplayObj.GnuplotChoice = int.Parse(consoleIn);
                    if (DisplayObj.GnuplotChoice != 2 && DisplayObj.GnuplotChoice != 3)
                    {
                        Console.WriteLine($" Entered the wrong choice, hence exiting, choice entered was {DisplayObj.GnuplotChoice}\n");
                        return;
                    }
                    if (DisplayObj.GnuplotChoice == 2)
                    {
                        Console.WriteLine(" Enter the objective for X axis display : ");
                        consoleIn = Console.ReadLine();
                        DisplayObj.GnuplotObjective1 = int.Parse(consoleIn);
                        if (DisplayObj.GnuplotObjective1 < 1 || DisplayObj.GnuplotObjective1 > ProblemObj.ObjectiveCount)
                        {
                            Console.WriteLine($" Wrong value of X objective entered, value entered was {DisplayObj.GnuplotObjective1}\n");
                            return;
                        }
                        Console.WriteLine(" Enter the objective for Y axis display : ");
                        consoleIn = Console.ReadLine();
                        DisplayObj.GnuplotObjective2 = int.Parse(consoleIn);
                        if (DisplayObj.GnuplotObjective2 < 1 || DisplayObj.GnuplotObjective2 > ProblemObj.ObjectiveCount)
                        {
                            Console.WriteLine($" Wrong value of Y objective entered, value entered was {DisplayObj.GnuplotObjective2}\n");
                            return;
                        }
                        DisplayObj.GnuplotObjective3 = -1;
                    }
                    else
                    {
                        Console.WriteLine(" Enter the objective for X axis display : ");
                        consoleIn = Console.ReadLine();
                        DisplayObj.GnuplotObjective1 = int.Parse(consoleIn);
                        if (DisplayObj.GnuplotObjective1 < 1 || DisplayObj.GnuplotObjective1 > ProblemObj.ObjectiveCount)
                        {
                            Console.WriteLine($" Wrong value of X objective entered, value entered was {DisplayObj.GnuplotObjective1}\n");
                            return;
                        }
                        Console.WriteLine(" Enter the objective for Y axis display : ");
                        consoleIn = Console.ReadLine();
                        DisplayObj.GnuplotObjective2 = int.Parse(consoleIn);
                        if (DisplayObj.GnuplotObjective2 < 1 || DisplayObj.GnuplotObjective2 > ProblemObj.ObjectiveCount)
                        {
                            Console.WriteLine($" Wrong value of Y objective entered, value entered was {DisplayObj.GnuplotObjective2}\n");
                            return;
                        }
                        Console.WriteLine(" Enter the objective for Z axis display : ");
                        consoleIn = Console.ReadLine();
                        DisplayObj.GnuplotObjective3 = int.Parse(consoleIn);
                        if (DisplayObj.GnuplotObjective3 < 1 || DisplayObj.GnuplotObjective3 > ProblemObj.ObjectiveCount)
                        {
                            Console.WriteLine($" Wrong value of Z objective entered, value entered was {DisplayObj.GnuplotObjective3}\n");
                            return;
                        }
                        Console.WriteLine(" You have chosen 3D display, hence location of eye required \n");
                        Console.WriteLine(" Enter the first angle (an integer in the range 0-180) (if not known, enter 60) :");
                        consoleIn = Console.ReadLine();
                        DisplayObj.GnuplotAngle1 = int.Parse(consoleIn);
                        if (DisplayObj.GnuplotAngle1 < 0 || DisplayObj.GnuplotAngle1 > 180)
                        {
                            Console.WriteLine(" Wrong value for first angle entered, hence exiting \n");
                            return;
                        }
                        Console.WriteLine(" Enter the second angle (an integer in the range 0-360) (if not known, enter 30) :");
                        consoleIn = Console.ReadLine();
                        DisplayObj.GnuplotAngle2 = int.Parse(consoleIn);
                        if (DisplayObj.GnuplotAngle2 < 0 || DisplayObj.GnuplotAngle2 > 360)
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

            writer5.WriteLine($" Population size = {ProblemObj.PopulationSize}");
            writer5.WriteLine($" Number of generations = {ProblemObj.GenCount}");
            writer5.WriteLine($" Number of objective functions = {ProblemObj.ObjectiveCount}");
            writer5.WriteLine($" Number of constraints = {ProblemObj.ConstraintCount}");
            writer5.WriteLine($" Number of real variables = {ProblemObj.RealVariableCount}");
            if (ProblemObj.RealVariableCount != 0)
            {
                for (int i = 0; i < ProblemObj.RealVariableCount; i++)
                {
                    writer5.WriteLine($" Lower limit of real variable {i + 1} = {ProblemObj.min_realvar[i]}");
                    writer5.WriteLine($" Upper limit of real variable {i + 1} = {ProblemObj.max_realvar[i]}");
                }
                writer5.WriteLine($" Probability of crossover of real variable = {ProblemObj.RealCrossoverProbability}");
                writer5.WriteLine($" Probability of mutation of real variable = {ProblemObj.RealMutationProbability}");
                writer5.WriteLine($" Distribution index for crossover = {ProblemObj.CrossoverDistributionIndex}");
                writer5.WriteLine($" Distribution index for mutation = {ProblemObj.MutationDistributionIndex}");
            }
            writer5.Write($" Number of binary variables = {ProblemObj.BinaryVariableCount}");
            if (ProblemObj.BinaryVariableCount != 0)
            {
                for (int i = 0; i < ProblemObj.BinaryVariableCount; i++)
                {
                    writer5.WriteLine($" Number of bits for binary variable {i + 1} = {ProblemObj.nbits[i]}");
                    writer5.WriteLine($" Lower limit of binary variable {i + 1} = {ProblemObj.min_binvar[i]}");
                    writer5.WriteLine($" Upper limit of binary variable {i + 1} = {ProblemObj.max_binvar[i]}");
                }
                writer5.WriteLine($" Probability of crossover of binary variable = {ProblemObj.BinaryCrossoverProbability}");
                writer5.WriteLine($" Probability of mutation of binary variable = {ProblemObj.BinaryMutationProbability}");
            }
            writer5.Write($" Seed for random number generator = {seed}");
            ProblemObj.TotalBinaryBitLength = 0;
            if (ProblemObj.BinaryVariableCount != 0)
            {
                /*printf("ProblemObj.BinaryVariableCount: %d \n", ProblemObj.BinaryVariableCount);*/
                for (int i = 0; i < ProblemObj.BinaryVariableCount; i++)
                {
                    ProblemObj.TotalBinaryBitLength += ProblemObj.nbits[i];
                    /*printf("ProblemObj.nbits[%d]: %d \n", i,ProblemObj.nbits[i]);*/
                }
            }

            writer1.Write($"# of objectives = {ProblemObj.ObjectiveCount}, # of constraints = {ProblemObj.ConstraintCount}, # of real_var = {ProblemObj.RealVariableCount}, # of bits of bin_var = {ProblemObj.TotalBinaryBitLength}, constr_violation, rank, crowding_distance\n");
            writer2.Write($"# of objectives = {ProblemObj.ObjectiveCount}, # of constraints = {ProblemObj.ConstraintCount}, # of real_var = {ProblemObj.RealVariableCount}, # of bits of bin_var = {ProblemObj.TotalBinaryBitLength}, constr_violation, rank, crowding_distance\n");
            writer3.Write($"# of objectives = {ProblemObj.ObjectiveCount}, # of constraints = {ProblemObj.ConstraintCount}, # of real_var = {ProblemObj.RealVariableCount}, # of bits of bin_var = {ProblemObj.TotalBinaryBitLength}, constr_violation, rank, crowding_distance\n");
            writer4.Write($"# of objectives = {ProblemObj.ObjectiveCount}, # of constraints = {ProblemObj.ConstraintCount}, # of real_var = {ProblemObj.RealVariableCount}, # of bits of bin_var = {ProblemObj.TotalBinaryBitLength}, constr_violation, rank, crowding_distance\n");
            ProblemObj.BinaryMutationCount = 0;
            ProblemObj.RealMutationCount = 0;
            ProblemObj.BinaryCrossoverCount = 0;
            ProblemObj.RealCrossoverCount = 0;

            writer1.Flush();
            writer2.Flush();
            writer3.Flush();
            writer4.Flush();
            writer5.Flush();

            #endregion

            #region first population
            parentPopulation = new Population(ProblemObj.PopulationSize, ProblemObj.RealVariableCount, ProblemObj.BinaryVariableCount, ProblemObj.MaxBitCount, ProblemObj.ObjectiveCount, ProblemObj.ConstraintCount);
            childPopulation = new Population(ProblemObj.PopulationSize, ProblemObj.RealVariableCount, ProblemObj.BinaryVariableCount, ProblemObj.MaxBitCount, ProblemObj.ObjectiveCount, ProblemObj.ConstraintCount);
            mixedPopulation = new Population(ProblemObj.PopulationSize * 2, ProblemObj.RealVariableCount, ProblemObj.BinaryVariableCount, ProblemObj.MaxBitCount, ProblemObj.ObjectiveCount, ProblemObj.ConstraintCount);
   
            RandomizationObj.Randomize();
            parentPopulation.Initialize(ProblemObj, RandomizationObj);
            Console.WriteLine(" Initialization done, now performing first generation");

            parentPopulation.Decode(ProblemObj);
            parentPopulation.Evaluate(ProblemObj);
            assign_rank_and_crowding_distance(parentPopulation);
            parentPopulation.ReportPopulation(writer1, ProblemObj);
            writer4.WriteLine("# gen = 1");
            parentPopulation.ReportPopulation(writer4, ProblemObj);
            Console.WriteLine(" gen = 1");

            if (DisplayObj.GnuplotChoice != 0)
            {
                DisplayObj.PlotPopulation(parentPopulation, ProblemObj, 1);
            }

            writer1.Flush();
            writer2.Flush();
            writer3.Flush();
            writer4.Flush();
            writer5.Flush();

            int totalHill = 0;
            #endregion

            #region generation loop
            for (int i = 2; i <= ProblemObj.GenCount; i++)
            {
                Selection(parentPopulation, childPopulation);
                MutatePopulation(childPopulation);

                childPopulation.Decode(ProblemObj);
                childPopulation.Evaluate(ProblemObj);
                //childPopulation.HillClimb(ProblemObj);

                mixedPopulation.Merge(parentPopulation, childPopulation, ProblemObj);
                //mixedPopulation.Decode(ProblemObj);
                //mixedPopulation.Evaluate(ProblemObj);

                fill_nondominated_sort(mixedPopulation, parentPopulation);

                parentPopulation.Decode(ProblemObj);
                parentPopulation.Evaluate(ProblemObj);

                int minimumResult = parentPopulation.IndList.Min(x => x.TotalResult);

                //IOrderedEnumerable<Individual> bestChild = parentPopulation.IndList.OrderBy(x=> x.CollisionList.Count).ThenBy(x=>x.TotalResult).Min(x=> x.TotalResult);
                var result = minimumResult;
                var bestChild = parentPopulation.IndList.Where(x => x.TotalResult == result).ToList();

                //foreach (var child in bestChild)
                //{
                //int imp = bestChild.First().HillClimb(ProblemObj);
                //Console.WriteLine($"hill: {imp} total: {totalHill+imp}");
                //if (imp > 0)
                //    totalHill += imp;
                //}
                //parentPopulation.IndList[index].HillClimb(ProblemObj);
                minimumResult = parentPopulation.IndList.Min(x => x.TotalResult);

                /* Comment following four lines if information for all
                generations is not desired, it will speed up the execution */
                //*fprintf(fpt4,"# gen = %d\n",i);
                //parent_pop.ReportPopulation(fpt4,ProblemObj);
                //fflush(fpt4);*/

                if (DisplayObj.GnuplotChoice != 0)
                {
                    DisplayObj.PlotPopulation(parentPopulation, ProblemObj, i,bestChild.ToList());
                }

                var bc = bestChild.First();
                Console.WriteLine($" gen = {i} min = {minimumResult}");
                Console.WriteLine($" best: coll  = {bc.CollisionList.Count} result = {bc.TotalResult} obj0:{bc.Obj[0]} obj1:{bc.Obj[1]} obj2:{bc.Obj[2]}");

//#if DEBUG
//Thread.Sleep(200);
//#endif
            }
            #endregion

            #region prepare final reports
            Console.WriteLine($" Generations finished, now reporting solutions");
            parentPopulation.ReportPopulation(writer2,ProblemObj);
            parentPopulation.ReportFeasiblePopulation( writer3, ProblemObj);

            if (ProblemObj.RealVariableCount != 0)
            {
                writer5.WriteLine($" Number of crossover of real variable = {ProblemObj.RealCrossoverCount}");
                writer5.WriteLine($" Number of mutation of real variable = {ProblemObj.RealMutationCount}");
            }
            if (ProblemObj.BinaryVariableCount != 0)
            {
                writer5.WriteLine($" Number of crossover of binary variable = {ProblemObj.BinaryCrossoverCount}");
                writer5.WriteLine($" Number of mutation of binary variable = {ProblemObj.BinaryMutationCount}");
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

        
        #region NSGAII stuff

        #region tourselect.c
        /* Routine for Tournament Selection, it creates a newPopulation from oldPopulation by performing Tournament Selection and the Crossover */
        static void Selection(Population oldPopulation, Population newPopulation)
        {
            int[] a1, a2; //todo: optmizasyon
            int temp;
            int i;
            int rand;
            Individual parent1, parent2;
            a1 = new int[ProblemObj.PopulationSize];
            a2 = new int[ProblemObj.PopulationSize];
            for (i = 0; i < ProblemObj.PopulationSize; i++)
            {
                a1[i] = a2[i] = i;
            }
            for (i = 0; i < ProblemObj.PopulationSize; i++)
            {
                rand = RandomizationObj.RandomInteger(i, ProblemObj.PopulationSize - 1);
                temp = a1[rand];
                a1[rand] = a1[i];
                a1[i] = temp;
                rand = RandomizationObj.RandomInteger(i, ProblemObj.PopulationSize - 1);
                temp = a2[rand];
                a2[rand] = a2[i];
                a2[i] = temp;
            }
            for (i = 0; i < ProblemObj.PopulationSize; i += 4)
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
            if (ProblemObj.RealVariableCount != 0)
            {
                RealCrossover(parent1, parent2, child1, child2);
            }
            if (ProblemObj.BinaryVariableCount != 0)
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
            if (RandomizationObj.RandomPercent() <= ProblemObj.RealCrossoverProbability)
            {
                ProblemObj.RealCrossoverCount++;
                for (i = 0; i < ProblemObj.RealVariableCount; i++)
                {
                    if (RandomizationObj.RandomPercent() <= 0.5)
                    {
                        if (Math.Abs(parent1.Xreal[i] - parent2.Xreal[i]) > ProblemObj.EPS)
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
                            yl = ProblemObj.min_realvar[i];
                            yu = ProblemObj.max_realvar[i];
                            rand = RandomizationObj.RandomPercent();
                            beta = 1.0 + 2.0 * (y1 - yl) / (y2 - y1);
                            alpha = 2.0 - Math.Pow(beta, -(ProblemObj.CrossoverDistributionIndex + 1.0));
                            if (rand <= 1.0 / alpha)
                            {
                                betaq = Math.Pow(rand * alpha, 1.0 / (ProblemObj.CrossoverDistributionIndex + 1.0));
                            }
                            else
                            {
                                betaq = Math.Pow(1.0 / (2.0 - rand * alpha), 1.0 / (ProblemObj.CrossoverDistributionIndex + 1.0));
                            }
                            c1 = 0.5 * (y1 + y2 - betaq * (y2 - y1));
                            beta = 1.0 + 2.0 * (yu - y2) / (y2 - y1);
                            alpha = 2.0 - Math.Pow(beta, -(ProblemObj.CrossoverDistributionIndex + 1.0));
                            if (rand <= 1.0 / alpha)
                            {
                                betaq = Math.Pow(rand * alpha, 1.0 / (ProblemObj.CrossoverDistributionIndex + 1.0));
                            }
                            else
                            {
                                betaq = Math.Pow(1.0 / (2.0 - rand * alpha), 1.0 / (ProblemObj.CrossoverDistributionIndex + 1.0));
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
                for (i = 0; i < ProblemObj.RealVariableCount; i++)
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
            for (i = 0; i < ProblemObj.BinaryVariableCount; i++)
            {
                rand = RandomizationObj.RandomPercent();
                if (rand <= ProblemObj.BinaryCrossoverProbability)
                {
                    ProblemObj.BinaryCrossoverCount++;
                    site1 = RandomizationObj.RandomInteger(0, ProblemObj.nbits[i] - 1);
                    site2 = RandomizationObj.RandomInteger(0, ProblemObj.nbits[i] - 1);
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
                    for (j = site2; j < ProblemObj.nbits[i]; j++)
                    {
                        child1.Gene[i, j] = parent1.Gene[i, j];
                        child2.Gene[i, j] = parent2.Gene[i, j];
                    }
                }
                else
                {
                    for (j = 0; j < ProblemObj.nbits[i]; j++)
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

            for (i = 0; i < ProblemObj.ObjectiveCount; i++)
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
            for (i = 0; i < ProblemObj.PopulationSize; i++)
            {
                MutateIndividual(pop.IndList[i]);
            }
        }

        /* Function to perform mutation of an individual */
        static void MutateIndividual(Individual ind)
        {
            if (ProblemObj.RealVariableCount != 0)
            {
                RealMutate(ind);
            }
            if (ProblemObj.BinaryVariableCount != 0)
            {
                BinaryMutate(ind);
            }
        }

        /* Routine for binary mutation of an individual */
        static void BinaryMutate(Individual ind)
        {
            int j, k;
            double prob;
            for (j = 0; j < ProblemObj.BinaryVariableCount; j++)
            {
                for (k = 0; k < ProblemObj.nbits[j]; k++)
                {
                    prob = RandomizationObj.RandomPercent();
                    if (prob <= ProblemObj.BinaryMutationProbability)
                    {
                        if (ind.Gene[j, k] == 0)
                        {
                            ind.Gene[j, k] = 1;
                        }
                        else
                        {
                            ind.Gene[j, k] = 0;
                        }
                        ProblemObj.BinaryMutationCount += 1;
                    }
                }
            }
        }

        /* Routine for real polynomial mutation of an individual */
        static void RealMutate(Individual ind)
        {
            int j;
            double rnd, delta1, delta2, mutPow, deltaq;
            double y, yl, yu, val, xy;
            for (j = 0; j < ProblemObj.RealVariableCount; j++)
            {
                if (RandomizationObj.RandomPercent() <= ProblemObj.RealMutationProbability)
                {
                    y = ind.Xreal[j];
                    yl = ProblemObj.min_realvar[j];
                    yu = ProblemObj.max_realvar[j];
                    delta1 = (y - yl) / (yu - yl);
                    delta2 = (yu - y) / (yu - yl);
                    rnd = RandomizationObj.RandomPercent();
                    mutPow = 1.0 / (ProblemObj.MutationDistributionIndex + 1.0);
                    if (rnd <= 0.5)
                    {
                        xy = 1.0 - delta1;
                        val = 2.0 * rnd + (1.0 - 2.0 * rnd) * Math.Pow(xy, ProblemObj.MutationDistributionIndex + 1.0);
                        deltaq = Math.Pow(val, mutPow) - 1.0;
                    }
                    else
                    {
                        xy = 1.0 - delta2;
                        val = 2.0 * (1.0 - rnd) + 2.0 * (rnd - 0.5) * Math.Pow(xy, ProblemObj.MutationDistributionIndex + 1.0);
                        deltaq = 1.0 - Math.Pow(val, mutPow);
                    }
                    y = y + deltaq * (yu - yl);
                    if (y < yl)
                        y = yl;
                    if (y > yu)
                        y = yu;
                    ind.Xreal[j] = y;
                    ProblemObj.RealMutationCount += 1;
                }
            }
        }
        #endregion

        #region list.c
        /* Insert an element X into the list at location specified by NODE */
        static void Insert(Lists node, int x)
        {
            if (node == null)
            {
                Console.WriteLine(" Error!! asked to enter after a null pointer, hence exiting \n");
                Environment.Exit(1);
            }
            var temp = new Lists
            {
                index = x,
                child = node.child,
                parent = node
            };
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
            int frontSize;
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
            for (i = 0; i < ProblemObj.PopulationSize; i++)
            {
                Insert(temp1, i);
                temp1 = temp1.child;
            }
            do
            {
                if (orig.child.child == null)
                {
                    newPopulation.IndList[orig.child.index].Rank = rank;
                    newPopulation.IndList[orig.child.index].CrowdDist = ProblemObj.INF;
                    break;
                }
                temp1 = orig.child;
                Insert(cur, temp1.index);
                frontSize = 1;
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
                            frontSize--;
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
                        frontSize++;
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
                assign_crowding_distance_list(newPopulation, cur.child, frontSize);
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
            int[][] objArray;
            int[] dist;
            int i, j;
            Lists temp;
            temp = lst;
            if (frontSize == 1)
            {
                pop.IndList[lst.index].CrowdDist = ProblemObj.INF;
                return;
            }
            if (frontSize == 2)
            {
                pop.IndList[lst.index].CrowdDist = ProblemObj.INF;
                pop.IndList[lst.child.index].CrowdDist = ProblemObj.INF;
                return;
            }
            dist = new int[frontSize];
            objArray = new int[ProblemObj.ObjectiveCount][];
            //obj_array = (int**)malloc(ProblemObj.ObjectiveCount * sizeof(int*));
            for (i = 0; i < ProblemObj.ObjectiveCount; i++)
            {
                objArray[i] = new int[frontSize];
            }
            for (j = 0; j < frontSize; j++)
            {
                dist[j] = temp.index;
                temp = temp.child;
            }
            assign_crowding_distance(pop, dist, objArray, frontSize);

        }

        /* Routine to compute crowding distance based on objective function values when the population in in the form of an array */
        static void assign_crowding_distance_indices(Population pop, int c1, int c2)
        {
            int[][] objArray;
            int[] dist;
            int i, j;
            int frontSize;
            frontSize = c2 - c1 + 1;
            if (frontSize == 1)
            {
                pop.IndList[c1].CrowdDist = ProblemObj.INF;
                return;
            }
            if (frontSize == 2)
            {
                pop.IndList[c1].CrowdDist = ProblemObj.INF;
                pop.IndList[c2].CrowdDist = ProblemObj.INF;
                return;
            }
            dist = new int[frontSize];
            objArray = new int[ProblemObj.ObjectiveCount][];
            //obj_array = (int**)malloc(ProblemObj.ObjectiveCount * sizeof(int*));
            for (i = 0; i < ProblemObj.ObjectiveCount; i++)
            {
                objArray[i] = new int[frontSize];
            }

            for (j = 0; j < frontSize; j++)
            {
                dist[j] = c1++;
            }
            assign_crowding_distance(pop, dist, objArray, frontSize);

        }

        /* Routine to compute crowding distances */
        static void assign_crowding_distance(Population pop, int[] dist, int[][] objArray, int frontSize)
        {
            int i, j;
            for (i = 0; i < ProblemObj.ObjectiveCount; i++)
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
            for (i = 0; i < ProblemObj.ObjectiveCount; i++)
            {
                pop.IndList[objArray[i][0]].CrowdDist = ProblemObj.INF;
            }
            for (i = 0; i < ProblemObj.ObjectiveCount; i++)
            {
                for (j = 1; j < frontSize - 1; j++)
                {
                    if (pop.IndList[objArray[i][j]].CrowdDist != ProblemObj.INF)
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
                if (pop.IndList[dist[j]].CrowdDist != ProblemObj.INF)
                {
                    pop.IndList[dist[j]].CrowdDist = pop.IndList[dist[j]].CrowdDist / ProblemObj.ObjectiveCount;
                }
            }
        }
        #endregion

        #region sort.c
        /* Randomized quick sort routine to sort a population based on a particular objective chosen */
        static void quicksort_front_obj(Population pop, int objcount, int[] objArray, int objArraySize)
        {
            q_sort_front_obj(pop, objcount, objArray, 0, objArraySize - 1);
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

        #region fillnds.c

        /* Routine to perform non-dominated sorting */
        static void fill_nondominated_sort(Population mixedPop, Population newPop)
        {
            int flag;
            int i, j;
            int end;
            int frontSize;
            int archieveSize;
            int rank = 1;
            Lists pool;
            Lists elite;
            Lists temp1, temp2;
            pool = new Lists();
            elite = new Lists();
            frontSize = 0;
            archieveSize = 0;
            pool.index = -1;
            pool.parent = null;
            pool.child = null;
            elite.index = -1;
            elite.parent = null;
            elite.child = null;
            temp1 = pool;
            for (i = 0; i < 2 * ProblemObj.PopulationSize; i++)
            {
                Insert(temp1, i);
                temp1 = temp1.child;
            }
            i = 0;
            do
            {
                temp1 = pool.child;
                Insert(elite, temp1.index);
                frontSize = 1;
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
                            frontSize--;
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
                        frontSize++;
                        temp1 = Delete(temp1);
                    }
                    temp1 = temp1.child;
                }
                while (temp1 != null);
                temp2 = elite.child;
                j = i;
                if (archieveSize + frontSize <= ProblemObj.PopulationSize)
                {
                    do
                    {
                        newPop.IndList[i].Copy(mixedPop.IndList[temp2.index],ProblemObj);
                        //CopyIndividual(mixedPop.IndList[temp2.index], newPop.IndList[i]);
                        
                        newPop.IndList[i].Rank = rank;
                        archieveSize += 1;
                        temp2 = temp2.child;
                        i += 1;
                    }
                    while (temp2 != null);
                    assign_crowding_distance_indices(newPop, j, i - 1);
                    rank += 1;
                }
                else
                {
                    crowding_fill(mixedPop, newPop, i, frontSize, elite);
                    archieveSize = ProblemObj.PopulationSize;
                    for (j = i; j < ProblemObj.PopulationSize; j++)
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
            while (archieveSize < ProblemObj.PopulationSize);

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
            for (i = count, j = frontSize - 1; i < ProblemObj.PopulationSize; i++, j--)
            {
                newPop.IndList[i].Copy(mixedPop.IndList[dist[j]],ProblemObj);
                //CopyIndividual(mixedPop.IndList[dist[j]], newPop.IndList[i]);
                //newPop.IndList[i] = new Individual(mixedPop.IndList[dist[j]], ProblemObj);

            }

        }

        #endregion

        #endregion

    }
}
