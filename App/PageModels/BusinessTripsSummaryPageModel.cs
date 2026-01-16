using App.Services;
using App.ViewModels;
using BusinessLayer;
using ClosedXML.Excel;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
namespace App.PageModels;

public partial class BusinessTripsSummaryPageModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    public BusinessTripsSummaryPageModel(DatabaseService dbService)
    {
        _dbService = dbService;

        // Initialize filters
        AvailableYears = new ObservableCollection<int>(Enumerable.Range(DateTime.Now.Year - 5, 10));
        SelectedYear = DateTime.Now.Year;

        AvailableMonths = new ObservableCollection<string>(
            CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12));
        AvailableMonths.Add("Всички месеци");
        SelectedMonth = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[DateTime.Now.Month - 1];

        LoadProjects();
    }

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool hasNoResults = false;
    [ObservableProperty]
    private decimal summary;

    // Filter properties
    [ObservableProperty]
    private ObservableCollection<int> availableYears;

    [ObservableProperty]
    private int selectedYear;

    [ObservableProperty]
    private ObservableCollection<string> availableMonths;

    [ObservableProperty]
    private string selectedMonth;

    [ObservableProperty]
    private ObservableCollection<string> availableProjects = new();

    [ObservableProperty]
    private string selectedProject;

    [ObservableProperty]
    private StatusFilter selectedStatus;
    private List<BusinessTripViewModel> _originalTrips = new();
    [ObservableProperty]
    private ObservableCollection<BusinessTripViewModel> trips = new();

    partial void OnSelectedStatusChanged(StatusFilter value)
    {
        if (value != null)
            FilterTrips();
    }

    [RelayCommand]
    private async Task LoadTrips()
    {
        try
        {
            IsLoading = true;
            HasNoResults = false;

            var trips = await _dbService.GetAllBusinessTripsAsync();
            var viewModels = trips.Where(t => t.Status == BusinessTripStatus.Approved).Select(t => new BusinessTripViewModel(t)).ToList();
            _originalTrips = viewModels;
            Trips = new ObservableCollection<BusinessTripViewModel>(viewModels);
            HasNoResults = !Trips.Any();
            Summary = Trips.Sum(t => t.Wage * t.Days + t.AccommodationMoney * t.Days);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Грешка", $"Възникна грешка при зареждане: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }
    [RelayCommand]
    private async Task Back()
    {
        await Shell.Current.GoToAsync("//AdminAllBusinessTripsPage");
    }



    [RelayCommand]
    private async Task Export()
    {
        using var workbook = new XLWorkbook();
        if (SelectedMonth == "Всички месеци")
        {
            if (SelectedProject == "Всички проект")
            {
                List<decimal> monthsExpences = new List<decimal>();
                foreach (var month in AvailableMonths)
                {
                    monthsExpences.Add(AddSheet(workbook, month));
                }
                var worksheet = workbook.Worksheets.Add($"Обобщение {SelectedYear}");
                worksheet.Cell(1, 1).Value = "Месец";
                worksheet.Cell(1, 2).Value = "Общо разходи";
                int currentIndex = 2;
                foreach (var month in AvailableMonths)
                {
                    worksheet.Cell(currentIndex, 1).Value = month;
                    worksheet.Cell(currentIndex, 2).Value = monthsExpences[currentIndex - 2];
                    currentIndex++;
                }
                worksheet.Cell(currentIndex, 1).Value = "Общо за годината";
                worksheet.Cell(currentIndex, 11).FormulaA1 = $"SUM(B2:B{currentIndex - 1})";
            }
            else
            {
                var worksheet = workbook.Worksheets.Add(SelectedProject);
                worksheet.Cell(1, 1).Value = "Номер";
                worksheet.Cell(1, 2).Value = "Проект";
                worksheet.Cell(1, 3).Value = "Място";
                worksheet.Cell(1, 4).Value = "Дата";
                worksheet.Cell(1, 5).Value = "Дни";
                worksheet.Cell(1, 6).Value = "Общо дневни";
                worksheet.Cell(1, 7).Value = "Пътни";
                worksheet.Cell(1, 8).Value = "Хотел";
                worksheet.Cell(1, 9).Value = "Други";
                worksheet.Cell(1, 10).Value = "Цел";
                worksheet.Cell(1, 11).Value = "Общо";
                int currentIndex = 2;
                var filteredTrips = Trips.Where(t => t.ProjectName == SelectedProject).ToList();
                foreach (var trip in filteredTrips)
                {
                    var businessTrip = trip.BusinessTrip;
                    worksheet.Cell(currentIndex, 1).Value = businessTrip.IssueId;
                    worksheet.Cell(currentIndex, 2).Value = businessTrip.ProjectName;
                    worksheet.Cell(currentIndex, 3).Value = businessTrip.CarTripDestination;
                    worksheet.Cell(currentIndex, 4).Value = businessTrip.IssueDate;
                    worksheet.Cell(currentIndex, 5).Value = businessTrip.TotalDays;
                    worksheet.Cell(currentIndex, 6).Value = businessTrip.Wage * trip.Days;
                    worksheet.Cell(currentIndex, 7).Value = businessTrip.Wage;
                    worksheet.Cell(currentIndex, 8).Value = businessTrip.AccommodationMoney;
                    worksheet.Cell(currentIndex, 9).Value = businessTrip.AdditionalExpences;
                    worksheet.Cell(currentIndex, 10).Value = businessTrip.Task;
                    worksheet.Cell(currentIndex, 11).FormulaA1 = $"F{currentIndex} + G{currentIndex} + H{currentIndex} + I{currentIndex}";
                    currentIndex++;
                }
                worksheet.Cell(currentIndex, 1).Value = "Общо за проекта";
                worksheet.Cell(currentIndex, 11).FormulaA1 = $"SUM(F2:F{currentIndex - 1})";
            }
        }
        else
        {
            AddSheet(workbook, SelectedMonth);
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        var result = await FileSaver.Default.SaveAsync($"BusinessTrips-{Guid.NewGuid().ToString().Substring(0, 5)}.xlsx", stream);
    }
    private decimal AddSheet(XLWorkbook workbook, string month)
    {
        var worksheet = workbook.Worksheets.Add(month);
        worksheet.Cell(1, 1).Value = "Номер";
        worksheet.Cell(1, 2).Value = "Проект";
        worksheet.Cell(1, 3).Value = "Място";
        worksheet.Cell(1, 4).Value = "Дата";
        worksheet.Cell(1, 5).Value = "Дни";
        worksheet.Cell(1, 6).Value = "Общо дневни";
        worksheet.Cell(1, 7).Value = "Пътни";
        worksheet.Cell(1, 8).Value = "Хотел";
        worksheet.Cell(1, 9).Value = "Други";
        worksheet.Cell(1, 10).Value = "Цел";
        worksheet.Cell(1, 11).Value = "Общо";
        int currentIndex = 2;
        var filteredTrips = Trips.Where(t => (t.StartDate.Year == SelectedYear && t.StartDate.ToString("MMMM", CultureInfo.CurrentCulture) == month) || month == "Всички месеци").ToList();
        decimal totalExpenses = 0;
        foreach (var trip in filteredTrips)
        {
            var businessTrip = trip.BusinessTrip;
            worksheet.Cell(currentIndex, 1).Value = businessTrip.IssueId;
            worksheet.Cell(currentIndex, 2).Value = businessTrip.ProjectName;
            worksheet.Cell(currentIndex, 3).Value = businessTrip.CarTripDestination;
            worksheet.Cell(currentIndex, 4).Value = businessTrip.IssueDate;
            worksheet.Cell(currentIndex, 5).Value = businessTrip.TotalDays;
            worksheet.Cell(currentIndex, 6).Value = businessTrip.Wage * trip.Days;
            worksheet.Cell(currentIndex, 7).Value = businessTrip.Wage;
            worksheet.Cell(currentIndex, 8).Value = businessTrip.AccommodationMoney;
            worksheet.Cell(currentIndex, 9).Value = businessTrip.AdditionalExpences;
            worksheet.Cell(currentIndex, 10).Value = businessTrip.Task;
            worksheet.Cell(currentIndex, 11).FormulaA1 = $"F{currentIndex} + G{currentIndex} + H{currentIndex} + I{currentIndex}";
            totalExpenses += businessTrip.Wage * trip.Days + businessTrip.AccommodationMoney + businessTrip.AdditionalExpences;
            currentIndex++;
        }
        worksheet.Cell(currentIndex, 1).Value = "Общо за месеца";
        worksheet.Cell(currentIndex, 11).FormulaA1 = $"SUM(F2:F{currentIndex - 1})";
        return totalExpenses;
    }
    [RelayCommand]
    private async Task LoadProjects()
    {
        try
        {
            var projects = await _dbService.GetAllBusinessTripsAsync();
            AvailableProjects = new ObservableCollection<string>(projects.Select(bt => bt.ProjectName).Distinct().ToList());
            AvailableProjects.Insert(0, "Всички проекти");
            SelectedProject = "Всички проекти";
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Грешка", $"Възникна грешка при зареждане на проекти: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private void FilterTrips()
    {
        try
        {
            IsLoading = true;

            ObservableCollection<BusinessTripViewModel> filtered = new ObservableCollection<BusinessTripViewModel>(_originalTrips);

            // Filter by year
            if (SelectedYear > 0)
            {
                filtered = new ObservableCollection<BusinessTripViewModel>(
                    filtered.Where(t => t.StartDate.Year == SelectedYear || t.EndDate.Year == SelectedYear));
            }

            // Filter by month
            if (!string.IsNullOrEmpty(SelectedMonth) && selectedMonth != "Всички месеци")
            {
                var monthIndex = Array.IndexOf(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames, SelectedMonth) + 1;
                filtered = new ObservableCollection<BusinessTripViewModel>(
                    filtered.Where(t => t.StartDate.Month == monthIndex || t.EndDate.Month == monthIndex));
            }

            // Filter by project
            if (!string.IsNullOrEmpty(SelectedProject) && SelectedProject != "Всички проекти")
            {
                filtered = new ObservableCollection<BusinessTripViewModel>(
                    filtered.Where(t => t.ProjectName == SelectedProject));
            }

            Trips = filtered;
            Summary = Trips.Sum(t => t.Wage * t.Days + t.AccommodationMoney * t.Days);
            HasNoResults = !Trips.Any();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadTrips();
        await LoadProjects();
    }
}

public class StatusFilter
{
    public string Name { get; }
    public Color Color { get; }

    public StatusFilter(string name, Color color)
    {
        Name = name;
        Color = color;
    }
}

