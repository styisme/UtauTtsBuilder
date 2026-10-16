namespace UtauTtsBuilder.Services;

public static class DialogService
{
    /// <summary>单按钮提示弹窗</summary>
    public static async Task ShowAlertAsync(
        Page page, string title, string message, string buttonText)
    {
        var alert = new AlertPage(title, message, buttonText);
        var task = alert.WaitResultAsync();
        await page.Navigation.PushModalAsync(alert);
        await task;
    }

    /// <summary>双按钮确认弹窗，返回 true 表示点了确认</summary>
    public static async Task<bool> ShowConfirmAsync(
        Page page, string title, string message,
        string acceptText, string cancelText)
    {
        var alert = new AlertPage(title, message, cancelText, acceptText);
        var task = alert.WaitResultAsync();
        await page.Navigation.PushModalAsync(alert);
        return await task;
    }
}
