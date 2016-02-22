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
        }

    }
}
