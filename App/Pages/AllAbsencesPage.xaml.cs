using ServiceLayer.PageModels;
using ApplicationLayer.ViewModels;

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
            ((AllAbsencesPageModel)BindingContext).SelectAbsence(selectedAbsence);
        }
    }
}