using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

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


    }
}
