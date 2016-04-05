using System.Windows.Controls;


namespace UCT
{
    /// <summary>
    /// Interaction logic for timetable.xaml
    /// </summary>
    public partial class TimeTable : UserControl
    {
        public WrapPanel[,] ControlArray = new WrapPanel[5, 10];

        public TimeTable()
        {
            InitializeComponent();

            ControlArray[0, 0] = Slot1 ;
            ControlArray[0, 1] = Slot2 ;
            ControlArray[0, 2] = Slot3 ;
            ControlArray[0, 3] = Slot4 ;
            ControlArray[0, 4] = Slot5 ;
            ControlArray[0, 5] = Slot6 ;
            ControlArray[0, 6] = Slot7 ;
            ControlArray[0, 7] = Slot8 ;
            ControlArray[0, 8] = Slot9 ;
            ControlArray[0, 9] = Slot10 ;

            ControlArray[1, 0] = Slot11;
            ControlArray[1, 1] = Slot12;
            ControlArray[1, 2] = Slot13;
            ControlArray[1, 3] = Slot14;
            ControlArray[1, 4] = Slot15;
            ControlArray[1, 5] = Slot16;
            ControlArray[1, 6] = Slot17;
            ControlArray[1, 7] = Slot18;
            ControlArray[1, 8] = Slot19;
            ControlArray[1, 9] = Slot20;

            ControlArray[2, 0] = Slot21;
            ControlArray[2, 1] = Slot22;
            ControlArray[2, 2] = Slot23;
            ControlArray[2, 3] = Slot24;
            ControlArray[2, 4] = Slot25;
            ControlArray[2, 5] = Slot26;
            ControlArray[2, 6] = Slot27;
            ControlArray[2, 7] = Slot28;
            ControlArray[2, 8] = Slot29;
            ControlArray[2, 9] = Slot30;

            ControlArray[3, 0] = Slot31;
            ControlArray[3, 1] = Slot32;
            ControlArray[3, 2] = Slot33;
            ControlArray[3, 3] = Slot34;
            ControlArray[3, 4] = Slot35;
            ControlArray[3, 5] = Slot36;
            ControlArray[3, 6] = Slot37;
            ControlArray[3, 7] = Slot38;
            ControlArray[3, 8] = Slot39;
            ControlArray[3, 9] = Slot40;

            ControlArray[4, 0] = Slot41;
            ControlArray[4, 1] = Slot42;
            ControlArray[4, 2] = Slot43;
            ControlArray[4, 3] = Slot44;
            ControlArray[4, 4] = Slot45;
            ControlArray[4, 5] = Slot46;
            ControlArray[4, 6] = Slot47;
            ControlArray[4, 7] = Slot48;
            ControlArray[4, 8] = Slot49;
            ControlArray[4, 9] = Slot50;
        }

        public void Clear()
        {
            foreach (var control in ControlArray)
            {
                control.Background = null;
                control.Children.Clear();
                control.ToolTip = null;
            }
        }
    }
}
