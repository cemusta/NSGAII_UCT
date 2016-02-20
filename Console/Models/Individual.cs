namespace ConsoleApp.Models
{
    public class Individual
    {
        public double constr_violation { get; set; }
        public int rank { get; set; }
        public double[] xreal { get; set; }
        public int[,] gene { get; set; }
        public double[] xbin { get; set; }
        public double[] obj { get; set; }
        public double[] constr { get; set; }
        public double crowd_dist { get; set; }

        public Individual(int nreal, int nbin, int maxnbits, int nobj, int ncon)
        {
            if (nreal != 0)
                xreal = new double[nreal];

            if (nbin != 0)
            {
                xbin = new double[nbin];
                gene = new int[nbin, maxnbits];
            }

            obj = new double[nobj];

            if (ncon != 0)
                constr = new double[ncon];
        }

    }
}
