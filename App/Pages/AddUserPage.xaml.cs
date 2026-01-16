using App.PageModels;

namespace App.Pages;

public partial class AddUserPage : ContentPage
{
    public AddUserPage(AddUserPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}