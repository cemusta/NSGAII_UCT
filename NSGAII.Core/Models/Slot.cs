using System.Collections.Generic;

namespace NSGAII.Models
{
    public class Slot
    {
        public List<Course> Courses;
        public Dictionary<int,int> Teacher;
        public int labCount;
        public bool meetingHour;

        public Slot(int teacherCount)
        {
            meetingHour = false;
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
