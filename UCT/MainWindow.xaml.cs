using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using NSGAII;
using NSGAII.Models;

namespace UCT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        UCTProblem _uctproblem;
        readonly DispatcherTimer _generationTimer;
        Dictionary<string, TimeTable> TeacherTables = new Dictionary<string, TimeTable>();

        public MainWindow()
        {
            InitializeComponent();
            RadioHillNone.IsChecked = true;
            EnableGenerationControls(false);
            _generationTimer = new DispatcherTimer();
            _generationTimer.Tick += generationTimer_Tick;
            _generationTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        private void EnableGenerationControls(bool state = true)
        {
            StartPauseGeneration.IsEnabled = state;
            StepGeneration.IsEnabled = state;
            PlotNow.IsEnabled = state;
            ChkUsePlot.IsEnabled = state;
            ReportBest.IsEnabled = state;
            ReportSelected.IsEnabled = state;
            CloseProblem.IsEnabled = state;
            SaveProblem.IsEnabled = state;
            CreateProblem.IsEnabled = !state;
            OpenProblem.IsEnabled = !state;
        }

        private void CreateProblem_Click(object sender, RoutedEventArgs e)
        {
            _generationTimer.Stop();
            StartPauseGeneration.Content = "Start";
            //Random rnd = new Random((int)DateTime.Now.Ticks);

            double seed = 0.75;
            double customSeed;
            if (CustomSeed.Text != "0.75" && double.TryParse(CustomSeed.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out customSeed))
            {
                if (customSeed >= 0 & customSeed <= 1)
                {
                    seed = customSeed;

                }

            }

            int population = 200;
            int customPop;
            if (CustomPopulation.Text != "200" && int.TryParse(CustomPopulation.Text, out customPop))
            {
                if (customPop >= 10 & customPop <= 100000)
                {
                    population = customPop - (customPop % 4);

                }

            }
            int generation = 20000;

            _uctproblem = new UCTProblem(seed, population, generation, 3, 0, true, 0.75, 0.0232558);
            ProblemTitle.Content = _uctproblem.ProblemObj.Title;
            EnableGenerationControls();
            //CreateProblem.IsEnabled = false;
            LogBox.Items.Clear();
            LogBox.Items.Add("Create completed.");

            SetTitleToProblem();

            TeacherTables.Clear();
            TeacherTab.Items.Clear();
            foreach (var teacher in _uctproblem.ProblemObj.TeacherList)
            {
                if (teacher == "ASSISTANT")
                    continue;

                TabItem temp = new TabItem
                {
                    Header = teacher
                };
                TeacherTab.Items.Add(temp);

                TimeTable tempTt = new TimeTable();
                //tempTt.Name = teacher;
                temp.Content = tempTt;
                TeacherTables.Add(teacher, tempTt);
            }
        }

        private void SetTitleToProblem()
        {
            this.Title =
                $"UCT: {_uctproblem.ProblemObj.Title} seed: {_uctproblem.Seed} pop:{_uctproblem.ProblemObj.PopulationSize} {_uctproblem.CurrentGeneration}/{_uctproblem.ProblemObj.MaxGeneration}";
        }

        private void generationTimer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        private void StartPauseGeneration_Click(object sender, RoutedEventArgs e)
        {
            if (_generationTimer.IsEnabled)
            {
                _generationTimer.Stop();
                ListIndividuals();
                StepGeneration.IsEnabled = true;
                CollisionList.IsEnabled = true;
                StartPauseGeneration.Content = "Continue";
            }
            else
            {
                StepGeneration.IsEnabled = false;
                CollisionList.IsEnabled = false;
                StartPauseGeneration.Content = "Pause";
                _generationTimer.Start();
            }
        }

        private void StepGeneration_Click(object sender, RoutedEventArgs e)
        {
            if (!_generationTimer.IsEnabled) //if auto mode off
            {
                NextGeneration();
                ListIndividuals();
            }
        }

        private void NextGeneration()
        {
            if (_uctproblem.CurrentGeneration == 0)
            {
                _uctproblem.FirstGeneration();
                LogBox.Items.Insert(0, _uctproblem.BestReport());
                LogBox.Items.Insert(0, _uctproblem.GenerationReport());
            }
            else if (_uctproblem.CurrentGeneration < _uctproblem.ProblemObj.MaxGeneration)
            {
                UCTProblem.HillClimbMode temp = UCTProblem.HillClimbMode.None;
                if (RadioHillChild.IsChecked ?? false)
                    temp = UCTProblem.HillClimbMode.ChildOnly;
                else if (RadioHillParent.IsChecked ?? false)
                    temp = UCTProblem.HillClimbMode.ParentOnly;
                else if (RadioHillAll.IsChecked ?? false)
                    temp = UCTProblem.HillClimbMode.All;
                else if (RadioHillBest.IsChecked ?? false)
                    temp = UCTProblem.HillClimbMode.BestOfParent;
                else if (RadioHillAllBest.IsChecked ?? false)
                    temp = UCTProblem.HillClimbMode.AllBestOfParent;
                else if (RadioAdaptiveParent.IsChecked ?? false)
                    temp = UCTProblem.HillClimbMode.AdaptiveParent;
                else if (RadioRankBest.IsChecked ?? false)
                    temp = UCTProblem.HillClimbMode.Rank1Best;
                else if (RadioRankAll.IsChecked ?? false)
                    temp = UCTProblem.HillClimbMode.Rank1All;
                else if (RadioAdaptiveRankAll.IsChecked ?? false)
                    temp = UCTProblem.HillClimbMode.AdaptiveRank1All;

                _uctproblem.NextGeneration(temp);
                LogBox.Items.Insert(0, _uctproblem.BestReport());
                LogBox.Items.Insert(0, _uctproblem.GenerationReport());
            }
            else
            {
                _generationTimer.Stop();
                StartPauseGeneration.Content = "Start";
                LogBox.Items.Insert(0, "Problem reached upper generation limit.");
                _uctproblem.WriteBestGeneration();
                _uctproblem.WriteFinalGeneration();
            }

            SetTitleToProblem();
        }

        private void PlotNow_Click(object sender, RoutedEventArgs e)
        {
            _uctproblem.PlotNow();
        }

        private void chkUsePlot_Click(object sender, RoutedEventArgs e)
        {
            if (ChkUsePlot.IsChecked ?? false)
            {
                PlotNow.IsEnabled = false;
                _uctproblem.UsePlot = true;
            }
            else
            {
                PlotNow.IsEnabled = true;
                _uctproblem.UsePlot = false;
            }
        }

        private void ReportBest_Click(object sender, RoutedEventArgs e)
        {
            if (_uctproblem == null || _uctproblem.CurrentGeneration == 0)
                return;

            double minimumResult = _uctproblem.ParentPopulation.IndList.Min(x => x.TotalResult);
            var result = minimumResult;
            var bc = _uctproblem.ParentPopulation.IndList.First(x => x.TotalResult == result);
            bc.Decode(_uctproblem.ProblemObj);
            bc.Evaluate(_uctproblem.ProblemObj);

            ReportIndividual(bc);

            MainTab.SelectedIndex = 1;
        }

        private void ReportSelected_Click(object sender, RoutedEventArgs e)
        {
            if (_uctproblem == null || _uctproblem.CurrentGeneration == 0 || IndBox.Items.Count < 0)
                return;

            if (IndBox.SelectedItems.Count <= 0)
                return;

            int selectedIndex = IndBox.SelectedIndex;

            var individual = _uctproblem.ParentPopulation.IndList[selectedIndex];
            individual.Decode(_uctproblem.ProblemObj);
            individual.Evaluate(_uctproblem.ProblemObj);

            ReportIndividual(individual);

            MainTab.SelectedIndex = 1;
        }

        private void ReportIndividual(Individual ind)
        {
            #region Initialize
            var timeTables = ClearTables();

            #endregion

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Slot temp = ind.Timetable[i][j];
                    TextBlock tempTextBlock, tempTextBlockForSemester;

                    var collix = ind.CollisionList.Where(x => x.SlotId == temp.Id);
                    List<int> crashingCourses = new List<int>();
                    foreach (var collision in collix)
                    {
                        crashingCourses.AddRange(collision.CrashingCourses.Select(x => x.Id));
                    }

                    foreach (var course in temp.Courses)
                    {
                        tempTextBlock = new TextBlock { Text = course.PrintableName };
                        tempTextBlockForSemester = new TextBlock { Text = course.PrintableName };
                        if (crashingCourses.Contains(course.Id))
                        {
                            tempTextBlock.Background = Brushes.PaleVioletRed;
                            tempTextBlockForSemester.Background = Brushes.PaleVioletRed;
                        }

                        MainTimetable.ControlArray[i, j].Children.Add(tempTextBlock);

                        if (course.Semester > 0 && course.Semester < 9)
                            timeTables[course.Semester - 1].ControlArray[i, j].Children.Add(tempTextBlockForSemester);

                        if (course.Type == 1)
                        {
                            var labTextBlock = new TextBlock { Text = course.PrintableName };
                            LabTimetable.ControlArray[i, j].Children.Add(labTextBlock);
                        }
                    }

                    foreach (var course in temp.facultyCourses)
                    {
                        tempTextBlock = new TextBlock
                        {
                            Text = course.PrintableName,
                            Foreground = Brushes.CadetBlue
                        };
                        tempTextBlockForSemester = new TextBlock
                        {
                            Text = course.PrintableName,
                            Foreground = Brushes.CadetBlue
                        };


                        MainTimetable.ControlArray[i, j].Children.Add(tempTextBlock);

                        if (course.Semester > 0 && course.Semester < 9)
                            timeTables[course.Semester - 1].ControlArray[i, j].Children.Add(tempTextBlockForSemester);

                    }


                    if (temp.facultyLab > 0)
                    {
                        tempTextBlock = new TextBlock { Text = $"Faculty Lab (count:{temp.facultyLab})" };
                        MainTimetable.ControlArray[i, j].Children.Add(tempTextBlock);

                        for (int k = 0; k < temp.facultyLab; k++)
                        {
                            var labTextBlock = new TextBlock
                            {
                                Text = $"Faculty Lab",
                                Foreground = Brushes.CadetBlue
                            };
                            LabTimetable.ControlArray[i, j].Children.Add(labTextBlock);
                        }
                    }

                    if (LabTimetable.ControlArray[i, j].Children.Count > 4)
                    {
                        LabTimetable.ControlArray[i, j].Background = Brushes.PaleVioletRed;
                    }

                    if (temp.meetingHour)
                    {
                        tempTextBlock = new TextBlock
                        {
                            FontStyle = FontStyles.Italic,
                            Text = $"Meeting"
                        };

                        MainTimetable.ControlArray[i, j].Children.Add(tempTextBlock);
                        for (int n = 0; n < 8; n++)
                        {
                            tempTextBlockForSemester = new TextBlock
                            {
                                FontStyle = FontStyles.Italic,
                                Text = $"Meeting"
                            };
                            timeTables[n].ControlArray[i, j].Children.Add(tempTextBlockForSemester);
                        }
                    }

                    foreach (var coll in ind.CollisionList)
                    {
                        if (temp.Id == coll.SlotId)
                        {
                            string collisionRep = $"obj:{coll.Obj} {coll.Reason}";
                            int count = 0;
                            if (coll.CrashingCourses.Count > 0)
                                collisionRep += " : ";
                            foreach (var cc in coll.CrashingCourses)
                            {
                                count++;
                                collisionRep += cc.Code;
                                if (count != coll.CrashingCourses.Count)
                                    collisionRep += "|";
                            }

                            MainTimetable.ControlArray[i, j].ToolTip += collisionRep + "\n";
                        }
                    }

                    foreach (var teacher in _uctproblem.ProblemObj.TeacherList)
                    {
                        if (teacher == "ASSISTANT")
                            continue;

                        if (temp.Courses.Any(x => x.Teacher == teacher))
                        {
                            foreach (var course in temp.Courses.Where(x => x.Teacher == teacher))
                            {
                                tempTextBlock = new TextBlock { Text = course.PrintableName };
                                tempTextBlockForSemester = new TextBlock { Text = course.PrintableName };
                                if (crashingCourses.Contains(course.Id))
                                {
                                    tempTextBlock.Background = Brushes.PaleVioletRed;
                                    tempTextBlockForSemester.Background = Brushes.PaleVioletRed;
                                }

                                TeacherTables[teacher].ControlArray[i, j].Children.Add(tempTextBlockForSemester);


                            }

                        }

                    }
                } //hour
            } //day

            int tid = 0;
            foreach (var teacher in _uctproblem.ProblemObj.TeacherList)
            {
                bool hasTeacherCollision = false;
                string teacherColl = "";
                foreach (var coll in ind.CollisionList)
                {
                    if (coll.TeacherId == tid)
                    {
                        teacherColl += $"obj:{coll.Obj} {coll.Reason} \n";
                        hasTeacherCollision = true;
                    }
                }

                TeacherPanel.Children.Add(new TextBlock
                {
                    Background = hasTeacherCollision ? Brushes.PaleVioletRed : null,
                    ToolTip = hasTeacherCollision ? teacherColl : null,
                    Text = $"{tid}: {teacher}",
                    Margin = new Thickness(5)
                });
                tid++;
            }

            CollisionTab.Header = $"Coll:{ind.CollisionList.Count} T:{ind.TotalResult}";
            var collindex = 1;
            foreach (var collision in ind.CollisionList)
            {
                CollisionList.Items.Add(collindex + " " + collision.Printable);
                collindex++;
            }


        }

        private List<TimeTable> ClearTables()
        {
            List<TimeTable> timeTables = new List<TimeTable>
            {
                S1Timetable,
                S2Timetable,
                S3Timetable,
                S4Timetable,
                S5Timetable,
                S6Timetable,
                S7Timetable,
                S8Timetable
            };

            //clear tables.
            MainTimetable.Clear();
            foreach (TimeTable t in timeTables)
            {
                t.Clear();
            }
            LabTimetable.Clear();
            TeacherPanel.Children.Clear();
            CollisionTab.Header = "Coll";
            CollisionList.Items.Clear();

            foreach (var teacher in _uctproblem.ProblemObj.TeacherList)
            {
                if (teacher != "ASSISTANT")
                    TeacherTables[teacher].Clear();
            }
            return timeTables;
        }

        private void ListIndividuals()
        {
            IndBox.Items.Clear();
            foreach (var individual in _uctproblem.ParentPopulation.IndList)
            {
                IndBox.Items.Add(individual.ToString());
                if (individual.Obj[0] == 0)
                {
                    //Renk ver.
                }
            }
        }

        private void LoadProblem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".out";
            dlg.Filter = "Problem save (*.problem)|*.problem";

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                _generationTimer.Stop();
                StartPauseGeneration.Content = "Start";
                string filename = dlg.FileName;
                _uctproblem = UCTProblem.LoadFromFile(filename);
                if (_uctproblem != null)
                {
                    ProblemTitle.Content = _uctproblem.ProblemObj.Title;
                    EnableGenerationControls();
                    //CreateProblem.IsEnabled = false;
                    LogBox.Items.Clear();
                    ListIndividuals();
                    LogBox.Items.Add("Load completed.");
                    SetTitleToProblem();
                }

            }

        }

        private void SaveProblem_Click(object sender, RoutedEventArgs e)
        {
            LogBox.Items.Insert(0, "Save completed.");
            UCTProblem.SaveToFile(_uctproblem, _uctproblem.ProblemObj.Title);
        }

        private void CloseProblem_Click(object sender, RoutedEventArgs e)
        {
            _generationTimer.Stop();
            StartPauseGeneration.Content = "Start";
            _uctproblem = null;
            ProblemTitle.Content = "";
            EnableGenerationControls(false);
            LogBox.Items.Clear();
            IndBox.Items.Clear();
            TeacherTab.Items.Clear();
            ClearTables();
            this.Title = "UCT";
        }

        private void ResetGenerationNumber_Click(object sender, RoutedEventArgs e)
        {
            if (_uctproblem == null)
                return;

            LogBox.Items.Insert(0, "Generation Number has been reset.");
            _uctproblem.CurrentGeneration = 1; //can't set to 0, it will reset population.
            SetTitleToProblem();
        }

        private void HillClimbButton_Click(object sender, RoutedEventArgs e)
        {
            if (_uctproblem == null)
                return;

            LogBox.Items.Insert(0, "HillClimbing Parent");
            _uctproblem.HillClimbParent();
            LogBox.Items.Insert(0, _uctproblem.BestReport());
            LogBox.Items.Insert(0, _uctproblem.GenerationReport());
        }

        private void HillClimbBest_Click(object sender, RoutedEventArgs e)
        {
            if (_uctproblem == null)
                return;

            LogBox.Items.Insert(0, "HillClimbing Best");
            _uctproblem.HillClimbBest();
            LogBox.Items.Insert(0, _uctproblem.BestReport());
            LogBox.Items.Insert(0, _uctproblem.GenerationReport());
        }

        private void CustomPopulation_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

    }
}
