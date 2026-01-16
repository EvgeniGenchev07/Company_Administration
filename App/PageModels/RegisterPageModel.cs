using App.Services;
using BusinessLayer;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace App.PageModels;

public class RegisterPageModel : INotifyPropertyChanged
{
    private string _email;
    private string _password;
    private string _errorMessage;
    private bool _isBusy;
    private bool _isEnabled;
    private bool _isPasswordHidden = true;
    private readonly DatabaseService _dbService;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            IsEnabled = CanRegister();
            OnPropertyChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            IsEnabled = CanRegister();
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool IsPasswordHidden
    {
        get => _isPasswordHidden;
        set
        {
            _isPasswordHidden = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EyeIcon));
        }
    }

    public string EyeIcon => IsPasswordHidden ? "eye_off.png" : "eye_on.png";

    public ICommand RegisterCommand { get; }
    public ICommand TogglePasswordCommand { get; }

    public RegisterPageModel(DatabaseService dbService)
    {
        RegisterCommand = new Command(async () => await RegisterAsync(), CanRegister);
        TogglePasswordCommand = new Command(() => IsPasswordHidden = !IsPasswordHidden);

        PropertyChanged += (_, __) => ((Command)RegisterCommand).ChangeCanExecute();
        _dbService = dbService;
    }

    private bool CanRegister()
    {
        return !IsBusy
            && !string.IsNullOrWhiteSpace(Email)
            && !string.IsNullOrWhiteSpace(Password);
    }

    private async Task RegisterAsync()
    {
        try
        {
            IsBusy = true;
            IsEnabled = false;
            ErrorMessage = string.Empty;

            if (!Email.Contains("@") || !Email.Contains("."))
            {
                ErrorMessage = "Моля въведи правилен имейл";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Паролата трябва да е поне 6 символа";
                return;
            }

            User user = await _dbService.UserLogin(Email, Password);
            if (user == null)
            {
                ErrorMessage = "Неуспешна регистрация. Моля опитайте отново.";
                return;
            }

            App.User = user;
            await Shell.Current.GoToAsync(user.Role == Role.Admin ? "//AdminPage" : "//MainPage");

            Email = string.Empty;
            Password = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Неуспешна регистрация: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            IsEnabled = true;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}