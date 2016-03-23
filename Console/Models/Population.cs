using System.Collections.Generic;
using System.IO;

namespace ConsoleApp.Models
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

        public void HillClimb(ProblemDefinition problemObj)
        {
            for (int i = 0; i < this.IndList.Count; i++)
            {
                IndList[i].HillClimb(problemObj);
            }
        }


        /* Function to print the information of a population in a file */
        public void ReportPopulation(StreamWriter writer, ProblemDefinition problemObj)
        {
            int i, j, k;
            for (i = 0; i < problemObj.PopulationSize; i++)
            {
                for (j = 0; j < problemObj.ObjectiveCount; j++)
                {
                    writer.Write($"{IndList[i].Obj[j].ToString()}\t");
                }

                if (problemObj.ConstraintCount != 0)
                {
                    for (j = 0; j < problemObj.ConstraintCount; j++)
                    {
                        writer.Write($"{IndList[i].Constr[j].ToString("E")}\t");
                    }
                }

                if (problemObj.RealVariableCount != 0)
                {
                    for (j = 0; j < problemObj.RealVariableCount; j++)
                    {
                        writer.Write($"{IndList[i].Xreal[j].ToString("E")}\t");
                    }
                }

                if (problemObj.BinaryVariableCount != 0)
                {
                    for (j = 0; j < problemObj.BinaryVariableCount; j++)
                    {
                        for (k = 0; k < problemObj.nbits[j]; k++)
                        {
                            writer.Write($"{IndList[i].Gene[j, k]}\t");
                        }
                    }
                }

                writer.Write($"{IndList[i].ConstrViolation.ToString("E")}\t");

                writer.Write($"{IndList[i].Rank}\t");

                writer.Write($"{IndList[i].CrowdDist.ToString("E")}\n");
            }

        }

        /* Function to print the information of feasible and non-dominated population in a file */
        public void ReportFeasiblePopulation(StreamWriter writer,ProblemDefinition problemObj)
        {
            int i, j, k;
            for (i = 0; i < problemObj.PopulationSize; i++)
            {
                if (IndList[i].ConstrViolation == 0.0 && IndList[i].Rank == 1)
                {
                    for (j = 0; j < problemObj.ObjectiveCount; j++)
                    {
                        writer.Write($"{IndList[i].Obj[j].ToString()}\t");
                    }
                    if (problemObj.ConstraintCount != 0)
                    {
                        for (j = 0; j < problemObj.ConstraintCount; j++)
                        {
                            writer.Write($"{IndList[i].Constr[j].ToString("E")}\t");
                        }
                    }
                    if (problemObj.RealVariableCount != 0)
                    {
                        for (j = 0; j < problemObj.RealVariableCount; j++)
                        {
                            writer.Write($"{IndList[i].Xreal[j].ToString("E")}\t");
                        }
                    }
                    if (problemObj.BinaryVariableCount != 0)
                    {
                        for (j = 0; j < problemObj.BinaryVariableCount; j++)
                        {
                            for (k = 0; k < problemObj.nbits[j]; k++)
                            {
                                writer.Write($"{IndList[i].Gene[j, k]}\t");
                            }
                        }
                    }
                    writer.Write($"{IndList[i].ConstrViolation.ToString("E")}\t");
                    writer.Write($"{IndList[i].Rank}\t");
                    writer.Write($"{ IndList[i].CrowdDist.ToString("E")}\n");
                }
            }
        }

    }
}
