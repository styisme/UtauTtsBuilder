using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public partial class CloudSettingsPage : ContentPage
{
    public CloudSettingsPage()
    {
        InitializeComponent();
        ApplyLocalization();
        LoadCurrent();
    }

    private void ApplyLocalization()
    {
        TopTitle.Text = Loc.T("cloud_title");
        DescLabel.Text = Loc.T("cloud_desc");
        AzureTitle.Text = Loc.T("cloud_azure_title");
        GoogleTitle.Text = Loc.T("cloud_google_title");
        SaveBtn.Text = Loc.T("btn_save");
        HintLabel.Text = Loc.T("cloud_hint");
        AzureKeyEntry.Placeholder = "Azure API Key";
        AzureRegionEntry.Placeholder = "eastasia";
        AzureVoiceEntry.Placeholder = "ja-JP-NanamiNeural";
        GoogleKeyEntry.Placeholder = "Google API Key";
    }

    private void LoadCurrent()
    {
        AzureKeyEntry.Text = CloudConfigService.AzureApiKey;
        AzureRegionEntry.Text = CloudConfigService.AzureRegion;
        AzureVoiceEntry.Text = CloudConfigService.AzureVoice;
        GoogleKeyEntry.Text = CloudConfigService.GoogleApiKey;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        CloudConfigService.AzureApiKey = AzureKeyEntry.Text?.Trim() ?? "";
        CloudConfigService.AzureRegion = string.IsNullOrWhiteSpace(AzureRegionEntry.Text)
            ? "eastasia" : AzureRegionEntry.Text.Trim();
        CloudConfigService.AzureVoice = string.IsNullOrWhiteSpace(AzureVoiceEntry.Text)
            ? "ja-JP-NanamiNeural" : AzureVoiceEntry.Text.Trim();
        CloudConfigService.GoogleApiKey = GoogleKeyEntry.Text?.Trim() ?? "";

        await DialogService.ShowAlertAsync(this, Loc.T("btn_save"),
            Loc.T("cloud_saved"), Loc.T("btn_ok"));
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
