using System.Windows.Controls;


namespace UCT
{
    /// <summary>
    /// Interaction logic for timetable.xaml
    /// </summary>
    public partial class TimeTable : UserControl
    {
        public TextBlock[,] ControlArray = new TextBlock[5, 10];

        public TimeTable()
        {
            InitializeComponent();

            ControlArray[0, 0] = slot1 ;
            ControlArray[0, 1] = slot2 ;
            ControlArray[0, 2] = slot3 ;
            ControlArray[0, 3] = slot4 ;
            ControlArray[0, 4] = slot5 ;
            ControlArray[0, 5] = slot6 ;
            ControlArray[0, 6] = slot7 ;
            ControlArray[0, 7] = slot8 ;
            ControlArray[0, 8] = slot9 ;
            ControlArray[0, 9] = slot10 ;

            ControlArray[1, 0] = slot11;
            ControlArray[1, 1] = slot12;
            ControlArray[1, 2] = slot13;
            ControlArray[1, 3] = slot14;
            ControlArray[1, 4] = slot15;
            ControlArray[1, 5] = slot16;
            ControlArray[1, 6] = slot17;
            ControlArray[1, 7] = slot18;
            ControlArray[1, 8] = slot19;
            ControlArray[1, 9] = slot20;

            ControlArray[2, 0] = slot21;
            ControlArray[2, 1] = slot22;
            ControlArray[2, 2] = slot23;
            ControlArray[2, 3] = slot24;
            ControlArray[2, 4] = slot25;
            ControlArray[2, 5] = slot26;
            ControlArray[2, 6] = slot27;
            ControlArray[2, 7] = slot28;
            ControlArray[2, 8] = slot29;
            ControlArray[2, 9] = slot30;

            ControlArray[3, 0] = slot31;
            ControlArray[3, 1] = slot32;
            ControlArray[3, 2] = slot33;
            ControlArray[3, 3] = slot34;
            ControlArray[3, 4] = slot35;
            ControlArray[3, 5] = slot36;
            ControlArray[3, 6] = slot37;
            ControlArray[3, 7] = slot38;
            ControlArray[3, 8] = slot39;
            ControlArray[3, 9] = slot40;

            ControlArray[4, 0] = slot41;
            ControlArray[4, 1] = slot42;
            ControlArray[4, 2] = slot43;
            ControlArray[4, 3] = slot44;
            ControlArray[4, 4] = slot45;
            ControlArray[4, 5] = slot46;
            ControlArray[4, 6] = slot47;
            ControlArray[4, 7] = slot48;
            ControlArray[4, 8] = slot49;
            ControlArray[4, 9] = slot50;
        }

        public void Clear()
        {
            foreach (var control in ControlArray)
            {
                control.Background = null;
                control.Text = "";
                control.ToolTip = null;
            }
        }
    }
}
