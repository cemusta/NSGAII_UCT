using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
            CloseProblem.IsEnabled = state;
            SaveProblem.IsEnabled = state;
            CreateProblem.IsEnabled = !state;
            OpenProblem.IsEnabled = !state;
        }

        private void CreateProblem_Click(object sender, RoutedEventArgs e)
        {
            _generationTimer.Stop();
            StartPauseGeneration.Content = "Start";
            Random rnd = new Random((int)DateTime.Now.Ticks);

            double seed = 0.75;
            int population = 200;
            int generation = 20000;
            this.Title = $"UCT - seed: {seed}";
            _uctproblem = new UCTProblem(seed, population, generation, 3, 0, true, 0.75, 0.0232558, false);
            ProblemTitle.Content = _uctproblem.ProblemObj.Title;
            EnableGenerationControls();
            //CreateProblem.IsEnabled = false;
            LogBox.Items.Clear();
            LogBox.Items.Add("Create completed.");
        }

        private void generationTimer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        private void StartPauseGeneration_Click(object sender, RoutedEventArgs e)
        {
            if (_generationTimer.IsEnabled)
            {
                StepGeneration.IsEnabled = true;
                StartPauseGeneration.Content = "Continue";
                _generationTimer.Stop();
            }
            else
            {
                StepGeneration.IsEnabled = false;
                StartPauseGeneration.Content = "Pause";
                _generationTimer.Start();
            }
        }

        private void StepGeneration_Click(object sender, RoutedEventArgs e)
        {
            if (!_generationTimer.IsEnabled)
            {
                NextGeneration();
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
            if (_uctproblem.CurrentGeneration == 0)
                return;

            MainTab.SelectedIndex = 1;

            int minimumResult = _uctproblem.ParentPopulation.IndList.Min(x => x.TotalResult);
            var result = minimumResult;
            var bc = _uctproblem.ParentPopulation.IndList.First(x => x.TotalResult == result);
            bc.Decode(_uctproblem.ProblemObj);
            bc.Evaluate(_uctproblem.ProblemObj);

            List<TimeTable> TTList = new List<TimeTable> { S1TT, S2TT, S3TT, S4TT, S5TT, S6TT, S7TT, S8TT };


            MainTT.Clear();
            for (int i = 0; i < 8; i++)
            {
                TTList[i].Clear();
            }

            Sp.Children.Clear();

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot temp = bc.Timetable[i][j];
                    TextBlock tempTextBlock, tempTextBlockForSemester;

                    var collix = bc.CollisionList.Where(x => x.SlotId == temp.Id);
                    List<int> tempClist = new List<int>();
                    foreach (var collision in collix)
                    {
                        tempClist.AddRange(collision.CrashingCourses.Select(x => x.Id));
                    }

                    foreach (var course in temp.Courses)
                    {
                        tempTextBlock = new TextBlock { Text = course.PrintableName };
                        tempTextBlockForSemester = new TextBlock { Text = course.PrintableName };
                        if (tempClist.Contains(course.Id))
                        {
                            tempTextBlock.Background = Brushes.PaleVioletRed;
                            tempTextBlockForSemester.Background = Brushes.PaleVioletRed;
                        }

                        MainTT.ControlArray[i, j].Children.Add(tempTextBlock);

                        if (course.Semester > 0 && course.Semester < 9)
                            TTList[course.Semester - 1].ControlArray[i, j].Children.Add(tempTextBlockForSemester);

                    }
                    for (int k = 0; k < 8; k++)
                    {
                        if (temp.facultyCourse[k] > 0)
                        {
                            tempTextBlock = new TextBlock { Text = $"Faculty Course (sem:{k + 1})" };
                            tempTextBlockForSemester = new TextBlock { Text = $"Faculty Course (sem:{k + 1})" };
                            MainTT.ControlArray[i, j].Children.Add(tempTextBlock);

                            if (k + 1 == 1)
                                S1TT.ControlArray[i, j].Children.Add(tempTextBlockForSemester);
                            else if (k + 1 == 2)
                                S2TT.ControlArray[i, j].Children.Add(tempTextBlockForSemester);
                            else if (k + 1 == 3)
                                S3TT.ControlArray[i, j].Children.Add(tempTextBlockForSemester);
                            else if (k + 1 == 4)
                                S4TT.ControlArray[i, j].Children.Add(tempTextBlockForSemester);
                            else if (k + 1 == 5)
                                S5TT.ControlArray[i, j].Children.Add(tempTextBlockForSemester);
                            else if (k + 1 == 6)
                                S6TT.ControlArray[i, j].Children.Add(tempTextBlockForSemester);
                            else if (k + 1 == 7)
                                S7TT.ControlArray[i, j].Children.Add(tempTextBlockForSemester);
                            else if (k + 1 == 8)
                                S8TT.ControlArray[i, j].Children.Add(tempTextBlockForSemester);


                        }
                    }

                    if (temp.facultyLab > 0)
                    {
                        tempTextBlock = new TextBlock { Text = $"Faculty Lab (count:{temp.facultyLab})" };
                        MainTT.ControlArray[i, j].Children.Add(tempTextBlock);
                    }

                    if (temp.meetingHour)
                    {
                        tempTextBlock = new TextBlock
                        {
                            FontStyle = FontStyles.Italic,
                            Text = $"Meeting"
                        };

                        MainTT.ControlArray[i, j].Children.Add(tempTextBlock);
                        for (int n = 0; n < 8; n++)
                        {
                            tempTextBlockForSemester = new TextBlock
                            {
                                FontStyle = FontStyles.Italic,
                                Text = $"Meeting"
                            };
                            TTList[n].ControlArray[i, j].Children.Add(tempTextBlockForSemester);
                        }
                    }

                    foreach (var coll in bc.CollisionList)
                    {
                        if (temp.Id == coll.SlotId)
                        {
                            string collisionRep = $"obj:{coll.Obj} {coll.Reason} :";
                            int count = 0;
                            foreach (var cc in coll.CrashingCourses)
                            {
                                count++;
                                collisionRep += cc.Code;
                                if (count != coll.CrashingCourses.Count)
                                    collisionRep += "|";
                            }

                            // MainTT.ControlArray[i, j].Background = Brushes.PaleVioletRed;
                            MainTT.ControlArray[i, j].ToolTip += collisionRep + "\n";
                        }
                    }
                }
            }

            int tid = 0;
            foreach (var teacher in _uctproblem.ProblemObj.TeacherList)
            {
                bool hasTeacherCollision = false;
                string teacherColl = "";
                foreach (var coll in bc.CollisionList)
                {
                    if (coll.TeacherId == tid)
                    {
                        teacherColl += $"obj:{coll.Obj} {coll.Reason} :";
                        hasTeacherCollision = true;
                    }
                }

                Sp.Children.Add(new TextBlock
                {
                    Background = hasTeacherCollision ? Brushes.PaleVioletRed : null,
                    ToolTip = hasTeacherCollision ? teacherColl : null,
                    Text = $"{tid}: {teacher}",
                    Margin = new Thickness(5)
                });
                tid++;
            }



        }

        private void OpenProblem_Click(object sender, RoutedEventArgs e)
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
                    LogBox.Items.Add("Load completed.");
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

        private void ResetGenerationNumber_Click(object sender, RoutedEventArgs e)
        {
            if (_uctproblem == null)
                return;

            LogBox.Items.Insert(0, "Generation Number has been reset.");
            _uctproblem.CurrentGeneration = 1; //0 yaparsam ilk jenerasyonu yapıyor.
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
    }
}
