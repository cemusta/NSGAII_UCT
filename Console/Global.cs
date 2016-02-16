using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console
{
    public class Global
    {
    }

    public class course_detail
    {
        public readonly char[] code = new char[100];
        public readonly char[] teacher = new char[200];
        public int type;
        public int semester;
        public int duration;
        public int labHour;
        public int elective;

        public course_detail(char[] teacher, char[] code, int type, int semester, int duration, int labHour, int elective)
        {
            this.type = type;
            this.semester = semester;
            this.duration = duration;
            this.labHour = labHour;
            this.elective = elective;
            this.code = code;
            this.teacher = teacher;
        }
    }


    public class individual
    {
        int rank;
        double constr_violation;
        double xreal;
        int gene;
        double xbin;
        double obj;
        double constr;
        double crowd_dist;
    }

    public class population
    {
        individual ind;
    }

    public class lists
    {
        int index;
        lists parent;
        lists child;
    }

}
