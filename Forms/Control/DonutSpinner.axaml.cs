using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Styling;
using Avalonia.Threading;
using System;
using System.Threading;

namespace FloatlyRemake;

public partial class DonutSpinner : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<DonutSpinner, string>(nameof(Text), "Loading...");

    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<DonutSpinner, double>(nameof(Size), 36);

    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<DonutSpinner, double>(nameof(Thickness), 4);

    public static readonly StyledProperty<IBrush> StrokeProperty =
        AvaloniaProperty.Register<DonutSpinner, IBrush>(nameof(Stroke), Brushes.DodgerBlue);

    public string Text { get => GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public double Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
    public double Thickness { get => GetValue(ThicknessProperty); set => SetValue(ThicknessProperty, value); }
    public IBrush Stroke { get => GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }

    private RotateTransform? _rot;

    private readonly Animation _spin = new()
    {
        Duration = TimeSpan.FromMilliseconds(900),
        IterationCount = IterationCount.Infinite,
        Children =
        {
            new KeyFrame
            {
                Cue = new Cue(0d),
                Setters = { new Setter(RotateTransform.AngleProperty, 0d) }
            },
            new KeyFrame
            {
                Cue = new Cue(1d),
                Setters = { new Setter(RotateTransform.AngleProperty, 360d) }
            }
        }
    };

    private DispatcherTimer? _timer;

    public DonutSpinner()
    {
        InitializeComponent();

        var ring = this.FindControl<Ellipse>("Ring");
        _rot = ring.RenderTransform as RotateTransform;

        void UpdateCenter()
        {
            if (_rot == null) return;
            _rot.CenterX = Size / 2;
            _rot.CenterY = Size / 2;
        }

        UpdateCenter();
        this.GetObservable(SizeProperty)
    .Subscribe(new AnonymousObserver<double>(_ => UpdateCenter()));

        AttachedToVisualTree += (_, __) =>
        {
            if (_rot == null) return;

            _timer ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // ~60fps
            _timer.Tick += (_, __) => _rot.Angle = (_rot.Angle + 6) % 360; // speed here
            _timer.Start();
        };

        DetachedFromVisualTree += (_, __) =>
        {
            _timer?.Stop();
        };
    }
}