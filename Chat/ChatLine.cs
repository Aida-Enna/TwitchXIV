using System.Numerics;

namespace TwitchXIV.Chat
{
    #region ChatLine
    public class ChatLine(
        string timestamp,
        string channel,
        string user,
        string message,
        Vector4 userColor
        )
    {
        public string Timestamp { get; init; } = timestamp;
        public string Channel { get; init; } = channel;
        public string User { get; init; } = user;
        public string Message { get; init; } = message;

        public Vector4 UserColor { get; set; } = userColor;

        public string FullText { get => $"{Timestamp} {Channel} {User}: {Message}"; }
    }

    #endregion ChatLine
    #region WrappedChatLine

    public sealed class WrappedChatLine(string timestamp, string channel, string user, string message, Vector4 userColor) : ChatLine(timestamp, channel, user, message, userColor)
    {
        public float WrappedLength { get; set; } = -1f;

        public string WrappedText { get; set; } = message;
    }

    #endregion WrappedChatLine
}
