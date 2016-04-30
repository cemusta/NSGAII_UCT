namespace NSGAII.Models
{
    public class FacultySection
    {
        public string Code { get; set; }
        public int Section { get; set; }
        public bool Crashing { get; set; }
        public int CrashCount { get; set; }

        public FacultySection()
        {
            Crashing = false;
            CrashCount = 0;
        }
    }
}