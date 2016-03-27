using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NSGAII;

namespace UCT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        UCTProblem _uctproblem;
        readonly DispatcherTimer generationTimer;

        public MainWindow()
        {
            InitializeComponent();
            EnableGenerationControls(false);
            generationTimer = new DispatcherTimer();
            generationTimer.Tick += generationTimer_Tick;
            generationTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            UIElement thumb = e.Source as UIElement;

            Canvas.SetLeft(thumb, Canvas.GetLeft(thumb) + e.HorizontalChange);
            Canvas.SetTop(thumb, Canvas.GetTop(thumb) + e.VerticalChange);
        }

        public void EnableGenerationControls(bool state = true)
        {
            StartPauseGeneration.IsEnabled = state;
            StepGeneration.IsEnabled = state;
            PlotNow.IsEnabled = state;
            chkUsePlot.IsEnabled = state;
            ReportBest.IsEnabled = state;
        }

        private void CreateProblem_Click(object sender, RoutedEventArgs e)
        {
            _uctproblem = new UCTProblem(0.75, 200, 10000, 3, 0, 43, 0, false);
            ProblemTitle.Content = _uctproblem.ProblemObj.Title;
            EnableGenerationControls();
            //CreateProblem.IsEnabled = false;
            LogBox.Items.Clear();
        }

        private async void generationTimer_Tick(object sender, EventArgs e)
        {
            await NextGeneration();
        }

        private void StartPauseGeneration_Click(object sender, RoutedEventArgs e)
        {
            if (generationTimer.IsEnabled)
            {
                StepGeneration.IsEnabled = true;
                StartPauseGeneration.Content = "Continue";
                generationTimer.Stop();
            }
            else
            {
                StepGeneration.IsEnabled = false;
                StartPauseGeneration.Content = "Pause";
                generationTimer.Start();
            }
        }

        private async void StepGeneration_Click(object sender, RoutedEventArgs e)
        {
            if (!generationTimer.IsEnabled)
            {
                await NextGeneration();
            }
        }

        public async Task NextGeneration()
        {
            if (_uctproblem.CurrentGeneration == 0)
            {
                _uctproblem.FirstGeneration();
                LogBox.Items.Insert(0, _uctproblem.BestReport());
                LogBox.Items.Insert(0, _uctproblem.GenerationReport());
            }
            else if (_uctproblem.CurrentGeneration <= _uctproblem.ProblemObj.MaxGeneration)
            {
                _uctproblem.NextGeneration();
                LogBox.Items.Insert(0, _uctproblem.BestReport());
                LogBox.Items.Insert(0, _uctproblem.GenerationReport());
            }
            else
            {
                generationTimer.Stop();
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
            if (chkUsePlot.IsChecked ?? false)
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

        private void OpenProblem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".out";
            dlg.Filter = "Problem out (*.out)|*.out";

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;

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

            MainTT.Clear();

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    foreach (var course in bc.TimeTable[i, j].Courses)
                    {
                        MainTT.ControlArray[i, j].Text += course.PrintableName + "\n";
                    }
                }
            }


        }
    }
}
