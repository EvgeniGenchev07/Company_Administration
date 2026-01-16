using App.PageModels;

namespace App.Pages
{
    public partial class BusinessTripsPage : ContentPage
    {
        public BusinessTripsPage(BusinessTripsPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is BusinessTripsPageModel viewModel)
            {
                viewModel.LoadBusinessTripsCommand.Execute(null);
            }
        }
    }
}