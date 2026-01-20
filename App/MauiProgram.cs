using App.Pages;
using ApplicationLayer.Interfaces;
using CommunityToolkit.Maui;
using DataLayer.Interfaces.Repository;
using DataLayer.Persistence;
using DataLayer.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using ServiceLayer.PageModels;
using ServiceLayer.Services;

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
            builder.Services.AddTransientWithShellRoute<AddProjectPage,AddProjectPageModel>("AddProjectPage");
            builder.Services.AddScoped<IDatabaseService,DatabaseService>();
            builder.Services.AddScoped<IAuthenticationService,AuthenticationService>();
            builder.Services.AddScoped<IBusinessTripContext, BusinessTripContext>();
            builder.Services.AddScoped<IAbsenceContext,AbsenceContext>();
            builder.Services.AddScoped<IUserContext,UserContext>();
            builder.Services.AddScoped<IHolidayDayContext,HolidayDayContext>();
            builder.Services.AddScoped<IProjectContext, ProjectContext>();
            const string connectionString = "new_connection";
            builder.Services.AddScoped(_=>
            {
                var connection = new MySqlConnection(connectionString);
                return new CompanyAdministrationDbContext(connection);
            });
            return builder.Build();
        }
    }
}
