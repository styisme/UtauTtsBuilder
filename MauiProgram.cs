using Microsoft.Extensions.Logging;

namespace UtauTtsBuilder;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

#if ANDROID
        // ===== 移除 Entry / Picker 默认下划线（MD3 风格要求） =====
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(
            "RemoveUnderline", (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList =
                    global::Android.Content.Res.ColorStateList.ValueOf(
                        global::Android.Graphics.Color.Transparent);
            });

        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping(
            "RemoveUnderline", (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList =
                    global::Android.Content.Res.ColorStateList.ValueOf(
                        global::Android.Graphics.Color.Transparent);
            });
#endif

        return builder.Build();
    }
}