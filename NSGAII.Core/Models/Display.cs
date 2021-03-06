﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NSGAII.Models
{
    public class Display
    {
        public bool Use3D;
        public int GnuplotObjective1;
        public int GnuplotObjective2;
        public int GnuplotObjective3;

        public Display() { }

        ///* Function to display the current population for the subsequent generation */
        public void PlotPopulation(Population pop, ProblemDefinition problem, int genNo = 0, List<Individual> best = null)
        {
            if (!Use3D) //2D
            {

                var xr = new double[problem.PopulationSize];
                var yr = new double[problem.PopulationSize];

                for (int x = 0; x < problem.PopulationSize; x++)
                {
                    xr[x] = pop.IndList[x].Obj[GnuplotObjective1];
                    yr[x] = pop.IndList[x].Obj[GnuplotObjective2];
                }

                if (best != null)
                {
                    GnuPlot.HoldOn();
                }

                GnuPlot.Plot(xr, yr, $"title 'Generation #{genNo} of {problem.MaxGeneration}' with points pointtype 8 lc rgb 'blue'");

                if (best != null)
                {
                    var x = new double[best.Count];
                    var y = new double[best.Count];

                    for (int i = 0; i < best.Count; i++)
                    {
                        x[i] = best[i].Obj[GnuplotObjective1];
                        y[i] = best[i].Obj[GnuplotObjective2];
                    }
                    GnuPlot.Plot(x, y, $"title 'best ({best.First().TotalResult})' with points pointtype 6 lc rgb 'red'");
                }

                GnuPlot.Set($"xlabel \"obj[{GnuplotObjective1 }]\"");
                GnuPlot.Set($"ylabel \"obj[{GnuplotObjective2 }]\"");

            }
            else //3D
            {
                var rank1Count = pop.IndList.Count(x => x.Rank == 1);
                var rankOtherCount = pop.IndList.Count(x => x.Rank != 1);

                if (rank1Count > 0)
                {
                    var xr = new double[rank1Count];
                    var yr = new double[rank1Count];
                    var zr = new double[rank1Count];

                    int index = 0;
                    foreach (var ind in pop.IndList.Where(x => x.Rank == 1))
                    {
                        xr[index] = ind.Obj[GnuplotObjective1];
                        yr[index] = ind.Obj[GnuplotObjective2];
                        zr[index] = ind.Obj[GnuplotObjective3];
                        index++;
                    }

                    if (best != null || rankOtherCount > 0)
                    {
                        GnuPlot.HoldOn();
                    }

                    GnuPlot.SPlot(xr, yr, zr, $"title 'Rank 1 individuals' with points pointtype 8 lc rgb 'red'");

                }

                if (rankOtherCount > 0)
                {
                    var xr = new double[rankOtherCount];
                    var yr = new double[rankOtherCount];
                    var zr = new double[rankOtherCount];

                    int index = 0;
                    foreach (var ind in pop.IndList.Where(x => x.Rank != 1))
                    {
                        xr[index] = ind.Obj[GnuplotObjective1];
                        yr[index] = ind.Obj[GnuplotObjective2];
                        zr[index] = ind.Obj[GnuplotObjective3];
                        index++;
                    }
                    GnuPlot.SPlot(xr, yr, zr, $"title 'Rank 2-{pop.IndList.Max(x=>x.Rank)} individuals' with points pointtype 8 lc rgb 'blue'");
                }

                if (best != null)
                {

                    var x = new double[best.Count];
                    var y = new double[best.Count];
                    var z = new double[best.Count];

                    for (int i = 0; i < best.Count; i++)
                    {
                        x[i] = best[i].Obj[GnuplotObjective1];
                        y[i] = best[i].Obj[GnuplotObjective2];
                        z[i] = best[i].Obj[GnuplotObjective3];
                    }

                    GnuPlot.SPlot(x, y, z, $"title 'best ({best.First().TotalResult})' with points pointtype 6 lc rgb 'green'");
                }

                GnuPlot.Set($"title 'Generation #{genNo} of {problem.MaxGeneration}'");

                GnuPlot.Set($"xlabel \"obj[{GnuplotObjective1}]\"");
                GnuPlot.Set($"ylabel \"obj[{GnuplotObjective2}]\"");
                GnuPlot.Set($"zlabel \"obj[{GnuplotObjective3}]\"");
            }
        }

    }
}
