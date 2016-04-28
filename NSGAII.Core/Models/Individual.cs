using System;
using System.Collections.Generic;
using System.Linq;

namespace NSGAII.Models
{
    public class Individual
    {
        public double ConstrViolation { get; set; }
        public int Rank { get; set; }
        public double[] Xreal { get; set; }
        public List<List<int>> Gene { get; set; }
        public int[] SlotId { get; set; }
        public int[] Obj { get; set; }
        public double[] Constr { get; set; }
        public double CrowdDist { get; set; }
        public int TotalResult;

        public List<Collision> CollisionList { get; set; }
        private List<FacultySection> FacultySections { get; set; }
        private List<FacultySection> DiffSemesterFacultySections { get; set; }

        public int NRealVar;
        public int NBinVar;
        public int NMaxBit;
        public int NObj;
        public int NCons;

        public readonly List<List<Slot>> Timetable;

        public Individual(int nRealVar, int nBinVar, int nMaxBit, int nObj, int nCons)
        {
            CollisionList = new List<Collision>();
            FacultySections = new List<FacultySection>();
            DiffSemesterFacultySections = new List<FacultySection>();

            NRealVar = nRealVar;
            NBinVar = nBinVar;
            NMaxBit = nMaxBit;
            NObj = nObj;
            NCons = nCons;

            if (nRealVar != 0)
                Xreal = new double[nRealVar];

            if (nBinVar != 0)
            {
                SlotId = new int[nBinVar];
                Gene = new List<List<int>>();
                for (int i = 0; i < nBinVar; i++)
                {
                    Gene.Add(new List<int>(nMaxBit));
                    for (int j = 0; j < nMaxBit; j++)
                    {
                        Gene[i].Add(0);
                    }
                }
                //Gene = new int[nBinVar, nMaxBit];
            }

            Obj = new int[nObj];

            if (nCons != 0)
                Constr = new double[nCons];

            Timetable = new List<List<Slot>>();
        }

        public Individual(Individual ind, ProblemDefinition problem)
        {
            CollisionList = new List<Collision>();
            FacultySections = new List<FacultySection>();
            DiffSemesterFacultySections = new List<FacultySection>();

            TotalResult = 0;

            NRealVar = ind.NRealVar;
            NBinVar = ind.NBinVar;
            NMaxBit = ind.NMaxBit;
            NObj = ind.NObj;
            NCons = ind.NCons;

            if (ind.NRealVar != 0)
                Xreal = new double[NRealVar];

            if (ind.NBinVar != 0)
            {
                SlotId = new int[ind.NBinVar];
                Gene = new List<List<int>>();
                for (int i = 0; i < ind.NBinVar; i++)
                {
                    Gene.Add(new List<int>(ind.NBinVar));
                    for (int j = 0; j < NMaxBit; j++)
                    {
                        Gene[i].Add(0);
                    }
                }
                //Gene = new int[ind._nBinVar, ind._nMaxBit];
            }

            Obj = new int[ind.NObj];

            if (ind.NCons != 0)
                Constr = new double[ind.NCons];


            Rank = ind.Rank;
            ConstrViolation = ind.ConstrViolation;
            CrowdDist = ind.CrowdDist;
            if (ind.NRealVar > 0)
            {
                for (int i = 0; i < ind.NRealVar; i++)
                {
                    Xreal[i] = ind.Xreal[i];
                }
            }
            if (ind.NBinVar > 0)
            {
                for (int i = 0; i < ind.NBinVar; i++)
                {
                    SlotId[i] = ind.SlotId[i];
                    for (int j = 0; j < problem.Nbits[i]; j++)
                    {
                        Gene[i][j] = ind.Gene[i][j];
                    }
                }
            }
            for (int i = 0; i < ind.NObj; i++)
            {
                Obj[i] = ind.Obj[i];
            }
            if (ind.NCons > 0)
            {
                for (int i = 0; i < ind.NCons; i++)
                {
                    Constr[i] = ind.Constr[i];
                }
            }

            Timetable = new List<List<Slot>>();

        }

        public Individual()
        {
            Timetable = new List<List<Slot>>();

            if (NRealVar != 0)
                Xreal = new double[NRealVar];

            if (NBinVar != 0)
            {
                SlotId = new int[NBinVar];
                Gene = new List<List<int>>();
                for (int i = 0; i < NBinVar; i++)
                {
                    Gene.Add(new List<int>(NMaxBit));
                    for (int j = 0; j < NMaxBit; j++)
                    {
                        Gene[i].Add(0);
                    }
                }
                //Gene = new int[nBinVar, nMaxBit];
            }

            Obj = new int[NObj];

            if (NCons != 0)
                Constr = new double[NCons];
        }

        public void Copy(Individual ind, ProblemDefinition problem)
        {
            NRealVar = ind.NRealVar;
            NBinVar = ind.NBinVar;
            NMaxBit = ind.NMaxBit;
            NObj = ind.NObj;
            NCons = ind.NCons;

            CollisionList.Clear();
            FacultySections.Clear();
            DiffSemesterFacultySections.Clear();

            TotalResult = 0;

            Rank = ind.Rank;
            ConstrViolation = ind.ConstrViolation;
            CrowdDist = ind.CrowdDist;
            if (ind.NRealVar > 0)
            {
                for (int i = 0; i < ind.NRealVar; i++)
                {
                    Xreal[i] = ind.Xreal[i];
                }
            }
            if (ind.NBinVar > 0)
            {
                for (int i = 0; i < ind.NBinVar; i++)
                {
                    SlotId[i] = ind.SlotId[i];
                    for (int j = 0; j < problem.Nbits[i]; j++)
                    {
                        Gene[i][j] = ind.Gene[i][j];
                    }
                }
            }
            for (int i = 0; i < ind.NObj; i++)
            {
                Obj[i] = ind.Obj[i];
            }
            if (ind.NCons > 0)
            {
                for (int i = 0; i < ind.NCons; i++)
                {
                    Constr[i] = ind.Constr[i];
                }
            }

        }

