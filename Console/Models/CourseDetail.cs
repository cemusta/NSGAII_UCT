namespace ConsoleApp.Models {
    public class CourseDetail {
        public string code { get; set; }
        public string teacher { get; set; }
        public int type { get; set; }
        public int semester { get; set; }
        public int duration { get; set; }
        public int labHour { get; set; }
        public int elective { get; set; }

        public CourseDetail(string code, string teacher,  int type, int semester, int duration, int labHour, int elective) {
            this.code = code;
            this.teacher = teacher;
            this.type = type;
            this.semester = semester;
            this.duration = duration;
            this.labHour = labHour;
            this.elective = elective;
        }
    }
}
