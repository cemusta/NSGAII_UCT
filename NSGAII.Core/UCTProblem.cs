using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using NSGAII.Models;

namespace NSGAII
{
    public class UCTProblem
    {
        #region Variable 

        public double Seed;
        public bool UsePlot;
        public int CurrentGeneration { get; set; }
        public ProblemDefinition ProblemObj;
        public Randomization RandomizationObj;
        private Display DisplayObj;

        public Population ParentPopulation;
        public Population ChildPopulation;
        public Population MixedPopulation;
        public int Best = 0;
        public int AdaptiveClimb = 0;

        #endregion

        public UCTProblem(double dSeed, int nPopulation, int nMaxGeneration, int nObjective, int nConstraint, int nBinaryVar, int nRealVar, bool usePlot = false)
        {
            CurrentGeneration = 0;
            Seed = dSeed;
            if (Seed <= 0.0 || Seed >= 1.0)
            {
                Console.WriteLine("\n Entered seed value is wrong, seed value must be in (0,1) \n");
                Seed = 0.75;
            }

            UsePlot = usePlot;

            var title = RandomTitle.GetRandomTitle();

            ProblemObj = new ProblemDefinition(title)
            {
                PopulationSize = nPopulation,
                MaxGeneration = nMaxGeneration,
                ObjectiveCount = nObjective,
                ConstraintCount = nConstraint,
                BinaryVariableCount = nBinaryVar,
                RealVariableCount = nRealVar
            };

            if (ProblemObj.RealVariableCount == 0 && ProblemObj.BinaryVariableCount == 0)
            {
                Console.WriteLine("\n Number of real as well as binary variables, both are zero, hence exiting \n");
                throw new Exception("Number of real as well as binary variables, both are zero");
            }

            for (int i = 0; i < 8; i++)
            {
                ProblemObj.FacultyCourses.Add(new List<List<int>>());
                for (int j = 0; j < 5; j++)
                {
                    ProblemObj.FacultyCourses[i].Add(new List<int>());
                    for (int k = 0; k < 9; k++)
                    {
                        ProblemObj.FacultyCourses[i][j].Add(0);
                    }
                }
            }

            if (ProblemObj.BinaryVariableCount > 0)
                ReadBinaryValues();

            if (ProblemObj.RealVariableCount > 0)
                ReadRealValues();

            WriteStartParams();

            RandomizationObj = new Randomization(dSeed);
            RandomizationObj.Randomize();

            DisplayObj = new Display();
            InitDisplay(true, true, new[] { 0, 1, 2 });

            ReadScheduling();
            ReadLab();
            ReadMeeting();
            ReadCourseList();
            ReadPreq();

            CreatePopulationObject();
        }

        private UCTProblem()
        {

        }

        private void ReadBinaryValues()
        {
            try
            {
                FileStream fileStr = File.OpenRead("binary.in");
                StreamReader reader = new StreamReader(fileStr);
                string line;

                ProblemObj.nbits = new int[ProblemObj.BinaryVariableCount];
                ProblemObj.min_binvar = new double[ProblemObj.BinaryVariableCount];
                ProblemObj.max_binvar = new double[ProblemObj.BinaryVariableCount];
                for (int i = 0; i < ProblemObj.BinaryVariableCount; i++)
                {
                    line = reader.ReadLine();
                    var parts = line.Split(new char[] { ' ' });
                    ProblemObj.nbits[i] = int.Parse(parts[0]);
                    if (ProblemObj.nbits[i] > ProblemObj.MaxBitCount)
                        ProblemObj.MaxBitCount = ProblemObj.nbits[i];
                    if (ProblemObj.nbits[i] < 1)
                    {
                        throw new Exception("Wrong number of bits for binary variable entered, hence exiting");
                    }

                    ProblemObj.min_binvar[i] = int.Parse(parts[1]);

                    ProblemObj.max_binvar[i] = int.Parse(parts[2]);

                    if (ProblemObj.max_binvar[i] <= ProblemObj.min_binvar[i])
                    {
                        throw new Exception(
                            " Wrong limits entered for the min and max bounds of binary variable entered, hence exiting");
                    }
                }

                line = reader.ReadLine();
                ProblemObj.BinaryCrossoverProbability = double.Parse(line);
                if (ProblemObj.BinaryCrossoverProbability < 0.0 || ProblemObj.BinaryCrossoverProbability > 1.0)
                {
                    ProblemObj.BinaryCrossoverProbability = 0.75;
                }

                line = reader.ReadLine();
                ProblemObj.BinaryMutationProbability = double.Parse(line);
                if (ProblemObj.BinaryMutationProbability < 0.0 || ProblemObj.BinaryMutationProbability > 1.0)
                {
                    ProblemObj.BinaryMutationProbability = 0.0232558;
                }
            }
            catch (Exception)
            {
                throw;
            }


        }

