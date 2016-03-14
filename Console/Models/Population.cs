namespace ConsoleApp.Models
{
    public class Population
    {
        public Individual[] IndList { get; set; }

        public Population(int size,int nreal,int nbin, int maxnbits, int nobj, int ncon)
        {
            IndList = new Individual[size];

            for (int i = 0; i < size; i++)
            {
                IndList[i] = new Individual(nreal,nbin,maxnbits,nobj,ncon);
            }
        }

        public void Decode(ProblemDefinition problem)
        {
            if (problem.BinaryVariableCount == 0)
                return;

            for (int i = 0; i < problem.PopulationSize; i++)
            {
                IndList[i].Decode(problem);
                    
            }
        }
    }
}
