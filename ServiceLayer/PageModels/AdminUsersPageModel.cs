using ApplicationLayer.Interfaces;
using ApplicationLayer.ViewModels;
using ServiceLayer.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ServiceLayer.PageModels;

public class AdminUsersPageModel : INotifyPropertyChanged
{
    private readonly IDatabaseService _dbService;
    private bool _isBusy;
    private bool _isRefreshing;
    private string _searchText = string.Empty;
    private List<UserViewModel> _allUsers = new();

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<UserViewModel> Users { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                Task.Run(() => SearchUsersAsync());
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            if (_isRefreshing != value)
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand AddUserCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand DeleteUserCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshCommand { get; }

    public AdminUsersPageModel(IDatabaseService dbService)
    {
        _dbService = dbService;

        AddUserCommand = new Command(async () => await AddUserAsync());
        EditUserCommand = new Command<UserViewModel>(async (user) => await EditUserAsync(user));
        DeleteUserCommand = new Command<UserViewModel>(async (user) => await DeleteUserAsync(user));
        SearchCommand = new Command(async () => await SearchUsersAsync());
        CancelCommand = new Command(async () => await CancelAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());

    }

    public async Task LoadUsersAsync()
    {
        try
        {
            IsBusy = true;
            Users.Clear();
            _allUsers.Clear();

            var users = await _dbService.GetAllUsersAsync();
            _allUsers = users.Select(u => new UserViewModel(u)).ToList();

            foreach (var user in _allUsers)
            {
                Users.Add(user);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспено зареждане на потребители: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchUsersAsync()
    {
        try
        {
            IsBusy = true;

            var filteredUsers = await Task.Run(() =>
                string.IsNullOrWhiteSpace(SearchText)
                    ? _allUsers
                    : _allUsers.Where(u =>
                        u.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList());

            Device.BeginInvokeOnMainThread(() =>
            {
                Users.Clear();
                foreach (var user in filteredUsers)
                {
                    Users.Add(user);
                }
            });
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно търсене на потребители: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddUserAsync()
    {
        await Shell.Current.GoToAsync("AddUserPage");
    }

    private async Task EditUserAsync(UserViewModel user)
    {
        if (user != null)
        {
            await Shell.Current.GoToAsync("EditUserPage", new Dictionary<string, object>
            {
                ["User"] = user
            });
        }
    }

    private async Task DeleteUserAsync(UserViewModel user)
    {
        if (user == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Потвърди изтриване",
            $"Искате ли да изтриете потребител '{user.Name}'?",
            "Изтрий",
            "Отказ");

        if (confirm)
        {
            try
            {
                IsBusy = true;
                var success = await _dbService.DeleteUserAsync(user.Id);

                if (success)
                {
                    _allUsers.Remove(user);
                    Users.Remove(user);
                    await Application.Current.MainPage.DisplayAlert("Успех", "Успешно изтрит потребител", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Грешка", "Неуспешно изтриване на потребител", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно изтриване на потребител: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("//AdminPage");
    }

    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadUsersAsync();
        IsRefreshing = false;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}