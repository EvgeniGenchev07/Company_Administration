using ServiceLayer.PageModels;
using ApplicationLayer.ViewModels;
using BusinessLayer.Entities;

namespace App.Pages
{
    [QueryProperty(nameof(BusinessTrip), "BusinessTrip")]
    public partial class BusinessTripDetailsPage : ContentPage
    {
        
        public BusinessTripViewModel BusinessTrip { get; set; }

        public BusinessTripDetailsPage(BusinessTripDetailsPageModel businessTripDetailsPageModel)
        {
            BusinessTripDetailsPageModel.SelectedBusinessTrip = BusinessTrip;
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