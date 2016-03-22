﻿using System.Collections.Generic;

namespace ConsoleApp.Models
{
    public class Collision
    {
        public readonly List<Course> CrashingCourses;
        public int TeacherId;
        public CollisionType Type;
        public string Reason;
        public int Result;
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

        public Collision(CollisionType type = CollisionType.CourseCollision)
        {
            TeacherId = 0;
            Type = type;
            CrashingCourses = new List<Course>();
        }

    }

    public enum CollisionType
    {
        BaseLectureWithFaculty = 0,
        BaseLectureWithBase = 1,
        CourseCollision = 2,
        TeacherCollision = 3,
        LabCollision = 4
    }
}
