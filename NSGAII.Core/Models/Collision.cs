﻿using System.Collections.Generic;

namespace NSGAII.Models
{
    public class Collision
    {
        public readonly List<Course> CrashingCourses;
        public int TeacherId;
        public int SlotId;
        public CollisionType Type;
        public string Reason;
        public double Result;
        public int Obj = -1;
        public string Printable 
        {
            get
            {
                int count = 0;
                string ret = $"obj:{Obj} {Reason}";

                if (CrashingCourses.Count > 0)
                    ret += " : ";
                foreach (var cc in CrashingCourses)
                {
                    count++;
                    ret += cc.Code;
                    if (count != CrashingCourses.Count)
                        ret += "|";
                }

                ret += $" Result:{Result}";
                return ret;
            }
        }

        public Collision(CollisionType type = CollisionType.CourseCollision)
        {
            Result = 0;
            TeacherId = -1;
            SlotId = -1;
            Type = type;
            CrashingCourses = new List<Course>();
        }

        public Collision()
        {
            Result = 0;
            TeacherId = -1;
            SlotId = -1;
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
