using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloatlyRemake.Utils
{
    public static class GlobalLoading
    {
        public static void SearchLoading(bool isLoading, string text = "Loading...")
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (isLoading)
                {
                    MainWindow.Instance.Tbx_Search.IsEnabled = false;
                    MainWindow.Instance.MainListBox.IsEnabled = false;
                    MainWindow.Instance.Loading_Search.IsVisible = true;
                    MainWindow.Instance.Loading_Search.Text = text;
                }
                else
                {
                    MainWindow.Instance.Tbx_Search.IsEnabled = true;
                    MainWindow.Instance.MainListBox.IsEnabled = true;
                    MainWindow.Instance.Loading_Search.IsVisible = false;
                }
            });
        }
        public static void MediaLoading(bool isLoading, string text = "Loading...")
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (isLoading)
                {
                    MainWindow.Instance.PlayerCard.IsEnabled = false;
                    MainWindow.Instance.Loading_Player.IsVisible = true;
                    FloatingWindow.Instance?.Loading_Player.IsVisible = true;
                    MainWindow.Instance.MainListBox.IsHitTestVisible = false;
                    MainWindow.Instance.Loading_Player.Text = text;
                    FloatingWindow.Instance?.Loading_Player.Text = text;
                }
                else
                {
                    MainWindow.Instance.PlayerCard.IsEnabled = true;
                    MainWindow.Instance.Loading_Player.IsVisible = false;
                    MainWindow.Instance.MainListBox.IsHitTestVisible = true;
                    FloatingWindow.Instance?.Loading_Player.IsVisible = false;
                }
                MainWindow.Instance?.Loading_Player.InvalidateVisual();
                FloatingWindow.Instance?.Loading_Player.InvalidateVisual();
            });
        }
        public static void MediaInfo(bool isLoading, string text = "Loading...")
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (isLoading)
                {
                    FloatingWindow.Instance?.Loading_Player.IsVisible = true;
                    FloatingWindow.Instance?.Loading_Player.Text = text;
                }
                else
                {
                    FloatingWindow.Instance?.Loading_Player.IsVisible = false;
                }
                FloatingWindow.Instance?.Loading_Player.InvalidateVisual();
            });
        }
    }
}
