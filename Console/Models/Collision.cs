using System.Collections.Generic;

namespace ConsoleApp.Models
{
    public class Collision
    {
        public List<Course> CrashingCourses;
        public string Reason;
        public int result;
        public string Printable
        {
            get
            {
                string ret = "col: ";

                for (int i = 0; i < CrashingCourses.Count; i++)
                {
                    ret += $"{CrashingCourses[i].Code} ";
                }
                ret += $"| {Reason}";

                return ret;
            }
        }

        public Collision()
        {
            CrashingCourses = new List<Course>();
        }


    }
}
