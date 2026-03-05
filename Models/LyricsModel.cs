using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace FloatlyRemake.Models
{
    public class LyricList : INotifyPropertyChanged
    {
        public int LyricIndex { get => field; set { field = value; OnPropertyChanged(); } } = 0;
        public TimeSpan Start { get => field; set { field = value; OnPropertyChanged(); } } = TimeSpan.Zero;
        public TimeSpan End { get => field; set { field = value; OnPropertyChanged(); } } = TimeSpan.MaxValue;

        public bool IsActive
        {
            get => field;
            set { field = value; OnPropertyChanged(); }
        }

        public string Text { get => field; set { field = value; OnPropertyChanged(); } } = "";
        public string Text2 { get => field; set { field = value; OnPropertyChanged(); } } = "";

        public string CombinedText
        {
            get => $"{Text}\n{Text2}".Trim();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string n = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
