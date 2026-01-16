using App.Pages;
using App.Services;
using App.ViewModels;
using BusinessLayer;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace App.PageModels;

public class AbsenceDetailsPageModel : INotifyPropertyChanged
{
    private readonly DatabaseService _dbService;
    private bool _isBusy;
    private AbsenceViewModel _absence;

    public event PropertyChangedEventHandler PropertyChanged;

    public AbsenceViewModel Absence
    {
        get => _absence;
        set
        {
            _absence = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TypeText));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(StatusIcon));
            OnPropertyChanged(nameof(StatusDescription));
            OnPropertyChanged(nameof(DurationText));
            OnPropertyChanged(nameof(StartDateText));
            OnPropertyChanged(nameof(EndDateText));
            OnPropertyChanged(nameof(CreatedText));
            OnPropertyChanged(nameof(IsApproved));
            OnPropertyChanged(nameof(IsRejected));
            OnPropertyChanged(nameof(CanEdit));
        }
    }

    public string TypeText => Absence?.TypeText ?? string.Empty;
    public string StatusText => Absence?.StatusText ?? string.Empty;
    public Color StatusColor => Absence?.StatusColor ?? Colors.Gray;

    public string StatusIcon => Absence?.Status switch
    {
        BusinessLayer.AbsenceStatus.Pending => "\u23F3", // Pending
        BusinessLayer.AbsenceStatus.Approved => "\u2713", // Approved
        BusinessLayer.AbsenceStatus.Rejected => "\u274C", // Rejected
        _ => "\u2753"
    };

    public string StatusDescription => Absence?.Status switch
    {
        BusinessLayer.AbsenceStatus.Pending => "Твоята молба се разглежда от ръководството",
        BusinessLayer.AbsenceStatus.Approved => "Твоята молбa бе одобрена",
        BusinessLayer.AbsenceStatus.Rejected => "Твоята молба бе отхвърлена",
        _ => "Неизвестно състояние"
    };

    public string DurationText => Absence?.DurationText ?? string.Empty;
    public string StartDateText => Absence != null ? $"{Absence.StartDate:dd/MM/yyyy}" : string.Empty;
    public string EndDateText => Absence != null ? $"{Absence.StartDate.AddDays(Absence.Days - 1):dd/MM/yyyy}" : string.Empty;
    public string CreatedText => Absence?.CreatedText ?? string.Empty;

    public bool IsApproved => Absence?.Status == BusinessLayer.AbsenceStatus.Approved;
    public bool IsRejected => Absence?.Status == BusinessLayer.AbsenceStatus.Rejected;
    public bool CanEdit => Absence?.Status == BusinessLayer.AbsenceStatus.Pending; // Only pending requests can be edited

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public ICommand BackCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand CancelCommand { get; }

    public AbsenceDetailsPageModel(DatabaseService dbService)
    {
        _dbService = dbService;

        BackCommand = new Command(async () => await BackAsync());
        EditCommand = new Command(async () => await EditAsync());
        CancelCommand = new Command(async () => await CancelAsync());

        LoadAbsenceDetails();
    }

    private void LoadAbsenceDetails()
    {
        if (AbsenceDetailsPage.SelectedAbsence != null)
        {
            Absence = AbsenceDetailsPage.SelectedAbsence;
        }
    }

    private async Task BackAsync()
    {
        if (App.User.Role == Role.Admin)
        {
            await Shell.Current.GoToAsync("//AdminAllAbsencesPage");
        }
        else
        {
            await Shell.Current.GoToAsync("//AllAbsencesPage");
        }
    }

    private async Task EditAsync()
    {
        if (Absence == null) return;

        try
        {
            IsBusy = true;

            // Navigate to edit page (could be the same as AbsencePage with pre-filled data)
            await Shell.Current.DisplayAlert("Редактиране", "Фукнционалността за редактиране ще бъде въведена в бъдеще", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Грешка", $"Неуспешно редактиране на отсъствие: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelAsync()
    {
        if (Absence == null) return;

        var confirmed = await Shell.Current.DisplayAlert(
            "Откажете молбата",
            "Искате ли да откажете тази молба? Това действие не може да бъде отменено.",
            "Откажете молбата",
            "Запазете молбата");

        if (!confirmed) return;

        try
        {
            IsBusy = true;

            // Call API to cancel the absence request
            var success = await _dbService.CancelAbsenceAsync(Absence.Id);

            if (success)
            {
                await Shell.Current.DisplayAlert("Успех", "Молбата за отсъствие бе отказана", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Грешка", "Неуспешно отказване на молба за отсъствие", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Грешка", $"Неуспешно отказване на молба за отсъствие: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}