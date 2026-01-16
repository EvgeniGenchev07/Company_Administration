using App.PageModels;
using App.Pages;
using App.Services;
using CommunityToolkit.Maui;
using DataLayer;
using DataLayer.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddTransientWithShellRoute<RequestPage, RequestPageModel>("request");
            builder.Services.AddTransientWithShellRoute<RegisterPage, RegisterPageModel>("register");
            builder.Services.AddTransientWithShellRoute<MainPage, MainPageModel>("MainPage");
            builder.Services.AddTransientWithShellRoute<AdminPage, AdminPageModel>("AdminPage");
            builder.Services.AddTransientWithShellRoute<AdminUsersPage, AdminUsersPageModel>("AdminUsersPage");
            builder.Services.AddTransientWithShellRoute<AddUserPage, AddUserPageModel>("AddUserPage");
            builder.Services.AddTransientWithShellRoute<EditUserPage, EditUserPageModel>("EditUserPage");
            builder.Services.AddTransientWithShellRoute<AdminAllAbsencesPage, AdminAllAbsencesPageModel>("AdminAllAbsencesPage");
            builder.Services.AddTransientWithShellRoute<AdminAllBusinessTripsPage, AdminAllBusinessTripsPageModel>("AdminAllBusinessTripsPage");
            builder.Services.AddTransientWithShellRoute<AbsencePage, AbsencePageModel>("AbsencePage");
            builder.Services.AddTransientWithShellRoute<AllAbsencesPage, AllAbsencesPageModel>("AllAbsencesPage");
            builder.Services.AddTransientWithShellRoute<AbsenceDetailsPage, AbsenceDetailsPageModel>("AbsenceDetailsPage");
            builder.Services.AddTransientWithShellRoute<BusinessTripsPage, BusinessTripsPageModel>("businesstrips");
            builder.Services.AddTransientWithShellRoute<BusinessTripsSummaryPage, BusinessTripsSummaryPageModel>("BusinessTripsSummaryPage");
            builder.Services.AddTransientWithShellRoute<BusinessTripDetailsPage, BusinessTripDetailsPageModel>("businesstripdetails");
            builder.Services.AddScoped<DatabaseService>();
            builder.Services.AddScoped<BusinessTripContext>();
            builder.Services.AddScoped<AbsenceContext>();
            builder.Services.AddScoped<UserContext>();
            builder.Services.AddScoped<HolidayDayContext>();
            builder.Services.AddScoped<AuthenticationContext>();
            const string connectionString = "sfsfdf";
            builder.Services.AddScoped(_=>
            {
                var connection = new MySqlConnection(connectionString);
                return new EapDbContext(connection);
            });
            return builder.Build();
        }
    }
}
