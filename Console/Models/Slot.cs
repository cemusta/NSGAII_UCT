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
            labCount = 0;
        }
    }
}
