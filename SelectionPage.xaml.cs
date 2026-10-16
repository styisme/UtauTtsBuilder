namespace UtauTtsBuilder;

public partial class SelectionPage : ContentPage
{
    private readonly TaskCompletionSource<int> _tcs = new();
    private readonly List<string> _items;
    private readonly int _currentIndex;

    public SelectionPage(string title, List<string> items, int currentIndex)
    {
        InitializeComponent();
        _items = items;
        _currentIndex = currentIndex;

        TitleLabel.Text = title;
        BuildItems();
    }

    public Task<int> WaitResultAsync() => _tcs.Task;

    private void BuildItems()
    {
        ItemsContainer.Clear();

        for (int i = 0; i < _items.Count; i++)
        {
            int index = i;
            bool selected = i == _currentIndex;

            var dot = new Label
            {
                Text = selected ? "●" : "○",
                FontSize = 16,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 10, 0),
                TextColor = selected
                    ? (Color)Application.Current!.Resources["Md3Primary"]
                    : (Color)Application.Current!.Resources["Md3OnSurfaceVariant"]
            };

            var label = new Label
            {
                Text = _items[i],
                FontSize = 15,
                VerticalOptions = LayoutOptions.Center,
                TextColor = (Color)Application.Current!.Resources["Md3OnSurface"]
            };

            var content = new HorizontalStackLayout
            {
                Spacing = 8,
                Children = { dot, label }
            };

            var border = new Border
            {
                StrokeThickness = selected ? 0 : 1,
                Padding = new Thickness(16, 14),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = 12
                },
                Content = content
            };

            if (selected)
            {
                border.SetDynamicResource(Border.BackgroundColorProperty, "Md3PrimaryContainer");
                border.SetDynamicResource(Border.StrokeProperty, "Md3PrimaryContainer");
            }
            else
            {
                border.SetDynamicResource(Border.BackgroundColorProperty, "Md3SurfaceContainer");
                border.SetDynamicResource(Border.StrokeProperty, "Md3OutlineVariant");
            }

            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) => OnItemSelected(index);
            border.GestureRecognizers.Add(tap);

            ItemsContainer.Add(border);
        }
    }

    private async void OnItemSelected(int index)
    {
        _tcs.TrySetResult(index);
        await Navigation.PopAsync();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        _tcs.TrySetResult(-1);
        await Navigation.PopAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        _tcs.TrySetResult(-1);
        return base.OnBackButtonPressed();
    }
}
