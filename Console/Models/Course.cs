namespace ConsoleApp.Models {
    public class Course {
        public string Code { get; set; }
        public string Teacher { get; set; }
        public int TeacherId { get; set; }
        public int Type { get; set; }       // type 0: lecture , type 1: lab
        public int Section { get; set; } // not used yet
        public int Semester { get; set; }
        public int Duration { get; set; }
        public int LabHour { get; set; }
        public bool Elective { get; set; }
        public string PrintableName => $"{Code}{(Type == 1 ? " Lab " : " ")}{(Duration > 0 ? Duration + "hr" : "")}{(Elective == true ? " elective" : "")}";

        public Course(string code, string teacher, int teacherId, int type, int semester, int duration, int labHour, bool elective) {
            Code = code;
            Teacher = teacher;
            TeacherId = teacherId;
            Type = type;
            Semester = semester;
            Duration = duration;
            LabHour = labHour;
            Elective = elective;
        }

    }
}
