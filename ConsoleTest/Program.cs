using System;
using NSGAII;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            UCTProblem test;
            UCTProblem.HillClimbMode temp = UCTProblem.HillClimbMode.None;
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Starting with default parameters."); // Check for null array
                test = new UCTProblem(0.75, 200, 10000, 3, 0, 43, 0, false);
            }
            else
            {
                if (args.Length == 4)
                {
                    try
                    {
                        double seed = double.Parse(args[0]);

                        int pop = int.Parse(args[1]);
                        int gen = int.Parse(args[2]);

                        test = new UCTProblem(seed, pop, gen, 3, 0, 43, 0, false);


                        if (args[3] == "0")
                            temp = UCTProblem.HillClimbMode.None;
                        else if(args[3] == "1")
                            temp = UCTProblem.HillClimbMode.ChildOnly;
                        else if (args[3] == "3")
                            temp = UCTProblem.HillClimbMode.ParentOnly;
                        else if (args[3] == "4")
                            temp = UCTProblem.HillClimbMode.All;
                        else if (args[3] == "5")
                            temp = UCTProblem.HillClimbMode.BestOfParent;
                        else if (args[3] == "6")
                            temp = UCTProblem.HillClimbMode.AllBestOfParent;
                        else if (args[3] == "7")
                            temp = UCTProblem.HillClimbMode.AdaptiveParent;
                        else if (args[3] == "8")
                            temp = UCTProblem.HillClimbMode.Rank1Best;
                        else if (args[3] == "9")
                            temp = UCTProblem.HillClimbMode.Rank1All;
                        else if (args[3] == "10")
                            temp = UCTProblem.HillClimbMode.AdaptiveRank1All;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Parameter Error: {ex}.");
                        Console.WriteLine("Usage: -seed -pop -generation");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Wrong number of parameters.");
                    Console.WriteLine("Usage: -seed -pop -generation");
                    return;
                }
            }

            test.WriteMethod(temp);
            test.FirstGeneration();

            for (int i = 0; i < test.ProblemObj.MaxGeneration; i++)
            {
                test.NextGeneration(temp);
            }

            test.WriteBestGeneration();
            test.WriteFinalGeneration();
            UCTProblem.SaveToFile(test, test.ProblemObj.Title);

        }
    }
}
