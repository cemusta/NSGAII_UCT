namespace ConsoleApp.Models
{
    public class Population
    {
        public Individual[] ind { get; set; }

        public Population(int size,int nreal,int nbin, int maxnbits, int nobj, int ncon)
        {
            ind = new Individual[size];

            for (int i = 0; i < size; i++)
            {
                ind[i] = new Individual(nreal,nbin,maxnbits,nobj,ncon);
            }
        }
    }
}
