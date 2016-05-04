using System.Collections.Generic;

namespace NSGAII.Models
{
    public class ProblemDefinition
    {
        public string Title;

        public double Inf = 1.0e14;
        public double Eps = 1.0e-14;
               
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
               
        public int[] Nbits = new int[50];
        public double[] MinRealvar = new double[50];
        public double[] MaxRealvar = new double[50];
        public double[] MinBinvar = new double[50];
        public double[] MaxBinvar = new double[50];

        public readonly List<string> TeacherList = new List<string>(8);
        public readonly List<Course> CourseList = new List<Course>(8);
        public readonly List<Course> FacultyCourseList = new List<Course>(8);
        public readonly List<List<int>> Meeting = new List<List<int>>(); // bölüm hocalarının ortak meeting saatleri.
        public readonly List<List<int>> LabScheduling = new List<List<int>>(); // labda dönem tutulmuyor 

        public UCTProblem.DisabledCollisions DisabledCollisions;

        public ProblemDefinition(string title)
        {
            Title = title;
        }

        // ReSharper disable once UnusedMember.Local
        private ProblemDefinition()
        {
            //xml load save için gerekiyor.
        }
    }

}
