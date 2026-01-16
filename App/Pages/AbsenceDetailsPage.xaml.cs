using ServiceLayer.PageModels;
using ApplicationLayer.ViewModels;
using BusinessLayer.Entities;
using ServiceLayer.PageModels;

namespace App.Pages;

[QueryProperty(nameof(Absence), "Absence")]
public partial class AbsenceDetailsPage : ContentPage
{
    public AbsenceViewModel Absence { get; set; }

    public AbsenceDetailsPage(AbsenceDetailsPageModel pageModel)
    {
        InitializeComponent();
        AbsenceDetailsPageModel.SelectedAbsence = Absence;
        BindingContext = pageModel;
    }
}