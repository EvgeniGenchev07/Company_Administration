using ServiceLayer.PageModels;
namespace App.Pages;
public partial class AddProjectPage : ContentPage
{
	public AddProjectPage(AddProjectPageModel viewModel)
	{
		InitializeComponent();
		BindingContext= viewModel;
	}
}