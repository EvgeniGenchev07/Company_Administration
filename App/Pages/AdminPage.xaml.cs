using App.PageModels;

namespace App.Pages;

public partial class AdminPage : ContentPage
{
    public AdminPage(AdminPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }

    private void OnDaySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CalendarDay selectedDay)
        {
            ((AdminPageModel)BindingContext).SelectDay(selectedDay);
        }
    }
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AdminPageModel pageModel)
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