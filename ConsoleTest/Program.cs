using NSGAII;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            UCTProblem test = new UCTProblem(0.75,200,10000,3,0,43,0,true);

            test.FirstGeneration();

            for (int i = 0; i < test.ProblemObj.GenCount; i++)
            {
                test.NextGeneration();
            }

        }
    }
}
