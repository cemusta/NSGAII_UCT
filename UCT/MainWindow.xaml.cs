using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
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
            _uctproblem = new UCTProblem(0.75, 200, 500, 3, 0, 43, 0, false);
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

            MainTt.Clear();
            Sp.Children.Clear();

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Slot temp = bc.TimeTable[i][j];
                    foreach (var course in temp.Courses)
                    {
                        MainTt.ControlArray[i, j].Text += course.PrintableName + "\n";
                    }
                    for (int k = 0; k < 8; k++)
                    {
                        if (temp.facultyCourse[k] > 0)
                        {
                            MainTt.ControlArray[i, j].Text += $"Faculty Course (sem:{k + 1})\n";
                        }
                    }

                    if (temp.facultyLab > 0)
                    {
                        MainTt.ControlArray[i, j].Text += $"Faculty Lab (count:{temp.facultyLab})\n";
                    }

                    if (temp.meetingHour)
                    {
                        MainTt.ControlArray[i, j].Text += $"Meeting\n";
                    }

                    foreach (var coll in bc.CollisionList)
                    {
                        if (temp.Id == coll.SlotId)
                        {
                            MainTt.ControlArray[i, j].Background = Brushes.PaleVioletRed;
                            string collisionRep = $"obj:{coll.Obj} {coll.Reason} :";
                            int count = 0;
                            foreach (var cc in coll.CrashingCourses)
                            {
                                count++;
                                collisionRep += cc.Code;
                                if (count != coll.CrashingCourses.Count)
                                    collisionRep += "|";
                            }

                            MainTt.ControlArray[i, j].ToolTip += collisionRep + "\n";
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
                ProblemTitle.Content = _uctproblem.ProblemObj.Title;
                EnableGenerationControls();
                //CreateProblem.IsEnabled = false;
                LogBox.Items.Clear();
                LogBox.Items.Add("Load completed.");
            }

        }

        private void SaveProblem_Click(object sender, RoutedEventArgs e)
        {
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
