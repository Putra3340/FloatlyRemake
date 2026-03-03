using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace FloatlyRemake.Models
{
    public class Song : INotifyPropertyChanged
    {
        public string? Id { get => field; set { field = value; OnPropertyChanged(); } }
        public string? ExtId { get => field; set { field = value; OnPropertyChanged(); } }
        public string? Title { get => field; set { field = value; OnPropertyChanged(); } }
        public string? Music { get => field; set { field = value; OnPropertyChanged(); } }
        public string? Lyrics { get => field; set { field = value; OnPropertyChanged(); } }
        private Bitmap? _coverBitmap;
        private Task<Bitmap?>? _coverLoadTask;

        public string? Cover
        {
            get => field;
            set
            {
                field = value;
                _coverBitmap = null;
                _coverLoadTask = null;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CoverBitmap));
            }
        }

        public Bitmap? CoverBitmap
        {
            get
            {
                if (_coverBitmap == null)
                    _coverLoadTask ??= LoadCoverAsync(); // fire once
                return _coverBitmap;
            }
            private set
            {
                _coverBitmap = value;
                OnPropertyChanged();
            }
        }

        public string? Banner { get => field; set { field = value; OnPropertyChanged(); } }
        public string? MoviePath { get => field; set { field = value; OnPropertyChanged(); } }
        public string? HDMoviePath { get => field; set { field = value; OnPropertyChanged(); } }
        public string? UploadedBy { get => field; set { field = value; OnPropertyChanged(); } }
        public string? SongLength { get => field; set { field = value; OnPropertyChanged(); } }
        public string? PlayCount { get => field; set { field = value; OnPropertyChanged(); } }

        public string? ArtistId { get => field; set { field = value; OnPropertyChanged(); } }
        public string? ArtistName { get => field; set { field = value; OnPropertyChanged(); } }
        public string? ArtistBio { get => field; set { field = value; OnPropertyChanged(); } }
        public string? ArtistCover { get => field; set { field = value; OnPropertyChanged(); } } = "/Assets/Images/default.png";
        public DateTime CreatedAt { get => field; set { field = value; OnPropertyChanged(); } }
        public string? NextQueueImage { get; set { field = value; OnPropertyChanged(); } } = "/Assets/Images/default.png";
        public string? NextQueueTitle { get; set { field = value; OnPropertyChanged(); } } = "Next Up Title";
        public async Task<Bitmap?> LoadCoverAsync()
        {
            // IMPORTANT: use backing field, not CoverBitmap property
            if (_coverBitmap != null)
                return _coverBitmap;

            var cover = Cover;
            if (string.IsNullOrWhiteSpace(cover))
                return null;

            try
            {
                Bitmap bmp;

                if (cover.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
                {
                    var uri = new Uri(cover);
                    using var assets = AssetLoader.Open(uri);
                    bmp = new Bitmap(assets);
                }
                else if (cover.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    using var http = new HttpClient();
                    using var stream = await http.GetStreamAsync(cover);
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    ms.Position = 0;
                    bmp = new Bitmap(ms);
                }
                else
                {
                    bmp = new Bitmap(cover);
                }

                CoverBitmap = bmp;
            }
            catch
            {
                var fallbackUri = new Uri("avares://FloatlyRemake/Assets/Images/default.png");
                using var fallbackStream = AssetLoader.Open(fallbackUri);
                CoverBitmap = new Bitmap(fallbackStream);
            }

            return _coverBitmap;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    public class Notification
    {
        public string? Message { get; set; }
    }
}
