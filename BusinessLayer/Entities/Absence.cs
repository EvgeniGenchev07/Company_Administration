using BusinessLayer.Enums;

namespace BusinessLayer.Entities;
    public class Absence
    {
        public int Id { get; set; }
        public AbsenceType Type { get; set; }
        public byte DaysCount { get; set; }
        public DateTime Created { get; set; }
        public AbsenceStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public string UserId { get; set; }

        public string? UserName { get; set; }
        public byte DaysTaken { get; set; }
    }

