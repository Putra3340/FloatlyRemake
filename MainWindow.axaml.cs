using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FloatlyRemake.Api;
using FloatlyRemake.Models;
using FloatlyRemake.Utils;
using LibVLCSharp.Shared;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FloatlyRemake
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; } = null!;
        DispatcherTimer slidertimer = new DispatcherTimer(); // for slider
        public bool isDragging = false; // dragging slider
        FloatingWindow? fw = null;
        public MainWindow()
        {
            InitializeComponent();
            MainListBox.ItemsSource = StaticBinding.Songs;
            Instance = this;

            VideoView.MediaPlayer = MediaHandler.Player;
            PlayerSlider.AddHandler(InputElement.PointerPressedEvent, PlayerSlider_PointerPressed, RoutingStrategies.Tunnel);
            PlayerSlider.AddHandler(InputElement.PointerReleasedEvent, PlayerSlider_PointerReleased, RoutingStrategies.Tunnel);
            PlayerSlider.AddHandler(InputElement.PointerCaptureLostEvent, PlayerSlider_PointerCaptureLost, RoutingStrategies.Tunnel);

            // Rules : Only Listen on here
            MediaHandler.Player.Opening += MediaHandler_Opening;
            MediaHandler.Player.Playing += MediaHandler_Playing;
            MediaHandler.Player.Paused += MediaHandler_Paused;
            MediaHandler.Player.Stopped += MediaHandler_Stopped;
            MediaHandler.Player.EndReached += MediaHandler_Finished;
            MediaHandler.Player.EncounteredError += MediaHandler_Error;
            MediaHandler.Player.Buffering += MediaHandler_Buffering;

            slidertimer.Interval = TimeSpan.FromMilliseconds(100);
            slidertimer.Tick += SliderTimer_Tick;
            slidertimer.Start();
            MediaHandler.OnNewMediaLoaded += MediaHandler_OnVideoLoaded;

        }

        private void MediaHandler_Buffering(object? sender, MediaPlayerBufferingEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void MediaHandler_Error(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MediaHandler_Finished(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MediaHandler_Stopped(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MediaHandler_Paused(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                // Main
                var targetBorder = BtnPlayPause.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.OpacityMask is ImageBrush);
                if (targetBorder?.OpacityMask is ImageBrush brush)
                {
                    brush.Source = new Bitmap(
                        AssetLoader.Open(new Uri("avares://FloatlyRemake/Assets/Images/icon-pause.png")));
                }
                // Floating Window
                var fwBorder = FloatingWindow.Instance?.BtnPlayPause.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.OpacityMask is ImageBrush);
                if (fwBorder?.OpacityMask is ImageBrush fwBrush)
                {
                    fwBrush.Source = new Bitmap(
                        AssetLoader.Open(new Uri("avares://FloatlyRemake/Assets/Images/icon-pause.png"))
                    );
                }
                targetBorder?.InvalidateVisual();
                fwBorder?.InvalidateVisual();
                BtnPlayPause.InvalidateVisual();
                FloatingWindow.Instance?.BtnPlayPause.InvalidateVisual();
            });
        }

        private void MediaHandler_Opening(object? sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void MediaHandler_Playing(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                // Main
                var targetborder = BtnPlayPause.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.OpacityMask is ImageBrush);
                if (targetborder?.OpacityMask is ImageBrush brush)
                {
                    brush.Source = new Bitmap(
                        AssetLoader.Open(new Uri("avares://FloatlyRemake/Assets/Images/icon-resume.png"))
                    );
                }
                // Floating Window
                var fwBorder = FloatingWindow.Instance?.BtnPlayPause.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.OpacityMask is ImageBrush);
                if (fwBorder?.OpacityMask is ImageBrush fwBrush)
                {
                    fwBrush.Source = new Bitmap(
                        AssetLoader.Open(new Uri("avares://FloatlyRemake/Assets/Images/icon-resume.png"))
                    );
                }
                targetborder?.InvalidateVisual();
                fwBorder?.InvalidateVisual();
                BtnPlayPause.InvalidateVisual();
                FloatingWindow.Instance?.BtnPlayPause.InvalidateVisual();
            });
        }

        private void SliderTimer_Tick(object? sender, EventArgs e)
        {
            if (!isDragging && MediaHandler.Player.Length > 0)
            {
                var player = MediaHandler.Player;

                // Total duration
                PlayerSlider.Maximum = player.Length;
                FloatingWindow.Instance?.PlayerSlider.Maximum = player.Length;

                // Current time
                PlayerSlider.Value = player.Time;
                FloatingWindow.Instance?.PlayerSlider.Value = player.Time;

                // Format current time
                var current = TimeSpan.FromMilliseconds(player.Time);
                Lbl_Duration.Text = current.ToString(@"mm\:ss");
                FloatingWindow.Instance?.Lbl_Duration.Text = current.ToString(@"mm\:ss");

                // Format total time
                var total = TimeSpan.FromMilliseconds(player.Length);
                Lbl_MaxDuration.Text = total.ToString(@"mm\:ss");
                FloatingWindow.Instance?.Lbl_MaxDuration.Text = total.ToString(@"mm\:ss");
            }
            if (StaticBinding.LyricLists.Count > 0)
            {
                var currentTime = TimeSpan.FromMilliseconds(MediaHandler.Player.Time);
                foreach (var lyric in StaticBinding.LyricLists)
                {
                    if (lyric.Start <= currentTime && lyric.End >= currentTime)
                    {
                        Lbl_Lyric.Text = lyric.CombinedText;
                        FloatingWindow.Instance?.Lbl_ActiveLyric.Text = lyric.CombinedText;
                        break;
                    }
                }
            }
        }

        private void Nav_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is not Button btn)
                return;

        }

        private void Tbx_Search_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
                ApiLibrary.Search(Tbx_Search.Text);
        }

        private void Control_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is not Button btn)
                return;
            if (btn.Name == "BtnPiP")
            {
                ApiLibrary.PlayVideo(StaticBinding.CurrentSong.Id);
                fw ??= new FloatingWindow();
                fw.Show();
            }
            if (btn.Name == "BtnFullScreen")
            {

            }
            if (btn.Name == "BtnPlaylist")
            {

            }
            if (btn.Name == "BtnEqualizer")
            {

            }
            if (btn.Name == "BtnPrev")
            {

            }
            if (btn.Name == "BtnPlayPause")
            {
                MediaHandler.PauseResume();
            }
            if (btn.Name == "BtnNext")
            {

            }
            if (btn.Name == "BtnLoop")
            {

            }
            if (btn.Name == "BtnLike")
            {

            }
        }

        private void DragWindow(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            BeginMoveDrag(e);
        }

        private void Song_Selected(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox || listBox.SelectedItem is not Song song)
                return;

            ApiLibrary.Play(song.Id);
        }
        private void PlayerSlider_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            isDragging = true;
        }

        private void PlayerSlider_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            isDragging = false;
            MediaHandler.SeekTo(TimeSpan.FromMilliseconds(PlayerSlider.Value)); // ms
        }

        private void PlayerSlider_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
        {
            isDragging = false;
        }
        public async Task ShowNotification(string message)
        {
            var notif = new Notification { Message = message };
            StaticBinding.NotificationList.Add(notif);

            await Task.Delay(3000);

            StaticBinding.NotificationList.Remove(notif);
        }
        private void MediaHandler_OnVideoLoaded(bool isVideo)
        {
            VideoView.IsVisible = true;
            VideoView.MediaPlayer = MediaHandler.Player;

            if (isVideo)
            {
                // Make it visible BEFORE play so it can create a native surface
                VideoView.IsVisible = true;
                // We trigger playback here (or right after), VLC will embed properly
                MediaHandler.Play(StaticBinding.CurrentSong.MoviePath); // or wherever you call Play()
            }
            else
            {
                VideoView.IsVisible = false;
                MediaHandler.Play(StaticBinding.CurrentSong.Music); // or wherever you call Play()
            }
        }

        private void Minimize_Click(object? sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object? sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}