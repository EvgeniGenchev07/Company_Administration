using App.PageModels;

namespace App.Pages;


public partial class RequestPage : ContentPage
{
    public RequestPage(RequestPageModel model)
    {
        BindingContext = model;
        InitializeComponent();
    }
    private void OnDestinationCityTextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is RequestPageModel vm)
        {
            vm.FilterCitiesCommand.Execute(e.NewTextValue);
        }
    }

    private void OnReturnCityTextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is RequestPageModel vm)
        {
            vm.FilterCitiesCommand.Execute(e.NewTextValue);
        }
    }
}