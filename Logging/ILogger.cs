using TwitchLib.Client.Models;

namespace TwitchXIV.Logging
{
    public interface ILogger
    {
        public void Print(ChatMessage message);
        public void Print(SentMessage message);
        public void Print(string message);
    }
}
