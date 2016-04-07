using System.Collections.Generic;

namespace NSGAII.Models
{
    public class ProblemDefinition
    {
        public string Title;

        public double INF = 1.0e14;
        public double EPS = 1.0e-14;
               
        public int RealVariableCount;
        public int BinaryVariableCount;
        public int MaxBitCount;
        public int ObjectiveCount;
        public int ConstraintCount;
        public int PopulationSize;
        public double RealCrossoverProbability;
        public double BinaryCrossoverProbability;
        public double RealMutationProbability;
        public double BinaryMutationProbability;
        public double CrossoverDistributionIndex;
        public double MutationDistributionIndex;
        public int MaxGeneration;
               
        public int BinaryMutationCount;      //for reporting only.
        public int RealMutationCount;        //for reporting only
        public int BinaryCrossoverCount;     //for reporting only
        public int RealCrossoverCount;       //for reporting only
        public int TotalBinaryBitLength;     //for reporting only
               
        public int[] nbits = new int[50];
        public double[] min_realvar = new double[50];
        public double[] max_realvar = new double[50];
        public double[] min_binvar = new double[50];
        public double[] max_binvar = new double[50];

        public readonly List<string> TeacherList = new List<string>(8);
        public readonly List<Course> CourseList = new List<Course>(8);
        public readonly List<Course> FacultyCourseList = new List<Course>(8);
        public readonly List<List<int>> Meeting = new List<List<int>>(); // bölüm hocalarının ortak meeting saatleri.
        public readonly List<List<int>> LabScheduling = new List<List<int>>(); // labda dönem tutulmuyor 
        public readonly List<List<List<int>>> FacultyCourses = new List<List<List<int>>>(8); //8 dönem, 5 gün, 9 ders   

        public ProblemDefinition(string title)
        {
            Title = title;
        }

        private ProblemDefinition() { }
    }
}
