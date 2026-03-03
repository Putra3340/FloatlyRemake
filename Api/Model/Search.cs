using System;
using System.Collections.Generic;
using System.Text;

namespace FloatlyRemake.Api.Model
{
    public class ApiSong
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ArtistName { get; set; }
        public string Cover { get; set; }
        public string SongLength { get; set; }
        public string PlayCount { get; set; }
    }
    public class LibrarySearchResult
    {
        public List<ApiSong> Songs { get; set; }
    }
}
