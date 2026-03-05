using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FloatlyRemake.Models
{
    public static class StaticBinding
    {
        public static ObservableCollection<Song> Songs { get; } = new()
{
    new Song
    {
        Id = "1",
        Title = "Starlight Echo",
        ArtistName = "Elysian Dreams",
        SongLength = "03:45",
        PlayCount = "1024",
        Cover = "avares://FloatlyRemake/Assets/Images/default.png",
        CreatedAt = DateTime.Now
    },
    new Song
    {
        Id = "2",
        Title = "Crimson Horizon",
        ArtistName = "Nova Pulse",
        SongLength = "04:12",
        PlayCount = "2048",
        Cover = "avares://FloatlyRemake/Assets/Images/default.png",
        CreatedAt = DateTime.Now
    },
    new Song
    {
        Id = "3",
        Title = "Midnight Bloom",
        ArtistName = "Lunar Veil",
        SongLength = "05:01",
        PlayCount = "512",
        Cover = "avares://FloatlyRemake/Assets/Images/default.png",
        CreatedAt = DateTime.Now
    }
};
        public static Song CurrentSong = new Song
        {
            Id = "1",
            Title = "Starlight Echo",
            ArtistName = "Elysian Dreams",
            SongLength = "03:45",
            PlayCount = "1024",
            Cover = "avares://FloatlyRemake/Assets/Images/default.png",
            CreatedAt = DateTime.Now
        };
        public static ObservableCollection<Notification> NotificationList = new ObservableCollection<Notification>() { new Notification { Message = "This is notification"} };
        public static ObservableCollection<LyricList> LyricLists = new ObservableCollection<LyricList>() { new LyricList { Start = TimeSpan.Zero, End = TimeSpan.FromSeconds(5), Text = "This is lyric" } };
    }
}
