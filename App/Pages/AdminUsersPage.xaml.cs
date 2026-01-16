using App.PageModels;

namespace App.Pages;

public partial class AdminUsersPage : ContentPage
{
    public AdminUsersPage(AdminUsersPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminUsersPageModel viewModel)
        {
            viewModel.LoadUsersAsync();
        }
    }
}