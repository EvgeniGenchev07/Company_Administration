using BusinessLayer.Entities;
using DataLayer.Repositories;

namespace TestingLayer;

    [TestFixture]
    public class HolidayDayContextTest
    {
        private HolidayDayContext holidayContext;

        [SetUp]
        public void Setup()
        {
            holidayContext = new HolidayDayContext(TestManager.DbContext);
        }

        [Test]
        public async Task CreateHoliday()
        {
            HolidayDay holiday = new HolidayDay
            {
                Name = "Test Holiday",
                Date = new DateTime(2026, 1, 10),
                IsCustom = true
            };

            bool result = await holidayContext.Create(holiday);

            Assert.That(result, Is.True, "Holiday was not created.");
        }

        [Test]
        public async Task GetById()
        {
            HolidayDay holiday = new HolidayDay
            {
                Name = "GetById Holiday",
                Date = new DateTime(2026, 2, 1),
                IsCustom = true
            };

            await holidayContext.Create(holiday);

            var all = await holidayContext.GetAll();
            var last = all.First(h => h.Name == "GetById Holiday");

            HolidayDay fromDb = await holidayContext.GetById(last.Id);

            Assert.That(fromDb.Name, Is.EqualTo("GetById Holiday"));
        }

        [Test]
        public async Task UpdateHoliday()
        {
            HolidayDay holiday = new HolidayDay
            {
                Name = "Update Holiday",
                Date = new DateTime(2026, 3, 1),
                IsCustom = true
            };

            await holidayContext.Create(holiday);

            var all = await holidayContext.GetAll();
            var toUpdate = all.First(h => h.Name == "Update Holiday");

            toUpdate.Name = "Updated Holiday";
            toUpdate.IsCustom = false;

            bool updated = await holidayContext.Update(toUpdate);
            HolidayDay updatedFromDb = await holidayContext.GetById(toUpdate.Id);

            Assert.That(updated && updatedFromDb.Name == "Updated Holiday");
        }

        [Test]
        public async Task DeleteHoliday()
        {
            HolidayDay holiday = new HolidayDay
            {
                Name = "Delete Holiday",
                Date = new DateTime(2026, 4, 1),
                IsCustom = true
            };

            await holidayContext.Create(holiday);

            var all = await holidayContext.GetAll();
            var toDelete = all.First(h => h.Name == "Delete Holiday");

            bool deleted = await holidayContext.Delete(toDelete.Id);

            Assert.That(deleted, Is.True);
        }

        [Test]
        public async Task GetAll()
        {
            var holidays = await holidayContext.GetAll();

            Assert.That(holidays, Is.Not.Null);
            Assert.That(holidays.Count, Is.GreaterThan(0));
        }
    }
