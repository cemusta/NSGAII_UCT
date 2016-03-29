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

        public int _nRealVar;
        public int _nBinVar;
        public int _nMaxBit;
        public int _nObj;
        public int _nCons;

        public readonly List<List<Slot>> TimeTable;

        public Individual(int nRealVar, int nBinVar, int nMaxBit, int nObj, int nCons)
        {
            CollisionList = new List<Collision>();

            _nRealVar = nRealVar;
            _nBinVar = nBinVar;
            _nMaxBit = nMaxBit;
            _nObj = nObj;
            _nCons = nCons;

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

            TimeTable = new List<List<Slot>>();
        }

        public Individual(Individual ind, ProblemDefinition problem)
        {
            CollisionList = new List<Collision>();
            TotalResult = 0;

            _nRealVar = ind._nRealVar;
            _nBinVar = ind._nBinVar;
            _nMaxBit = ind._nMaxBit;
            _nObj = ind._nObj;
            _nCons = ind._nCons;

            if (ind._nRealVar != 0)
                Xreal = new double[_nRealVar];

            if (ind._nBinVar != 0)
            {
                SlotId = new int[ind._nBinVar];
                Gene = new List<List<int>>();
                for (int i = 0; i < ind._nBinVar; i++)
                {
                    Gene.Add(new List<int>(ind._nBinVar));
                    for (int j = 0; j < _nMaxBit; j++)
                    {
                        Gene[i].Add(0);
                    }
                }
                //Gene = new int[ind._nBinVar, ind._nMaxBit];
            }

            Obj = new int[ind._nObj];

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
                    SlotId[i] = ind.SlotId[i];
                    for (int j = 0; j < problem.nbits[i]; j++)
                    {
                        Gene[i][j] = ind.Gene[i][j];
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

            TimeTable = new List<List<Slot>>();

        }

        public Individual()
        {
            TimeTable = new List<List<Slot>>();

            if (_nRealVar != 0)
                Xreal = new double[_nRealVar];

            if (_nBinVar != 0)
            {
                SlotId = new int[_nBinVar];
                Gene = new List<List<int>>();
                for (int i = 0; i < _nBinVar; i++)
                {
                    Gene.Add(new List<int>(_nMaxBit));
                    for (int j = 0; j < _nMaxBit; j++)
                    {
                        Gene[i].Add(0);
                    }
                }
                //Gene = new int[nBinVar, nMaxBit];
            }

            Obj = new int[_nObj];

            if (_nCons != 0)
                Constr = new double[_nCons];
        }

        public void Copy(Individual ind, ProblemDefinition problem)
        {
            _nRealVar = ind._nRealVar;
            _nBinVar = ind._nBinVar;
            _nMaxBit = ind._nMaxBit;
            _nObj = ind._nObj;
            _nCons = ind._nCons;

            CollisionList.Clear();
            TotalResult = 0;

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
                    SlotId[i] = ind.SlotId[i];
                    for (int j = 0; j < problem.nbits[i]; j++)
                    {
                        Gene[i][j] = ind.Gene[i][j];
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

        public void Decode(ProblemDefinition problem)
        {
            TotalResult = 0;
            CollisionList.Clear();

            if (problem.BinaryVariableCount == 0)
                return;

            for (int j = 0; j < problem.BinaryVariableCount; j++)
            {
                var sum = 0;
                for (int k = 0; k < problem.nbits[j]; k++)
                {
                    if (Gene[j][ k] == 1)
                    {
                        sum += (int)Math.Pow(2, problem.nbits[j] - 1 - k);
                    }
                }

                SlotId[j] = (int)(problem.min_binvar[j] + sum * (problem.max_binvar[j] - problem.min_binvar[j]) / (Math.Pow(2, problem.nbits[j]) - 1));
            }
        }

        private void ChangeGene(int geneId, int value, ProblemDefinition problem)
        {
            int valueToAdd = (int)(problem.min_binvar[geneId] + value * ((Math.Pow(2, problem.nbits[geneId]) - 1) / problem.max_binvar[geneId] - problem.min_binvar[geneId]));

            int bits = problem.nbits[geneId];
            for (int k = bits - 1; k >= 0; k--)
            {
                int divident = (int)Math.Pow(2, bits - 1 - k);
                int modulus = (int)Math.Pow(2, bits - k);
                if (valueToAdd % modulus == divident)
                {
                    valueToAdd = valueToAdd - divident;
                    Gene[geneId][ k] = 1;
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
                            Gene[j][ k] = 0;
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

        private void EvaluateProblem(ProblemDefinition problemObj)
        {
            TotalResult = 0;
            CollisionList.Clear();

            #region fill variables

            TimeTable.Clear();
            for (int x = 0; x < 5; x++)
            {
                TimeTable.Add(new List<Slot>());
                for (int y = 0; y < 9; y++)
                {
                    TimeTable[x].Add(new Slot(problemObj.TeacherList.Count));
                }
            }

            Obj[0] = 0;
            Obj[1] = 0;
            Obj[2] = 0;


            for (int j = 0; j < problemObj.BinaryVariableCount; j++) //ders sayisi kadar.
            {
                int slotId = SlotId[j];
                adding_course_timeTable(TimeTable, slotId, problemObj.CourseList[j]);
            }

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (problemObj.Meeting[x][y] > 0)
                        TimeTable[x][y].meetingHour = true; // meeting hours
                }
            }
            #endregion

            #region calc. collisions

            #region Base vs Faculty same semester
            //dönem ici dekanlik/bolum dersi cakismasi todo: scheduling'in normal slot halinde gelmesi lazım
            for (int j = 0; j < 8; j++)
            {
                List<Collision> col = calculate_collisionBaseLectureWithFaculty(TimeTable, problemObj.Scheduling[j], 0, j + 1);
                var result = col.Sum(item => item.Result);
                Obj[0] += result;
                CollisionList.AddRange(col);
            }
            #endregion

            #region Base vs Base same semester
            //donem ici bolum dersi cakismasi
            for (int j = 0; j < 8; j++)
            {
                List<Collision> col = calculate_collisionInSemester(TimeTable, 1, j + 1);
                var result = col.Sum(item => item.Result);
                Obj[0] += result;
                CollisionList.AddRange(col);
            }
            #endregion

            #region Base vs Faculty -1 +1 semester.
            //dönemler arasi dekanlik/bolum dersi cakismasi todo: scheduling'in normal slot halinde gelmesi lazım
            for (int j = 1; j < 8; j++)
            {
                // 1-2  2-3  3-4  4-5  5-6  6-7  7-8
                // 2-1  3-2  4-3  5-4  6-5  7-6  8-7     consecutive CSE&faculty courses
                List<Collision> col = calculate_collisionBaseLectureWithFaculty(TimeTable, problemObj.Scheduling[j], 0, j, 1);
                col.AddRange(calculate_collisionBaseLectureWithFaculty(TimeTable, problemObj.Scheduling[j - 1], 0, j + 1, 1));
                var result = col.Sum(item => item.Result);
                Obj[1] += result;
                CollisionList.AddRange(col);
            }
            #endregion

            #region Base vs Base -1 +1 semester
            //dönemler arası CSE çakışmaları
            for (int j = 1; j < 8; j++)
            {
                List<Collision> col = calculate_collisionInSemesters(TimeTable, 1, new List<int> { j, j + 1 }, 1);
                var y = col.Sum(item => item.Result);

                Obj[1] += y;
                CollisionList.AddRange(col);
            }
            #endregion

            #region Lab Count
            //aynı saatte 4'ten fazla lab olmaması lazim todo: hangi lab? inputtan alacaz.
            {
                List<Collision> labcol = calculate_LabCollision(TimeTable, 4);
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
                    List<Collision> col = calculate_TeacherCollision(TimeTable, j, 1);
                    var yy1 = col.Sum(item => item.Result);
                    Obj[0] += yy1;
                    CollisionList.AddRange(col);
                }


                {
                    //og. gor. gunluk 4 saatten fazla pespese dersinin olmamasi
                    var y = calculate_collisionTeacherConsicutive(TimeTable, j, 4);
                    if (y > 0)
                    {
                        Obj[2] += 1;
                        Collision tempCollision = new Collision
                        {
                            Obj = 2,
                            Type = CollisionType.TeacherCollision,
                            TeacherId = j,
                            Result = 1, // how many crash
                            Reason = "Teacher has consicutive course crash."
                        };
                        CollisionList.Add(tempCollision);
                    }
                    //teacher have at most 4 consective lesson per day
                }
                {
                    //og. gor. boş gununun olması
                    var y = calculate_collisionTeacherFreeDay(TimeTable, j);
                    if (y > 0)
                    {
                        Obj[2] += 1;
                        Collision tempCollision = new Collision
                        {
                            Obj = 2,
                            Type = CollisionType.TeacherCollision,
                            TeacherId = j,
                            Result = 1, // 1
                            Reason = "Teacher doesnt have free day"
                        };
                        CollisionList.Add(tempCollision);
                    }

                }
                /* teacher have free day*/
            }
            #endregion

            //lab ve lecture farklı günlerde olsun
            {
                //var x = 0;
                //for (j = 0; j < 8; j++)
                //{
                //    x += calculate_collision6(schedulingOnlyCse[j]);    /*lab lecture hours must be in seperate day*/
                //}
                //Obj[2] += x;  //todo: sayılar tutmuyor.

                List<Collision> col = calculate_LectureLabCollision(TimeTable);
                var y = col.Sum(item => item.Result);
                Obj[2] += y;
                CollisionList.AddRange(col);
            }

            #region Elecive vs Elective
            //elective vs elective collision
            {
                List<Collision> col = calculate_ElectiveCollision(TimeTable, 1);
                var y = col.Sum(item => item.Result);
                Obj[0] += y;
                CollisionList.AddRange(col);
            }
            #endregion

            #region Elective vs Faculty in semester 6 7 8
            //elective vs faculty courses in semester
            {
                List<Collision> col = calculate_collisionElectiveWithBaseCourses(TimeTable, problemObj.Scheduling[5], 0);
                var y = col.Sum(item => item.Result);
                Obj[2] += y;
                CollisionList.AddRange(col);
                col.Clear();

                col = calculate_collisionElectiveWithBaseCourses(TimeTable, problemObj.Scheduling[6], 0);
                y = col.Sum(item => item.Result);
                Obj[2] += y;
                CollisionList.AddRange(col);
                col.Clear();

                col = calculate_collisionElectiveWithBaseCourses(TimeTable, problemObj.Scheduling[7], 0);
                y = col.Sum(item => item.Result);
                Obj[2] += y;
                CollisionList.AddRange(col);
            }
            #endregion

            #region Elective vs Base in semester 6 7 8
            //elective vs base courses in semester
            {
                List<Collision> col = calculate_collisionElectiveWithSemester(TimeTable, 0, 6, 1);
                var y = col.Sum(item => item.Result);
                Obj[1] += y;
                CollisionList.AddRange(col);
                col.Clear();

                col = calculate_collisionElectiveWithSemester(TimeTable, 0, 7);
                y = col.Sum(item => item.Result);
                Obj[0] += y;
                CollisionList.AddRange(col);
                col.Clear();

                col = calculate_collisionElectiveWithSemester(TimeTable, 0, 8);
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
            int tCollisionImprovement = 0;
            int tResultImprovement = 0;

            if (CollisionList.Count > 0)
            {
                bool continueClimb;
                Individual original = new Individual(this, problemObj);
                original.Decode(problemObj);
                original.Evaluate(problemObj);


                do
                {
                    var oldCollisionCount = CollisionList.Count;
                    var oldResult = CollisionList.Sum(x => x.Result);

                    HillClimber(problemObj);
                    Decode(problemObj);
                    Evaluate(problemObj);

                    int newResult = CollisionList.Sum(x => x.Result);
                    var resultImprovement = oldResult - newResult;
                    var collisionImprovement = oldCollisionCount - CollisionList.Count;

                    if (collisionImprovement > 0)
                    {
                        continueClimb = true;
                        original = new Individual(this, problemObj);
                        original.Decode(problemObj);
                        original.Evaluate(problemObj);
                    }
                    else if (collisionImprovement == 0 && resultImprovement > 0)
                    {
                        continueClimb = true;
                        original = new Individual(this, problemObj);
                        original.Decode(problemObj);
                        original.Evaluate(problemObj);
                    }
                    else
                    {
                        continueClimb = false;
                    }

                    tCollisionImprovement += collisionImprovement;
                    tResultImprovement += resultImprovement;

                } while (continueClimb);

                if (tCollisionImprovement < 0 || tResultImprovement < 0) //todo check sanki geri almıyor.
                {
                    Copy(original, problemObj);
                    Decode(problemObj);
                    Evaluate(problemObj);
                    tResultImprovement = 0;
                }
            }
            return tResultImprovement;
        }

        private void HillClimber(ProblemDefinition problemObj)
        {
            int maxClimb = 5;
            bool UseSemiFit = false;
            int seeder = (int) DateTime.Now.Ticks;
            Random rnd = new Random(seeder);

            var tempColl = CollisionList.ToList();

            if (tempColl.Count > 0)
            {

                while (tempColl.Count > 0)
                {
                    var randomColl = rnd.Next(tempColl.Count);
                    var collision = tempColl[randomColl];
                    tempColl.RemoveAt(randomColl);

                    if (collision.CrashingCourses.Count == 0)
                        continue;

                    List<int> fittingSlots = new List<int>();
                    List<int> semiFittingSlots = new List<int>();

                    int climbCount = 0;

                    #region Course type collisions
                    foreach (var crashingCourse in collision.CrashingCourses.OrderBy(x => x.Duration)) //önce küçügü koy bir yerlere
                    {
                        int maxSlot = crashingCourse.Duration == 3 ? 20 : 25;
                        List<int> testSlots = new List<int>(maxSlot);
                        for (int i = 0; i < maxSlot; i++)
                        {
                            testSlots.Add(i);
                        }
                        bool fittingSlot = false;
                        bool semiFittingSlot = false;

                        int semester = crashingCourse.Semester;
                        int duration = crashingCourse.Duration;

                        while (testSlots.Count > 0)
                        {
                            var randomSlotToTest = rnd.Next(testSlots.Count);
                            int i = testSlots[randomSlotToTest];
                            testSlots.RemoveAt(randomSlotToTest);

                            var temp = GetSlot(i, duration);
                            int day = GetX(i, duration);
                            int hour = GetY(i, duration);

                            #region Check Fitting
                            for (int j = 0; j < duration; j++)
                            {
                                fittingSlot = true;
                                semiFittingSlot = false;
                                temp[j].Courses.RemoveAll(x => x.Id == crashingCourse.Id);

                                #region base vs faculty same semester
                                if (!crashingCourse.Elective)
                                {
                                    if (problemObj.Scheduling[semester - 1][day][hour + j] > 0) //base vs faculty collision, same semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                }
                                #endregion

                                #region base vs base same semester
                                if (!crashingCourse.Elective)
                                {
                                    if (temp[j].Courses.Count(x => x.Semester == semester && !x.Elective) > 0) //base vs base collision, same semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                }
                                #endregion

                                #region base vs faculty +1 -1
                                if (!crashingCourse.Elective) //todo: bu obj1 ??? birşeyler yapak.
                                {
                                    if (semester - 2 >= 0)
                                    {
                                        if (problemObj.Scheduling[semester - 2][day][ hour + j] > 0) //base vs faculty collision, -1 semester.
                                        {
                                            if (UseSemiFit)
                                                semiFittingSlot = true;
                                            else
                                            {
                                                fittingSlot = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (semester < 8)
                                    {
                                        if (problemObj.Scheduling[semester][day][hour + j] > 0) //base vs faculty collision, +1 semester.
                                        {
                                            if (UseSemiFit)
                                                semiFittingSlot = true;
                                            else
                                            {
                                                fittingSlot = false;
                                                break;
                                            }

                                        }
                                    }
                                }
                                #endregion

                                #region base vs base +1 -1
                                if (!crashingCourse.Elective) //todo: bu obj1 ??? birşeyler yapak.
                                {
                                    if (semester - 1 > 0)
                                    {
                                        if (temp[j].Courses.Count(x => x.Semester == semester - 1 && !x.Elective) > 0) //base vs base collision, -1 semester.
                                        {
                                            if (UseSemiFit)
                                                semiFittingSlot = true;
                                            else
                                            {
                                                fittingSlot = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (semester + 1 < 9)
                                    {
                                        if (temp[j].Courses.Count(x => x.Semester == semester + 1 && !x.Elective) > 0) //base vs base faculty collision, +1 semester.
                                        {
                                            if (UseSemiFit)
                                                semiFittingSlot = true;
                                            else
                                            {
                                                fittingSlot = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region LAB base vs faculty 
                                if (crashingCourse.Type == 1) //lab ise
                                {
                                    if (problemObj.LabScheduling[day][hour + j] > 4) //base vs faculty collision, same semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                }
                                #endregion

                                #region Teacher coll
                                if (temp[j].Courses.Count(x => x.TeacherId == crashingCourse.TeacherId) > 0) //check teacher collision
                                {
                                    fittingSlot = false;
                                    break;
                                }
                                #endregion

                                //todo: obj2 og. gor. gunluk 4 saatten fazla pespese dersinin olmamasi

                                //todo: obj2 og. gor. boş gununun olması

                                #region lab lecture collision
                                //todo: obj2 lab ve lecture farklı günlerde olsun
                                //if (crashingCourse.LabHour > 0) //lab'ı var.
                                //{
                                //    if (temp[j].Courses.Count(x => x.Code == crashingCourse.Code && x.Type == 1 ) > 0) //base vs base collision, same semester.
                                //    {
                                //        fittingSlot = false;
                                //        break;
                                //    }
                                //}
                                #endregion

                                #region Elective vs Elective
                                if (crashingCourse.Elective)
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
                                if (crashingCourse.Elective)
                                {
                                    if (problemObj.Scheduling[5][day][ hour + j] > 0) //base vs faculty collision, same semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                    if (problemObj.Scheduling[6][day][hour + j] > 0) //base vs faculty collision, same semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                    if (problemObj.Scheduling[7][day][hour + j] > 0) //base vs faculty collision, same semester.
                                    {
                                        fittingSlot = false;
                                        break;
                                    }
                                }
                                #endregion

                                #region Elective vs Base
                                //elective vs base courses in semester
                                if (crashingCourse.Elective)
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
                                        if (UseSemiFit)
                                            semiFittingSlot = true;
                                        else
                                        {
                                            fittingSlot = false;
                                            break;
                                        }
                                    }
                                }
                                #endregion
                            }
                            #endregion

                            if (fittingSlot)
                            {
                                if (semiFittingSlot)
                                {
                                    semiFittingSlots.Add(i);
                                }
                                else
                                {
                                    fittingSlots.Add(i);
                                    break;
                                }
                            }
                        }

                        if (fittingSlots.Count > 0)
                        {
                            int selectedSlot = fittingSlots[rnd.Next(fittingSlots.Count)];

                            ChangeGene(crashingCourse.Id, selectedSlot, problemObj);
                            SlotId[crashingCourse.Id] = selectedSlot;
                            climbCount++;
                            break;
                        }
                        else if (semiFittingSlots.Count > 0)
                        {
                            //int selectedSlot = semiFittingSlots[rnd.Next(fittingSlots.Count)];

                            //ChangeGene(firstOne.Id, selectedSlot, problemObj);
                            //SlotId[firstOne.Id] = selectedSlot;
                            //break;
                        }
                        else
                        {
                            //no fit
                        }
                    }
                    #endregion

                    if (climbCount >= maxClimb)
                        break;

                }
            }
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
                temp.Add(TimeTable[x][y]);
            }
            else if (hour == 2)
            {
                temp.Add(TimeTable[x][y]);
                temp.Add(TimeTable[x][y + 1]);
            }
            else if (hour == 3)
            {
                temp.Add(TimeTable[x][y]);
                temp.Add(TimeTable[x][y + 1]);
                temp.Add(TimeTable[x][y + 2]);
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
        static void adding_course_timeTable(List<List<Slot>> array, int slotId, Course cor)
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
                array[x][y].Courses.Add(cor);
                array[x][y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    array[x][y].labCount++;
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
                array[x][y].Courses.Add(cor);
                array[x][y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    array[x][y].labCount++;

                y++;
                array[x][y].Courses.Add(cor);
                array[x][y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    array[x][y].labCount++;
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
                array[x][y].Courses.Add(cor);
                array[x][y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    array[x][y].labCount++;
                y++;
                array[x][y].Courses.Add(cor);
                array[x][y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    array[x][y].labCount++;
                y++;
                array[x][y].Courses.Add(cor);
                array[x][y].Teacher[cor.TeacherId]++;
                if (cor.Type == 1)
                    array[x][y].labCount++;
            }
        }



        #region Collisions

        static List<Collision> calculate_collisionBaseLectureWithFaculty(List<List<Slot>> timeTable, List<List<int>> array2, int minimumCollision, int semester, int obj = 0)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i][j];

                    if (tempSlot.Courses.Count(x => x.Semester == semester && !x.Elective) > minimumCollision && array2[i][j] > minimumCollision)
                    {
                        Collision tempCollision = new Collision
                        {
                            Obj = obj,
                            Type = CollisionType.BaseLectureWithFaculty,
                            Result = tempSlot.Courses.Count(x => x.Semester == semester && !x.Elective) + array2[i][j] - 1,
                            Reason = "collision between Base Lecture with Faculty course"
                        };
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.Semester == semester && !x.Elective));

                        collisionList.Add(tempCollision);
                    }
                }
            }
            return collisionList;
        }
        static List<Collision> calculate_collisionInSemester(List<List<Slot>> timeTable, int minimumCollision, int semester, int obj = 0)
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
                            Obj = obj,
                            Type = CollisionType.BaseLectureWithBase,
                            Result = tempSlot.Courses.FindAll(x => x.Semester == semester && !x.Elective).Count - 1,
                            Reason = "base course collision in same semester"
                        };
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.Semester == semester && !x.Elective));

                        collisionList.Add(tempCollision);
                    }
                }
            }
            return collisionList;
        }
        static List<Collision> calculate_collisionInSemesters(List<List<Slot>> timeTable, int minimumCollision, List<int> semesters, int obj = 0)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i][j];

                    List<Course> selectedSemesterCourses;

                    selectedSemesterCourses = tempSlot.Courses.Where(x => semesters.Contains(x.Semester) && !x.Elective).ToList();

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
                            Obj = obj,
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
        static List<Collision> calculate_LabCollision(List<List<Slot>> timeTable, int minimumCollision, int obj = 0)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i][j];

                    if (tempSlot.labCount > minimumCollision)
                    {
                        Collision tempCollision = new Collision
                        {
                            Obj = obj,
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

        static List<Collision> calculate_TeacherCollision(List<List<Slot>> timeTable, int teacherId, int minimumCollision, int obj = 0)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i][j];

                    int hours = tempSlot.Courses.FindAll(x => x.TeacherId == teacherId).Count;
                    if (tempSlot.meetingHour)
                        hours++;

                    if (hours > minimumCollision)
                    {
                        Collision tempCollision = new Collision
                        {
                            Obj = obj,
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
        static int calculate_collisionTeacherFreeDay(List<List<Slot>> timeTable, int teacherId)
        {
            for (int i = 0; i < 5; i++) //gun
            {
                var counter = 0;
                for (int j = 0; j < 9; j++) //dersler
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
                    return 0;
                }

            }

            return 1;
        }
        static int calculate_collisionTeacherConsicutive(List<List<Slot>> timeTable, int teacherId, int maxConsecutiveHour)
        {
            int result = 0;
            for (int i = 0; i < 5; i++)
            {
                var counter = 0;
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i][j];

                    if (tempSlot.Courses.Any(x => x.TeacherId == teacherId) || tempSlot.meetingHour) //dersi veya meetingi varsa ++
                    {
                        counter++;
                    }
                    else {
                        counter = 0; // ara varsa 0'la
                    }
                    if (counter > maxConsecutiveHour)
                    {
                        result++;
                    }
                }
            }

            return result;
        }

        static List<Collision> calculate_LectureLabCollision(List<List<Slot>> timeTable, int obj = 2)
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
                                    Obj = obj,
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

        static List<Collision> calculate_ElectiveCollision(List<List<Slot>> timeTable, int minimumCollision, int obj = 0)
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
                            Obj = obj,
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

        static List<Collision> calculate_collisionElectiveWithBaseCourses(List<List<Slot>> timeTable, List<List<int>> array2, int minimumCollision, int obj = 2)
        {
            List<Collision> collisionList = new List<Collision>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot tempSlot = timeTable[i][j];

                    if (tempSlot.Courses.Count(x => x.Elective) > minimumCollision && array2[i][j] > minimumCollision)
                    {

                        Collision tempCollision = new Collision
                        {
                            Obj = obj,
                            Type = CollisionType.CourseCollision,
                            Result = tempSlot.Courses.Count(x => x.Elective) + array2[i][j] - 1,
                            Reason = "collision with faculty course"
                        };
                        tempCollision.CrashingCourses.AddRange(tempSlot.Courses.FindAll(x => x.Elective));

                        collisionList.Add(tempCollision);
                    }
                }
            }
            return collisionList;
        }

        static List<Collision> calculate_collisionElectiveWithSemester(List<List<Slot>> timeTable, int minimumCollision, int semester, int obj = 0)
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
                                        Obj = obj,
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

        #endregion

    }
}