        public void Decode(ProblemDefinition problem)
        {
            TotalResult = 0;
            CollisionList.Clear();
            FacultySections.Clear();
            DiffSemesterFacultySections.Clear();

            if (problem.BinaryVariableCount == 0)
                return;

            for (int j = 0; j < problem.BinaryVariableCount; j++)
            {
                SlotId[j] = DecodeGene(problem, j);
            }
        }

        private int DecodeGene(ProblemDefinition problem, int j)
        {
            var sum = 0;
            for (int k = 0; k < problem.Nbits[j]; k++)
            {
                if (Gene[j][k] == 1)
                {
                    sum += (int)Math.Pow(2, problem.Nbits[j] - 1 - k);
                }
            }

            double minbin = problem.MinBinvar[j];
            double maxbin = problem.MaxBinvar[j];
            double nbit = problem.Nbits[j];

            return (int)(minbin + sum * (maxbin - minbin) / (Math.Pow(2, nbit) - 1));
        }

        private void ChangeGene(int geneId, int value, ProblemDefinition problem)
        {
            double minbin = problem.MinBinvar[geneId];
            double maxbin = problem.MaxBinvar[geneId];
            double nbit = problem.Nbits[geneId];

            //int valueToAdd = (int)((value - minbin) * ((Math.Pow(2, nbit) - 1) / (maxbin - minbin)));
            double sum = (double)((value - minbin) / ((maxbin - minbin) / (Math.Pow(2, nbit) - 1)));
            int valueToAdd = (int)sum;

            if (sum > (int)sum)
                valueToAdd++;

            int bits = problem.Nbits[geneId];
            for (int k = bits - 1; k >= 0; k--)
            {
                int divident = (int)Math.Pow(2, bits - 1 - k);
                int modulus = (int)Math.Pow(2, bits - k);
                if (valueToAdd % modulus == divident)
                {
                    valueToAdd = valueToAdd - divident;
                    Gene[geneId][k] = 1;
                }
                else
                {
                    Gene[geneId][k] = 0;
                }
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
                    Xreal[j] = randomObj.RandomDouble(problem.MinRealvar[j], problem.MaxRealvar[j]);
                }
            }
            if (problem.BinaryVariableCount != 0)
            {
                for (j = 0; j < problem.BinaryVariableCount; j++)
                {
                    for (int k = 0; k < problem.Nbits[j]; k++)
                    {
                        if (randomObj.RandomPercent() <= 0.5)
                        {
                            Gene[j][k] = 0;
                        }
                        else
                        {
                            Gene[j][k] = 1;
                        }
                    }
                }
            }
        }

        public void Evaluate(ProblemDefinition problemObj)
        {
            EvaluateProblem(problemObj);

            #region constraint kısmı yok
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
            #endregion
        }

