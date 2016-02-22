using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UCT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Globals.CourseDetail> CourseList;
        public List<string> TearcherList;

        public MainWindow()
        {
            CourseList = new List<Globals.CourseDetail>();
            TearcherList = new List<string>();
            InitializeComponent();
            ReadValues();
        }

        private void ReadValues()
        {
            CourseList.Clear();
            TearcherList.Clear();
            sp.Children.Clear();

            try  //todo better input handling
            {
                FileStream courseListFilestream = File.OpenRead("course_list.csv");
                StreamReader reader = new StreamReader(courseListFilestream);

                string line = reader.ReadLine();
                int course_count = int.Parse(line); //43 gibi bir sayı dönüyor

                Console.WriteLine($"SIZE: {course_count} \n");

                for (int course_ID = 0; course_ID < course_count; course_ID++)
                {
                    line = reader.ReadLine();
                    //Console.WriteLine($"{line}\n");

                    var parts = line.Split(new char[] { ';' });
                    // token = strtok(record, ";");

                    for (int i = 0; i < parts.Length; i++)
                    {
                        Console.WriteLine($"{i}.{parts[i]}");
                    }
                    Console.WriteLine();

                    CourseList.Add(new Globals.CourseDetail(parts[0], parts[1], int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]), int.Parse(parts[6])));

                    if (TearcherList.Contains(parts[1]))
                    {
                        TearcherList.Add(parts[1]);
                    }

                }

                for (int i = 0; i < CourseList.Count; i++)
                {
                    Label newLabel = new Label();

                    newLabel.Content = $"{CourseList[i].PrintableName}";
                    newLabel.Name = "CourseL" + i.ToString();
                    newLabel.Margin = new Thickness(2, 3, 2, 3);
                    newLabel.BorderThickness = new Thickness(1, 1, 1, 1);
                    newLabel.BorderBrush = Brushes.Black;

                    sp.Children.Add(newLabel);
                }

            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Input file missing.\n EX: {ex.FileName}");
                return;
            }
        }

        private void ReadX_Click(object sender, RoutedEventArgs e)
        {
            ReadValues();
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            UIElement thumb = e.Source as UIElement;

            Canvas.SetLeft(thumb, Canvas.GetLeft(thumb) + e.HorizontalChange);
            Canvas.SetTop(thumb, Canvas.GetTop(thumb) + e.VerticalChange);
        }
    }
}
