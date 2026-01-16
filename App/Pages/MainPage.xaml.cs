using App.PageModels;

namespace App.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }

    private void OnDaySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CalendarDay selectedDay)
        {
            ((MainPageModel)BindingContext).SelectDay(selectedDay);
        }
    }
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MainPageModel pageModel)
        {
            await pageModel.LoadDataAsync();
            CalendarDay calendarDay = pageModel.CalendarDays.FirstOrDefault(d => d.IsToday);
            if (calendarDay != null)
            {
                pageModel.SelectDay(calendarDay);
            }
        }
    }
}