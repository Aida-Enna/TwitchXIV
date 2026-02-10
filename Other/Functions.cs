using System.Diagnostics;

namespace TwitchXIV.Other
{
    public class Functions
    {
        public static void OpenWebsite(string URL)
        {
            Process.Start(new ProcessStartInfo { FileName = URL, UseShellExecute = true });
        }
    }
}