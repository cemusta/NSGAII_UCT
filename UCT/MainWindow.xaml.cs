using System;
using System.IO;
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
            _uctproblem = new UCTProblem(0.75, 400, 10000, 3, 0, 43, 0, false);
            ProblemTitle.Content = _uctproblem.ProblemObj.Title;
            EnableGenerationControls();
        }

        private void generationTimer_Tick(object sender, EventArgs e)
        {
            if (_uctproblem.CurrentGeneration == 0)
            {
                _uctproblem.FirstGeneration();
            }
            else if (_uctproblem.CurrentGeneration < _uctproblem.ProblemObj.MaxGeneration)
            {
                _uctproblem.NextGeneration();
            }

            listBox1.Items.Add(_uctproblem.GenerationReport());
            listBox1.Items.Add(_uctproblem.BestReport());
        }

        private void StartPauseGeneration_Click(object sender, RoutedEventArgs e)
        {
            if (generationTimer.IsEnabled)
            {
                StepGeneration.IsEnabled = true;
                StartPauseGeneration.Content = "Start";
                generationTimer.Stop();
            }
            else
            {
                StepGeneration.IsEnabled = false;
                StartPauseGeneration.Content = "Stop";
                generationTimer.Start();
            }
        }

        private void StepGeneration_Click(object sender, RoutedEventArgs e)
        {
            if (!generationTimer.IsEnabled)
            {
                if (_uctproblem.CurrentGeneration == 0)
                {
                    _uctproblem.FirstGeneration();
                }
                else if (_uctproblem.CurrentGeneration < _uctproblem.ProblemObj.MaxGeneration)
                {
                    _uctproblem.NextGeneration();
                }

                listBox1.Items.Add(_uctproblem.GenerationReport());
                listBox1.Items.Add(_uctproblem.BestReport());
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


        }
    }
}
