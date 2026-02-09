using System;
using System.Numerics;
using TwitchLib.Client.Models;

namespace TwitchXIV.Logging
{
    public sealed class ChatLogger(ChatUi chatUi) : ILogger
    {
        #region Variables

        private ChatUi ChatUi { get; init; } = chatUi;
        private static readonly string[] TwitchDefaultColors =
        [
            "#FF0000", // Red
            "#0000FF", // Blue
            "#008000", // Green
            "#B22222", // Firebrick
            "#FF7F50", // Coral
            "#9ACD32", // YellowGreen
            "#FF4500", // OrangeRed
            "#2E8B57", // SeaGreen
            "#DAA520", // GoldenRod
            "#D2691E", // Chocolate
            "#5F9EA0", // CadetBlue
            "#1E90FF", // DodgerBlue
            "#FF69B4", // HotPink
            "#8A2BE2", // BlueViolet
            "#00FF7F"  // SpringGreen
        ];

        #endregion Variables
        #region Printing

        public void Print(ChatMessage message)
        {
            ChatUi?.AddLine(message.Channel, message.DisplayName, message.Message, GetColor(message.DisplayName, message.ColorHex));
        }

        public void Print(SentMessage message)
        {
            ChatUi?.AddLine(message.Channel, message.DisplayName, message.Message, GetColor(message.DisplayName, message.ColorHex));
        }

        public void Print(string message)
        {
            ChatUi?.AddLine($"TwitchXIV", "System", message, new(1f, 0f, 0f, 1f));
        }

        #endregion Printing
        #region Colors

        private Vector4 GetColor(string username, string hexColor)
        {
            var hex = string.IsNullOrEmpty(hexColor) ? GetTwitchLikeColor(username) : hexColor;
            var r = Convert.ToInt32(hex[1..3], 16) / 255f;
            var g = Convert.ToInt32(hex[3..5], 16) / 255f;
            var b = Convert.ToInt32(hex[5..7], 16) / 255f;
            return new(r, g, b, 1f);
        }

        private static string GetTwitchLikeColor(string username)
        {
            if (string.IsNullOrEmpty(username))
                return "#FFFFFF";

            int hash = 0;
            foreach (char c in username.ToLowerInvariant())
            {
                hash = (hash * 31) + c;
            }

            return TwitchDefaultColors[Math.Abs(hash) % TwitchDefaultColors.Length];
        }

        #endregion Colors
    }
}
