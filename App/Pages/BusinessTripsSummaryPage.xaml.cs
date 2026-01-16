using App.PageModels;

namespace App.Pages;

public partial class BusinessTripsSummaryPage : ContentPage
{
    public BusinessTripsSummaryPage(BusinessTripsSummaryPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BusinessTripsSummaryPageModel viewModel)
        {
            await viewModel.LoadTripsCommand.ExecuteAsync(null);
        }
    }

    private void SelectedIndexChanged(object sender, EventArgs e)
    {
        if (BindingContext is BusinessTripsSummaryPageModel viewModel)
        {
            viewModel.FilterTripsCommand.Execute(null);
        }
    }
}