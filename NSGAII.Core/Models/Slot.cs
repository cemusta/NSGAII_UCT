using System.Collections.Generic;

namespace NSGAII.Models
{
    public class Slot
    {
        public int Id;
        public List<Course> Courses;
        public List<int> Teacher;
        public int labCount;
        public bool meetingHour;

        public List<Course> facultyCourses;
        public int facultyLab;

        public Slot(int teacherCount, int id)
        {
            Id = id;
            meetingHour = false;
            Courses = new List<Course>();
            Teacher = new List<int>(teacherCount);
            facultyCourses = new List<Course>();
            for (int i = 0; i < teacherCount; i++)
            {
                Teacher.Add( 0);
            }
            labCount = 0;            
            facultyLab = 0;
        }

        public Slot()
        {
            Courses = new List<Course>();
            Teacher = new List<int>();
            facultyCourses = new List<Course>();
        }
    }
}
