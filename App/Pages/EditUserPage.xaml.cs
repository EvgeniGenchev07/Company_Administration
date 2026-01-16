using App.PageModels;
using App.ViewModels;

namespace App.Pages;

public partial class EditUserPage : ContentPage
{
    public static UserViewModel SelectedUser { get; set; }

    public EditUserPage(EditUserPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}