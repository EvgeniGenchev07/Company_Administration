using App.PageModels;

namespace App.Pages;

public partial class AdminAllBusinessTripsPage : ContentPage
{
    public AdminAllBusinessTripsPage(AdminAllBusinessTripsPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminAllBusinessTripsPageModel viewModel)
        {
            await viewModel.LoadBusinessTripsAsync();
        }
    }
}