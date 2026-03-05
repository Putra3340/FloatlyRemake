using Avalonia.Threading;
using FloatlyRemake.Api.Model;
using FloatlyRemake.Models;
using FloatlyRemake.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FloatlyRemake.Api
{
    public static class ApiLibrary
    {
        public static HttpClient http = new();
        public static async Task Search(string query)
        {
            var response = await http.GetAsync($"{Prefs.ServerUrl}/api/library/v4/search?anycontent={Uri.EscapeDataString(query)}&token={Prefs.LoginToken}");
            if (!response.IsSuccessStatusCode)
                throw new Exception("API request failed with status code: " + response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var apiSongs = JsonSerializer.Deserialize<LibrarySearchResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (apiSongs == null || apiSongs.Songs.Count == 0)
                throw new Exception("No songs found for the query.");
            var songs = apiSongs.Songs.Select(a => new Song { Id = a.Id, Title = a.Title, ArtistName = a.ArtistName, Cover = a.Cover, SongLength = a.SongLength, PlayCount = a.PlayCount }).ToList();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StaticBinding.Songs.Clear();
                foreach (var song in songs)
                    StaticBinding.Songs.Add(song);
            });

            // load covers in background
            foreach (var song in songs)
                _ = song.LoadCoverAsync();
        }
        public static async Task Play(string songId)
        {
            var response = await http.GetAsync($"{Prefs.ServerUrl}/api/library/v3/play/{Uri.EscapeDataString(songId)}?token={Prefs.LoginToken}");
            if (!response.IsSuccessStatusCode)
                throw new Exception("API request failed with status code: " + response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var apiSongs = JsonSerializer.Deserialize<Song>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new Exception("No songs found for the query.");
            var lyrics = await SRTParser.ParseSRT(apiSongs.Lyrics);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StaticBinding.CurrentSong = apiSongs;
                MainWindow.Instance.PlayerCard.DataContext = apiSongs;
                MainWindow.Instance.DataContext = apiSongs;
                MediaHandler.Play(apiSongs.Music);
                MediaHandler.TriggerLoad(false);
                MainWindow.Instance.ShowNotification($"Now Playing {apiSongs.Title} - {apiSongs.ArtistName}");
                StaticBinding.LyricLists.Clear();
                foreach (var lyric in lyrics)
                    StaticBinding.LyricLists.Add(lyric);
            });
        }
        public static async Task PlayVideo(string songId)
        {
            var response = await http.GetAsync($"{Prefs.ServerUrl}/api/library/v3/video/{Uri.EscapeDataString(songId)}?token={Prefs.LoginToken}");
            if (!response.IsSuccessStatusCode)
                throw new Exception("API request failed with status code: " + response.StatusCode);
            var res = await response.Content.ReadAsStringAsync();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StaticBinding.CurrentSong.MoviePath = res;
                MediaHandler.Play(res);
                MediaHandler.TriggerLoad(true);
            });
        }
    }
}
