using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using FloatlyRemake.Models;
using FloatlyRemake.Utils;
using SharpHook;
using SharpHook.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FloatlyRemake;

public partial class FloatingWindow : Window
{
    public static FloatingWindow Instance { get; private set; }
    public static DispatcherTimer hideTimer = new DispatcherTimer
    {
        Interval = TimeSpan.FromSeconds(2)
    };
    private DispatcherTimer? _animTimer;
    private readonly SimpleGlobalHook _hook = new();
    private readonly HashSet<KeyCode> _pressedKeys = new();
    bool IsMoving = false;
    public enum WindowLocation { TopLeft, TopRight, BottomRight, BottomLeft }
    private WindowLocation Location;
    private double _workAreaWidth;
    private double _workAreaHeight;
    private PixelRect _workingArea;
    public FloatingWindow()
    {
        InitializeComponent();
        Instance = this;

        this.AddHandler(InputElement.PointerPressedEvent, Window_ControlBarToggle, RoutingStrategies.Tunnel);
        PlayerSlider.AddHandler(InputElement.PointerPressedEvent, PlayerSlider_PointerPressed, RoutingStrategies.Tunnel);
        PlayerSlider.AddHandler(InputElement.PointerReleasedEvent, PlayerSlider_PointerReleased, RoutingStrategies.Tunnel);
        PlayerSlider.AddHandler(InputElement.PointerCaptureLostEvent, PlayerSlider_PointerCaptureLost, RoutingStrategies.Tunnel);

        hideTimer.Tick += (s, e) =>
        {
            hideTimer.Stop();
            ControlBar.Opacity = 1;
            FadeControlBar(ControlBar.Opacity, 0);
            ControlBar.Opacity = 0;
        };
        _hook.KeyPressed += KeyPressed;
        _hook.KeyReleased += KeyReleased;

        Task.Run(() => _hook.RunAsync());
        Opened += (_, _) =>
        {
            var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary!;
            _workingArea = screen.WorkingArea;
        };
    }

    private void KeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        _pressedKeys.Add(e.Data.KeyCode);

        // Single keys
        if (e.Data.KeyCode == KeyCode.VcF12) return;
        if (e.Data.KeyCode == KeyCode.VcF11) return;
        if (e.Data.KeyCode == KeyCode.VcLeftControl) IsMoving = true;

        // Helper for Ctrl+Shift combos
        bool ctrl = _pressedKeys.Contains(KeyCode.VcLeftControl) || _pressedKeys.Contains(KeyCode.VcRightControl);
        bool shift = _pressedKeys.Contains(KeyCode.VcLeftShift) || _pressedKeys.Contains(KeyCode.VcRightShift);

        if (!(ctrl && shift)) return;

        // Ctrl+Shift+Space => Toggle pause
        if (e.Data.KeyCode == KeyCode.VcSpace) MediaHandler.PauseResume();

        // Ctrl+Shift+A => Move left
        if (e.Data.KeyCode == KeyCode.VcA)
        {
            if (Location == WindowLocation.TopRight) MoveWindow(WindowLocation.TopLeft);
            else if (Location == WindowLocation.TopLeft) MoveWindow(WindowLocation.TopLeft);
            else if (Location == WindowLocation.BottomRight) MoveWindow(WindowLocation.BottomLeft);
            else if (Location == WindowLocation.BottomLeft) MoveWindow(WindowLocation.BottomLeft);
            return;
        }

        // Ctrl+Shift+D => Move right
        if (e.Data.KeyCode == KeyCode.VcD)
        {
            if (Location == WindowLocation.TopRight) MoveWindow(WindowLocation.TopRight);
            else if (Location == WindowLocation.TopLeft) MoveWindow(WindowLocation.TopRight);
            else if (Location == WindowLocation.BottomRight) MoveWindow(WindowLocation.BottomRight);
            else if (Location == WindowLocation.BottomLeft) MoveWindow(WindowLocation.BottomRight);
            return;
        }

        // Ctrl+Shift+S => Move down
        if (e.Data.KeyCode == KeyCode.VcS)
        {
            if (Location == WindowLocation.TopRight) MoveWindow(WindowLocation.BottomRight);
            else if (Location == WindowLocation.TopLeft) MoveWindow(WindowLocation.BottomLeft);
            else if (Location == WindowLocation.BottomRight) MoveWindow(WindowLocation.BottomRight);
            else if (Location == WindowLocation.BottomLeft) MoveWindow(WindowLocation.BottomLeft);
            return;
        }

        // Ctrl+Shift+W => Move up
        if (e.Data.KeyCode == KeyCode.VcW)
        {
            if (Location == WindowLocation.TopRight) MoveWindow(WindowLocation.TopRight);
            else if (Location == WindowLocation.TopLeft) MoveWindow(WindowLocation.TopLeft);
            else if (Location == WindowLocation.BottomRight) MoveWindow(WindowLocation.TopRight);
            else if (Location == WindowLocation.BottomLeft) MoveWindow(WindowLocation.TopLeft);
            return;
        }

