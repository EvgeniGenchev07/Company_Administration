using App.PageModels;

namespace App.Pages;


public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterPageModel model)
    {
        BindingContext = model;
        InitializeComponent();
    }
}