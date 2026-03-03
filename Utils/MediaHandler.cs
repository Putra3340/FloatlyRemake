using System;
using System.IO;
using Avalonia.Threading;
using LibVLCSharp.Shared;

namespace FloatlyRemake.Utils
{
    /// <summary>
    /// Centralized LibVLC + MediaPlayer manager.
    /// Call MediaHandler.Attach(videoView.MediaPlayer = MediaHandler.Player) once in UI,
    /// then MediaHandler.Play(uri) anywhere (on UI thread is fine, not required).
    /// </summary>
    public static class MediaHandler
    {
        private static readonly object _lock = new();

        private static LibVLC? _libVlc;
        public static MediaPlayer? _player;

        public static bool IsInitialized => _libVlc != null && _player != null;

        public static LibVLC LibVlc
        {
            get
            {
                EnsureInitialized();
                return _libVlc!;
            }
        }

        public static MediaPlayer Player
        {
            get
            {
                EnsureInitialized();
                return _player!;
            }
        }

        /// <summary>
        /// Initialize LibVLC + MediaPlayer once. Safe to call multiple times.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (IsInitialized) return;

            lock (_lock)
            {
                if (IsInitialized) return;

                // Initializes native lib loading logic inside LibVLCSharp.
                // (Still requires native libvlc to exist on the OS / in output.)
                Core.Initialize();

                // Try to locate the packaged VLC folder (common when using VideoLAN.LibVLC.* packages)
                // Windows example output: .../libvlc/win-x64/
                // Linux example output (if bundled): .../libvlc/linux-x64/
                var libvlcDir = TryFindBundledLibVlcDir();

                if (!string.IsNullOrWhiteSpace(libvlcDir))
                {
                    var pluginsDir = Path.Combine(libvlcDir, "plugins");

                    // Create LibVLC and explicitly point plugin directory (critical on many setups)
                    _libVlc = new LibVLC(
                        "--quiet",
                        "--no-video-title-show",
                        $"--plugin-path={pluginsDir}"
                    );
                }
                else
                {
                    // Fallback: rely on system-installed libvlc (common on Linux)
                    _libVlc = new LibVLC("--quiet", "--no-video-title-show");
                }

                _player = new MediaPlayer(_libVlc);
            }
        }

        /// <summary>
        /// Attach the shared MediaPlayer to a LibVLCSharp.Avalonia VideoView.
        /// Call once after InitializeComponent() in your Window/UserControl.
        /// </summary>
        public static void AttachToVideoView(object videoView)
        {
            // Avoid hard reference here if you want (keeps this class UI-agnostic).
            // But easiest is to do in UI:
            // VideoView.MediaPlayer = MediaHandler.Player;
            _ = videoView;
            EnsureInitialized();
        }

        public static void Play(Uri uri, bool autoplay = true)
        {
            EnsureInitialized();

            // Dispose previous media to avoid leaks
            using var media = new Media(_libVlc!, uri);

            // Optional: network-friendly options (uncomment if you want)
            // media.AddOption(":network-caching=1000"); // ms
            // media.AddOption(":clock-jitter=0");
            // media.AddOption(":clock-synchro=0");

            _player!.Play(media);

            if (!autoplay)
                _player.Pause();
        }

        public static void Play(string uriOrPath, bool autoplay = true)
        {
            if (string.IsNullOrWhiteSpace(uriOrPath))
                return;

            // If it's already a URL, use it. If it's a file path, convert to file:// URI.
            Uri uri;
            if (Uri.TryCreate(uriOrPath, UriKind.Absolute, out var abs) &&
                (abs.Scheme == Uri.UriSchemeHttp || abs.Scheme == Uri.UriSchemeHttps || abs.Scheme == Uri.UriSchemeFile))
            {
                uri = abs;
            }
            else
            {
                uri = new Uri(Path.GetFullPath(uriOrPath));
            }

            Play(uri, autoplay);
        }

        public static void Pause()
        {
            if (!IsInitialized) return;
            _player!.Pause();
        }

        public static void Stop()
        {
            if (!IsInitialized) return;
            _player!.Stop();
        }

        public static void SetVolume(int volume0to100)
        {
            if (!IsInitialized) return;
            var v = Math.Clamp(volume0to100, 0, 100);
            _player!.Volume = v;
        }

        public static void SeekTo(TimeSpan position)
        {
            if (!IsInitialized) return;
            if (position < TimeSpan.Zero) position = TimeSpan.Zero;
            _player!.Time = (long)position.TotalMilliseconds;
        }

        public static void Dispose()
        {
            lock (_lock)
            {
                try
                {
                    _player?.Stop();
                    _player?.Dispose();
                }
                catch { /* ignore */ }

                try
                {
                    _libVlc?.Dispose();
                }
                catch { /* ignore */ }

                _player = null;
                _libVlc = null;
            }
        }

        private static string? TryFindBundledLibVlcDir()
        {
            // Common layouts when using VideoLAN.LibVLC.* NuGet packages:
            // AppContext.BaseDirectory/libvlc/win-x64/
            // AppContext.BaseDirectory/libvlc/linux-x64/
            // AppContext.BaseDirectory/libvlc/osx-x64/ etc.
            var baseDir = AppContext.BaseDirectory;

            var libvlcRoot = Path.Combine(baseDir, "libvlc");
            if (!Directory.Exists(libvlcRoot))
                return null;

            // Check a few common rid folders. Add more if needed.
            var candidates = new[]
            {
                Path.Combine(libvlcRoot, "win-x64"),
                Path.Combine(libvlcRoot, "win-arm64"),
                Path.Combine(libvlcRoot, "linux-x64"),
                Path.Combine(libvlcRoot, "linux-arm64"),
                Path.Combine(libvlcRoot, "osx-x64"),
                Path.Combine(libvlcRoot, "osx-arm64"),
            };

            foreach (var dir in candidates)
            {
                if (!Directory.Exists(dir)) continue;

                // Detect presence of libvlc + plugins
                var plugins = Path.Combine(dir, "plugins");
                if (Directory.Exists(plugins))
                    return dir;
            }

            return null;
        }
    }
}