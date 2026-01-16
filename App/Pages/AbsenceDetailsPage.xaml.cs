using App.PageModels;
using App.ViewModels;

namespace App.Pages;

public partial class AbsenceDetailsPage : ContentPage
{
    public static AbsenceViewModel SelectedAbsence { get; set; }

    public AbsenceDetailsPage(AbsenceDetailsPageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
}