        // Ctrl+Shift+Right/Left => Seek
        if (e.Data.KeyCode == KeyCode.VcRight)
        {
            MediaHandler.SeekTo(TimeSpan.FromMilliseconds(MediaHandler.Player.Time).Add(TimeSpan.FromSeconds(5)));
            return;
        }
        if (e.Data.KeyCode == KeyCode.VcLeft)
        {
            MediaHandler.SeekTo(TimeSpan.FromMilliseconds(MediaHandler.Player.Time).Subtract(TimeSpan.FromSeconds(5)));
            return;
        }

        // Ctrl+Shift + Plus/Minus => Volume
        // Note: On many layouts "plus" is Shift+'=' key -> KeyCode.VcEquals (not always VcPlus).
        if (e.Data.KeyCode == KeyCode.VcUp)
        {
            MediaHandler.SetVolume(MediaHandler.Player.Volume + 5);
            return;
        }
        if (e.Data.KeyCode == KeyCode.VcDown)
        {
            MediaHandler.SetVolume(MediaHandler.Player.Volume - 5);
            return;
        }

        // Ctrl+Shift+M => Mute
        if (e.Data.KeyCode == KeyCode.VcM)
        {
            MediaHandler.Player.Mute = !MediaHandler.Player.Mute;
            return;
        }
    }

    private void KeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        _pressedKeys.Remove(e.Data.KeyCode);
        if (e.Data.KeyCode == KeyCode.VcLeftControl) IsMoving = false;
    }
    private void Animate(double toLeft, double toTop)
    {
        _animTimer?.Stop();

        var duration = TimeSpan.FromMilliseconds(300);
        var start = DateTime.UtcNow;

        var from = Position;
        var fromX = (double)from.X;
        var fromY = (double)from.Y;

        _animTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, __) =>
        {
            var t = (DateTime.UtcNow - start).TotalMilliseconds / duration.TotalMilliseconds;
            if (t >= 1)
            {
                Position = new PixelPoint((int)Math.Round(toLeft), (int)Math.Round(toTop));
                _animTimer?.Stop();
                return;
            }

            var eased = 1 - Math.Pow(1 - t, 3);
            var x = fromX + (toLeft - fromX) * eased;
            var y = fromY + (toTop - fromY) * eased;

            Position = new PixelPoint((int)Math.Round(x), (int)Math.Round(y));
        });

        _animTimer.Start();
    }
    private void MoveWindow(WindowLocation location)
    {
        Dispatcher.UIThread.Post(() =>
        {
            const int margin = 20;

            // Fallback if called before Opened
            if (_workingArea.Width <= 0 || _workingArea.Height <= 0)
            {
                var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary!;
                _workingArea = screen.WorkingArea;
            }

            double toX, toY;

            switch (location)
            {
                case WindowLocation.TopLeft:
                    toX = _workingArea.X + margin;
                    toY = _workingArea.Y + margin;
                    break;

                case WindowLocation.TopRight:
                    toX = _workingArea.X + _workingArea.Width - Width - margin;
                    toY = _workingArea.Y + margin;
                    break;

                case WindowLocation.BottomRight:
                    toX = _workingArea.X + _workingArea.Width - Width - margin;
                    toY = _workingArea.Y + _workingArea.Height - Height - margin;
                    break;

                case WindowLocation.BottomLeft:
                default:
                    toX = _workingArea.X + margin;
                    toY = _workingArea.Y + _workingArea.Height - Height - margin;
                    break;
            }

            Animate(toX, toY);
            Location = location;
        });
    }

    private void Window_ControlBarToggle(object? sender, PointerPressedEventArgs e)
    {
        ControlBar.Opacity = 0;
        FadeControlBar(ControlBar.Opacity, 1);
        ControlBar.Opacity = 1;
        hideTimer.Start();
    }

    private void Control_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not Button btn)
            return;
        if (btn.Name == "BtnPrev")
        {
            MediaHandler.SeekTo(TimeSpan.FromMilliseconds(0));
        }
        if (btn.Name == "BtnPlayPause")
        {
            MediaHandler.PauseResume();
        }
        if (btn.Name == "BtnNext")
        {
            MediaHandler.SeekTo(TimeSpan.FromMilliseconds(MediaHandler.Player.Length));
        }
        if(btn.Name == "BtnClose")
        {
            this.Close();
            Instance = null!;
        }
    }
    private void PlayerSlider_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        MainWindow.Instance.isDragging = true;
    }

    private void PlayerSlider_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        MainWindow.Instance.isDragging = false;
        MediaHandler.SeekTo(TimeSpan.FromMilliseconds(PlayerSlider.Value)); // ms
    }

    private void PlayerSlider_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        MainWindow.Instance.isDragging = false;
    }
    private CancellationTokenSource? _fadeCts;

    private async Task FadeControlBar(double from, double to)
    {
        _fadeCts?.Cancel();
        _fadeCts = new CancellationTokenSource();
        var token = _fadeCts.Token;

        var anim = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(300),
            Easing = new CubicEaseOut(),
            Children =
        {
            new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(Control.OpacityProperty, from) } },
            new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(Control.OpacityProperty, to) } },
        }
        };

        try
        {
            await anim.RunAsync(ControlBar, token);
        }
        catch (TaskCanceledException)
        {
            // Expected when a new fade interrupts the old one.
        }
    }

    private void DragWindow(object? sender, PointerPressedEventArgs e)
    {
        if(IsMoving)
            BeginMoveDrag(e);
    }
}