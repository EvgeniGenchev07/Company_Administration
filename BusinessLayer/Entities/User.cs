using BusinessLayer.Enums;

namespace BusinessLayer.Entities;

    public class User
    {
        public string? Id { get; set; }
        public int ContractDays { get; set; }
        public int AbsenceDays { get; set; }
        public string? Name { get; set; }
        public Role Role { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }

        public List<Absence>? Absences { get; set; }
        public List<BusinessTrip>? BusinessTrips { get; set; }
    }
