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
        // Entry / Picker 去下划线
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

        // Switch 统一样式（track + thumb）
        Microsoft.Maui.Handlers.SwitchHandler.Mapper.AppendToMapping(
            "UnifiedSwitch", (handler, view) =>
            {
                var ctx = global::Microsoft.Maui.ApplicationModel.Platform.AppContext;
                if (ctx == null) return;

                // thumb 白色
                handler.PlatformView.ThumbTintList =
                    global::Android.Content.Res.ColorStateList.ValueOf(
                        global::Android.Graphics.Color.White);

                // track 颜色由 MAUI 的 OnColor 属性控制
                // 这里只统一 track 的透明度
                var bg = handler.PlatformView.BackgroundTintList;
                if (bg == null)
                {
                    handler.PlatformView.BackgroundTintList =
                        global::Android.Content.Res.ColorStateList.ValueOf(
                            global::Android.Graphics.Color.Transparent);
                }
            });

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
