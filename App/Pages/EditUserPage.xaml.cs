using ApplicationLayer.ViewModels;
using BusinessLayer.Entities;
using ServiceLayer.PageModels;

namespace App.Pages;

[QueryProperty(nameof(User), "User")]
public partial class EditUserPage : ContentPage
{
    public UserViewModel User { get; set; }

    public EditUserPage(EditUserPageModel viewModel)
    {
        InitializeComponent();
        EditUserPageModel.SelectedUser = User;
        BindingContext = viewModel;
    }
}