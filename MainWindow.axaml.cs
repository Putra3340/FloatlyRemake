using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FloatlyRemake.Api;
using FloatlyRemake.Models;
using FloatlyRemake.Utils;
using LibVLCSharp.Shared;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FloatlyRemake
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; } = null!;
        DispatcherTimer slidertimer = new DispatcherTimer(); // for slider
        bool isDragging = false; // dragging slider
        public MainWindow()
        {
            InitializeComponent();
            MainListBox.ItemsSource = StaticBinding.Songs;
            Instance = this;

            VideoView.MediaPlayer = MediaHandler.Player;
            PlayerSlider.AddHandler(InputElement.PointerPressedEvent, PlayerSlider_PointerPressed, RoutingStrategies.Tunnel);
            PlayerSlider.AddHandler(InputElement.PointerReleasedEvent, PlayerSlider_PointerReleased, RoutingStrategies.Tunnel);
            PlayerSlider.AddHandler(InputElement.PointerCaptureLostEvent, PlayerSlider_PointerCaptureLost, RoutingStrategies.Tunnel);


            slidertimer.Interval = TimeSpan.FromMilliseconds(100); // set it to very low if building a music player with lyrics support
            slidertimer.Tick += SliderTimer_Tick;
            slidertimer.Start();

        }

        private void SliderTimer_Tick(object? sender, EventArgs e)
        {
            if (!isDragging && MediaHandler.Player.Length > 0)
            {
                var player = MediaHandler.Player;

                // Total duration
                PlayerSlider.Maximum = player.Length;

                // Current time
                PlayerSlider.Value = player.Time;

                // Format current time
                var current = TimeSpan.FromMilliseconds(player.Time);
                Lbl_Duration.Text = current.ToString(@"mm\:ss");

                // Format total time
                var total = TimeSpan.FromMilliseconds(player.Length);
                Lbl_MaxDuration.Text = total.ToString(@"mm\:ss");
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
        public async Task ShowNotification(string message)
        {
            var notif = new Notification { Message = message };
            StaticBinding.NotificationList.Add(notif);

            await Task.Delay(3000);

            StaticBinding.NotificationList.Remove(notif);
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
    }
}