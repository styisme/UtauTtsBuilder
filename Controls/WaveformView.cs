namespace UtauTtsBuilder.Controls;

public class WaveformView : GraphicsView
{
    private readonly WaveformDrawable _drawable = new();

    public WaveformView()
    {
        Drawable = _drawable;
    }

    public static readonly BindableProperty WaveformDataProperty =
        BindableProperty.Create(
            nameof(WaveformData), typeof(float[]), typeof(WaveformView), null,
            propertyChanged: (b, o, n) =>
            {
                if (b is WaveformView v)
                {
                    v._drawable.Data = (float[]?)n;
                    v.Invalidate();
                }
            });

    public float[]? WaveformData
    {
        get => (float[]?)GetValue(WaveformDataProperty);
        set => SetValue(WaveformDataProperty, value);
    }

    public static readonly BindableProperty WaveColorProperty =
        BindableProperty.Create(
            nameof(WaveColor), typeof(Color), typeof(WaveformView), Colors.SlateBlue,
            propertyChanged: (b, o, n) =>
            {
                if (b is WaveformView v)
                {
                    v._drawable.Color = (Color?)n ?? Colors.SlateBlue;
                    v.Invalidate();
                }
            });

    public Color WaveColor
    {
        get => (Color)GetValue(WaveColorProperty);
        set => SetValue(WaveColorProperty, value);
    }

    private class WaveformDrawable : IDrawable
    {
        public float[]? Data { get; set; }
        public Color Color { get; set; } = Colors.SlateBlue;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // 背景
            canvas.FillColor = Colors.Transparent;
            canvas.FillRectangle(dirtyRect);

            if (Data == null || Data.Length == 0)
            {
                canvas.StrokeColor = Colors.Gray;
                canvas.StrokeSize = 1;
                canvas.DrawLine(0, dirtyRect.Height / 2,
                    dirtyRect.Width, dirtyRect.Height / 2);
                return;
            }

            int bars = Data.Length / 2;
            float barW = dirtyRect.Width / bars;
            float midY = dirtyRect.Height / 2;

            canvas.FillColor = Color;
            for (int i = 0; i < bars; i++)
            {
                float max = Data[i * 2 + 1];
                float h = max * midY;
                canvas.FillRectangle(
                    i * barW, midY - h,
                    Math.Max(1, barW - 0.5f), h * 2);
            }
        }
    }
}
