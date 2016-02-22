namespace ConsoleApp.Models {
    public class CourseDetail {
        public string Code { get; set; }
        public string Teacher { get; set; }
        public int Type { get; set; }
        public int Semester { get; set; }
        public int Duration { get; set; }
        public int LabHour { get; set; }
        public int Elective { get; set; }
        public string PrintableName => $"{Code}{(Type == 1 ? " Lab" : " ")}{(Duration > 0 ? Duration + "hr" : "")}{(Elective > 0 ? " elective" : "")}";

        public CourseDetail(string code, string teacher,  int type, int semester, int duration, int labHour, int elective) {
            Code = code;
            Teacher = teacher;
            Type = type;
            Semester = semester;
            Duration = duration;
            LabHour = labHour;
            Elective = elective;
        }



    }
}
