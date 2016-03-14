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

        private readonly int _nRealVar;
        private readonly int _nBinVar;
        private readonly int _nMaxBit;
        private readonly int _nObj;
        private readonly int _nCons;


        public Individual(int nRealVar, int nBinVar, int nMaxBit, int nObj, int nCons)
        {
            _nRealVar = nRealVar;
            _nBinVar = nBinVar;
            _nMaxBit = nMaxBit;
            _nObj = nObj;
            _nCons = nCons;

            if (nRealVar != 0)
                Xreal = new double[nRealVar];

            if (nBinVar != 0)
            {
                Xbin = new double[nBinVar];
                Gene = new int[nBinVar, nMaxBit];
            }

            Obj = new double[nObj];

            if (nCons != 0)
                Constr = new double[nCons];

            CollisionList = new List<Collision>();
        }

        public Individual(Individual ind, ProblemDefinition problem)
        {
            if (ind._nRealVar != 0)
                Xreal = new double[_nRealVar];

            if (ind._nBinVar != 0)
            {
                Xbin = new double[ind._nBinVar];
                Gene = new int[ind._nBinVar, ind._nMaxBit];
            }

            Obj = new double[ind._nObj];

            if (ind._nCons != 0)
                Constr = new double[ind._nCons];


            Rank = ind.Rank;
            ConstrViolation = ind.ConstrViolation;
            CrowdDist = ind.CrowdDist;
            if (ind._nRealVar > 0)
            {
                for (int i = 0; i < ind._nRealVar; i++)
                {
                    Xreal[i] = ind.Xreal[i];
                }
            }
            if (ind._nBinVar > 0)
            {
                for (int i = 0; i < ind._nBinVar; i++)
                {
                    Xbin[i] = ind.Xbin[i];
                    for (int j = 0; j < problem.nbits[i]; j++)
                    {
                        Gene[i, j] = ind.Gene[i, j];
                    }
                }
            }
            for (int i = 0; i < ind._nObj; i++)
            {
                Obj[i] = ind.Obj[i];
            }
            if (ind._nCons > 0)
            {
                for (int i = 0; i < ind._nCons; i++)
                {
                    Constr[i] = ind.Constr[i];
                }
            }

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
