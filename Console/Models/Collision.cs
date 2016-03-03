using System.Collections.Generic;

namespace ConsoleApp.Models
{
    public class Collision
    {
        public List<Course> CrashingCourses;
        public string Reason;
        public int result;

        public Collision()
        {
            CrashingCourses = new List<Course>();
        }
    }
}
