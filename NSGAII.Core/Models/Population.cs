using System.Collections.Generic;
using System.IO;

namespace NSGAII.Models
{
    public class Population
    {
        public List<Individual> IndList { get; set; }

        public Population(int populationSize,int nRealVar,int nBinVar, int nMatBit, int nObj, int nCons)
        {
            IndList = new List<Individual>(populationSize);

            for (int i = 0; i < populationSize; i++)
            {
                IndList.Add( new Individual(nRealVar,nBinVar,nMatBit,nObj,nCons) );
            }
        }

        public Population() {}

        public void Decode(ProblemDefinition problem)
        {
            if (problem.BinaryVariableCount == 0)
                return;

            for (int i = 0; i < IndList.Count; i++)
            {
                IndList[i].Decode(problem);
            }
        }

        public void Initialize(ProblemDefinition problem, Randomization randomObj)
        {
            for (int i = 0; i < IndList.Count; i++)
            {
                IndList[i].Initialize(problem, randomObj);
            }
        }

        public void Merge( Population pop1, Population pop2, ProblemDefinition problem)
        {
            IndList.Clear();

            for (int i = 0; i < pop1.IndList.Count; i++)
            {
                IndList.Add( new Individual(pop1.IndList[i], problem));                
            }
            for (int i = 0; i < pop2.IndList.Count; i++)
            {
                IndList.Add(new Individual(pop2.IndList[i], problem));
            }
        }

        public void Evaluate(ProblemDefinition problemObj)
        {
            for (int i = 0; i < this.IndList.Count; i++)
            {
                IndList[i].Evaluate(problemObj);
            }
        }

        public int HillClimb(ProblemDefinition problemObj)
        {
            int retVal = 0;
            for (int i = 0; i < IndList.Count; i++)
            {
                retVal += IndList[i].HillClimb(problemObj);
            }
            return retVal;
        }

        /* Function to print the information of a population in a file */
        public void ReportPopulation(ProblemDefinition problemObj, string name, string info, int generation = 0)
        {
            Directory.CreateDirectory("report"); 
            var file = File.OpenWrite($"report\\{problemObj.Title}_{name}_pop.out");
            StreamWriter writer = new StreamWriter(file);
            writer.WriteLine($"# {info}");
            writer.WriteLine($"# of objectives = {problemObj.ObjectiveCount}, # of constraints = {problemObj.ConstraintCount}, # of real_var = {problemObj.RealVariableCount}, # of bits of bin_var = {problemObj.TotalBinaryBitLength}, constr_violation, rank, crowding_distance");

            if (generation > 0)
            {
                writer.WriteLine($"# Generation no: {generation}/{problemObj.MaxGeneration}");
            }

            for (int i = 0; i < problemObj.PopulationSize; i++)
            {
                for (int j = 0; j < problemObj.ObjectiveCount; j++)
                {
                    writer.Write($"{IndList[i].Obj[j].ToString()}\t");
                }

                if (problemObj.ConstraintCount != 0)
                {
                    for (int j = 0; j < problemObj.ConstraintCount; j++)
                    {
                        writer.Write($"{IndList[i].Constr[j].ToString("E")}\t");
                    }
                }

                if (problemObj.RealVariableCount != 0)
                {
                    for (int j = 0; j < problemObj.RealVariableCount; j++)
                    {
                        writer.Write($"{IndList[i].Xreal[j].ToString("E")}\t");
                    }
                }

                if (problemObj.BinaryVariableCount != 0)
                {
                    for (int j = 0; j < problemObj.BinaryVariableCount; j++)
                    {
                        for (int k = 0; k < problemObj.nbits[j]; k++)
                        {
                            writer.Write($"{IndList[i].Gene[j][ k]}\t");
                        }
                    }
                }

                writer.Write($"{IndList[i].ConstrViolation.ToString("E")}\t");

                writer.Write($"{IndList[i].Rank}\t");

                writer.Write($"{IndList[i].CrowdDist.ToString("E")}\n");
            }

            writer.Flush();
            writer.Close();
        }

        /* Function to print the information of feasible and non-dominated population in a file */
        public void ReportFeasiblePopulation(ProblemDefinition problemObj, string name, string info)
        {
            Directory.CreateDirectory("report");
            var file = File.OpenWrite($"{problemObj.Title}_{name}_pop.out");
            StreamWriter writer = new StreamWriter(file);
            writer.WriteLine($"# {info}");
            writer.WriteLine($"# of objectives = {problemObj.ObjectiveCount}, # of constraints = {problemObj.ConstraintCount}, # of real_var = {problemObj.RealVariableCount}, # of bits of bin_var = {problemObj.TotalBinaryBitLength}, constr_violation, rank, crowding_distance");

            for (int i = 0; i < problemObj.PopulationSize; i++)
            {
                if (IndList[i].ConstrViolation == 0.0 && IndList[i].Rank == 1)
                {
                    for (int j = 0; j < problemObj.ObjectiveCount; j++)
                    {
                        writer.Write($"{IndList[i].Obj[j].ToString()}\t");
                    }
                    if (problemObj.ConstraintCount != 0)
                    {
                        for (int j = 0; j < problemObj.ConstraintCount; j++)
                        {
                            writer.Write($"{IndList[i].Constr[j].ToString("E")}\t");
                        }
                    }
                    if (problemObj.RealVariableCount != 0)
                    {
                        for (int j = 0; j < problemObj.RealVariableCount; j++)
                        {
                            writer.Write($"{IndList[i].Xreal[j].ToString("E")}\t");
                        }
                    }
                    if (problemObj.BinaryVariableCount != 0)
                    {
                        for (int j = 0; j < problemObj.BinaryVariableCount; j++)
                        {
                            for (int k = 0; k < problemObj.nbits[j]; k++)
                            {
                                writer.Write($"{IndList[i].Gene[j][k]}\t");
                            }
                        }
                    }
                    writer.Write($"{IndList[i].ConstrViolation.ToString("E")}\t");
                    writer.Write($"{IndList[i].Rank}\t");
                    writer.Write($"{ IndList[i].CrowdDist.ToString("E")}\n");
                }
            }

            writer.Flush();
            writer.Close();
        }

    }
}
