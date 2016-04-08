using System.Collections.Generic;

namespace NSGAII.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Teacher { get; set; }
        public int TeacherId { get; set; }
        public int Type { get; set; }       // type 0: lecture , type 1: lab
        public int Section { get; set; } // not used yet
        public int Semester { get; set; }
        public int Duration { get; set; }
        public int LabHour { get; set; }
        public bool Elective { get; set; }
        public string PrintableName
        {
            get
            {
                if (FacultyCourse)
                {
                    if (Type == 0)
                    {
                        return Section == 0 ? $"{Code}" : $"{Code}.{Section}";
                    }
                    else if (Type == 1)
                    {
                        return Section == 0 ? $"{Code} Lab" : $"{Code}.{Section} Lab";
                    }
                    else if (Type == 2)
                    {
                        return Section == 0 ? $"{Code} PS" : $"{Code}.{Section} PS";
                    }
                }
                return
                    $"{Code}{(Type == 1 ? " Lab " : " ")}{(Duration > 0 ? Duration + "hr" : "")}{(Elective == true ? " elective" : " (sem:" + Semester + ")")}";
            }
        }

        public bool FacultyCourse { get; set; }
        public int SlotId { get; set; }

        public List<int> prerequisites { get; set; }

        public Course(int id, string code, string teacher, int teacherId, int type, int semester, int duration, int labHour, bool elective, int section = 0, bool faculty = false, int slotid = 0)
        {
            Id = id;
            Code = code;
            Teacher = teacher;
            TeacherId = teacherId;
            Type = type;
            Semester = semester;
            Duration = duration;
            LabHour = labHour;
            Elective = elective;
            prerequisites = new List<int>();
            FacultyCourse = faculty;
            SlotId = slotid;
            Section = section;
        }

        public Course() { }

    }
}
