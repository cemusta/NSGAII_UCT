using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp.Models
{
    public class Individual
    {
        public double ConstrViolation { get; set; }
        public int Rank { get; set; }
        public double[] Xreal { get; set; }
        public int[,] Gene { get; set; }
        public double[] Xbin { get; set; }
        public double[] Obj { get; set; }
        public double[] Constr { get; set; }
        public double CrowdDist { get; set; }

        public List<Collision> CollisionList { get; set; }

        private readonly int _nRealVar;
        private readonly int _nBinVar;
        private readonly int _nMaxBit;
        private readonly int _nObj;
        private readonly int _nCons;
        public List<Collision> Collisions = new List<Collision>(8);


        public Individual(int nRealVar, int nBinVar, int nMaxBit, int nObj, int nCons)
        {
            _nRealVar = nRealVar;
            _nBinVar = nBinVar;
            _nMaxBit = nMaxBit;
            _nObj = nObj;
            _nCons = nCons;

            if (nRealVar != 0)
                Xreal = new double[nRealVar];

            if (nBinVar != 0)
            {
                Xbin = new double[nBinVar];
                Gene = new int[nBinVar, nMaxBit];
            }

            Obj = new double[nObj];

            if (nCons != 0)
                Constr = new double[nCons];

            CollisionList = new List<Collision>();
        }

        public Individual(Individual ind, ProblemDefinition problem)
        {
            if (ind._nRealVar != 0)
                Xreal = new double[_nRealVar];

            if (ind._nBinVar != 0)
            {
                Xbin = new double[ind._nBinVar];
                Gene = new int[ind._nBinVar, ind._nMaxBit];
            }

            Obj = new double[ind._nObj];

            if (ind._nCons != 0)
                Constr = new double[ind._nCons];


            Rank = ind.Rank;
            ConstrViolation = ind.ConstrViolation;
            CrowdDist = ind.CrowdDist;
            if (ind._nRealVar > 0)
            {
                for (int i = 0; i < ind._nRealVar; i++)
                {
                    Xreal[i] = ind.Xreal[i];
                }
            }
            if (ind._nBinVar > 0)
            {
                for (int i = 0; i < ind._nBinVar; i++)
                {
                    Xbin[i] = ind.Xbin[i];
                    for (int j = 0; j < problem.nbits[i]; j++)
                    {
                        Gene[i, j] = ind.Gene[i, j];
                    }
                }
            }
            for (int i = 0; i < ind._nObj; i++)
            {
                Obj[i] = ind.Obj[i];
            }
            if (ind._nCons > 0)
            {
                for (int i = 0; i < ind._nCons; i++)
                {
                    Constr[i] = ind.Constr[i];
                }
            }

        }

        public void AddCollision(Collision temp)
        {
            CollisionList.Add(temp);
        }

        public void Decode(ProblemDefinition problem)
        {
            if (problem.BinaryVariableCount == 0)
                return;

            for (int j = 0; j < problem.BinaryVariableCount; j++)
            {
                var sum = 0;
                for (int k = 0; k < problem.nbits[j]; k++)
                {
                    if (Gene[j, k] == 1)
                    {
                        sum += (int)Math.Pow(2, problem.nbits[j] - 1 - k);
                    }
                }
                Xbin[j] = problem.min_binvar[j] + sum * (problem.max_binvar[j] - problem.min_binvar[j]) / (Math.Pow(2, problem.nbits[j]) - 1);
            }
        }

        /* Function to initialize an individual randomly */
        public void Initialize(ProblemDefinition problem, Randomization randomObj)
        {
            int j;
            if (problem.RealVariableCount != 0)
            {
                for (j = 0; j < problem.RealVariableCount; j++)
                {
                    Xreal[j] = randomObj.RandomDouble(problem.min_realvar[j], problem.max_realvar[j]);
                }
            }
            if (problem.BinaryVariableCount != 0)
            {
                for (j = 0; j < problem.BinaryVariableCount; j++)
                {
                    for (int k = 0; k < problem.nbits[j]; k++)
                    {
                        if (randomObj.RandomPercent() <= 0.5)
                        {
                            Gene[j, k] = 0;
                        }
                        else
                        {
                            Gene[j, k] = 1;
                        }
                    }
                }
            }
        }

        public void Evaluate(ProblemDefinition problemObj)
        {
            EvaluateProblem(problemObj);

            if (problemObj.ConstraintCount == 0)
            {
                ConstrViolation = 0.0;
            }
            else
            {
                ConstrViolation = 0.0;
                for (int j = 0; j < problemObj.ConstraintCount; j++)
                {
                    if (Constr[j] < 0.0)
                    {
                        ConstrViolation += Constr[j];
                    }
                }
            }
        }

        public void EvaluateProblem(ProblemDefinition problemObj)
        {

            Slot[,] timeTable = new Slot[5, 9];
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    timeTable[x, y] = new Slot(problemObj.TeacherList.Count);
                }
            }
            Obj[0] = 0;
            Obj[1] = 0;
            Obj[2] = 0;

            #region fill variables new
            for (int j = 0; j < problemObj.BinaryVariableCount; j++) //ders sayisi kadar.
            {
                int slotId = (int)Xbin[j];
                adding_course_timeTable(timeTable, slotId, problemObj.CourseList[j]);
            }

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (problemObj.Meeting[x, y] > 0)
                        timeTable[x, y].meetingHour = true;
                }
            }
            #endregion

            #region calc. collisions

            //dönem ici dekanlik/bolum dersi cakismasi todo: scheduling'in normal slot halinde gelmesi lazım
            for (int j = 0; j < 8; j++)
            {
                List<Collision> col = calculate_collisionSemesterWithBaseCourses(timeTable, problemObj.Scheduling[j], 0, j + 1);
                var result = col.Sum(item => item.Result);
                Obj[0] += result;
                Collisions.AddRange(col);
            }
            //donem ici bolum dersi cakismasi
            for (int j = 0; j < 8; j++)
            {
                List<Collision> col = calculate_collisionInSemester(timeTable, 1, j + 1);
                var result = col.Sum(item => item.Result);
                Obj[0] += result;
                Collisions.AddRange(col);
            }

            //dönemler arasi dekanlik/bolum dersi cakismasi todo: scheduling'in normal slot halinde gelmesi lazım
            for (int j = 1; j < 8; j++)
            {
                // 1-2  2-3  3-4  4-5  5-6  6-7  7-8
                // 2-1  3-2  4-3  5-4  6-5  7-6  8-7     consecutive CSE&faculty courses
                List<Collision> col = calculate_collisionSemesterWithBaseCourses(timeTable, problemObj.Scheduling[j], 0, j);
                col.AddRange(calculate_collisionSemesterWithBaseCourses(timeTable, problemObj.Scheduling[j - 1], 0, j + 1));
                var result = col.Sum(item => item.Result);
                Obj[1] += result;
                Collisions.AddRange(col);
            }

            //dönemler arası CSE çakışmaları
            for (int j = 1; j < 8; j++)
            {
                /*consecutive only CSE courses*/
                //var x = calculate_collision7(schedulingOnlyCse[j - 1], schedulingOnlyCse[j], 0);
                //Obj[1] += x;

                List<Collision> col = calculate_collisionInSemesters(timeTable, 1, new List<int> { j, j + 1 });
                var y = col.Sum(item => item.Result);

                Obj[1] += y;
                Collisions.AddRange(col);
            }

            //aynı saatte 3'ten fazla lab olmaması lazim todo: hangi lab? inputtan alacaz.
            {
                //var x = calculate_collision1(labCounter, 4);
                //Obj[0] += x;
                List<Collision> labcol = calculate_LabCollision(timeTable, 4);
                var y = labcol.Sum(item => item.Result);
                Obj[0] += y;
                Collisions.AddRange(labcol);
            }
            //# of lab at most 4

            for (int j = 0; j < problemObj.TeacherList.Count; j++) //Bütün hocalar için
            {
                if (problemObj.TeacherList[j].Equals("ASSISTANT"))
                {
                    continue;
                }

                {
                    //og. gor. aynı saatte baska dersinin olmaması
                    //var xx1 = calculate_collision1(teacherSchedulingCounter[j], 1);
                    //Obj[0] += xx1;

                    List<Collision> col = calculate_TeacherCollision(timeTable, j, 1);
                    var yy1 = col.Sum(item => item.Result);
                    Obj[0] += yy1;
                    Collisions.AddRange(col);
                }


                {
                    //og. gor. gunluk 4 saatten fazla pespese dersinin olmamasi
                    //var x = calculate_collision3(teacherSchedulingCounter[j], 4);
                    //Obj[2] += x;

                    var y = calculate_collisionTeacherConsicutive(timeTable, j, 4);
                    Obj[2] += 1;
                    Collision tempCollision = new Collision
                    {
                        Type = CollisionType.TeacherCollision,
                        TeacherId = j,
                        Result = y, // how many crash
                        Reason = "Teacher has consicutive course crash."
                    };
                    Collisions.Add(tempCollision);
                    //teacher have at most 4 consective lesson per day
                }
                {
                    //og. gor. boş gununun olması
                    //var x = calculate_collision4(teacherSchedulingCounter[j]);
                    //Obj[2] += x;

                    var y = calculate_collisionTeacherFreeDay(timeTable, j);
                    Obj[2] += y;
                    Collision tempCollision = new Collision
                    {
                        Type = CollisionType.TeacherCollision,
                        TeacherId = j,
                        Result = y, // 1
                        Reason = "Teacher doesnt have free day"
                    };
                    Collisions.Add(tempCollision);
                }
                /* teacher have free day*/
            }

            //lab ve lecture farklı günlerde olsun
            {
                //var x = 0;
                //for (j = 0; j < 8; j++)
                //{
                //    x += calculate_collision6(schedulingOnlyCse[j]);    /*lab lecture hours must be in seperate day*/
                //}
                //Obj[2] += x;  //todo: sayılar tutmuyor.

                List<Collision> col = calculate_LectureLabCollision(timeTable);
                var y = col.Sum(item => item.Result);
                Obj[2] += y;
                Collisions.AddRange(col);
            }

            {
                List<Collision> col = calculate_ElectiveCollision(timeTable, 1);
                var y = col.Sum(item => item.Result);
                Obj[0] += y;
                Collisions.AddRange(col);
            }

            /*elective+faculty courses in semester*/
            {
                List<Collision> col = calculate_collisionElectiveWithBaseCourses(timeTable, problemObj.Scheduling[5], 0);
                var y = col.Sum(item => item.Result);
                Obj[2] += y;
                Collisions.AddRange(col);
                col.Clear();

                col = calculate_collisionElectiveWithBaseCourses(timeTable, problemObj.Scheduling[6], 0);
                y = col.Sum(item => item.Result);
                Obj[2] += y;
                Collisions.AddRange(col);
                col.Clear();

                col = calculate_collisionElectiveWithBaseCourses(timeTable, problemObj.Scheduling[7], 0);
                y = col.Sum(item => item.Result);
                Obj[2] += y;
                Collisions.AddRange(col);
            }

            /*elective+faculty courses in semester*/

            {
                //var x = calculate_collision7(schedulingOnlyCse[5], electiveCourses, 0);    /*CSE+elective courses(consecutive)*/
                //Obj[1] += x;
                //Obj[0] += calculate_collision7(schedulingOnlyCse[6], electiveCourses, 0);    /*CSE+elective courses*/
                //Obj[0] += calculate_collision7(schedulingOnlyCse[7], electiveCourses, 0);    /*CSE+elective courses*/

                List<Collision> col = calculate_collisionElectiveWithSemester(timeTable, 0, 6);
                var y = col.Sum(item => item.Result);
                Obj[1] += y;
                Collisions.AddRange(col);
                col.Clear();

                col = calculate_collisionElectiveWithSemester(timeTable, 0, 7);
                y = col.Sum(item => item.Result);
                Obj[0] += y;
                Collisions.AddRange(col);
                col.Clear();

                col = calculate_collisionElectiveWithSemester(timeTable, 0, 8);
                y = col.Sum(item => item.Result);
                Obj[0] += y;
                Collisions.AddRange(col);
            }

            //todo: dekanlık derslerinin sectionları??
            #endregion

        }

        #region functions.c

        /* filling scheduling table for 1-hour class by using slot number 
        9-10	-	-	-	-	-
        10-11	-	-	-	-	-
        11-12	0	5	10	15	20
        12-13	1	6	11	16	21
        13-14	2	7	12	17	22
        14-15	-	-	-	-	-
        15-16	-	-	-	-	-
        16-17	3	8	13	18	23
        17-18	4	9	14	19	24

        filling scheduling table for 2-hour class by using slot number 
        9-10	0	5	10	15	20
        10-11	-	-	-	-	-
        11-12	1	6	11	16	21
        12-13	2	7	12	17	22
        13-14	-	-	-	-	-
        14-15	3	8	13	18	23
        15-16	-	-	-	-	-
        16-17	4	9	14	19	24
        17-18	-	-	-	-	-

        filling scheduling table for 3-hour class by using slot number 
        9-10	0	4	8	12	16
        10-11	-	-	-	-	-
        11-12	1	5	9	13	17
        12-13	-	-	-	-	-
        13-14	2	6	10	14	18
        14-15	3	7	11	15	19
        15-16	-	-	-	-	-
        16-17	-	-	-	-	-
        17-18	-	-	-	-	-
        */
        static void adding_course_timeTable(Slot[,] array, int slotId, Course cor)
        {
            int x;
            int y = 0;
            if (cor.Duration == 1) // bir saatlik ders ise.
            {
                if (slotId % 5 < 3)
                {
                    x = slotId / 5;
                    y = slotId % 5 + 2;
                }
                else
                {
                    x = slotId / 5;
                    y = slotId % 5 + 4;
                }
                array[x, y].Courses.Add(cor);
                array[x, y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    array[x, y].labCount++;
            }
            else if (cor.Duration == 2) // 2 saatlik ders ise.
            {
                x = slotId / 5;
                if (slotId % 5 == 0)
                {
                    y = 0;
                }
                if (slotId % 5 == 1)
                {
                    y = 2;
                }
                if (slotId % 5 == 2)
                {
                    y = 3;
                }
                if (slotId % 5 == 3)
                {
                    y = 5;
                }
                if (slotId % 5 == 4)
                {
                    y = 7;
                }
                array[x, y].Courses.Add(cor);
                array[x, y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    array[x, y].labCount++;

                y++;
                array[x, y].Courses.Add(cor);
                array[x, y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    array[x, y].labCount++;
            }
            else if (cor.Duration == 3) // 3 saatlik ders ise.
            {
                x = slotId / 4;
                if (slotId % 4 == 0)
                {
                    y = 0;
                }
                if (slotId % 4 == 1)
                {
                    y = 2;
                }
                if (slotId % 4 == 2)
                {
                    y = 4;
                }
                if (slotId % 4 == 3)
                {
                    y = 5;
                }
                array[x, y].Courses.Add(cor);
                array[x, y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    array[x, y].labCount++;
                y++;
                array[x, y].Courses.Add(cor);
                array[x, y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    array[x, y].labCount++;
                y++;
                array[x, y].Courses.Add(cor);
                array[x, y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    array[x, y].labCount++;
            }
        }

        static List<Collision> calculate_collisionSemesterWithBaseCourses(Slot[,] timeTable, int[,] array2, int minimumCollision, int semester, bool elective = false)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i, j];

                    if (tempSlot.Courses.Count(x => x.Semester == semester && x.Elective == elective) > minimumCollision && array2[i, j] > minimumCollision)
                    {

                        Collision tempCollision = new Collision
                        {
                            Type = CollisionType.CourseCollision,
                            Result = tempSlot.Courses.Count(x => x.Semester == semester && x.Elective == elective) + array2[i, j] - 1,
                            Reason = "collision with faculty course"
                        };
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.Semester == semester && x.Elective == elective));

                        collisionList.Add(tempCollision);
                    }
                }
            }
            return collisionList;
        }
        static List<Collision> calculate_collisionElectiveWithBaseCourses(Slot[,] timeTable, int[,] array2, int minimumCollision)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i, j];

                    if (tempSlot.Courses.Count(x => x.Elective) > minimumCollision && array2[i, j] > minimumCollision)
                    {

                        Collision tempCollision = new Collision
                        {
                            Type = CollisionType.CourseCollision,
                            Result = tempSlot.Courses.Count(x => x.Elective) + array2[i, j] - 1,
                            Reason = "collision with faculty course"
                        };
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.Elective));

                        collisionList.Add(tempCollision);
                    }
                }
            }
            return collisionList;
        }

        static List<Collision> calculate_ElectiveCollision(Slot[,] timeTable, int minimumCollision)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i, j];

                    if (tempSlot.Courses.FindAll(x => x.Elective).Count > minimumCollision)
                    {
                        Collision tempCollision = new Collision
                        {
                            Type = CollisionType.CourseCollision,
                            Result = tempSlot.Courses.FindAll(x => x.Elective).Count - 1,
                            Reason = "base course collision in same semester"
                        };
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.Elective));

                        collisionList.Add(tempCollision);
                    }
                }
            }
            return collisionList;
        }
        static List<Collision> calculate_LabCollision(Slot[,] timeTable, int minimumCollision)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i, j];

                    if (tempSlot.labCount > minimumCollision)
                    {
                        Collision tempCollision = new Collision
                        {
                            Type = CollisionType.CourseCollision,
                            Result = tempSlot.labCount - 1,
                            Reason = "lab lectures > available lab count"
                        };
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.Type == 1));

                        collisionList.Add(tempCollision);
                    }

                }

            }
            return collisionList;
        }

        static List<Collision> calculate_collisionInSemester(Slot[,] timeTable, int minimumCollision, int semester, bool elective = false)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i, j];

                    if (tempSlot.Courses.FindAll(x => x.Semester == semester && x.Elective == elective).Count > minimumCollision)
                    {
                        Collision tempCollision = new Collision();
                        tempCollision.Result = tempSlot.Courses.FindAll(x => x.Semester == semester && x.Elective == elective).Count - 1;
                        tempCollision.Reason = "base course collision in same semester";
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.Semester == semester && x.Elective == elective));

                        collisionList.Add(tempCollision);
                    }
                }
            }
            return collisionList;
        }
        static List<Collision> calculate_collisionInSemesters(Slot[,] timeTable, int minimumCollision, List<int> semesters, bool compareElective = false)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i, j];

                    List<Course> selectedSemesterCourses;

                    if (compareElective)
                    {
                        selectedSemesterCourses = tempSlot.Courses.Where(x => semesters.Contains(x.Semester)).ToList();
                    }
                    else
                    {
                        selectedSemesterCourses = tempSlot.Courses.Where(x => semesters.Contains(x.Semester) && !x.Elective).ToList();
                    }


                    List<Course> nonPreCourses = new List<Course>();

                    foreach (var item in selectedSemesterCourses)
                    {
                        if (selectedSemesterCourses.Any(x => item.prerequisites.Contains(x.Id))) //derslerden preqsu olanı varsa ve aynı saatteyse
                        {
                            continue; //cakisabilir sorun degil.
                        }

                        nonPreCourses.Add(item);
                    }

                    var slotSemesters = nonPreCourses.Select(x => x.Semester);

                    bool multiSemesterInSlot = semesters.All(x => slotSemesters.Contains(x));

                    if (multiSemesterInSlot && nonPreCourses.Count(x => !x.Elective) > minimumCollision)
                    {
                        Collision tempCollision = new Collision
                        {
                            Result = nonPreCourses.FindAll(x => semesters.Contains(x.Semester) && !x.Elective).Count - 1,
                            Reason = "consicutive collision"
                        };
                        tempCollision.CrashingCourses.AddRange(nonPreCourses.Where(x => !x.Elective));

                        collisionList.Add(tempCollision);
                    }
                }
            }
            return collisionList;
        }

        static List<Collision> calculate_TeacherCollision(Slot[,] timeTable, int teacherId, int minimumCollision)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i, j];

                    int hours = tempSlot.Courses.FindAll(x => x.TeacherId == teacherId).Count;
                    if (tempSlot.meetingHour)
                        hours++;

                    if (hours > minimumCollision)
                    {
                        Collision tempCollision = new Collision
                        {
                            Type = CollisionType.TeacherCollision,
                            TeacherId = teacherId,
                            Result = hours - 1,
                            Reason = "teacher has multiple course at same hour"
                        };
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.TeacherId == teacherId));

                        collisionList.Add(tempCollision);
                    }

                }

            }
            return collisionList;
        }
        static int calculate_collisionTeacherFreeDay(Slot[,] timeTable, int teacherId)
        {
            for (int i = 0; i < 5; i++) //gun
            {
                var counter = 0;
                for (int j = 0; j < 9; j++) //dersler
                {
                    Slot tempSlot = timeTable[i, j];

                    if (tempSlot.Courses.Any(x => x.TeacherId == teacherId) || tempSlot.meetingHour)
                    {
                        counter = 0;
                        break;
                    }

                    counter++; //boş saat
                }
                if (counter == 9) //9saat boşsa 1 günü boş.
                {
                    return 0;
                }

            }

            return 1;
        }
        static int calculate_collisionTeacherConsicutive(Slot[,] timeTable, int teacherId, int maxConsecutiveHour)
        {
            int result = 0;
            for (int i = 0; i < 5; i++)
            {
                var counter = 0;
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i, j];

                    if (tempSlot.Courses.Any(x => x.TeacherId == teacherId) || tempSlot.meetingHour)
                    {
                        counter++;
                    }
                    else {
                        counter = 0;
                    }
                    if (counter >= maxConsecutiveHour)
                    {
                        result++;
                    }
                }
            }

            return result;
        }

        static List<Collision> calculate_LectureLabCollision(Slot[,] timeTable)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                List<Course> lectures = new List<Course>();
                List<Course> labs = new List<Course>();

                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i, j];

                    lectures.AddRange(tempSlot.Courses.Where(x => x.Type == 0));
                    labs.AddRange(tempSlot.Courses.Where(x => x.Type == 1));
                }

                if (labs.Count > 0 && lectures.Count > 0)
                {

                    foreach (var lab in labs)
                    {

                        for (int j = 0; j < lectures.Count; j++)
                        {
                            if (lectures[j].Code == lab.Code)
                            {
                                Collision tempCollision = new Collision
                                {
                                    Type = CollisionType.CourseCollision,
                                    Result = 1,
                                    Reason = "lab and lecture in same day"
                                };
                                tempCollision.CrashingCourses.Add(lab);
                                tempCollision.CrashingCourses.Add(lectures[j]);

                                collisionList.Add(tempCollision);
                            }
                        }

                    }


                }

            }
            return collisionList;
        }

        static List<Collision> calculate_collisionElectiveWithSemester(Slot[,] timeTable, int minimumCollision, int semester)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i, j];

                    if (tempSlot.Courses.Count(x => x.Elective) > minimumCollision && tempSlot.Courses.Count(x => x.Semester == semester && !x.Elective) > minimumCollision)
                    {
                        var electives = tempSlot.Courses.Where(x => x.Elective).ToList();
                        var baselectures = tempSlot.Courses.Where(x => x.Semester == semester && !x.Elective).ToList();

                        foreach (var elective in electives)
                        {
                            foreach (var baselecture in baselectures)
                            {
                                if (!elective.prerequisites.Contains(baselecture.Id))
                                {
                                    Collision tempCollision = new Collision
                                    {
                                        Type = CollisionType.CourseCollision,
                                        Result = 1,
                                        Reason = "collision between elective and base course"
                                    };
                                    tempCollision.CrashingCourses.Add(elective);
                                    tempCollision.CrashingCourses.Add(baselecture);

                                    collisionList.Add(tempCollision);
                                }
                            }
                        }


                    }
                }
            }
            return collisionList;
        }

        //static int calculate_collision1(List<int>[,] array, int minimumCollision)
        //{
        //    int result = 0;
        //    for (int i = 0; i < 5; i++)
        //    {
        //        for (int j = 0; j < 9; j++)
        //        {
        //            if (array[i, j].Count > minimumCollision)
        //            {
        //                result += array[i, j].Count - 1;
        //            }
        //        }
        //    }
        //    return result;
        //}

        //static int calculate_collision6(List<int>[,] array)
        //{
        //    int result = 0;
        //    List<int> day = new List<int>(5);
        //    for (int i = 0; i < 5; i++)
        //    {
        //        for (int j = 0; j < 9; j++)
        //        {
        //            for (int k = 0; k < (int)array[i, j].Count; k++)
        //            {
        //                day.Add(array[i, j][k]);
        //            }
        //        }
        //        for (int j = 0; j < (int)day.Count; j++)
        //        {
        //            for (int k = 0; k < (int)day.Count; k++)
        //            {
        //                if (j != k && CourseList[j].Code.Equals(CourseList[k].Code))
        //                {
        //                    var type1 = CourseList[j].Type;
        //                    var type2 = CourseList[k].Type;
        //                    if (type1 != type2 && type1 + type2 <= 1)
        //                    {
        //                        result++;
        //                    }
        //                }
        //            }
        //        }
        //        day.Clear();
        //    }
        //    return result;
        //}


        // collision of CSE courses at the same time

        // collision of CSE courses -1 0(calculate_collision) +1 semester

        //static int calculate_collision2(List<int>[,] array1, int[,] array2, int minimumCollision)
        //{
        //    int result = 0;
        //    for (int i = 0; i < 5; i++)
        //    {
        //        for (int j = 0; j < 9; j++)
        //        {
        //            if (array1[i, j].Count > minimumCollision && array2[i, j] > minimumCollision)
        //            {
        //                var x = array1[i, j].Count + array2[i, j] - 1;
        //                result += x;
        //            }
        //        }
        //    }
        //    return result;
        //}


        //static int calculate_collision7(List<int>[,] array1, List<int>[,] array2, int minimumCollision)
        //{
        //    int result = 0;
        //    for (int i = 0; i < 5; i++) //dönem
        //    {
        //        for (int j = 0; j < 9; j++) // saat
        //        {
        //            if (array1[i, j].Count > minimumCollision && array2[i, j].Count > minimumCollision) //ikiside min col'u geçiyorsa.
        //            {

        //                for (int k = 0; k < array2[i, j].Count; k++)
        //                {
        //                    for (int l = 0; l < array1[i, j].Count; l++)
        //                    {
        //                        if (!is_prerequisite(array1[i, j][l], array2[i, j][k]))
        //                        {
        //                            result++;
        //                        }
        //                    }
        //                }

        //            }
        //        }
        //    }
        //    return result;
        //}

        //static bool is_prerequisite(int preIndexOfCourseList, int postIndexOfCourseList)
        //{
        //    return CourseList[postIndexOfCourseList].prerequisites.Contains(preIndexOfCourseList);
        //}

        #endregion

    }
}
