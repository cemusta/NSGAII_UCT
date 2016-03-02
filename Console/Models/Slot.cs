using System.Collections.Generic;

namespace ConsoleApp.Models
{
    public class Slot
    {
        public List<Course> Courses;
        public Dictionary<int,int> Teacher;
        public int labCount;

        public Slot(int teacherCount)
        {
            Courses = new List<Course>();
            Teacher = new Dictionary<int, int>(teacherCount);
            for (int i = 0; i < teacherCount; i++)
            {
                Teacher.Add(i, 0);
            }
            labCount = 0;
        }
    }
}