        private void EvaluateProblem(ProblemDefinition problemObj)
        {
            TotalResult = 0;
            CollisionList.Clear();

            FacultySections.Clear();
            int fc = 0;
            foreach (var course in problemObj.FacultyCourseList.OrderBy(x => x.Code))
            {
                if (!FacultySections.Any(x => x.Code == course.Code && x.Section == course.Section))
                {
                    var temp = new FacultySection
                    {
                        Id = fc,
                        Code = course.Code,
                        Section = course.Section
                    };
                    FacultySections.Add(temp);
                    fc++;
                }
            }

            DiffSemesterFacultySections.Clear();
            fc = 0;
            foreach (var course in problemObj.FacultyCourseList.OrderBy(x => x.Code))
            {
                if (!DiffSemesterFacultySections.Any(x => x.Code == course.Code && x.Section == course.Section))
                {
                    var temp = new FacultySection
                    {
                        Id = fc,
                        Code = course.Code,
                        Section = course.Section
                    };
                    DiffSemesterFacultySections.Add(temp);
                    fc++;
                }
            }

            #region fill variables

            Slot[] easy = new Slot[50];
            Timetable.Clear();
            int idcounter = 1;
            for (int x = 0; x < 5; x++)
            {
                Timetable.Add(new List<Slot>());
                for (int y = 0; y < 10; y++)
                {

                    Timetable[x].Add(new Slot(problemObj.TeacherList.Count, idcounter));
                    easy[idcounter - 1] = Timetable[x][y];
                    idcounter++;
                }
            }

            Obj[0] = 0;
            Obj[1] = 0;
            Obj[2] = 0;


            for (int j = 0; j < problemObj.CourseList.Count; j++) //ders sayisi kadar,
            {
                int slotId = SlotId[j];
                AddToTimetable(slotId, problemObj.CourseList[j]); //dersleri timetable'a yerleştirdim.
            }

            foreach (Course facultyCourse in problemObj.FacultyCourseList)
            {
                for (int i = 0; i < facultyCourse.Duration; i++)
                {
                    easy[facultyCourse.SlotId + i - 1].facultyCourses.Add(facultyCourse);
                }
            }

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (problemObj.Meeting[x][y] > 0)
                        Timetable[x][y].meetingHour = true; // meeting hours

                    if (problemObj.LabScheduling[x][y] > 0)
                        Timetable[x][y].facultyLab = problemObj.LabScheduling[x][y];

                }
            }

            #endregion

            #region calc. collisions

            #region Base vs Faculty same semester

            //dönem ici dekanlik/bolum dersi cakismasi todo: scheduling'in normal slot halinde gelmesi lazım
            {
                List<Collision> col = CollisionBaseVsFaculty(0);

                foreach (var collision in col)
                {
                    int changeResult = 0;
                    var crashingfacultycourses = collision.CrashingCourses.Where(x => x.FacultyCourse);

                    foreach (var course in crashingfacultycourses)
                    {
                        if (FacultySections.Any(x => x.Code == course.Code && x.Crashing == false))
                        {
                            collision.Reason += $"; {course.Code} has noncrashing section";
                        }
                        else
                        {
                            changeResult++;
                        }
                    }

                    collision.Result = changeResult;
                }


                var result = col.Sum(item => item.Result);
                Obj[0] += result;
                CollisionList.AddRange(col);
            }

            #endregion

            #region Base vs Base same semester
            //donem ici bolum dersi cakismasi
            for (int j = 1; j < 9; j++)
            {
                List<Collision> col = CollisionBaseVsBase(Timetable, 1, j);
                var result = col.Sum(item => item.Result);
                Obj[0] += result;
                CollisionList.AddRange(col);
            }
            #endregion

            #region Base vs Faculty -1 +1 semester.
            //dönemler arasi dekanlik/bolum dersi cakismasi todo: scheduling'in normal slot halinde gelmesi lazım
            {
                List<Collision> col = new List<Collision>();
                for (int j = 1; j < 8; j++)
                {
                    // 1-2  2-3  3-4  4-5  5-6  6-7  7-8
                    // 2-1  3-2  4-3  5-4  6-5  7-6  8-7     consecutive CSE&faculty courses
                    col.AddRange(CollisionBaseVsFacultyDiffSemester(0, j, j + 1, 1));
                    col.AddRange(CollisionBaseVsFacultyDiffSemester(0, j + 1, j, 1));

                }

                foreach (var collision in col)
                {
                    int changeResult = 0;
                    var crashingfacultycourses = collision.CrashingCourses.Where(x => x.FacultyCourse);

                    foreach (var course in crashingfacultycourses)
                    {
                        if (DiffSemesterFacultySections.Any(x => x.Code == course.Code && x.Crashing == false))
                        {
                            collision.Reason += $"; {course.Code} has noncrashing section";
                        }
                        else
                        {
                            changeResult++;
                        }
                    }

                    collision.Result = changeResult;
                }



                var result = col.Sum(item => item.Result);
                Obj[1] += result;
                CollisionList.AddRange(col);
            }
            #endregion

            #region Base vs Base -1 +1 semester
            //dönemler arası CSE çakışmaları
            for (int j = 1; j < 8; j++)
            {
                List<Collision> col = CollisionBaseVsBaseDiffSemester(Timetable, 1, new List<int> { j, j + 1 }, 1);
                var y = col.Sum(item => item.Result);

                Obj[1] += y;
                CollisionList.AddRange(col);
            }
            #endregion

            #region Lab Count
            //aynı saatte 4'ten fazla lab olmaması lazim
            {
                List<Collision> labcol = CollisionInLabs(Timetable, 4);
                var y = labcol.Sum(item => item.Result);
                Obj[0] += y;
                CollisionList.AddRange(labcol);
            }
            //# of lab at most 4
            #endregion

            #region Teacher Collisions
            for (int j = 0; j < problemObj.TeacherList.Count; j++) //Bütün hocalar için
            {
                if (problemObj.TeacherList[j].Equals("ASSISTANT"))
                {
                    continue;
                }

                {
                    //og. gor. aynı saatte baska dersinin olmaması
                    List<Collision> col = CollisionTeacher(Timetable, j, 1);
                    var yy1 = col.Sum(item => item.Result);
                    Obj[0] += yy1;
                    CollisionList.AddRange(col);
                }


                {
                    //og. gor. gunluk 4 saatten fazla pespese dersinin olmamasi
                    List<Collision> col = CollisionTeacherConsicutive(Timetable, j, 4);
                    var y = col.Sum(item => item.Result);
                    Obj[2] += y;
                    CollisionList.AddRange(col);
                    //teacher have at most 4 consective lesson per day
                }
                {
                    //og. gor. boş gununun olması
                    List<Collision> col = CollisionTeacherFreeDay(Timetable, j);
                    var y = col.Sum(item => item.Result);
                    Obj[2] += y;
                    CollisionList.AddRange(col);
                }
                /* teacher have free day*/
            }
            #endregion

            //lab ve lecture farklı günlerde olsun
            {
                List<Collision> col = CollisionLabLectureSameDay(Timetable);
                var y = col.Sum(item => item.Result);
                Obj[2] += y;
                CollisionList.AddRange(col);
            }

            #region Elecive vs Elective
            //elective vs elective collision
            {
                List<Collision> col = CollisionElectiveVsElective(Timetable, 1);
                var y = col.Sum(item => item.Result);
                Obj[0] += y;
                CollisionList.AddRange(col);
            }
            #endregion

            #region Elective vs Faculty in semester 6 7 8
            //elective vs faculty courses in semester
            {
                List<Collision> col = CollisionElectiveVsFacultyDiffSemester(Timetable, 6, 0);
                var y = col.Sum(item => item.Result);
                Obj[2] += y;
                CollisionList.AddRange(col);
                col.Clear();

                col = CollisionElectiveVsFacultyDiffSemester(Timetable, 7, 0);
                y = col.Sum(item => item.Result);
                Obj[2] += y;
                CollisionList.AddRange(col);
                col.Clear();

                col = CollisionElectiveVsFacultyDiffSemester(Timetable, 8, 0);
                y = col.Sum(item => item.Result);
                Obj[2] += y;
                CollisionList.AddRange(col);
            }
            #endregion

            #region Elective vs Base in semester 6 7 8
            //elective vs base courses in semester
            {
                List<Collision> col = CollisionElectiveVsBaseDiffSemester(Timetable, 0, 6, 1);
                var y = col.Sum(item => item.Result);
                Obj[1] += y;
                CollisionList.AddRange(col);
                col.Clear();

                col = CollisionElectiveVsBaseDiffSemester(Timetable, 0, 7);
                y = col.Sum(item => item.Result);
                Obj[0] += y;
                CollisionList.AddRange(col);
                col.Clear();

                col = CollisionElectiveVsBaseDiffSemester(Timetable, 0, 8);
                y = col.Sum(item => item.Result);
                Obj[0] += y;
                CollisionList.AddRange(col);
            }
            #endregion

            //todo: dekanlık derslerinin sectionları??
            #endregion

            TotalResult = Obj[0] + Obj[1] + Obj[2];

        }

        public int HillClimb(ProblemDefinition problemObj)
        {
            int tResultImprovement = 0;

            if (CollisionList.Count > 0)
            {
                bool continueClimb;
                Individual original = new Individual(this, problemObj);
                original.Decode(problemObj);
                original.Evaluate(problemObj);

                int seeder = 7;
                Random rnd = new Random(seeder);

                int climbRetryCount = 0;
                do
                {
                    climbRetryCount++;
                    bool climb = HillClimber(problemObj, rnd);
                    if (climb)
                        return 0;
                    //HillClimberF22(problemObj, rnd,22);
                    Decode(problemObj);
                    Evaluate(problemObj);


                    var obj0Imp = original.Obj[0] - Obj[0];
                    var obj1Imp = original.Obj[1] - Obj[1];
                    var obj2Imp = original.Obj[2] - Obj[2];

                    if (obj0Imp + obj1Imp + obj2Imp == 0)
                    {
                        if (obj0Imp > 0)
                        {
                            original = new Individual(this, problemObj);
                            original.Decode(problemObj);
                            original.Evaluate(problemObj);
                        }
                        continueClimb = false;
                    }
                    else if (obj0Imp + obj1Imp + obj2Imp > 0)
                    {
                        continueClimb = false;
                        original = new Individual(this, problemObj);
                        original.Decode(problemObj);
                        original.Evaluate(problemObj);
                    }
                    else
                    {
                        continueClimb = false;
                    }

                    tResultImprovement += obj0Imp + obj1Imp + obj2Imp;

                } while (continueClimb && climbRetryCount < 10);

                if (tResultImprovement < 0) //todo check sanki geri almıyor.
                {
                    Copy(original, problemObj);
                    Decode(problemObj);
                    Evaluate(problemObj);
                    tResultImprovement = 0;
                }
            }
            return tResultImprovement;
        }

        private bool HillClimber(ProblemDefinition problemObj, Random rnd)
        {
            int maxClimb = 1;
            int climbCount = 0;

            var tempColl = CollisionList.ToList();

            if (tempColl.Count > 0) //collision varsa
            {

                while (tempColl.Count > 0)
                {
                    var randomColl = rnd.Next(tempColl.Count);
                    var collision = tempColl[randomColl];
                    tempColl.RemoveAt(randomColl);
                    //var collision = tempColl.First();
                    //tempColl.RemoveAt(0);

                    if (collision.CrashingCourses.Count == 0) //ögrentmen ise geçiver şimdilik.
                        continue;

                    List<int> fittingSlots = new List<int>();
                    List<int> semiFittingSlots = new List<int>();

                    #region Course type collisions
                    foreach (var courseToReposition in collision.CrashingCourses.Where(x => !x.FacultyCourse).OrderBy(x => x.Duration)) //önce küçügü koy bir yerlere
                    {
                        int maxSlot = courseToReposition.Duration == 3 ? 20 : 25;
                        List<int> testSlots = new List<int>(maxSlot);
                        for (int i = 0; i < maxSlot; i++)
                        {
                            if (i != SlotId[courseToReposition.Id]) //eski yerini deneneceklerden çıkaralım.
                                testSlots.Add(i);
                        }

                        bool fittingSlot = false;

                        int semester = courseToReposition.Semester;
                        int duration = courseToReposition.Duration;

                        while (testSlots.Count > 0)
                        {
                            var randomSlotToTest = rnd.Next(testSlots.Count);
                            int slotToTest = testSlots[randomSlotToTest];
                            testSlots.RemoveAt(randomSlotToTest);

                            var temp = GetSlot(slotToTest, duration);
                            int day = GetX(slotToTest, duration);
                            int hour = GetY(slotToTest, duration);
                            var daySlots = GetDay(day);

                            #region Check Fitting
                            for (int j = 0; j < duration; j++)
                            {
                                fittingSlot = true;

                                temp[j].Courses.RemoveAll(x => x.Id == courseToReposition.Id); //kendini çıkarıyorum ki kendinle çakışmasın.

                                #region base vs faculty same semester
                                if (!courseToReposition.Elective) //elektif degilse, o saatte fakülte dersi olmamalı.
                                {
                                    if (temp[j].facultyCourses.Any(x => x.Semester == semester)) //base vs faculty collision, same semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                }
                                #endregion

                                #region base vs base same semester
                                if (!courseToReposition.Elective) //elektif degilse, o saatte başka bölüm dersi olmamalı
                                {
                                    if (temp[j].Courses.Count(x => x.Semester == semester && !x.Elective) > 0) //base vs base collision, same semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                }
                                #endregion

                                #region base vs faculty +1 -1
                                if (!courseToReposition.Elective) //elektif degilse,
                                {
                                    if (semester - 1 > 0) // o saate bir geri dönem fakülte dersi olmasın
                                    {
                                        if (temp[j].facultyCourses.Any(x => x.Semester == semester - 1)) //base vs faculty collision, -1 semester.
                                        {
                                            fittingSlot = false;
                                            break;
                                        }
                                    }
                                    if (semester <= 8) // o saate bir ileri dönem fakülte dersi olmasın
                                    {
                                        if (temp[j].facultyCourses.Any(x => x.Semester == semester + 1)) //base vs faculty collision, +1 semester.
                                        {
                                            fittingSlot = false;
                                            break;
                                        }
                                    }
                                }
                                #endregion

                                #region base vs base +1 -1
                                if (!courseToReposition.Elective)
                                {
                                    if (semester - 1 > 0)
                                    {
                                        if (temp[j].Courses.Count(x => x.Semester == semester - 1 && !x.Elective) > 0) //base vs base collision, -1 semester.
                                        {

                                            fittingSlot = false;
                                            break;

                                        }
                                    }
                                    if (semester + 1 < 9)
                                    {
                                        if (temp[j].Courses.Count(x => x.Semester == semester + 1 && !x.Elective) > 0) //base vs base faculty collision, +1 semester.
                                        {

                                            fittingSlot = false;
                                            break;

                                        }
                                    }
                                }
                                #endregion

                                #region LAB base vs faculty 
                                if (courseToReposition.Type == 1) //lab ise
                                {
                                    if (temp[j].labCount + temp[j].facultyLab >= 4) //base vs faculty collision, same semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                }
                                #endregion

                                #region Teacher coll
                                if (courseToReposition.Teacher != "ASSISTANT")
                                {
                                    if (temp[j].Courses.Count(x => x.TeacherId == courseToReposition.TeacherId || temp[j].meetingHour) > 0) //check teacher collision
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                }
                                #endregion

                                #region Teacher consecutive coll
                                //todo: obj2 og. gor. gunluk 4 saatten fazla pespese dersinin olmamasi
                                if (courseToReposition.Teacher != "ASSISTANT")
                                {
                                    int maxConsecutiveHour = 4;
                                    int counter = 0;
                                    for (int k = 0; k < 9; k++)
                                    {
                                        Slot tempSlot = daySlots[k];

                                        if (k == hour)
                                            counter++;

                                        if (tempSlot.Courses.Any(x => x.TeacherId == courseToReposition.TeacherId) || tempSlot.meetingHour) //dersi veya meetingi varsa ++
                                        {
                                            counter++;
                                        }
                                        else
                                        {
                                            counter = 0; // ara varsa 0'la
                                        }
                                        if (counter > maxConsecutiveHour)
                                        {
                                            fittingSlot = false;
                                            break;
                                        }
                                    }
                                }
                                #endregion

                                //todo: obj2 og. gor. boş gununun olması

                                //bunu duration loop dışına alabilirim.
                                #region lab lecture collision
                                if (courseToReposition.LabHour > 0) //lab'ı var. //todo: obj2 lab ve lecture farklı günlerde olsun 
                                {
                                    if (courseToReposition.Type == 1)//labı yerleştiriyoruz.
                                    {
                                        if (daySlots.Any(slot => slot.Courses.Count(x => x.Code == courseToReposition.Code && x.Type == 0) > 0))
                                        {
                                            fittingSlot = false;
                                            break;
                                        }
                                    }
                                    else //dersi yerleştiriyoruz
                                    {
                                        if (daySlots.Any(slot => slot.Courses.Count(x => x.Code == courseToReposition.Code && x.Type == 1) > 0))
                                        {
                                            fittingSlot = false;
                                            break;
                                        }
                                    }
                                }
                                #endregion

                                #region Elective vs Elective
                                if (courseToReposition.Elective)
                                {
                                    if (temp[j].Courses.Count(x => x.Elective) > 0) //elective vs elective collision, all semesters.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                }
                                #endregion

                                #region Elective vs Faculty in semester 6 7 8
                                //todo: obj2 
                                if (courseToReposition.Elective)
                                {
                                    if (temp[j].facultyCourses.Any(x => x.Semester == 6)) //base vs faculty collision, same semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                    if (temp[j].facultyCourses.Any(x => x.Semester == 7)) //base vs faculty collision, same semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                    if (temp[j].facultyCourses.Any(x => x.Semester == 8)) //base vs faculty collision, same semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                }
                                #endregion

                                #region Elective vs Base
                                //elective vs base courses in semester
                                if (courseToReposition.Elective)
                                {
                                    if (temp[j].Courses.Count(x => x.Semester == 8 & !x.Elective) > 0) //elective vs base collision, #8 semester.                                        
                                    {
                                        fittingSlot = false;
                                        break;
                                    }

                                    if (temp[j].Courses.Count(x => x.Semester == 7 & !x.Elective) > 0) //elective vs base collision, #7 semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }

                                    if (temp[j].Courses.Count(x => x.Semester == 6 & !x.Elective) > 0) //todo: obj1 elective vs base collision, #6 semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                }
                                else if (courseToReposition.Semester == 8 || courseToReposition.Semester == 7 || courseToReposition.Semester == 6)
                                {
                                    if (temp[j].Courses.Count(x => x.Elective) > 0) //elective vs base collision, #8 semester.                                        
                                    {
                                        fittingSlot = false;
                                        break;
                                    }

                                    if (temp[j].Courses.Count(x => x.Elective) > 0) //elective vs base collision, #7 semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }

                                    if (temp[j].Courses.Count(x => x.Elective) > 0) //todo: obj1 elective vs base collision, #6 semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                }
                                #endregion
                            } // DURATION LOOP, ders saati kadar ileriye bakıyor.
                            #endregion

                            if (fittingSlot)
                            {
                                fittingSlots.Add(slotToTest);
                                break;
                            }
                        }

                        if (fittingSlots.Count > 0)
                        {
                            int selectedSlot = fittingSlots[rnd.Next(fittingSlots.Count)];
                            int checkDay = GetX(selectedSlot, duration) + 1;
                            int checkHour = GetY(selectedSlot, duration) + 1;

                            int fixedObj = collision.Obj;
                            ChangeGene(courseToReposition.Id, selectedSlot, problemObj);
                            SlotId[courseToReposition.Id] = selectedSlot;
                            climbCount++;
                            break;
                        }
                        else
                        {
                            //no fit
                            //Console.WriteLine("0 fitting slot found :(");
                        }
                    } //CRASHING COURSE
                    #endregion

                    if (climbCount >= maxClimb)
                    {
                        return true;
                    }


                }

                if (climbCount > 0)
                    return true;

                //hiçbirini çözemedi.
                return false;

            }

            return false;
        }

        private List<Slot> GetDay(int dayId)
        {

            List<Slot> temp = new List<Slot>(10);
            temp.AddRange(Timetable[dayId]);

            return temp;
        }

        private List<Slot> GetSlot(int slotId, int hour)
        {
            int x = 0;
            int y = 0;

            if (slotId > 24)
                return null;

            #region determine x,y
            if (hour == 1) // bir saatlik ders ise.
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

            }
            else if (hour == 2) // 2 saatlik ders ise.
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
            }
            else if (hour == 3) // 3 saatlik ders ise.
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
            }
            #endregion

            List<Slot> temp = new List<Slot>(hour);

            if (hour == 1)
            {
                temp.Add(Timetable[x][y]);
            }
            else if (hour == 2)
            {
                temp.Add(Timetable[x][y]);
                temp.Add(Timetable[x][y + 1]);
            }
            else if (hour == 3)
            {
                temp.Add(Timetable[x][y]);
                temp.Add(Timetable[x][y + 1]);
                temp.Add(Timetable[x][y + 2]);
            }
            else
            {
                return null;
            }

            return temp;
        }

        private int GetX(int slotId, int hour)
        {
            int x = 0;

            if (slotId > 24)
                return -1;

            #region determine x
            if (hour == 1) // bir saatlik ders ise.
            {
                if (slotId % 5 < 3)
                {
                    x = slotId / 5;
                }
                else
                {
                    x = slotId / 5;
                }

            }
            else if (hour == 2) // 2 saatlik ders ise.
            {
                x = slotId / 5;
            }
            else if (hour == 3) // 3 saatlik ders ise.
            {
                x = slotId / 4;
            }
            #endregion

            return x;
        }
        private int GetY(int slotId, int hour)
        {
            int y = 0;

            if (slotId > 24)
                return -1;

            #region determine y
            if (hour == 1) // bir saatlik ders ise.
            {
                if (slotId % 5 < 3)
                {
                    y = slotId % 5 + 2;
                }
                else
                {
                    y = slotId % 5 + 4;
                }

            }
            else if (hour == 2) // 2 saatlik ders ise.
            {
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
            }
            else if (hour == 3) // 3 saatlik ders ise.
            {
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
            }
            #endregion

            return y;
        }

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
        private void AddToTimetable(int slotId, Course cor)
        {
            int x = GetX(slotId, cor.Duration);
            int y = GetY(slotId, cor.Duration);

            if (cor.Duration == 1) // bir saatlik ders ise.
            {
                Timetable[x][y].Courses.Add(cor);
                Timetable[x][y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    Timetable[x][y].labCount++;
            }
            else if (cor.Duration == 2) // 2 saatlik ders ise.
            {
                Timetable[x][y].Courses.Add(cor);
                Timetable[x][y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    Timetable[x][y].labCount++;

                y++;
                Timetable[x][y].Courses.Add(cor);
                Timetable[x][y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    Timetable[x][y].labCount++;
            }
            else if (cor.Duration == 3) // 3 saatlik ders ise.
            {
                Timetable[x][y].Courses.Add(cor);
                Timetable[x][y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    Timetable[x][y].labCount++;
                y++;
                Timetable[x][y].Courses.Add(cor);
                Timetable[x][y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    Timetable[x][y].labCount++;
                y++;
                Timetable[x][y].Courses.Add(cor);
                Timetable[x][y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    Timetable[x][y].labCount++;
            }
        }

        public override string ToString()
        {
            string ret = $"Rank:{Rank}\tTotal:{TotalResult}\t[0]:{Obj[0]}\t[1]:{Obj[1]}\t[2]:{Obj[2]}";
            return ret;
        }

        #region Collisions

        private List<Collision> CollisionBaseVsFaculty(int minimumCollision, int obj = 0)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int semester = 1; semester < 9; semester++)
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        Slot tempSlot = Timetable[i][j];

                        if (tempSlot.Courses.Count(x => x.Semester == semester && !x.Elective) > minimumCollision &&
                            tempSlot.facultyCourses.Count(x => x.Semester == semester) > minimumCollision)
                        {
                            var semester1 = semester;
                            var crashingFacultyCourses = tempSlot.facultyCourses.Where(x => x.Semester == semester1);

                            foreach (var course in crashingFacultyCourses)
                            {
                                if (FacultySections.Any(x => x.Code == course.Code && x.Section == course.Section))
                                {
                                    FacultySection temp =
                                        FacultySections.Find(x => x.Code == course.Code && x.Section == course.Section);
                                    temp.CrashCount++;
                                    temp.Crashing = true;
                                }
                            }

                            Collision tempCollision = new Collision
                            {
                                SlotId = tempSlot.Id,
                                Obj = obj,
                                Type = CollisionType.BaseLectureWithFaculty,
                                Result = tempSlot.facultyCourses.Count(x => x.Semester == semester),
                                Reason = "Base v Faculty same semester"
                            };
                            tempCollision.CrashingCourses.AddRange(
                                tempSlot.Courses.FindAll(x => x.Semester == semester && !x.Elective));
                            tempCollision.CrashingCourses.AddRange(
                                tempSlot.facultyCourses.FindAll(x => x.Semester == semester));

                            collisionList.Add(tempCollision);
                        }
                    }
                }
            }
            return collisionList;
        }
        private List<Collision> CollisionBaseVsFacultyDiffSemester(int minimumCollision, int semester, int facultySemester, int obj = 0)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = Timetable[i][j];

                    if (tempSlot.Courses.Count(x => x.Semester == semester && !x.Elective) > minimumCollision && tempSlot.facultyCourses.Count(x => x.Semester == facultySemester) > minimumCollision)
                    {
                        
                        var crashingFacultyCourses = tempSlot.facultyCourses.Where(x => x.Semester == facultySemester);

                        foreach (var course in crashingFacultyCourses)
                        {
                            if (DiffSemesterFacultySections.Any(x => x.Code == course.Code && x.Section == course.Section))
                            {
                                FacultySection temp =
                                    DiffSemesterFacultySections.Find(x => x.Code == course.Code && x.Section == course.Section);
                                temp.CrashCount++;
                                temp.Crashing = true;
                            }
                        }

                        Collision tempCollision = new Collision
                        {
                            SlotId = tempSlot.Id,
                            Obj = obj,
                            Type = CollisionType.BaseLectureWithFaculty,
                            Result = tempSlot.Courses.Count(x => x.Semester == semester && !x.Elective) + tempSlot.facultyCourses.Count(x => x.Semester == facultySemester) - 1,
                            Reason = "Base v Faculty different semesters"
                        };
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.Semester == semester && !x.Elective));
                        tempCollision.CrashingCourses.AddRange(tempSlot.facultyCourses.FindAll(x => x.Semester == facultySemester));

                        collisionList.Add(tempCollision);
                    }
                }
            }
            return collisionList;
        }
        static List<Collision> CollisionBaseVsBase(List<List<Slot>> timeTable, int minimumCollision, int semester, int obj = 0)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i][j];

                    if (tempSlot.Courses.FindAll(x => x.Semester == semester && !x.Elective).Count > minimumCollision)
                    {
                        Collision tempCollision = new Collision
                        {
                            SlotId = tempSlot.Id,
                            Obj = obj,
                            Type = CollisionType.BaseLectureWithBase,
                            Result = tempSlot.Courses.FindAll(x => x.Semester == semester && !x.Elective).Count - 1,
                            Reason = "Base v Base same semester"
                        };
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.Semester == semester && !x.Elective));

                        collisionList.Add(tempCollision);
                    }
                }
            }
            return collisionList;
        }
        static List<Collision> CollisionBaseVsBaseDiffSemester(List<List<Slot>> timeTable, int minimumCollision, List<int> semesters, int obj = 0)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i][j];

                    List<Course> selectedSemesterCourses = tempSlot.Courses.Where(x => semesters.Contains(x.Semester) && !x.Elective).ToList();

                    List<Course> nonPreCourses = selectedSemesterCourses.Where(item => !selectedSemesterCourses.Any(x => item.prerequisites.Contains(x.Id))).ToList();

                    var slotSemesters = nonPreCourses.Select(x => x.Semester);

                    bool multiSemesterInSlot = semesters.All(x => slotSemesters.Contains(x));

                    if (multiSemesterInSlot && nonPreCourses.Count(x => !x.Elective) > minimumCollision)
                    {
                        Collision tempCollision = new Collision
                        {
                            SlotId = tempSlot.Id,
                            Obj = obj,
                            Result = nonPreCourses.FindAll(x => semesters.Contains(x.Semester) && !x.Elective).Count - 1,
                            Reason = "Base v Base different semester"
                        };
                        tempCollision.CrashingCourses.AddRange(nonPreCourses.Where(x => !x.Elective));

                        collisionList.Add(tempCollision);
                    }
                }
            }
            return collisionList;
        }
        static List<Collision> CollisionInLabs(List<List<Slot>> timeTable, int minimumCollision, int obj = 0)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i][j];

                    if (tempSlot.labCount + tempSlot.facultyLab > minimumCollision)
                    {
                        Collision tempCollision = new Collision
                        {
                            SlotId = tempSlot.Id,
                            Obj = obj,
                            Type = CollisionType.CourseCollision,
                            Result = tempSlot.labCount + tempSlot.facultyLab - minimumCollision,
                            Reason = "Lab lectures > available lab count"
                        };
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.Type == 1));

                        collisionList.Add(tempCollision);
                    }

                }

            }
            return collisionList;
        }

        static List<Collision> CollisionTeacher(List<List<Slot>> timeTable, int teacherId, int minimumCollision, int obj = 0)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i][j];

                    int lectures = tempSlot.Courses.FindAll(x => x.TeacherId == teacherId).Count;
                    if (tempSlot.meetingHour)
                        lectures++;

                    if (lectures > minimumCollision)
                    {
                        Collision tempCollision = new Collision
                        {
                            SlotId = tempSlot.Id,
                            Obj = obj,
                            Type = CollisionType.TeacherCollision,
                            TeacherId = teacherId,
                            Result = lectures - minimumCollision,
                            Reason = "Teacher has multiple lectures in same hour"
                        };
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.TeacherId == teacherId));

                        collisionList.Add(tempCollision);
                    }

                }

            }
            return collisionList;
        }
        static List<Collision> CollisionTeacherFreeDay(List<List<Slot>> timeTable, int teacherId)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++) //gun
            {
                var counter = 0;
                for (int j = 0; j < 9; j++) //saatler
                {
                    Slot tempSlot = timeTable[i][j];

                    if (tempSlot.Courses.Any(x => x.TeacherId == teacherId) || tempSlot.meetingHour)
                    {
                        counter = 0;
                        break;
                    }

                    counter++; //boş saat
                }
                if (counter == 9) //9saat boşsa 1 günü boş.
                {
                    return collisionList;
                }

            }

            Collision tempCollision = new Collision
            {
                Obj = 2,
                Type = CollisionType.TeacherCollision,
                TeacherId = teacherId,
                Result = 1, // 1
                Reason = "Teacher doesnt have free day"
            };
            collisionList.Add(tempCollision);

            return collisionList;
        }
        static List<Collision> CollisionTeacherConsicutive(List<List<Slot>> timeTable, int teacherId, int maxConsecutiveHour)
        {
            List<Collision> collisionList = new List<Collision>();

            int result = 0;
            for (int i = 0; i < 5; i++) //gün
            {
                var counter = 0;
                for (int j = 0; j < 9; j++) //saat
                {
                    Slot tempSlot = timeTable[i][j];

                    if (tempSlot.Courses.Any(x => x.TeacherId == teacherId) || tempSlot.meetingHour) //dersi veya meetingi varsa ++
                    {
                        counter++;
                    }
                    else
                    {
                        counter = 0; // ara varsa 0'la
                    }
                    if (counter > maxConsecutiveHour)
                    {
                        result++;
                    }
                }
            }
            if (result > 0)
            {
                Collision tempCollision = new Collision
                {
                    Obj = 2,
                    Type = CollisionType.TeacherCollision,
                    TeacherId = teacherId,
                    Result = 1, // how many crash
                    Reason = "Teacher has too much consicutive courses."
                };
                collisionList.Add(tempCollision);
            }
            return collisionList;
        }

        static List<Collision> CollisionLabLectureSameDay(List<List<Slot>> timeTable, int obj = 2)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                List<Course> lectures = new List<Course>();
                List<Course> labs = new List<Course>();

                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i][j];

                    lectures.AddRange(tempSlot.Courses.Where(x => x.Type == 0));
                    labs.AddRange(tempSlot.Courses.Where(x => x.Type == 1));
                }

                if (labs.Count > 0 && lectures.Count > 0) //hem lab hem lecture varsa
                {
                    foreach (Course lab in labs)
                    {
                        foreach (Course lecture in lectures)
                        {
                            if (lecture.Code == lab.Code)
                            {
                                Collision tempCollision = new Collision
                                {
                                    Obj = obj,
                                    Type = CollisionType.CourseCollision,
                                    Result = 1,
                                    Reason = "Lab and Lecture in same day"
                                };
                                tempCollision.CrashingCourses.Add(lab);
                                tempCollision.CrashingCourses.Add(lecture);

                                collisionList.Add(tempCollision);
                                break; //bir çakışma yetiyor.
                            }
                        }
                    }


                }

            }
            return collisionList;
        }

        static List<Collision> CollisionElectiveVsElective(List<List<Slot>> timeTable, int minimumCollision, int obj = 0)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i][j];

                    if (tempSlot.Courses.FindAll(x => x.Elective).Count > minimumCollision)
                    {
                        Collision tempCollision = new Collision
                        {
                            SlotId = tempSlot.Id,
                            Obj = obj,
                            Type = CollisionType.CourseCollision,
                            Result = tempSlot.Courses.FindAll(x => x.Elective).Count - minimumCollision,
                            Reason = "Elective v Elective"
                        };
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.Elective));

                        collisionList.Add(tempCollision);
                    }
                }
            }
            return collisionList;
        }

        static List<Collision> CollisionElectiveVsFacultyDiffSemester(List<List<Slot>> timeTable, int facultySemester, int minimumCollision, int obj = 2)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i][j];

                    if (tempSlot.Courses.Count(x => x.Elective) > minimumCollision && tempSlot.facultyCourses.Count(x => x.Semester == facultySemester) > minimumCollision)
                    {

                        Collision tempCollision = new Collision
                        {
                            SlotId = tempSlot.Id,
                            Obj = obj,
                            Type = CollisionType.CourseCollision,
                            Result = tempSlot.Courses.Count(x => x.Elective) + tempSlot.facultyCourses.Count(x => x.Semester == facultySemester) - 1,
                            Reason = $"Elective v Faculty Semester {facultySemester}"
                        };
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.Elective));

                        collisionList.Add(tempCollision);
                    }
                }
            }
            return collisionList;
        }

        static List<Collision> CollisionElectiveVsBaseDiffSemester(List<List<Slot>> timeTable, int minimumCollision, int semester, int obj = 0)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i][j];

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
                                        SlotId = tempSlot.Id,
                                        Obj = obj,
                                        Type = CollisionType.CourseCollision,
                                        Result = 1,
                                        Reason = $"Elective v Base Semester {semester}"
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

        #endregion

    }
}
