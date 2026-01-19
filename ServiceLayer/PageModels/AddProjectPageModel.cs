using ApplicationLayer.Interfaces;
using BusinessLayer.Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ServiceLayer.PageModels
{
    public class AddProjectPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly IDatabaseService _dbService;
        private string _name;
        private string _description;
        private DateTime _startDate;
        private DateTime _endDate;
        private ObservableCollection<User> _availableUsers;
        private ObservableCollection<User> _selectedUsers;
        private bool _isBusy;

        private string _nameError;
        private string _descriptionError;
        private string _startDateError;
        private string _endDateError;
        private string _usersError;

        public AddProjectPageModel(IDatabaseService databaseService)
        {
            _dbService = databaseService;
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(1);
            AvailableUsers = new ObservableCollection<User>();
            SelectedUsers = new ObservableCollection<User>();

            CreateProjectCommand = new Command(async () => await CreateProjectAsync());
            CancelCommand = new Command(async () => await CancelAsync());
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                SetProperty(ref _startDate, value);
                OnPropertyChanged(nameof(TodayDate));
                if (EndDate < value)
                    EndDate = value;
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public ObservableCollection<User> AvailableUsers
        {
            get => _availableUsers;
            set => SetProperty(ref _availableUsers, value);
        }

        public ObservableCollection<User> SelectedUsers
        {
            get => _selectedUsers;
            set => SetProperty(ref _selectedUsers, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public DateTime TodayDate => DateTime.Today;
        public string SelectedUsersCountText => $"Избрани служители: {SelectedUsers?.Count ?? 0}";

        public string NameError
        {
            get => _nameError;
            set => SetProperty(ref _nameError, value);
        }

        public string DescriptionError
        {
            get => _descriptionError;
            set => SetProperty(ref _descriptionError, value);
        }

        public string StartDateError
        {
            get => _startDateError;
            set => SetProperty(ref _startDateError, value);
        }

        public string EndDateError
        {
            get => _endDateError;
            set => SetProperty(ref _endDateError, value);
        }

        public string UsersError
        {
            get => _usersError;
            set => SetProperty(ref _usersError, value);
        }

        public bool HasNameError => !string.IsNullOrEmpty(NameError);
        public bool HasDescriptionError => !string.IsNullOrEmpty(DescriptionError);
        public bool HasStartDateError => !string.IsNullOrEmpty(StartDateError);
        public bool HasEndDateError => !string.IsNullOrEmpty(EndDateError);
        public bool HasUsersError => !string.IsNullOrEmpty(UsersError);

        public Command CreateProjectCommand { get; }
        public Command CancelCommand { get; }

        private async Task CreateProjectAsync()
        {
            if (!ValidateInput())
                return;

            IsBusy = true;
            try
            {
                var project = new Project
                {
                    Name = Name,
                    Description = Description,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    Users = SelectedUsers.ToList()
                };

                bool success = await _dbService.CreateProjectAsync(project);

                if (success)
                {
                    ClearForm();
                    await Application.Current.MainPage.DisplayAlert("Успех", "Проектът е създаден успешно!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Грешка", "Неуспешно създаване на проекта!", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Грешка", $"Възникна грешка: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        private bool ValidateInput()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(Name))
            {
                NameError = "Името на проекта е задължително";
                isValid = false;
            }
            else if (Name.Length < 3)
            {
                NameError = "Името трябва да е поне 3 символа";
                isValid = false;
            }
            else
            {
                NameError = null;
            }

            if (!string.IsNullOrWhiteSpace(Description) && Description.Length > 500)
            {
                DescriptionError = "Описанието не може да надвишава 500 символа";
                isValid = false;
            }
            else
            {
                DescriptionError = null;
            }

            if (StartDate < DateTime.Today)
            {
                StartDateError = "Началната дата не може да е в миналото";
                isValid = false;
            }
            else
            {
                StartDateError = null;
            }

            if (EndDate < StartDate)
            {
                EndDateError = "Крайната дата не може да е преди началната";
                isValid = false;
            }
            else if (EndDate > StartDate.AddYears(5))
            {
                EndDateError = "Проектът не може да продължи повече от 5 години";
                isValid = false;
            }
            else
            {
                EndDateError = null;
            }

            UsersError = null;

            return isValid;
        }

        private void ClearForm()
        {
            Name = string.Empty;
            Description = string.Empty;
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddMonths(1);
            SelectedUsers.Clear();

            NameError = null;
            DescriptionError = null;
            StartDateError = null;
            EndDateError = null;
            UsersError = null;
        }

        public async Task LoadAvailableUsersAsync()
        {
            IsBusy = true;
            try
            {
                var users = await _dbService.GetAllUsersAsync();
                users = users.Where(u => u.Role != BusinessLayer.Enums.Role.Admin).ToList();
                AvailableUsers = new ObservableCollection<User>(users);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно зареждане на служители: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void SelectUser(User user)
        {
            if (SelectedUsers.Contains(user))
            {
                SelectedUsers.Remove(user);
            }
            else
            {
                SelectedUsers.Add(user);
            }
            OnPropertyChanged(nameof(SelectedUsersCountText));
        }

        public bool IsUserSelected(User user)
        {
            return SelectedUsers.Contains(user);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}