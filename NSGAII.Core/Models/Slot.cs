using System.Collections.Generic;

namespace NSGAII.Models
{
    public class Slot
    {
        public List<Course> Courses;
        public List<int> Teacher;
        public int labCount;
        public bool meetingHour;

        public Slot(int teacherCount)
        {
            meetingHour = false;
            Courses = new List<Course>();
            Teacher = new List<int>(teacherCount);
            for (int i = 0; i < teacherCount; i++)
            {
                Teacher.Add( 0);
            }
            labCount = 0;
        }

        public Slot() { }
    }
}