        private void ReadRealValues()
        {
            try
            {
                FileStream fileStr = File.OpenRead("real.in");
                StreamReader reader = new StreamReader(fileStr);
                string line;

                ProblemObj.min_realvar = new double[ProblemObj.RealVariableCount];
                ProblemObj.max_realvar = new double[ProblemObj.RealVariableCount];
                for (int i = 0; i < ProblemObj.RealVariableCount; i++)
                {
                    line = reader.ReadLine();
                    ProblemObj.min_realvar[i] = double.Parse(line);

                    line = reader.ReadLine();
                    ProblemObj.max_realvar[i] = double.Parse(line);
                    if (ProblemObj.max_realvar[i] <= ProblemObj.min_realvar[i])
                    {
                        throw new Exception("Wrong limits entered for the min and max bounds of real variable");
                    }
                }

                line = Console.ReadLine();
                ProblemObj.RealCrossoverProbability = double.Parse(line);
                if (ProblemObj.RealCrossoverProbability < 0.0 || ProblemObj.RealCrossoverProbability > 1.0)
                {
                    throw new Exception("Entered value of probability of Crossover of real variables is out of bounds");
                }

                line = Console.ReadLine();
                ProblemObj.RealMutationProbability = double.Parse(line);
                if (ProblemObj.RealMutationProbability < 0.0 || ProblemObj.RealMutationProbability > 1.0)
                {
                    throw new Exception("Entered value of probability of mutation of real variables is out of bounds");
                }

                line = Console.ReadLine();
                ProblemObj.CrossoverDistributionIndex = double.Parse(line);
                if (ProblemObj.CrossoverDistributionIndex <= 0)
                {
                    throw new Exception(" Wrong value of distribution index for Crossover entered, hence exiting \n");
                }

                line = Console.ReadLine();
                ProblemObj.MutationDistributionIndex = double.Parse(line);
                if (ProblemObj.MutationDistributionIndex <= 0)
                {
                    throw new Exception("Wrong value of distribution index for mutation entered");
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void InitDisplay(bool useGnuplot, bool use3D, int[] arrGnuplotObjective)
        {
            if (!useGnuplot)
            {
                return;
            }

            if (ProblemObj.ObjectiveCount == 2)
            {
                DisplayObj.GnuplotObjective1 = arrGnuplotObjective[0];

                DisplayObj.GnuplotObjective2 = arrGnuplotObjective[1];

                DisplayObj.GnuplotObjective3 = -1;
            }
            else
            {

                if (!use3D)
                {
                    Console.WriteLine(" Enter the objective for X axis display : ");

                    DisplayObj.GnuplotObjective1 = arrGnuplotObjective[0];

                    DisplayObj.GnuplotObjective2 = arrGnuplotObjective[1];

                    DisplayObj.GnuplotObjective3 = -1;
                }
                else
                {
                    DisplayObj.Use3D = true;

                    DisplayObj.GnuplotObjective1 = arrGnuplotObjective[0];

                    DisplayObj.GnuplotObjective2 = arrGnuplotObjective[1];

                    DisplayObj.GnuplotObjective3 = arrGnuplotObjective[2];
                }
            }
        }

        private void ReadScheduling()
        {
            try
            {
                var inputSchedulingFile = File.OpenRead("scheduling.in");
                StreamReader reader = new StreamReader(inputSchedulingFile);

                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        var line = reader.ReadLine();
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int k = 0; k < 5; k++)
                        {
                            ProblemObj.FacultyCourses[i][k][j] = int.Parse(parts[k]);
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
        }

        private void ReadLab()
        {
            ProblemObj.LabScheduling.Clear();
            for (int k = 0; k < 5; k++)
            {
                ProblemObj.LabScheduling.Add(new List<int>());
            }

            try
            {
                FileStream inputLabsFile = File.OpenRead("lab_list.in");
                StreamReader reader = new StreamReader(inputLabsFile);
                for (int j = 0; j < 9; j++)
                {
                    var line = reader.ReadLine();
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int k = 0; k < 5; k++)
                    {
                        ProblemObj.LabScheduling[k].Add(int.Parse(parts[k]));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private void ReadMeeting()
        {
            ProblemObj.Meeting.Clear();
            for (int k = 0; k < 5; k++)
            {
                ProblemObj.Meeting.Add(new List<int>());
            }

            try
            {
                FileStream meetingFile = File.OpenRead("Meeting.txt");
                var reader = new StreamReader(meetingFile);
                for (int j = 0; j < 9; j++)
                {
                    var line = reader.ReadLine();
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int k = 0; k < 5; k++)
                    {
                        ProblemObj.Meeting[k].Add(int.Parse(parts[k]));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }

        private void ReadCourseList()
        {
            try
            {
                FileStream courseListFile = File.OpenRead("course_list.csv");
                var reader = new StreamReader(courseListFile);

                var line = reader.ReadLine();
                int courseCount = int.Parse(line); //ilk satırda ders adedi olmalı...

                Console.WriteLine($"SIZE: {courseCount} \n");

                for (int courseId = 0; courseId < courseCount; courseId++)
                {
                    line = reader.ReadLine();

                    var parts = line.Split(';');

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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }


        }

        private void ReadPreq()
        {
            try
            {
                FileStream prerequisiteFile = File.OpenRead("Onkosul-list.csv");
                var reader = new StreamReader(prerequisiteFile);
                string line;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private void WriteStartParams()
        {
            Directory.CreateDirectory("report");
            var file = File.OpenWrite("report\\" + ProblemObj.Title + "_params.out");
            StreamWriter writer = new StreamWriter(file);
            writer.Write("# This file contains information about inputs as read by the program\n");
            writer.Flush();


            writer.WriteLine($" Population size = {ProblemObj.PopulationSize}");
            writer.WriteLine($" Number of generations = {ProblemObj.MaxGeneration}");
            writer.WriteLine($" Number of objective functions = {ProblemObj.ObjectiveCount}");
            writer.WriteLine($" Number of constraints = {ProblemObj.ConstraintCount}");
            writer.WriteLine($" Number of real variables = {ProblemObj.RealVariableCount}");
            if (ProblemObj.RealVariableCount != 0)
            {
                for (int i = 0; i < ProblemObj.RealVariableCount; i++)
                {
                    writer.WriteLine($" Lower limit of real variable {i + 1} = {ProblemObj.min_realvar[i]}");
                    writer.WriteLine($" Upper limit of real variable {i + 1} = {ProblemObj.max_realvar[i]}");
                }
                writer.WriteLine($" Probability of crossover of real variable = {ProblemObj.RealCrossoverProbability}");
                writer.WriteLine($" Probability of mutation of real variable = {ProblemObj.RealMutationProbability}");
                writer.WriteLine($" Distribution index for crossover = {ProblemObj.CrossoverDistributionIndex}");
                writer.WriteLine($" Distribution index for mutation = {ProblemObj.MutationDistributionIndex}");
            }
            writer.Write($" Number of binary variables = {ProblemObj.BinaryVariableCount}");
            if (ProblemObj.BinaryVariableCount != 0)
            {
                for (int i = 0; i < ProblemObj.BinaryVariableCount; i++)
                {
                    writer.WriteLine($" Number of bits for binary variable {i + 1} = {ProblemObj.nbits[i]}");
                    writer.WriteLine($" Lower limit of binary variable {i + 1} = {ProblemObj.min_binvar[i]}");
                    writer.WriteLine($" Upper limit of binary variable {i + 1} = {ProblemObj.max_binvar[i]}");
                }
                writer.WriteLine($" Probability of crossover of binary variable = {ProblemObj.BinaryCrossoverProbability}");
                writer.WriteLine($" Probability of mutation of binary variable = {ProblemObj.BinaryMutationProbability}");
            }

            writer.Write($" Seed for random number generator = {Seed}");

            ProblemObj.TotalBinaryBitLength = 0;
            if (ProblemObj.BinaryVariableCount != 0)
            {
                for (int i = 0; i < ProblemObj.BinaryVariableCount; i++)
                {
                    ProblemObj.TotalBinaryBitLength += ProblemObj.nbits[i];
                }
            }

        }

        private void CreatePopulationObject()
        {
            ParentPopulation = new Population(ProblemObj.PopulationSize, ProblemObj.RealVariableCount, ProblemObj.BinaryVariableCount, ProblemObj.MaxBitCount, ProblemObj.ObjectiveCount, ProblemObj.ConstraintCount);
            ChildPopulation = new Population(ProblemObj.PopulationSize, ProblemObj.RealVariableCount, ProblemObj.BinaryVariableCount, ProblemObj.MaxBitCount, ProblemObj.ObjectiveCount, ProblemObj.ConstraintCount);
            MixedPopulation = new Population(ProblemObj.PopulationSize * 2, ProblemObj.RealVariableCount, ProblemObj.BinaryVariableCount, ProblemObj.MaxBitCount, ProblemObj.ObjectiveCount, ProblemObj.ConstraintCount);
        }



        public void FirstGeneration()
        {
            CurrentGeneration++;

            ParentPopulation.Initialize(ProblemObj, RandomizationObj);

            ParentPopulation.Decode(ProblemObj);
            ParentPopulation.Evaluate(ProblemObj);

            assign_rank_and_crowding_distance(ParentPopulation, ProblemObj, RandomizationObj);

            ParentPopulation.ReportPopulation(ProblemObj, "initial", "This file contains the data of final population");
            ParentPopulation.ReportPopulation(ProblemObj, "current", "This file contains the data of current generation", CurrentGeneration);

            if (UsePlot)
            {
                DisplayObj.PlotPopulation(ParentPopulation, ProblemObj, CurrentGeneration);
            }

            int minimumResult = ParentPopulation.IndList.Min(x => x.TotalResult);
            Best = minimumResult;

        }

        public void NextGeneration(HillClimbMode mode = HillClimbMode.None)
        {
            if (CurrentGeneration >= ProblemObj.MaxGeneration)
                return;

            CurrentGeneration++;

            Selection(ParentPopulation, ChildPopulation, ProblemObj, RandomizationObj);
            MutatePopulation(ChildPopulation, ProblemObj, RandomizationObj);

            ChildPopulation.Decode(ProblemObj);
            ChildPopulation.Evaluate(ProblemObj);
            if (mode == HillClimbMode.All || mode == HillClimbMode.ChildOnly)
            {
                ChildPopulation.HillClimb(ProblemObj);
            }


            MixedPopulation.Merge(ParentPopulation, ChildPopulation, ProblemObj);
            //mixedPopulation.Decode(ProblemObj);
            //mixedPopulation.Evaluate(ProblemObj);

            fill_nondominated_sort(MixedPopulation, ParentPopulation, ProblemObj, RandomizationObj);

            ParentPopulation.Decode(ProblemObj);
            ParentPopulation.Evaluate(ProblemObj);
            if (mode == HillClimbMode.All || mode == HillClimbMode.ParentOnly)
            {
                ParentPopulation.HillClimb(ProblemObj);
            }

            int minimumResult = ParentPopulation.IndList.Min(x => x.TotalResult);
            if (minimumResult < Best)
            {
                Best = minimumResult;
                AdaptiveClimb = 0;
            }
            else
            {
                AdaptiveClimb++;
            }

            if (mode == HillClimbMode.AdaptiveParent && AdaptiveClimb >= 25)
            {
                ParentPopulation.HillClimb(ProblemObj);
                minimumResult = ParentPopulation.IndList.Min(x => x.TotalResult);
                Best = minimumResult;
                AdaptiveClimb = 0;
            }
            else if (mode == HillClimbMode.AdaptiveRank1All && AdaptiveClimb >= 25)
            {
                var rank1All = ParentPopulation.IndList.Where(x => x.Rank == 1);
                foreach (var child in rank1All)
                {
                    child.HillClimb(ProblemObj);
                }
                minimumResult = ParentPopulation.IndList.Min(x => x.TotalResult);
                Best = minimumResult;
                AdaptiveClimb = 0;
            }

            var result = minimumResult;
            var bestChild = ParentPopulation.IndList.Where(x => x.TotalResult == result).ToList();

            if (mode == HillClimbMode.BestOfParent)
            {
                bestChild.First().HillClimb(ProblemObj);
            }
            else if (mode == HillClimbMode.AllBestOfParent)
            {
                foreach (var child in bestChild)
                {
                    child.HillClimb(ProblemObj);
                }
            }
            else if (mode == HillClimbMode.Rank1Best)
            {
                var rank1Best = ParentPopulation.IndList.First(x => x.Rank == 1 && x.TotalResult == result);
                rank1Best.HillClimb(ProblemObj);
            }
            else if (mode == HillClimbMode.Rank1All)
            {
                var rank1All = ParentPopulation.IndList.Where(x => x.Rank == 1);
                foreach (var child in rank1All)
                {
                    child.HillClimb(ProblemObj);
                }
            }

            // Comment following  lines if information for all
            //parent_pop.ReportPopulation(fpt4,ProblemObj);

            if (UsePlot)
            {
                DisplayObj.PlotPopulation(ParentPopulation, ProblemObj, CurrentGeneration, bestChild.ToList());
            }

            Console.WriteLine(GenerationReport());
            Console.WriteLine(BestReport());


        }

        public string GenerationReport()
        {
            int minimumResult = ParentPopulation.IndList.Min(x => x.TotalResult);
            return ($" gen = {CurrentGeneration} min = {minimumResult}");
        }

        public string BestReport()
        {
            int minimumResult = ParentPopulation.IndList.Min(x => x.TotalResult);
            var result = minimumResult;
            var bc = ParentPopulation.IndList.First(x => x.TotalResult == result);
            return $" best: coll  = {bc.CollisionList.Count} result = {bc.TotalResult} obj0:{bc.Obj[0]} obj1:{bc.Obj[1]} obj2:{bc.Obj[2]}";

        }

        public void PlotNow()
        {
            if (CurrentGeneration != 0)
            {
                int minimumResult = ParentPopulation.IndList.Min(x => x.TotalResult);
                var bestChild = ParentPopulation.IndList.Where(x => x.TotalResult == minimumResult).ToList();
                DisplayObj.PlotPopulation(ParentPopulation, ProblemObj, CurrentGeneration, bestChild.ToList());
            }
        }



        public void WriteCurrentGeneration()
        {
            ParentPopulation.ReportPopulation(ProblemObj, "current", "This file contains the data of current generation", CurrentGeneration);
        }

        public void WriteFinalGeneration()
        {
            ParentPopulation.ReportPopulation(ProblemObj, "final", "This file contains the data of final generation", CurrentGeneration);
        }

        public void WriteBestGeneration()
        {
            ParentPopulation.ReportFeasiblePopulation(ProblemObj, "best", "This file contains the data of best individuals");
        }

        public void WriteMethod(HillClimbMode temp = HillClimbMode.None)
        {
            Directory.CreateDirectory("report");
            var file = File.OpenWrite($"report\\{ProblemObj.Title}_method.out");
            StreamWriter writer = new StreamWriter(file);

            writer.WriteLine($"seed: {RandomizationObj.seed}");
            writer.WriteLine($"pop.: {ProblemObj.PopulationSize}");
            writer.WriteLine($"gen.: {ProblemObj.MaxGeneration}");
            switch (temp)
            {
                case HillClimbMode.None:
                    writer.WriteLine("Method: Basic NSGAII");
                    break;
                case HillClimbMode.ChildOnly:
                    writer.WriteLine("Method: NSGAII with hillclimb on child pop");
                    break;
                case HillClimbMode.MixedOnly:
                    writer.WriteLine("Method: NSGAII with hillclimb on mixed pop");
                    break;
                case HillClimbMode.ParentOnly:
                    writer.WriteLine("Method: NSGAII with hillclimb on parent pop");
                    break;
                case HillClimbMode.All:
                    writer.WriteLine("Method: NSGAII with hillclimb on all pops");
                    break;
                case HillClimbMode.BestOfParent:
                    writer.WriteLine("Method: NSGAII with hillclimb on best of parent pop");
                    break;
                case HillClimbMode.AllBestOfParent:
                    writer.WriteLine("Method: NSGAII with hillclimb on all best of parent pop");
                    break;
                case HillClimbMode.AdaptiveParent:
                    writer.WriteLine("Method: NSGAII with adaptive hillclimb on parent pop");
                    break;
                case HillClimbMode.Rank1Best:
                    writer.WriteLine("Method: NSGAII with hillclimb on best rank 1 parent");
                    break;
                case HillClimbMode.Rank1All:
                    writer.WriteLine("Method: NSGAII with hillclimb on rank 1 parents");
                    break;
                case HillClimbMode.AdaptiveRank1All:
                    writer.WriteLine("Method: NSGAII with adaptive hillclimb on rank 1 parents");
                    break;
            }


            writer.Flush();
            writer.Close();
        }

        public static UCTProblem LoadFromFile(string filename)
        {
            try
            {
                var readFile = File.ReadAllText(filename, Encoding.UTF8);
                var temp = SerializationHelper.DeserializeObject<UCTProblem>(readFile);

                temp.DisplayObj = new Display();
                temp.InitDisplay(true, true, new[] { 0, 1, 2 });

                temp.ChildPopulation.Decode(temp.ProblemObj);
                temp.MixedPopulation.Decode(temp.ProblemObj);
                temp.ParentPopulation.Decode(temp.ProblemObj);

                temp.ChildPopulation.Evaluate(temp.ProblemObj);
                temp.MixedPopulation.Evaluate(temp.ProblemObj);
                temp.ParentPopulation.Evaluate(temp.ProblemObj);

                return temp;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static bool SaveToFile(UCTProblem uctToSave, string filename)
        {
            try
            {
                Directory.CreateDirectory("report");
                var textToSave = SerializationHelper.SerializeObject(uctToSave);

                File.WriteAllText("report\\" + filename + ".problem", textToSave, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }


        public int HillClimbParent()
        {
            ParentPopulation.Decode(ProblemObj);
            ParentPopulation.Evaluate(ProblemObj);
            return ParentPopulation.HillClimb(ProblemObj);
        }

        public int HillClimbBest()
        {
            ParentPopulation.Decode(ProblemObj);
            ParentPopulation.Evaluate(ProblemObj);

            int minimumResult = ParentPopulation.IndList.Min(x => x.TotalResult);
            var result = minimumResult;
            var bc = ParentPopulation.IndList.First(x => x.TotalResult == result);

            return bc.HillClimb(ProblemObj);
        }


        #region NSGAII stuff

        #region tourselect.c
        /* Routine for Tournament Selection, it creates a newPopulation from oldPopulation by performing Tournament Selection and the Crossover */
        static void Selection(Population oldPopulation, Population newPopulation, ProblemDefinition ProblemObj, Randomization RandomizationObj)
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
                parent1 = Tournament(oldPopulation.IndList[a1[i]], oldPopulation.IndList[a1[i + 1]], ProblemObj, RandomizationObj);
                parent2 = Tournament(oldPopulation.IndList[a1[i + 2]], oldPopulation.IndList[a1[i + 3]], ProblemObj, RandomizationObj);
                Crossover(parent1, parent2, newPopulation.IndList[i], newPopulation.IndList[i + 1], ProblemObj, RandomizationObj);
                parent1 = Tournament(oldPopulation.IndList[a2[i]], oldPopulation.IndList[a2[i + 1]], ProblemObj, RandomizationObj);
                parent2 = Tournament(oldPopulation.IndList[a2[i + 2]], oldPopulation.IndList[a2[i + 3]], ProblemObj, RandomizationObj);
                Crossover(parent1, parent2, newPopulation.IndList[i + 2], newPopulation.IndList[i + 3], ProblemObj, RandomizationObj);
            }

        }

        /* Routine for binary Tournament */
        static Individual Tournament(Individual ind1, Individual ind2, ProblemDefinition ProblemObj, Randomization RandomizationObj)
        {
            int flag;
            flag = CheckDominance(ind1, ind2, ProblemObj);
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

        #region crossover.c
        /* Function to cross two individuals */
        static void Crossover(Individual parent1, Individual parent2, Individual child1, Individual child2, ProblemDefinition ProblemObj, Randomization RandomizationObj)
        {
            if (ProblemObj.RealVariableCount != 0)
            {
                RealCrossover(parent1, parent2, child1, child2, ProblemObj, RandomizationObj);
            }
            if (ProblemObj.BinaryVariableCount != 0)
            {
                BinaryCrossover(parent1, parent2, child1, child2, ProblemObj, RandomizationObj);
            }
        }

        /* Routine for real variable SBX Crossover */
        static void RealCrossover(Individual parent1, Individual parent2, Individual child1, Individual child2, ProblemDefinition ProblemObj, Randomization RandomizationObj)
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
        static void BinaryCrossover(Individual parent1, Individual parent2, Individual child1, Individual child2, ProblemDefinition ProblemObj, Randomization RandomizationObj)
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
                        child1.Gene[i][j] = parent1.Gene[i][j];
                        child2.Gene[i][j] = parent2.Gene[i][j];
                    }
                    for (j = site1; j < site2; j++)
                    {
                        child1.Gene[i][j] = parent2.Gene[i][j];
                        child2.Gene[i][j] = parent1.Gene[i][j];
                    }
                    for (j = site2; j < ProblemObj.nbits[i]; j++)
                    {
                        child1.Gene[i][j] = parent1.Gene[i][j];
                        child2.Gene[i][j] = parent2.Gene[i][j];
                    }
                }
                else
                {
                    for (j = 0; j < ProblemObj.nbits[i]; j++)
                    {
                        child1.Gene[i][j] = parent1.Gene[i][j];
                        child2.Gene[i][j] = parent2.Gene[i][j];
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
        static int CheckDominance(Individual a, Individual b, ProblemDefinition ProblemObj)
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
        static void MutatePopulation(Population pop, ProblemDefinition ProblemObj, Randomization RandomizationObj)
        {
            int i;
            for (i = 0; i < ProblemObj.PopulationSize; i++)
            {
                MutateIndividual(pop.IndList[i], ProblemObj, RandomizationObj);
            }
        }

        /* Function to perform mutation of an individual */
        static void MutateIndividual(Individual ind, ProblemDefinition ProblemObj, Randomization RandomizationObj)
        {
            if (ProblemObj.RealVariableCount != 0)
            {
                RealMutate(ind, ProblemObj, RandomizationObj);
            }
            if (ProblemObj.BinaryVariableCount != 0)
            {
                BinaryMutate(ind, ProblemObj, RandomizationObj);
            }
        }

        /* Routine for binary mutation of an individual */
        static void BinaryMutate(Individual ind, ProblemDefinition ProblemObj, Randomization RandomizationObj)
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
                        if (ind.Gene[j][k] == 0)
                        {
                            ind.Gene[j][k] = 1;
                        }
                        else
                        {
                            ind.Gene[j][k] = 0;
                        }
                        ProblemObj.BinaryMutationCount += 1;
                    }
                }
            }
        }

        /* Routine for real polynomial mutation of an individual */
        static void RealMutate(Individual ind, ProblemDefinition ProblemObj, Randomization RandomizationObj)
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
        static void assign_rank_and_crowding_distance(Population newPopulation, ProblemDefinition ProblemObj, Randomization RandomizationObj)
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
                        flag = CheckDominance(newPopulation.IndList[temp1.index], newPopulation.IndList[temp2.index], ProblemObj);
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
                assign_crowding_distance_list(newPopulation, cur.child, frontSize, ProblemObj, RandomizationObj);
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
        static void assign_crowding_distance_list(Population pop, Lists lst, int frontSize, ProblemDefinition ProblemObj, Randomization RandomizationObj)
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
            assign_crowding_distance(pop, dist, objArray, frontSize, ProblemObj, RandomizationObj);

        }

        /* Routine to compute crowding distance based on objective function values when the population in in the form of an array */
        static void assign_crowding_distance_indices(Population pop, int c1, int c2, ProblemDefinition ProblemObj, Randomization RandomizationObj)
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
            assign_crowding_distance(pop, dist, objArray, frontSize, ProblemObj, RandomizationObj);

        }

        /* Routine to compute crowding distances */
        static void assign_crowding_distance(Population pop, int[] dist, int[][] objArray, int frontSize, ProblemDefinition ProblemObj, Randomization RandomizationObj)
        {
            int i, j;
            for (i = 0; i < ProblemObj.ObjectiveCount; i++)
            {
                for (j = 0; j < frontSize; j++)
                {
                    objArray[i][j] = dist[j];
                }
                quicksort_front_obj(pop, i, objArray[i], frontSize, RandomizationObj);
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
        static void quicksort_front_obj(Population pop, int objcount, int[] objArray, int objArraySize, Randomization RandomizationObj)
        {
            q_sort_front_obj(pop, objcount, objArray, 0, objArraySize - 1, RandomizationObj);
        }

        /* Actual implementation of the randomized quick sort used to sort a population based on a particular objective chosen */
        static void q_sort_front_obj(Population pop, int objcount, int[] objArray, int left, int right, Randomization RandomizationObj)
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
                q_sort_front_obj(pop, objcount, objArray, left, index - 1, RandomizationObj);
                q_sort_front_obj(pop, objcount, objArray, index + 1, right, RandomizationObj);
            }
        }

        /* Randomized quick sort routine to sort a population based on crowding distance */
        static void quicksort_dist(Population pop, int[] dist, int frontSize, Randomization RandomizationObj)
        {
            q_sort_dist(pop, dist, 0, frontSize - 1, RandomizationObj);
        }

        /* Actual implementation of the randomized quick sort used to sort a population based on crowding distance */
        static void q_sort_dist(Population pop, int[] dist, int left, int right, Randomization RandomizationObj)
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
                q_sort_dist(pop, dist, left, index - 1, RandomizationObj);
                q_sort_dist(pop, dist, index + 1, right, RandomizationObj);
            }

        }
        #endregion

        #region fillnds.c

        /* Routine to perform non-dominated sorting */
        static void fill_nondominated_sort(Population mixedPop, Population newPop, ProblemDefinition ProblemObj, Randomization RandomizationObj)
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
                        flag = CheckDominance(mixedPop.IndList[temp1.index], mixedPop.IndList[temp2.index], ProblemObj);
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
                        newPop.IndList[i].Copy(mixedPop.IndList[temp2.index], ProblemObj);
                        //CopyIndividual(mixedPop.IndList[temp2.index], newPop.IndList[i]);

                        newPop.IndList[i].Rank = rank;
                        archieveSize += 1;
                        temp2 = temp2.child;
                        i += 1;
                    }
                    while (temp2 != null);
                    assign_crowding_distance_indices(newPop, j, i - 1, ProblemObj, RandomizationObj);
                    rank += 1;
                }
                else
                {
                    crowding_fill(mixedPop, newPop, i, frontSize, elite, ProblemObj, RandomizationObj);
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
        static void crowding_fill(Population mixedPop, Population newPop, int count, int frontSize, Lists elite, ProblemDefinition ProblemObj, Randomization RandomizationObj)
        {
            int[] dist;
            Lists temp;
            int i, j;
            assign_crowding_distance_list(mixedPop, elite.child, frontSize, ProblemObj, RandomizationObj);
            dist = new int[frontSize];
            temp = elite.child;
            for (j = 0; j < frontSize; j++)
            {
                dist[j] = temp.index;
                temp = temp.child;
            }
            quicksort_dist(mixedPop, dist, frontSize, RandomizationObj);
            for (i = count, j = frontSize - 1; i < ProblemObj.PopulationSize; i++, j--)
            {
                newPop.IndList[i].Copy(mixedPop.IndList[dist[j]], ProblemObj);
                //CopyIndividual(mixedPop.IndList[dist[j]], newPop.IndList[i]);
                //newPop.IndList[i] = new Individual(mixedPop.IndList[dist[j]], ProblemObj);

            }

        }

        #endregion

        #endregion

        public enum HillClimbMode
        {
            None = 0,
            ChildOnly = 1,
            MixedOnly = 2,
            ParentOnly = 3,
            All = 4,
            BestOfParent = 5,
            AllBestOfParent = 6,
            AdaptiveParent = 7,
            Rank1Best = 8,
            Rank1All = 9,
            AdaptiveRank1All = 10
        }
    }
}
