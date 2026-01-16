namespace App;

public partial class App : Application
{
    internal static User User { get; set; }

    public App()
    {
        InitializeComponent();
        Current.UserAppTheme = AppTheme.Light;

        var culture = CultureInfo.CreateSpecificCulture("bg-BG");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}