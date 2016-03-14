using System;

namespace ConsoleApp.Models
{
    public class Display
    {
        public int GnuplotChoice;
        public int GnuplotObjective1;
        public int GnuplotObjective2;
        public int GnuplotObjective3;
        public int GnuplotAngle1;
        public int GnuplotAngle2;

        ///* Function to display the current population for the subsequent generation */
        public void PlotPopulation(Population pop, ProblemDefinition problem, int genNo = 0)
        {
            Console.WriteLine(" printing gnuplot");
            if (GnuplotChoice != 3)
            {
                var xr = new double[problem.PopulationSize];
                var yr = new double[problem.PopulationSize];

                for (int x = 0; x < problem.PopulationSize; x++)
                {
                    xr[x] = pop.IndList[x].Obj[GnuplotObjective1 - 1];
                    yr[x] = pop.IndList[x].Obj[GnuplotObjective2 - 1];
                }

                GnuPlot.Plot(xr, yr, $"title 'Generation #{genNo} of {problem.GenCount}' pt 1");
                GnuPlot.Set($"xlabel \"obj[{GnuplotObjective1 - 1}]\"");
                GnuPlot.Set($"ylabel \"obj[{GnuplotObjective2 - 1}]\"");

            }
            else
            {
                var xr = new double[problem.PopulationSize];
                var yr = new double[problem.PopulationSize];
                var zr = new double[problem.PopulationSize];

                for (int x = 0; x < problem.PopulationSize; x++)
                {
                    xr[x] = pop.IndList[x].Obj[GnuplotObjective1 - 1];
                    yr[x] = pop.IndList[x].Obj[GnuplotObjective2 - 1];
                    zr[x] = pop.IndList[x].Obj[GnuplotObjective3 - 1];
                }

                GnuPlot.SPlot(xr, yr, zr, $"title 'Generation #{genNo} of {problem.GenCount}' with points pointtype 8 lc rgb 'blue'");
                GnuPlot.Set($"xlabel \"obj[{GnuplotObjective1 - 1}]\"");
                GnuPlot.Set($"ylabel \"obj[{GnuplotObjective2 - 1}]\"");
                GnuPlot.Set($"zlabel \"obj[{GnuplotObjective3 - 1}]\"");
            }
        }

    }
}
