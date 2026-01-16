using App.PageModels;
using App.ViewModels;

namespace App.Pages;

public partial class AllAbsencesPage : ContentPage
{
    public AllAbsencesPage(AllAbsencesPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }

    private void OnAbsenceSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is AbsenceViewModel selectedAbsence)
        {
            // Could navigate to absence details page in the future
            ((AllAbsencesPageModel)BindingContext).SelectAbsence(selectedAbsence);
        }
    }
}