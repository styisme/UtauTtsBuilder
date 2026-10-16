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
        // ===== Entry / Picker 去下划线（MD3）=====
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

        // ===== Switch 去白底 =====
        Microsoft.Maui.Handlers.SwitchHandler.Mapper.AppendToMapping(
            "RemoveBackground", (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList =
                    global::Android.Content.Res.ColorStateList.ValueOf(
                        global::Android.Graphics.Color.Transparent);
            });

        // ===== RadioButton 去白底 =====
        Microsoft.Maui.Handlers.RadioButtonHandler.Mapper.AppendToMapping(
            "RemoveBackground", (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList =
                    global::Android.Content.Res.ColorStateList.ValueOf(
                        global::Android.Graphics.Color.Transparent);
            });
#endif

        return builder.Build();
    }
}
