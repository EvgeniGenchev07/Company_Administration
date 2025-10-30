namespace BusinessLayer.Entities;

    public class HolidayDay
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsCustom { get; set; }
        public DateTime Date { get; set; }
    }
