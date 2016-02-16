namespace ConsoleApp.Models {
    public class CourseDetail {
        public readonly char[] code = new char[100];
        public readonly char[] teacher = new char[200];
        public int type { get; set; }
        public int semester { get; set; }
        public int duration { get; set; }
        public int labHour { get; set; }
        public int elective { get; set; }

        public CourseDetail(char[] teacher, char[] code, int type, int semester, int duration, int labHour, int elective) {
            this.type = type;
            this.semester = semester;
            this.duration = duration;
            this.labHour = labHour;
            this.elective = elective;
            this.code = code;
            this.teacher = teacher;
        }
    }
}
