using System;
using System.Collections.Generic;

namespace ConsoleApp.Models
{
    public class Individual
    {
        public double ConstrViolation { get; set; }
        public int Rank { get; set; }
        public double[] Xreal { get; set; }
        public int[,] Gene { get; set; }
        public double[] Xbin { get; set; }
        public double[] Obj { get; set; }
        public double[] Constr { get; set; }
        public double CrowdDist { get; set; }

        public List<Collision> CollisionList { get; set; }

        public Individual(int nreal, int nbin, int maxnbits, int nobj, int ncon)
        {
            if (nreal != 0)
                Xreal = new double[nreal];

            if (nbin != 0)
            {
                Xbin = new double[nbin];
                Gene = new int[nbin, maxnbits];
            }

            Obj = new double[nobj];

            if (ncon != 0)
                Constr = new double[ncon];

            CollisionList = new List<Collision>();
        }

        public void AddCollision(Collision temp)
        {
            CollisionList.Add(temp);
        }

        public void Decode(ProblemDefinition problem)
        {
            if (problem.BinaryVariableCount == 0)
                return;

            for (int j = 0; j < problem.BinaryVariableCount; j++)
            {
                var sum = 0;
                for (int k = 0; k < problem.nbits[j]; k++)
                {
                    if (Gene[j, k] == 1)
                    {
                        sum += (int)Math.Pow(2, problem.nbits[j] - 1 - k);
                    }
                }
                Xbin[j] = problem.min_binvar[j] + sum * (problem.max_binvar[j] - problem.min_binvar[j]) / (Math.Pow(2, problem.nbits[j]) - 1);
            }
        }

        /* Function to initialize an individual randomly */
        public void Initialize(ProblemDefinition problem, Randomization randomObj)
        {
            int j;
            if (problem.RealVariableCount != 0)
            {
                for (j = 0; j < problem.RealVariableCount; j++)
                {
                    Xreal[j] = randomObj.RandomDouble(problem.min_realvar[j], problem.max_realvar[j]);
                }
            }
            if (problem.BinaryVariableCount != 0)
            {
                for (j = 0; j < problem.BinaryVariableCount; j++)
                {
                    for (int k = 0; k < problem.nbits[j]; k++)
                    {
                        if (randomObj.RandomPercent() <= 0.5)
                        {
                            Gene[j, k] = 0;
                        }
                        else
                        {
                            Gene[j, k] = 1;
                        }
                    }
                }
            }
        }
    }
}
