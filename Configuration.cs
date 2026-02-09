using Dalamud.Configuration;
using System.Numerics;

namespace TwitchXIV
{
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }
        public string Username = "your twitch.tv username";
        public string ChannelToSend = "twitch.tv channel to chat in";
        public string OAuthCode = "";
        public bool TwitchEnabled = true;
        public int MaxChatlines = 500;
        public Vector4 BackgroundColor = new(0f, 0f, 0f, 0.4f);
        public Vector4 TimeColor = new(0.7f, 0.7f, 0.7f, 1f);
        public Vector4 ChannelColor = new(1.0f, 0.8f, 0.4f, 1f);
        public Vector4 MessageColor = new(0.9f, 0.9f, 0.9f, 1f);
        public Vector4 HighlightColor = new(1f, 1f, 1f, 0.1f);
    }
}
