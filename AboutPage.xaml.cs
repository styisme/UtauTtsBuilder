using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class AboutPage : ContentPage
{
    private const string GithubUrl = "https://github.com/styisme/UtauTtsBuilder";

    public AboutPage()
    {
        InitializeComponent();
        ApplyLocalization();
        FillInfo();
    }

    private void ApplyLocalization()
    {
        TopTitle.Text = Loc.T("about_title");
        GithubBtn.Text = Loc.T("about_github");
        LicenseBtn.Text = Loc.T("about_license");
        LicenseTitle.Text = Loc.T("about_license_title");
        LicenseBody.Text = Loc.T("about_license_body");
    }

    private void FillInfo()
    {
        AppNameLabel.Text = AppInfo.Current.Name;
        VersionLabel.Text = "v" + AppInfo.Current.VersionString;
        DescLabel.Text = Loc.T("about_desc");
        CopyrightLabel.Text = "© " + DateTime.Now.Year + " styisme";
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnGithubClicked(object sender, EventArgs e)
    {
        try
        {
            await Launcher.Default.OpenAsync(GithubUrl);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, Loc.T("btn_ok"));
        }
    }

    private async void OnLicenseClicked(object sender, EventArgs e)
    {
        try
        {
            await Launcher.Default.OpenAsync(GithubUrl + "/blob/main/LICENSE");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, Loc.T("btn_ok"));
        }
    }
}
