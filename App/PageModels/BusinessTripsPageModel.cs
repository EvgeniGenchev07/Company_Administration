using App.Pages;
using App.ViewModels;
using BusinessLayer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace App.PageModels
{
    public partial class BusinessTripsPageModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<BusinessTrip> _allBusinessTrips = new();

        [ObservableProperty]
        private ObservableCollection<BusinessTrip> _filteredBusinessTrips = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private DateTime? _startDateFilter;

        [ObservableProperty]
        private DateTime? _endDateFilter;

        [ObservableProperty]
        private string _projectFilter = string.Empty;

        [ObservableProperty]
        private string _destinationFilter = string.Empty;

        [ObservableProperty]
        private byte? _statusFilter;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _userName = string.Empty;

        [ObservableProperty]
        private bool _hasResults = true;

        // Status options for filter
        [ObservableProperty]
        private ObservableCollection<StatusOption> _statusOptions = new()
        {
            new StatusOption { Value = null, DisplayName = "Всички" },
            new StatusOption { Value = 0, DisplayName = "Чакаща" },
            new StatusOption { Value = 1, DisplayName = "Одобрена" },
            new StatusOption { Value = 2, DisplayName = "Отхвърлена" },
            new StatusOption { Value = 3, DisplayName = "Завършена" }
        };

        [ObservableProperty]
        private StatusOption _selectedStatusOption;

        public BusinessTripsPageModel()
        {
            SelectedStatusOption = StatusOptions[0];
            LoadBusinessTrips();
        }

        [RelayCommand]
        private void LoadBusinessTrips()
        {
            if (App.User?.BusinessTrips == null)
            {
                AllBusinessTrips.Clear();
                FilteredBusinessTrips.Clear();
                return;
            }

            IsLoading = true;

            try
            {
                AllBusinessTrips.Clear();
                foreach (var trip in App.User.BusinessTrips)
                {
                    AllBusinessTrips.Add(trip);
                }

                UserName = App.User.Name;
                ApplyFilters();
                HasResults = FilteredBusinessTrips.Count > 0;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ApplyFilters()
        {
            if (AllBusinessTrips == null) return;

            var filtered = AllBusinessTrips.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(trip =>
                    (trip.ProjectName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (trip.Task?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (trip.CarTripDestination?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (trip.CarBrand?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (trip.CarModel?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                );
            }

            if (StartDateFilter.HasValue)
            {
                filtered = filtered.Where(trip => trip.StartDate >= StartDateFilter.Value);
            }

            if (EndDateFilter.HasValue)
            {
                filtered = filtered.Where(trip => trip.EndDate <= EndDateFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(ProjectFilter))
            {
                filtered = filtered.Where(trip =>
                    trip.ProjectName?.Contains(ProjectFilter, StringComparison.OrdinalIgnoreCase) ?? false
                );
            }

            if (!string.IsNullOrWhiteSpace(DestinationFilter))
            {
                filtered = filtered.Where(trip =>
                    trip.CarTripDestination?.Contains(DestinationFilter, StringComparison.OrdinalIgnoreCase) ?? false
                );
            }

            if (SelectedStatusOption?.Value.HasValue == true)
            {
                var statusValue = (BusinessTripStatus)SelectedStatusOption.Value.Value;
                filtered = filtered.Where(trip => trip.Status == statusValue);
            }

            FilteredBusinessTrips.Clear();
            foreach (var trip in filtered.OrderByDescending(t => t.StartDate))
            {
                FilteredBusinessTrips.Add(trip);
            }
            HasResults = FilteredBusinessTrips.Count > 0;
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            StartDateFilter = null;
            EndDateFilter = null;
            ProjectFilter = string.Empty;
            DestinationFilter = string.Empty;
            SelectedStatusOption = StatusOptions[0];
            ApplyFilters();
        }

        [RelayCommand]
        private async Task RefreshData()
        {
            LoadBusinessTrips();
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ItemTapped(BusinessTrip businessTrip)
        {
            if (businessTrip != null)
            {
                BusinessTripDetailsPage.SelectedBusinessTrip = new BusinessTripViewModel(businessTrip);
                await Shell.Current.GoToAsync("//businesstripdetails");
            }
        }

        [RelayCommand]
        private async Task Back()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        partial void OnSearchTextChanged(string value) => ApplyFilters();
        partial void OnStartDateFilterChanged(DateTime? value) => ApplyFilters();
        partial void OnEndDateFilterChanged(DateTime? value) => ApplyFilters();
        partial void OnProjectFilterChanged(string value) => ApplyFilters();
        partial void OnDestinationFilterChanged(string value) => ApplyFilters();
        partial void OnSelectedStatusOptionChanged(StatusOption value) => ApplyFilters();
    }

    public class StatusOption
    {
        public int? Value { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}