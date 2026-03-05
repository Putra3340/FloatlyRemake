using System;
using System.Collections.Generic;
using System.Text;

namespace FloatlyRemake
{
    // THIS TO STORE USER PREFERENCES, OR ANYTHING NECESSARY FOR GLOBAL ACCESS
    public static class Prefs
    {
        public static string ServerUrl = "https://floatly.starhost.web.id";
        public static string LoginToken = "MTc3MjQ4MDEzMzAwOXwyOWI0YjQ5Mnw3Mzk4OTZ8ZTcwYzEzNDMyNjY2NGMwNWE2ZTZhNmE2MTk4Y2U0ZTJ8MTc3MzA4NDkzMw==";
        public static string TempDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "FloatlyRemake");
    }
}
