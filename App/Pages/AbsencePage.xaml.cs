using App.PageModels;

namespace App.Pages;

public partial class AbsencePage : ContentPage
{
    public AbsencePage(AbsencePageModel pageModel)
    {
        InitializeComponent();
        BindingContext = pageModel;
    }
}