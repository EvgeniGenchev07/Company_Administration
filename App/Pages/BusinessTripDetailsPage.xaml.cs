using App.PageModels;
using App.ViewModels;

namespace App.Pages
{
    public partial class BusinessTripDetailsPage : ContentPage
    {
        public static BusinessTripViewModel SelectedBusinessTrip { get; set; }

        public BusinessTripDetailsPage(BusinessTripDetailsPageModel businessTripDetailsPageModel)
        {
            BindingContext = businessTripDetailsPageModel;
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is BusinessTripDetailsPageModel viewModel)
            {
                viewModel.LoadData();
            }
        }
    }
}