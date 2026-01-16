using ApplicationLayer.Interfaces;
using BusinessLayer;
using BusinessLayer.Entities;
using BusinessLayer.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceLayer.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace ServiceLayer.PageModels;

public partial class AbsencePageModel : ObservableObject
{
    private readonly IDatabaseService _dbService;

    [ObservableProperty]
    private string _employeeName;

    [ObservableProperty]
    private int _availableDays;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasValidationErrors;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _isFormValid = true;

    [ObservableProperty]
    private AbsenceTypeOption _selectedAbsenceType;

    public ObservableCollection<AbsenceTypeOption> AbsenceTypes { get; } = new()
    {
        new AbsenceTypeOption { Value = AbsenceType.SickLeave, DisplayName = "Болнични" },
        new AbsenceTypeOption { Value = AbsenceType.PersonalLeave, DisplayName = "Отпуск" },
        new AbsenceTypeOption { Value = AbsenceType.Other, DisplayName = "Други" }
    };

    public DateTime MinimumDate => DateTime.Today;
    private byte duration;
    public byte DurationDays => duration;

    public AbsencePageModel(IDatabaseService dbService)
    {
        _dbService = dbService;
        LoadUserData();
        ValidateForm();
        duration = (byte)((EndDate - StartDate).Days + 1);
        OnPropertyChanged(nameof(DurationDays));
    }

    private void LoadUserData()
    {
        if (DatabaseService.User != null)
        {
            EmployeeName = DatabaseService.User.Name;
            AvailableDays = DatabaseService.User.AbsenceDays;
        }
    }

    [RelayCommand]
    private async Task Back()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    [RelayCommand]
    private async Task SubmitRequest()
    {
        if (!ValidateForm())
        {
            return;
        }

        try
        {
            IsBusy = true;

            var absence = new Absence
            {
                Type = SelectedAbsenceType.Value,
                DaysCount = (byte)((EndDate - StartDate).Days + 1),
                DaysTaken = DurationDays,
                StartDate = StartDate,
                Status = AbsenceStatus.Pending,
                Created = DateTime.Now,
                UserId = DatabaseService.User?.Id ?? string.Empty
            };

            var success = await _dbService.CreateAbsenceAsync(absence);

            if (success)
            {
                await Shell.Current.DisplayAlert("Успех", "Молбата за отсъствие е изпратена успешно", "OK");
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Грешка", "Неуспешно изпращане на молба за отсъствие", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Грешка", $"Възникна грешка: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidateForm()
    {
        var errors = new List<string>();
        duration = (byte)((EndDate - StartDate).Days + 1);
        if (SelectedAbsenceType?.Value == AbsenceType.PersonalLeave) duration -= (byte)( _dbService.CalculateHolidays(StartDate, duration).Result);
        if (SelectedAbsenceType == null)
        {
            errors.Add("Моля избери причина за отсъствието");
        }

        if (StartDate < DateTime.Today)
        {
            errors.Add("Началната дата не може да бъде в миналото");
        }

        if (EndDate < StartDate)
        {
            errors.Add("Крайната дата не може да бъде преди началната");
        }

        if (DurationDays > AvailableDays && SelectedAbsenceType?.Value == AbsenceType.PersonalLeave)
        {
            errors.Add($"Имаш още само {AvailableDays} свободни дни");
        }

        HasValidationErrors = errors.Any();
        ValidationMessage = string.Join("\n", errors);
        IsFormValid = !HasValidationErrors;
        OnPropertyChanged(nameof(DurationDays));
        return IsFormValid;
    }

    partial void OnStartDateChanged(DateTime value)
    {
        if (EndDate < value)
        {
            EndDate = value;
        }
        ValidateForm();
    }

    partial void OnEndDateChanged(DateTime value)
    {
        ValidateForm();
    }

    partial void OnSelectedAbsenceTypeChanged(AbsenceTypeOption value)
    {
        ValidateForm();
    }
}

public class AbsenceTypeOption
{
    public AbsenceType Value { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}