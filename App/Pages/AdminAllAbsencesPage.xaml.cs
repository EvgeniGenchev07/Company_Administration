using App.PageModels;

namespace App.Pages;

public partial class AdminAllAbsencesPage : ContentPage
{
    public AdminAllAbsencesPage(AdminAllAbsencesPageModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminAllAbsencesPageModel pageModel)
        {
            await pageModel.LoadAbsencesCommand.ExecuteAsync(null);
        }
    }
}