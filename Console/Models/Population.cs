namespace ConsoleApp.Models
{
    public class Population
    {
        public Individual[] indList { get; set; }

        public Population(int size,int nreal,int nbin, int maxnbits, int nobj, int ncon)
        {
            indList = new Individual[size];

            for (int i = 0; i < size; i++)
            {
                indList[i] = new Individual(nreal,nbin,maxnbits,nobj,ncon);
            }
        }
    }
}
