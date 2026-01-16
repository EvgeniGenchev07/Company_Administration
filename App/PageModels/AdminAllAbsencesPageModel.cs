using App.Pages;
using App.Services;
using App.ViewModels;
using BusinessLayer;
using ClosedXML.Excel;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace App.PageModels;

public partial class AdminAllAbsencesPageModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    [ObservableProperty]
    private bool _isBusy;
    [ObservableProperty]
    private bool _isRefreshing;
    [ObservableProperty]
    private string _search;
    [ObservableProperty]
    private ObservableCollection<int> availableYears = new();
    [ObservableProperty]
    private int selectedYear;

    [ObservableProperty]
    private ObservableCollection<string> availableMonths = new();

    [ObservableProperty]
    private string selectedMonth;
    public event PropertyChangedEventHandler PropertyChanged;
    [ObservableProperty]
    public ObservableCollection<AbsenceViewModel> absences = new();
    private List<AbsenceViewModel> _originalAbsences = new();

    public int TotalAbsences => Absences.Sum(t => t.DaysTaken);
    public int PendingAbsences => Absences.Count(a => a.Status == AbsenceStatus.Pending);
    public int ApprovedAbsences => Absences.Count(a => a.Status == AbsenceStatus.Approved);
    public int RejectedAbsences => Absences.Count(a => a.Status == AbsenceStatus.Rejected);
    public bool HasNoResults => !(Absences.Any());

    public AdminAllAbsencesPageModel(DatabaseService dbService)
    {
        _dbService = dbService;
        AvailableYears = new ObservableCollection<int>(Enumerable.Range(DateTime.Now.Year - 5, 10));
        SelectedYear = DateTime.Now.Year;

        AvailableMonths = new ObservableCollection<string>(
            CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12));
        AvailableMonths.Add("Всички месеци");
        SelectedMonth = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[DateTime.Now.Month - 1];

    }
    [RelayCommand]
    private void FilterAbsence()
    {
        try
        {
            IsBusy = true;
            ObservableCollection<AbsenceViewModel> filtered = new ObservableCollection<AbsenceViewModel>(_originalAbsences);

            // Filter by year
            if (SelectedYear > 0)
            {
                filtered = new ObservableCollection<AbsenceViewModel>(
                    filtered.Where(t => t.StartDate.Year == SelectedYear || t.EndDate.Year == SelectedYear));
            }

            // Filter by month
            if (!string.IsNullOrEmpty(SelectedMonth) && selectedMonth != "Всички месеци")
            {
                var monthIndex = Array.IndexOf(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames, SelectedMonth) + 1;
                filtered = new ObservableCollection<AbsenceViewModel>(
                    filtered.Where(t => t.StartDate.Month == monthIndex || t.EndDate.Month == monthIndex));
            }

            if (!string.IsNullOrEmpty(Search))
            {
                filtered = new ObservableCollection<AbsenceViewModel>(filtered.Where(t => t.UserName.Contains(Search, StringComparison.OrdinalIgnoreCase)));
            }
            Absences.Clear();
            foreach (var item in filtered)
            {
                Absences.Add(item);
            }
            OnPropertyChanged(nameof(HasNoResults));
            OnPropertyChanged(nameof(TotalAbsences));
            OnPropertyChanged(nameof(PendingAbsences));
            OnPropertyChanged(nameof(ApprovedAbsences));
            OnPropertyChanged(nameof(RejectedAbsences));
        }
        finally
        {
            IsBusy = false;
        }
    }
    [RelayCommand]
    private async Task LoadAbsencesAsync()
    {
        try
        {
            IsBusy = true;

            var absences = await _dbService.GetAllAbsencesAsync();
            _originalAbsences = absences.Select(a => new AbsenceViewModel(a)).ToList();
            Absences.Clear();
            foreach (var absence in _originalAbsences)
            {
                Absences.Add(absence);
            }
            OnPropertyChanged(nameof(HasNoResults));
            OnPropertyChanged(nameof(TotalAbsences));
            OnPropertyChanged(nameof(PendingAbsences));
            OnPropertyChanged(nameof(ApprovedAbsences));
            OnPropertyChanged(nameof(RejectedAbsences));
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно зареждане на данните: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
    [RelayCommand]
    public async void SelectAbsence(AbsenceViewModel absence)
    {
        if (absence != null)
        {
            AbsenceDetailsPage.SelectedAbsence = absence;
            await Shell.Current.GoToAsync("AbsenceDetailsPage");
        }
    }
    [RelayCommand]
    private async Task ApproveAbsence(AbsenceViewModel absence)
    {
        if (absence == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Одобри отсъствие",
            $"Искате ли да одобрите това отсъствие?",
            "Одобри",
            "Откажи");

        if (confirm)
        {
            try
            {
                IsBusy = true;

                // Call API to approve absence
                var success = await _dbService.ApproveAbsenceAsync(absence.Id);
                if (success)
                {
                    absence.Status = AbsenceStatus.Approved;
                    int index = Absences.IndexOf(absence);
                    if (index >= 0)
                    {
                        Absences[index] = absence;
                    }
                    OnPropertyChanged(nameof(PendingAbsences));
                    OnPropertyChanged(nameof(ApprovedAbsences));
                    await Application.Current.MainPage.DisplayAlert("Успех", "Отсъствието бе одобрено успешно", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Грешка", "Неуспешно одобрение на отсъствие", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно одобрение на отсъствие: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
    [RelayCommand]
    private async Task RejectAbsence(AbsenceViewModel absence)
    {
        if (absence == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Потвърдете отхвърляне",
            $"Искате ли да отхвърлите молбата за командировка?",
            "Отхвърли",
            "Отказ");

        if (confirm)
        {
            try
            {
                IsBusy = true;

                var success = await _dbService.RejectAbsenceAsync(absence.Id);
                if (success)
                {
                    absence.Status = AbsenceStatus.Rejected;
                    int index = Absences.IndexOf(absence);
                    if (index >= 0)
                    {
                        Absences[index] = absence;
                    }
                    OnPropertyChanged(nameof(PendingAbsences));
                    OnPropertyChanged(nameof(RejectedAbsences));
                    await Application.Current.MainPage.DisplayAlert("Успех", "Отсъствието бе отхвърлено успешно", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Грешка", "Неуспешно отхвърляне на отсъствие", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно отхвърляне на отсъствие: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("//AdminPage");
    }
    [RelayCommand]
    private async Task Export()
    {
        var absencesExport = Absences.Where(a => a.Status == AbsenceStatus.Approved).ToList();
        using var workbook = new XLWorkbook();
        {
            if (SelectedMonth == "Всички месеци")
            {
                AddSheet(workbook, "Отпуски", absencesExport.Where(a => a.Type == AbsenceType.PersonalLeave).ToList());
                AddSheet(workbook, "Болнични", absencesExport.Where(a => a.Type == AbsenceType.SickLeave).ToList());
                AddSheet(workbook, "Други", absencesExport.Where(a => a.Type == AbsenceType.Other).ToList());
                AddSheet(workbook, "Всички", absencesExport.ToList());
            }
            else
            {
                var monthIndex = Array.IndexOf(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames, SelectedMonth) + 1;
                AddSheet(workbook, "Отпуски", absencesExport.Where(a => a.Type == AbsenceType.PersonalLeave && a.StartDate.Month == monthIndex).ToList());
                AddSheet(workbook, "Болнични", absencesExport.Where(a => a.Type == AbsenceType.SickLeave && a.StartDate.Month == monthIndex).ToList());
                AddSheet(workbook, "Други", absencesExport.Where(a => a.Type == AbsenceType.Other && a.StartDate.Month == monthIndex).ToList());
                AddSheet(workbook, "Всички", absencesExport.Where(a => a.StartDate.Month == monthIndex).ToList());
            }
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        var result = await FileSaver.Default.SaveAsync($"Absences-{Guid.NewGuid().ToString().Substring(0, 5)}.xlsx", stream);
    }

    private void AddSheet(XLWorkbook workbook, string type, List<AbsenceViewModel> absences)
    {
        var worksheet = workbook.Worksheets.Add($"{SelectedYear} {type}");
        worksheet.Cell(1, 1).Value = "Месец";
        int horizontalIndex = 2;
        var names = Absences.Select(t => t.UserName).Distinct().ToList();
        int verticalIndex = 2;

        foreach (var name in names)
        {
            worksheet.Cell(1, horizontalIndex).Value = name;
            for (int i = 0; i < 12; i++)
            {
                worksheet.Cell(i + verticalIndex, horizontalIndex).Value = 0;
            }
            horizontalIndex++;
        }
        for (int month = 1; month <= 12; month++)
        {
            worksheet.Cell(verticalIndex, 1).Value = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            foreach (var absence in absences.Where(a => a.StartDate.Year == SelectedYear && a.StartDate.Month == month))
            {
                int columnIndex = names.IndexOf(absence.UserName) + 2;
                var value = worksheet.Cell(verticalIndex, columnIndex).Value.GetNumber();
                worksheet.Cell(verticalIndex, columnIndex).Value = absence.DaysTaken + value;
            }
            verticalIndex++;
        }
        worksheet.Cell(verticalIndex, 1).Value = "Общо";
        for (int i = 2; i < horizontalIndex; i++)
        {
            worksheet.Cell(verticalIndex, i).FormulaA1 = $"SUM({worksheet.Cell(2, i).Address}:{worksheet.Cell(verticalIndex - 1, i).Address})";
        }
    }
    [RelayCommand]
    private async Task Refresh()
    {
        IsRefreshing = true;
        await LoadAbsencesAsync();
        IsRefreshing = false;
    }

    partial void OnSelectedMonthChanged(string value) => FilterAbsence();
    partial void OnSelectedYearChanged(int value) => FilterAbsence();
    partial void OnSearchChanged(string value) => FilterAbsence();
}